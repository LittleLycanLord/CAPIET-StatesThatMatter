using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ParticleBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private Camera mainCamera;
        private SphereCollider sphereCollider;
        private Rigidbody rb;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Drag Settings")]
        [SerializeField] private float dragDistanceFromCamera = 10f;
        [SerializeField] private bool enableDragging = true;
        [SerializeField] private float dragSpeed = 20f;
        [SerializeField] private float dragDamping = 15f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool isDragging = false;
        private Vector3 offset;
        private float zDistanceFromCamera;
        private float originalDrag;
        private Vector3 currentTargetPosition;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            mainCamera = Camera.main;
            sphereCollider = GetComponent<SphereCollider>();
            rb = GetComponent<Rigidbody>();
            
            // Store original drag value
            if (rb != null)
            {
                originalDrag = rb.linearDamping;
            }
        }

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Start() { }

        void Update() 
        {
            if (!enableDragging) return;

            HandleTouchInput();
        }

        void FixedUpdate()
        {
            if (isDragging && rb != null)
            {
                ApplyDragPhysics();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        private void HandleTouchInput()
        {
            // New Input System - Touch input
            if (Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    TryStartDrag(touch.screenPosition);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && isDragging)
                {
                    DragObject(touch.screenPosition);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    EndDrag();
                }
            }
            // Fallback to mouse input for editor testing
            else if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TryStartDrag(Mouse.current.position.ReadValue());
                }
                else if (Mouse.current.leftButton.isPressed && isDragging)
                {
                    DragObject(Mouse.current.position.ReadValue());
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    EndDrag();
                }
            }
        }

        private void TryStartDrag(Vector3 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == sphereCollider)
                {
                    isDragging = true;
                    zDistanceFromCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
                    
                    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistanceFromCamera));
                    offset = transform.position - worldPosition;
                    
                    // Increase drag for responsive control
                    if (rb != null)
                    {
                        rb.linearDamping = dragDamping;
                    }
                }
            }
        }

        private void DragObject(Vector3 screenPosition)
        {
            // Calculate and store target position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistanceFromCamera));
            currentTargetPosition = worldPosition + offset;
        }
        
        private void ApplyDragPhysics()
        {
            if (rb == null) return;
            
            // Calculate desired velocity to reach target
            Vector3 direction = currentTargetPosition - transform.position;
            Vector3 desiredVelocity = direction * dragSpeed;
            
            // Smoothly interpolate velocity
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, 0.5f);
        }

        private void EndDrag()
        {
            isDragging = false;
            
            // Restore original drag
            if (rb != null)
            {
                rb.linearDamping = originalDrag;
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}