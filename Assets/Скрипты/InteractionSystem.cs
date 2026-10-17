using TMPro;
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

    private GameObject _heldObject; 
    private bool _canInteract = true;
    private Rigidbody _heldRb;
    
    private float _holdTime = 0f;
    private float _requiredHoldTime = 1.0f;
    private bool _isActionTriggered = false;

    void Start()
    {
        if (!playerCamera)
        {
            playerCamera = Camera.main;
        }
    }
    
    void Update()
    {
        if (!_canInteract || !playerCamera)
        {
            if (interactionText) interactionText.text = "";
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, interactionDistance, interactionLayer);

        if (_heldObject == null)
        {
            if (hasHit)
            {
                // Grab item
                if (hit.collider.CompareTag("Pickable"))
                {
                    CrosshairController.Instance.UpdateCrosshair(true);
                    interactionText.text = $"Take the {hit.collider.name}";
                    
                    if (Input.GetKeyDown(interactionKeyBind))
                    {
                        PickUpObject(hit.collider.gameObject);
                    }
                }
                // Open / close the door
                else if (hit.collider.CompareTag("Door"))
                {
                    CrosshairController.Instance.UpdateCrosshair(true);
                    interactionText.text = "Press 'E' to use door";
                    
                    if (Input.GetKeyDown(interactionKeyBind))
                    {
                        if (hit.transform.TryGetComponent<SimpleDoor>(out var door))
                        {
                            door.ToggleDoorState();
                        }
                    }
                }
                else
                {
                    interactionText.text = "";
                    CrosshairController.Instance.UpdateCrosshair(false);
                }
            }
            else
            {
                interactionText.text = "";
                CrosshairController.Instance.UpdateCrosshair(false);
            }
        }
        else
        {
            // Place object on _
            if (hasHit && hit.collider.CompareTag("PlaceOn"))
            {
                interactionText.text = $"Place the {_heldObject.transform.name}";
                CrosshairController.Instance.UpdateCrosshair(true);
                
                if (Input.GetKeyDown(interactionKeyBind))
                {
                    PlaceObject(hit);
                    return;
                }
            }
            else
            {
                interactionText.text = $"Hold 'G' for 1 second to throw\nOR\nPress 'G' to drop {_heldObject.name}";
                CrosshairController.Instance.UpdateCrosshair(false);
            }

            // Drop or Throw
            if (Input.GetKeyDown(throwKeyBind))
            {
                _holdTime = 0f;
                _isActionTriggered = false;
            }

            // Throw item
            if (Input.GetKey(throwKeyBind) && !_isActionTriggered)
            {
                _holdTime += Time.deltaTime;
                if (_holdTime >= _requiredHoldTime)
                {
                    ThrowHeldObject();
                    _isActionTriggered = true;
                }
            }

            // Drop item
            if (Input.GetKeyUp(throwKeyBind))
            {
                if (!_isActionTriggered)
                {
                    DropHeldObject();
                }
                _holdTime = 0f;
                _isActionTriggered = false;
            }
        }
    }

    private void PickUpObject(GameObject targetObject)
    {
        interactionText.text = "";
        if (source && R.Instance && R.Instance.takeClip)
        {
            source.PlayOneShot(R.Instance.takeClip);
        }
        
        _heldObject = targetObject;
        _heldRb = _heldObject.GetComponent<Rigidbody>();
        
        if (_heldRb != null) 
        {
            _heldRb.isKinematic = true;
            _heldRb.useGravity = false;
        }

        _heldObject.transform.SetParent(heldPoint);
        _heldObject.transform.localPosition = Vector3.zero;
        _heldObject.transform.localRotation = Quaternion.identity;
    }

    private void PlaceObject(RaycastHit hit)
    {
        interactionText.text = "";
        if (source && R.Instance && R.Instance.placeClip)
        {
            source.PlayOneShot(R.Instance.placeClip);
        }
        
        _heldObject.transform.SetParent(null);
        _heldObject.transform.localRotation = Quaternion.identity;

        if (_heldRb != null)
        {
            _heldRb.isKinematic = false;
            _heldRb.useGravity = true;
        }

        _heldObject.transform.position = hit.point;
        Physics.SyncTransforms();

        if (_heldObject.TryGetComponent<Collider>(out var heldCollider))
        {
            float heightOffset = hit.point.y - heldCollider.bounds.min.y;
            _heldObject.transform.position += new Vector3(0f, heightOffset, 0f);
        }

        _heldObject = null;
        _heldRb = null;
    }

    private void ThrowHeldObject()
    {
        _heldObject.transform.SetParent(null);
        _heldRb.useGravity = true;
        _heldRb.isKinematic = false;

        if (playerCamera) _heldRb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

        _heldObject = null;
        _heldRb = null;
        interactionText.text = "";
    }

    private void DropHeldObject()
    {
        if (!_heldObject) return;

        _heldObject.transform.SetParent(null);
        _heldRb.useGravity = true;
        _heldRb.isKinematic = false;
        _heldRb.linearVelocity = Vector3.zero;
        _heldRb.angularVelocity = Vector3.zero;

        _heldObject = null;
        _heldRb = null;
        interactionText.text = "";
    }
}