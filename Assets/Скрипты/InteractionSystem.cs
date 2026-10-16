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

    private Ray _ray;
    private RaycastHit _hit;
    
    private float _holdTime = 0f;
    private float _requiredHoldTime = 1.0f;
    private bool _isActionTriggered = false;

    void Start()
    {
        if (!playerCamera)
        {
            playerCamera = Camera.main;
        }
        
        _ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    }
    
    void Update()
    {
        if (!_canInteract)
        {
            interactionText.text = "";
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (_heldObject == null)
        {
            // Grab item
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if (hit.collider.CompareTag("Pickable"))
                {
                    interactionText.text = $"Take the {hit.collider.name}";
                    
                    if (Input.GetKeyDown(interactionKeyBind))
                    {
                        interactionText.text = "";
                        source.PlayOneShot(R.Instance.takeClip);
                        
                        _heldObject = hit.collider.gameObject;
                        
                        _heldRb = _heldObject.GetComponent<Rigidbody>();
                        if (_heldRb != null) _heldRb.isKinematic = true;

                        _heldObject.transform.SetParent(heldPoint);
                        _heldObject.transform.localPosition = Vector3.zero;
                        _heldObject.transform.localRotation = Quaternion.identity;
                    }
                }
                else
                {
                    interactionText.text = "";
                }
            }
            else
            {
                interactionText.text = "";
            }
        }
        else
        {
            interactionText.text = $"Hold 'G' for 1 second to throw\nOR\nPress 'G' for drop {_heldObject.name}";
            
            if (Input.GetKeyDown(KeyCode.G))
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
                if (!_isActionTriggered) DropHeldObject();
                _holdTime = 0f;
                _isActionTriggered = false;
            }
            
            // Place item on _
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if (hit.collider.CompareTag("PlaceOn"))
                {
                    interactionText.text = $"Place the {_heldObject.transform.name}";
                    
                    if (Input.GetKeyDown(interactionKeyBind))
                    {
                        interactionText.text = "";
                        source.PlayOneShot(R.Instance.placeClip);
                        
                        _heldObject.transform.SetParent(null);
                        _heldObject.transform.localRotation = Quaternion.identity;

                        _heldRb = _heldObject.GetComponent<Rigidbody>();
                        if (_heldRb != null) _heldRb.isKinematic = false;

                        _heldObject.transform.position = hit.point;

                        Physics.SyncTransforms();

                        Collider heldCollider = _heldObject.GetComponent<Collider>();
                        if (heldCollider != null)
                        {
                            float heightOffset = hit.point.y - heldCollider.bounds.min.y;
                            _heldObject.transform.position += new Vector3(0f, heightOffset, 0f);
                        }

                        _heldObject = null;
                    }
                }
                else
                {
                    interactionText.text = "";
                }
            }
        }

        // Open or Close door
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Door"))
            {
                if (hit.transform.GetComponent<SimpleDoor>())
                {
                    SimpleDoor door = hit.transform.GetComponent<SimpleDoor>();

                    if (Input.GetKeyDown(interactionKeyBind))
                    {
                        door.ToggleDoorState();
                    }
                }
            }
        }
    }

    private void ThrowHeldObject()
    {
        _heldObject.transform.SetParent(null);
        _heldRb.useGravity = true;
        _heldRb.isKinematic = false;

        if (Camera.main) _heldRb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

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