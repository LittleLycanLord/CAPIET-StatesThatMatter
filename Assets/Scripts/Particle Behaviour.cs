using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
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
        private Transform bondingRadiusSprite;
        private TemperatureBrush temperatureBrush;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool enableDragging = true;
        [SerializeField] private float currentTemperature = 30.0f;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Drag Settings")]
        [SerializeField] private float dragDistanceFromCamera = 10f;
        [SerializeField] private float dragSpeed = 20f;
        [SerializeField] private float dragDamping = 15f;
        [SerializeField] private float bondCheckInterval = 0.1f;
        
        [Space(10)]
        [Header("Bond Settings")]
        [SerializeField] private int maximumBondsPerParticle = 4;
        [SerializeField] private GameObject bondingPreview;
        [SerializeField] private GameObject bondPreviewPrefab;
        [SerializeField] private Color bondPreviewColor = Color.yellow;
        [SerializeField] private float bondPreviewWidth = 2.0f;
        
        [Space(10)]
        [Header("Temperature Settings")]
        [SerializeField] [Tooltip("Maximum temperature in Celsius")] private float maxTemperature = 110.0f;
        [SerializeField] [Tooltip("Minimum temperature in Celsius")] private float minTemperature = -10.0f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool isDragging = false;
        private Vector3 offset;
        private float zDistanceFromCamera;
        private float originalDrag;
        private Vector3 currentTargetPosition;
        private ParticleLatticeManager latticeManager;
        private float lastBondCheckTime = 0f;
        private List<GameObject> connectedBonds = new List<GameObject>();
        private List<GameObject> bondPreviews = new List<GameObject>();
        private List<GameObject> previewTargets = new List<GameObject>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            mainCamera = Camera.main;
            sphereCollider = GetComponent<SphereCollider>();
            rb = GetComponent<Rigidbody>();
            
            // Get reference to bonding preview sprite
            if (bondingPreview != null)
            {
                bondingRadiusSprite = bondingPreview.transform;
                // Initially hide the bonding radius
                bondingRadiusSprite.gameObject.SetActive(false);
            }
            
            // Find the lattice manager in the scene
            latticeManager = FindObjectOfType<ParticleLatticeManager>();
            
            // Find the temperature brush in the scene
            temperatureBrush = FindObjectOfType<TemperatureBrush>();
            
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
            
            // Don't allow dragging if temperature brush is in heating or cooling mode
            if (temperatureBrush != null && temperatureBrush.IsAnyModeActive())
            {
                return;
            }

            HandleTouchInput();
        }

        void FixedUpdate()
        {
            if (isDragging && rb != null)
            {
                ApplyDragPhysics();
                
                // Check for bonding opportunities while dragging

                if (latticeManager != null && latticeManager.IsBondingModeEnabled()) {
                    if (Time.time - lastBondCheckTime > bondCheckInterval)
                    {
                        latticeManager.CheckDragBonding(gameObject);
                        UpdateBondPreviews();
                        lastBondCheckTime = Time.time;
                    }
                }
            }
        }
        
        void LateUpdate()
        {
            // Update bond preview line positions
            UpdateBondPreviewLines();
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
                    
                    // Show and resize bonding radius visualization
                    if (bondingRadiusSprite != null && latticeManager != null && latticeManager.IsBondingModeEnabled() )
                    {
                        bondingRadiusSprite.gameObject.SetActive(true);
                        UpdateBondingRadiusSize();
                    }
                    
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
            
            // Hide bonding radius visualization
            if (bondingRadiusSprite != null)
            {
                bondingRadiusSprite.gameObject.SetActive(false);
            }
            
            // Clear bond previews
            ClearBondPreviews();
            
            // Create pending bonds now that drag has ended
            if (latticeManager != null)
            {
                latticeManager.OnParticleDragEnd(gameObject);
            }
            
            // Restore original drag
            if (rb != null)
            {
                rb.linearDamping = originalDrag;
            }
        }
        
        /// <summary>
        /// Add a bond to this particle's tracking list
        /// </summary>
        public void AddBond(GameObject bond)
        {
            if (!connectedBonds.Contains(bond))
            {
                connectedBonds.Add(bond);
            }
        }
        
        /// <summary>
        /// Remove a bond from this particle's tracking list
        /// </summary>
        public void RemoveBond(GameObject bond)
        {
            connectedBonds.Remove(bond);
        }
        
        /// <summary>
        /// Check if this particle can accept more bonds
        /// </summary>
        public bool CanAcceptMoreBonds()
        {
            // Clean up any null bonds first
            connectedBonds.RemoveAll(bond => bond == null);
            return connectedBonds.Count < maximumBondsPerParticle;
        }
        
        /// <summary>
        /// Get the current number of bonds
        /// </summary>
        public int GetBondCount()
        {
            connectedBonds.RemoveAll(bond => bond == null);
            return connectedBonds.Count;
        }
        
        /// <summary>
        /// Get the current temperature of this particle
        /// </summary>
        public float GetTemperature()
        {
            return currentTemperature;
        }
        
        /// <summary>
        /// Increase particle temperature
        /// </summary>
        public void IncreaseTemperature(float amount)
        {
            currentTemperature = Mathf.Min(currentTemperature + amount, maxTemperature);
        }
        
        /// <summary>
        /// Decrease particle temperature
        /// </summary>
        public void DecreaseTemperature(float amount)
        {
            currentTemperature = Mathf.Max(currentTemperature - amount, minTemperature);
        }
        
        /// <summary>
        /// Get the minimum temperature threshold
        /// </summary>
        public float GetMinTemperature()
        {
            return minTemperature;
        }
        
        /// <summary>
        /// Get the maximum temperature threshold
        /// </summary>
        public float GetMaxTemperature()
        {
            return maxTemperature;
        }
        
        /// <summary>
        /// Update the size of the bonding radius sprite based on lattice manager settings
        /// </summary>
        private void UpdateBondingRadiusSize()
        {
            if (bondingRadiusSprite == null || latticeManager == null || sphereCollider == null) return;
                      
            // Scale the bonding radius sprite
            bondingRadiusSprite.localScale = Vector3.one * (sphereCollider.radius * latticeManager.bondingProximity) * 2f;
        }
        
        /// <summary>
        /// Update bond previews based on pending bonds from lattice manager
        /// </summary>
        private void UpdateBondPreviews()
        {
            if (latticeManager == null || bondPreviewPrefab == null) return;
            
            // Get pending bond targets from manager
            List<GameObject> pendingTargets = latticeManager.GetPendingBondTargets(gameObject);
            if (pendingTargets == null) return;
            
            // Clear old previews if targets changed
            if (!AreSameTargets(pendingTargets, previewTargets))
            {
                ClearBondPreviews();
                
                // Create new previews
                foreach (GameObject target in pendingTargets)
                {
                    if (target == null) continue;
                    
                    GameObject preview = Instantiate(bondPreviewPrefab, Vector3.zero, Quaternion.identity, transform);
                    preview.name = $"BondPreview_{target.name}";
                    
                    // Configure line renderer
                    LineRenderer lineRenderer = preview.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        lineRenderer.startColor = bondPreviewColor;
                        lineRenderer.endColor = bondPreviewColor;
                        lineRenderer.startWidth = bondPreviewWidth;
                        lineRenderer.endWidth = bondPreviewWidth;
                        lineRenderer.positionCount = 2;
                    }
                    
                    bondPreviews.Add(preview);
                }
                
                previewTargets = new List<GameObject>(pendingTargets);
            }
        }
        
        /// <summary>
        /// Update the positions of bond preview lines
        /// </summary>
        private void UpdateBondPreviewLines()
        {
            for (int i = 0; i < bondPreviews.Count && i < previewTargets.Count; i++)
            {
                if (bondPreviews[i] == null || previewTargets[i] == null) continue;
                
                LineRenderer lineRenderer = bondPreviews[i].GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, previewTargets[i].transform.position);
                }
            }
        }
        
        /// <summary>
        /// Clear all bond preview objects
        /// </summary>
        private void ClearBondPreviews()
        {
            foreach (GameObject preview in bondPreviews)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            bondPreviews.Clear();
            previewTargets.Clear();
        }
        
        /// <summary>
        /// Check if two target lists contain the same objects
        /// </summary>
        private bool AreSameTargets(List<GameObject> listA, List<GameObject> listB)
        {
            if (listA.Count != listB.Count) return false;
            
            for (int i = 0; i < listA.Count; i++)
            {
                if (listA[i] != listB[i]) return false;
            }
            
            return true;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}