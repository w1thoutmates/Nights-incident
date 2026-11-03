using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Values")]
    [SerializeField]
    [Range(1f, 10f)]
    private float interactionDistance = 3f;

    [SerializeField]
    [Range(5f, 20f)]
    private float throwForce = 10f;

    [SerializeField]
    private KeyCode interactionKeyBind = KeyCode.E;

    [SerializeField]
    private KeyCode throwKeyBind = KeyCode.G;

    [SerializeField]
    private KeyCode serveKeyBind = KeyCode.F;

    [SerializeField]
    private LayerMask interactionLayer;

    [Header("References")]
    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private Transform heldPoint;

    [SerializeField]
    private TextMeshProUGUI interactionText;

    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private Animator armsAnimator;

    [Header("Pickup Camera Animation (DOTween)")]
    [SerializeField] private float pickupAnimDuration = 0.25f;
    [SerializeField] private float fovZoomAmount = 5f;
    [SerializeField] private float placeFovAmount = 3f;
    [SerializeField] private CinemachineCamera virtualCamera;

    private GameObject _heldObject;
    private bool _canInteract = true;
    private Rigidbody _heldRb;
    private Usable _heldUsable;
    private RigidbodyInterpolation _savedInterpolation = RigidbodyInterpolation.None;

    private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];

    private float _holdTime = 0f;
    private float _requiredHoldTime = 1f;
    private bool _isActionTriggered = false;

    private int _startLayerValue;
    private int _handsLayerValue;

    private float _defaultFov;
    private Tween _cameraTween;

    private static readonly int HoldingHash = Animator.StringToHash("Holding");

    public bool IsHoldingItem { get { return _heldObject != null; } }

    public bool IsHoldingContainer { get { return _heldUsable is Container; } }

    private void Start()
    {
        if (!playerCamera)
        {
            playerCamera = Camera.main;
        }

        if (!virtualCamera)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (virtualCamera != null)
        {
            _defaultFov = virtualCamera.Lens.FieldOfView;
        }
        else if (playerCamera != null)
        {
            _defaultFov = playerCamera.fieldOfView;
        }

        _handsLayerValue = LayerMask.NameToLayer("Hands");
    }

    private void Update()
    {
        if (!_canInteract || !playerCamera)
        {
            if (interactionText) interactionText.text = "";
            return;
        }

        if (armsAnimator) armsAnimator.SetBool(HoldingHash, _heldObject != null);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, interactionDistance, interactionLayer);

        if (_heldObject == null)
        {
            HandleEmptyHands(hasHit, hit);
            return;
        }

        HandleFullHands(hasHit, hit);
    }

    private void HandleEmptyHands(bool hasHit, RaycastHit hit)
    {
        if (!hasHit)
        {
            interactionText.text = "";
            SetCrosshair(false);
            return;
        }

        if (hit.collider.CompareTag("Pickable"))
        {
            SetCrosshair(true);
            interactionText.text = "Take the " + hit.collider.name;

            if (Input.GetKeyDown(interactionKeyBind))
            {
                PickUpObject(GetGrabbable(hit.collider));
            }

            return;
        }

        if (hit.collider.CompareTag("Door"))
        {
            SetCrosshair(true);
            interactionText.text = "Press '" + interactionKeyBind + "' to use door";

            if (Input.GetKeyDown(interactionKeyBind))
            {
                SimpleDoor door;
                if (hit.transform.TryGetComponent(out door))
                {
                    door.ToggleDoorState();
                }
            }

            return;
        }

        interactionText.text = "";
        SetCrosshair(false);
    }

    private void HandleFullHands(bool hasHit, RaycastHit hit)
    {
        Container pourTarget = _heldUsable != null ? FindContainerTarget() : null;
        Customer customer = hasHit ? hit.collider.GetComponentInParent<Customer>() : null;

        if (hasHit && hit.collider.CompareTag("PlaceOn"))
        {
            interactionText.text = "Place the " + _heldObject.name;
            SetCrosshair(true);

            if (Input.GetKeyDown(interactionKeyBind))
            {
                PlaceObject(hit);
                return;
            }
        }
        else if (customer != null && _heldUsable is Container)
        {
            interactionText.text = "Press '" + serveKeyBind + "' to serve";
            SetCrosshair(true);

            if (Input.GetKeyDown(serveKeyBind))
            {
                TryServe(customer);
                return;
            }
        }
        else
        {
            interactionText.text = "Hold 'G' for 1 second to throw\nOR\nPress 'G' to drop " + _heldObject.name;

            if (_heldUsable != null)
            {
                interactionText.text += _heldUsable is Container
                    ? "\nLMB - shake, RMB - pour"
                    : "\nHold LMB to pour";
            }

            SetCrosshair(pourTarget != null);
        }

        if (_heldUsable != null)
        {
            if (Input.GetMouseButton(0)) _heldUsable.OnPrimaryHeld(pourTarget, Time.deltaTime);
            if (Input.GetMouseButtonUp(0)) _heldUsable.OnPrimaryReleased();
            if (Input.GetMouseButton(1)) _heldUsable.OnSecondaryHeld(pourTarget, Time.deltaTime);
        }

        if (Input.GetKeyDown(throwKeyBind))
        {
            _holdTime = 0f;
            _isActionTriggered = false;
        }

        if (Input.GetKey(throwKeyBind) && !_isActionTriggered)
        {
            _holdTime += Time.deltaTime;
            if (_holdTime >= _requiredHoldTime)
            {
                ThrowHeldObject();
                _isActionTriggered = true;
            }
        }

        if (Input.GetKeyUp(throwKeyBind))
        {
            if (!_isActionTriggered) DropHeldObject();
            _holdTime = 0f;
            _isActionTriggered = false;
        }
    }

    private void PickUpObject(GameObject targetObject)
    {
        if (targetObject == null) return;

        interactionText.text = "";

        if (source && R.Instance && R.Instance.takeClip)
        {
            source.PlayOneShot(R.Instance.takeClip);
        }

        _startLayerValue = targetObject.layer;
        PlayPickupCameraEffect();
        AttachToHand(targetObject);
    }

    private void AttachToHand(GameObject targetObject)
    {
        _heldObject = targetObject;
        _heldRb = targetObject.GetComponent<Rigidbody>();
        _heldUsable = targetObject.GetComponentInParent<Usable>();
        if (_heldUsable == null) _heldUsable = targetObject.GetComponentInChildren<Usable>();

        if (_heldRb != null)
        {
            _heldRb.isKinematic = true;
            _heldRb.useGravity = false;
            _savedInterpolation = _heldRb.interpolation;
            _heldRb.interpolation = RigidbodyInterpolation.None;
        }

        SetCollidersEnabled(targetObject, false);

        if (_handsLayerValue >= 0) SetLayerRecursive(targetObject, _handsLayerValue);

        if (heldPoint == null) return;

        targetObject.transform.SetParent(heldPoint);
        targetObject.transform.localRotation = _heldUsable != null
            ? Quaternion.Euler(_heldUsable.heldLocalEuler)
            : Quaternion.identity;
        targetObject.transform.localPosition = _heldUsable != null
            ? _heldUsable.heldLocalPosition
            : Vector3.zero;
    }

    private void PlaceObject(RaycastHit hit)
    {
        GameObject placed = _heldObject;
        interactionText.text = "";
        ReleaseHeldUsable();

        if (source && R.Instance && R.Instance.placeClip)
        {
            source.PlayOneShot(R.Instance.placeClip);
        }

        PlayPlaceCameraEffect();

        SetCollidersEnabled(placed, true);
        SetLayerRecursive(placed, _startLayerValue);
        placed.transform.SetParent(null);
        placed.transform.localRotation = Quaternion.identity;

        if (_heldRb != null)
        {
            _heldRb.isKinematic = false;
            _heldRb.useGravity = true;
            _heldRb.interpolation = _savedInterpolation;
        }

        placed.transform.position = hit.point;
        Physics.SyncTransforms();

        Bounds bounds;
        if (TryGetWorldBounds(placed, out bounds))
        {
            placed.transform.position += Vector3.up * (hit.point.y - bounds.min.y);
        }

        _heldObject = null;
        _heldRb = null;
    }

    private void ThrowHeldObject()
    {
        GameObject thrown = _heldObject;
        ReleaseHeldUsable();

        SetCollidersEnabled(thrown, true);
        SetLayerRecursive(thrown, _startLayerValue);
        thrown.transform.SetParent(null);

        if (_heldRb != null)
        {
            _heldRb.useGravity = true;
            _heldRb.isKinematic = false;
            _heldRb.interpolation = _savedInterpolation;

            if (playerCamera)
            {
                _heldRb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            }
        }

        _heldObject = null;
        _heldRb = null;
        interactionText.text = "";
    }

    private void DropHeldObject()
    {
        if (_heldObject == null) return;

        GameObject dropped = _heldObject;
        ReleaseHeldUsable();

        SetCollidersEnabled(dropped, true);
        SetLayerRecursive(dropped, _startLayerValue);
        dropped.transform.SetParent(null);

        if (_heldRb != null)
        {
            _heldRb.useGravity = true;
            _heldRb.isKinematic = false;
            _heldRb.interpolation = _savedInterpolation;
            _heldRb.linearVelocity = Vector3.zero;
            _heldRb.angularVelocity = Vector3.zero;
        }

        _heldObject = null;
        _heldRb = null;
        interactionText.text = "";
    }

    private void ReleaseHeldUsable()
    {
        if (_heldUsable == null) return;

        SetCollidersEnabled(_heldUsable.gameObject, true);
        _heldUsable.OnPrimaryReleased();
        _heldUsable = null;
    }

    private Container FindContainerTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int count = Physics.RaycastNonAlloc(ray, _hitBuffer, interactionDistance, interactionLayer, QueryTriggerInteraction.Ignore);

        float best = float.MaxValue;
        Container found = null;

        for (int i = 0; i < count; i++)
        {
            RaycastHit h = _hitBuffer[i];
            if (_heldObject != null && h.transform.IsChildOf(_heldObject.transform)) continue;

            if (h.distance < best)
            {
                best = h.distance;
                Container container = h.collider.GetComponentInParent<Container>();
                if (container != null) found = container;
            }
        }

        return found;
    }

    private void TryServe(Customer customer)
    {
        if (_heldUsable is Container glass && customer.TryServe(glass))
        {
            Destroy(_heldObject);
            _heldObject = null;
            _heldRb = null;
            _heldUsable = null;
            interactionText.text = "";
        }
    }

    private static GameObject GetGrabbable(Collider col)
    {
        Usable usable = col.GetComponentInParent<Usable>();
        return usable != null ? usable.gameObject : col.gameObject;
    }

    private static void SetCollidersEnabled(GameObject go, bool enabled)
    {
        Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = enabled;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        if (layer < 0) return;

        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private static bool TryGetWorldBounds(GameObject go, out Bounds bounds)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = new Bounds(go.transform.position, Vector3.zero);
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    private static void SetCrosshair(bool active)
    {
        if (CrosshairController.Instance != null) CrosshairController.Instance.UpdateCrosshair(active);
    }

    private void PlayPickupCameraEffect()
    {
        _cameraTween?.Kill();

        float targetFov = _defaultFov - fovZoomAmount;

        if (virtualCamera != null)
        {
            Sequence pickupSeq = DOTween.Sequence();
            pickupSeq.Append(DOTween.To(() => virtualCamera.Lens.FieldOfView, x => virtualCamera.Lens.FieldOfView = x, targetFov, pickupAnimDuration * 0.5f).SetEase(Ease.OutQuad));
            pickupSeq.Append(DOTween.To(() => virtualCamera.Lens.FieldOfView, x => virtualCamera.Lens.FieldOfView = x, _defaultFov, pickupAnimDuration * 0.5f).SetEase(Ease.InQuad));
            _cameraTween = pickupSeq;
        }
        else if (playerCamera != null)
        {
            Sequence pickupSeq = DOTween.Sequence();
            pickupSeq.Append(playerCamera.DOFieldOfView(targetFov, pickupAnimDuration * 0.5f).SetEase(Ease.OutQuad));
            pickupSeq.Append(playerCamera.DOFieldOfView(_defaultFov, pickupAnimDuration * 0.5f).SetEase(Ease.InQuad));
            _cameraTween = pickupSeq;
        }
    }

    private void PlayPlaceCameraEffect()
    {
        _cameraTween?.Kill();

        float targetFov = _defaultFov + placeFovAmount;

        if (virtualCamera != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => virtualCamera.Lens.FieldOfView, x => virtualCamera.Lens.FieldOfView = x, targetFov, pickupAnimDuration * 0.4f).SetEase(Ease.OutQuad));
            seq.Append(DOTween.To(() => virtualCamera.Lens.FieldOfView, x => virtualCamera.Lens.FieldOfView = x, _defaultFov, pickupAnimDuration * 0.6f).SetEase(Ease.InQuad));
            _cameraTween = seq;
        }
        else if (playerCamera != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(playerCamera.DOFieldOfView(targetFov, pickupAnimDuration * 0.4f).SetEase(Ease.OutQuad));
            seq.Append(playerCamera.DOFieldOfView(_defaultFov, pickupAnimDuration * 0.6f).SetEase(Ease.InQuad));
            _cameraTween = seq;
        }
    }
}
