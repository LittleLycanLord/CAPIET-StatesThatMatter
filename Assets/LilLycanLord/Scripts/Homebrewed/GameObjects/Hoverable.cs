using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced hoverable component that works with both 3D GameObjects and 2D UI elements.
    /// Supports timed hover events, duration-based triggers, and flexible hover detection.
    /// Compatible with Mouse & Keyboard and touch/pointer input systems.
    /// </summary>
    public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [System.Serializable]
        public enum HoverDetectionMode
        {
            Auto, // Automatically detect based on components
            UI_EventSystem, // Use Unity Event System (for UI elements)
            Raycast3D, // Use 3D raycasting (for 3D objects)
            Bounds2D, // Use 2D bounds checking (for 2D sprites)
        }

        [System.Serializable]
        public class TimedHoverEvent
        {
            [Tooltip("Time in seconds to wait before triggering this event")]
            public float triggerTime = 1f;

            [Tooltip("Event to trigger after the specified hover duration")]
            public UnityEvent onTrigger = new UnityEvent();

            [Tooltip("Optional name/description for this timed event")]
            public string eventName = "Timed Event";

            [HideInInspector]
            public bool hasTriggered = false;
        }

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [SerializeField]
        [Tooltip("Collider component (auto-assigned for 3D detection)")]
        private Collider targetCollider;

        [SerializeField]
        [Tooltip("Collider2D component (auto-assigned for 2D detection)")]
        private Collider2D targetCollider2D;

        [SerializeField]
        [Tooltip("Camera to use for raycasting (auto-assigned to main camera)")]
        private Camera raycastCamera;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField]
        [Tooltip("Currently being hovered")]
        private bool isHovered = false;

        [SerializeField]
        [Tooltip("Current hover duration in seconds")]
        private float currentHoverTime = 0f;

        [SerializeField]
        [Tooltip("Time since hover ended (for exit delay)")]
        private float exitDelayTimer = 0f;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Detection Settings")]
        [SerializeField]
        [Tooltip("How to detect hover events")]
        private HoverDetectionMode detectionMode = HoverDetectionMode.Auto;

        [SerializeField]
        [Tooltip("Layer mask for 3D raycast detection")]
        private LayerMask raycastLayerMask = -1;

        [SerializeField]
        [Tooltip("Maximum raycast distance for 3D detection")]
        private float maxRaycastDistance = 100f;

        [Header("Timing Settings")]
        [SerializeField]
        [Tooltip("Delay before triggering OnHoverOn (0 = immediate)")]
        [Range(0f, 10f)]
        private float hoverOnDelay = 0f;

        [SerializeField]
        [Tooltip("Delay before triggering OnHoverOff after mouse leaves")]
        [Range(0f, 10f)]
        private float hoverOffDelay = 0f;

        [Header("Basic Hover Events")]
        [SerializeField]
        [Tooltip("Called when hover begins (after enter delay)")]
        private UnityEvent onHoverOn = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when hover ends (after exit delay)")]
        private UnityEvent onHoverOff = new UnityEvent();

        [SerializeField]
        [Tooltip("Called every frame while hovering")]
        private UnityEvent onHoverStay = new UnityEvent();

        [Header("Timed Hover Events")]
        [SerializeField]
        [Tooltip("List of events triggered at specific hover durations")]
        private List<TimedHoverEvent> timedEvents = new List<TimedHoverEvent>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private bool wasHoveredLastFrame = false;
        private bool enterDelayActive = false;
        private bool exitDelayActive = false;
        private Coroutine enterDelayCoroutine;
        private Coroutine exitDelayCoroutine;
        private HoverDetectionMode actualDetectionMode;

        /// <summary>Is currently being hovered over</summary>
        public bool IsHovered => isHovered;

        /// <summary>Current hover duration in seconds</summary>
        public float HoverTime => currentHoverTime;

        /// <summary>Number of timed events that have been triggered</summary>
        public int TriggeredEventCount => timedEvents.Count(e => e.hasTriggered);

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            // Auto-assign components
            if (targetCollider == null)
                targetCollider = GetComponent<Collider>();

            if (targetCollider2D == null)
                targetCollider2D = GetComponent<Collider2D>();

            if (raycastCamera == null)
                raycastCamera = Camera.main;

            // Determine actual detection mode
            DetermineDetectionMode();
        }

        void Start()
        {
            // Validate setup
            ValidateSetup();

            // Sort timed events by trigger time
            timedEvents = timedEvents.OrderBy(e => e.triggerTime).ToList();
        }

        void Update()
        {
            // Handle hover detection based on mode
            bool currentlyHovered = DetectHover();

            // Handle hover state changes
            HandleHoverStateChange(currentlyHovered);

            // Update hover timing
            UpdateHoverTiming();

            // Check timed events
            CheckTimedEvents();

            // Call hover stay event
            if (isHovered)
            {
                onHoverStay.Invoke();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        /// <summary>
        /// Manually trigger enter hover (useful for external systems)
        /// </summary>
        public void ForceHoverOn()
        {
            HandleHoverStateChange(true);
        }

        /// <summary>
        /// Manually trigger exit hover (useful for external systems)
        /// </summary>
        public void ForceHoverOff()
        {
            HandleHoverStateChange(false);
        }

        /// <summary>
        /// Add a new timed event at runtime
        /// </summary>
        public void AddTimedEvent(
            float triggerTime,
            UnityAction callback,
            string eventName = "Runtime Event"
        )
        {
            var newEvent = new TimedHoverEvent
            {
                triggerTime = triggerTime,
                eventName = eventName,
                hasTriggered = currentHoverTime >= triggerTime,
            };
            newEvent.onTrigger.AddListener(callback);

            timedEvents.Add(newEvent);
            timedEvents = timedEvents.OrderBy(e => e.triggerTime).ToList();
        }

        /// <summary>
        /// Clear all timed events
        /// </summary>
        public void ClearTimedEvents()
        {
            timedEvents.Clear();
        }

        /// <summary>
        /// Reset hover state and timers
        /// </summary>
        [ContextMenu("Reset Hover State")]
        public void ResetHoverState()
        {
            isHovered = false;
            currentHoverTime = 0f;
            exitDelayTimer = 0f;
            enterDelayActive = false;
            exitDelayActive = false;

            // Stop any running coroutines
            if (enterDelayCoroutine != null)
            {
                StopCoroutine(enterDelayCoroutine);
                enterDelayCoroutine = null;
            }

            if (exitDelayCoroutine != null)
            {
                StopCoroutine(exitDelayCoroutine);
                exitDelayCoroutine = null;
            }

            // Reset timed events
            foreach (var timedEvent in timedEvents)
            {
                timedEvent.hasTriggered = false;
            }
        }

        /// <summary>
        /// Get debug information about current hover state
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Hoverable Debug:\n"
                + $"Detection Mode: {actualDetectionMode}\n"
                + $"Is Hovered: {isHovered}\n"
                + $"Hover Time: {currentHoverTime:F2}s\n"
                + $"Triggered Events: {TriggeredEventCount}/{timedEvents.Count}\n"
                + $"Enter Delay Active: {enterDelayActive}\n"
                + $"Exit Delay Active: {exitDelayActive}";
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        // IPointerEnterHandler implementation for UI
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (actualDetectionMode == HoverDetectionMode.UI_EventSystem)
            {
                HandleHoverStateChange(true);
            }
        }

        // IPointerExitHandler implementation for UI
        public void OnPointerExit(PointerEventData eventData)
        {
            if (actualDetectionMode == HoverDetectionMode.UI_EventSystem)
            {
                HandleHoverStateChange(false);
            }
        }

        //* ╔═══════════════════╗
        //* ║ Private Functions ║
        //* ╚═══════════════════╝

        private void DetermineDetectionMode()
        {
            if (detectionMode != HoverDetectionMode.Auto)
            {
                actualDetectionMode = detectionMode;
                return;
            }

            // Auto-detect based on components
            if (GetComponent<RectTransform>() != null)
            {
                actualDetectionMode = HoverDetectionMode.UI_EventSystem;
            }
            else if (targetCollider != null)
            {
                actualDetectionMode = HoverDetectionMode.Raycast3D;
            }
            else if (targetCollider2D != null)
            {
                actualDetectionMode = HoverDetectionMode.Bounds2D;
            }
            else
            {
                actualDetectionMode = HoverDetectionMode.Raycast3D; // Default fallback
            }
        }

        private void ValidateSetup()
        {
            // Fallback to active main camera if raycastCamera is null
            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;

                if (raycastCamera != null)
                {
                    Debug.Log($"{name}: Auto-assigned main camera for raycasting", this);
                }
            }

            switch (actualDetectionMode)
            {
                case HoverDetectionMode.UI_EventSystem:
                    if (GetComponent<RectTransform>() == null)
                        Debug.LogWarning(
                            $"{name}: UI_EventSystem mode requires RectTransform component",
                            this
                        );
                    break;

                case HoverDetectionMode.Raycast3D:
                    if (targetCollider == null)
                        Debug.LogWarning(
                            $"{name}: Raycast3D mode requires Collider component",
                            this
                        );
                    if (raycastCamera == null)
                        Debug.LogError(
                            $"{name}: No camera assigned for 3D raycasting and no main camera found",
                            this
                        );
                    break;

                case HoverDetectionMode.Bounds2D:
                    if (targetCollider2D == null)
                        Debug.LogWarning(
                            $"{name}: Bounds2D mode requires Collider2D component",
                            this
                        );
                    if (raycastCamera == null)
                        Debug.LogWarning(
                            $"{name}: No camera assigned for 2D bounds checking and no main camera found",
                            this
                        );
                    break;
            }
        }

        private bool DetectHover()
        {
            switch (actualDetectionMode)
            {
                case HoverDetectionMode.UI_EventSystem:
                    // Handled by IPointerEnter/Exit handlers
                    return isHovered;

                case HoverDetectionMode.Raycast3D:
                    return DetectHover3D();

                case HoverDetectionMode.Bounds2D:
                    return DetectHover2D();

                default:
                    return false;
            }
        }

        private bool DetectHover3D()
        {
            if (raycastCamera == null || targetCollider == null)
                return false;

            Vector3 mousePosition = Input.mousePosition;
            Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayerMask))
            {
                return hit.collider == targetCollider;
            }

            return false;
        }

        private bool DetectHover2D()
        {
            if (raycastCamera == null || targetCollider2D == null)
                return false;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = raycastCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = targetCollider2D.transform.position.z;

            return targetCollider2D.bounds.Contains(worldPosition);
        }

        private void HandleHoverStateChange(bool newHoverState)
        {
            if (newHoverState && !wasHoveredLastFrame)
            {
                // Started hovering
                OnHoverStarted();
            }
            else if (!newHoverState && wasHoveredLastFrame)
            {
                // Stopped hovering
                OnHoverEnded();
            }

            wasHoveredLastFrame = newHoverState;
        }

        private void OnHoverStarted()
        {
            // Cancel any active exit delay
            if (exitDelayCoroutine != null)
            {
                StopCoroutine(exitDelayCoroutine);
                exitDelayCoroutine = null;
                exitDelayActive = false;
            }

            // Start enter delay if needed
            if (hoverOnDelay > 0f)
            {
                enterDelayActive = true;
                enterDelayCoroutine = StartCoroutine(EnterDelayCoroutine());
            }
            else
            {
                TriggerHoverOn();
            }
        }

        private void OnHoverEnded()
        {
            // Cancel any active enter delay
            if (enterDelayCoroutine != null)
            {
                StopCoroutine(enterDelayCoroutine);
                enterDelayCoroutine = null;
                enterDelayActive = false;
            }

            // If we were actually hovering, start exit delay
            if (isHovered)
            {
                if (hoverOffDelay > 0f)
                {
                    exitDelayActive = true;
                    exitDelayCoroutine = StartCoroutine(ExitDelayCoroutine());
                }
                else
                {
                    TriggerHoverOff();
                }
            }
        }

        private IEnumerator EnterDelayCoroutine()
        {
            yield return new WaitForSeconds(hoverOnDelay);
            TriggerHoverOn();
            enterDelayActive = false;
            enterDelayCoroutine = null;
        }

        private IEnumerator ExitDelayCoroutine()
        {
            yield return new WaitForSeconds(hoverOffDelay);
            TriggerHoverOff();
            exitDelayActive = false;
            exitDelayCoroutine = null;
        }

        private void TriggerHoverOn()
        {
            isHovered = true;
            currentHoverTime = 0f;
            onHoverOn.Invoke();

            // Reset timed events
            foreach (var timedEvent in timedEvents)
            {
                timedEvent.hasTriggered = false;
            }
        }

        private void TriggerHoverOff()
        {
            isHovered = false;
            currentHoverTime = 0f;
            onHoverOff.Invoke();
        }

        private void UpdateHoverTiming()
        {
            if (isHovered)
            {
                currentHoverTime += Time.deltaTime;
            }

            if (exitDelayActive)
            {
                exitDelayTimer += Time.deltaTime;
            }
            else
            {
                exitDelayTimer = 0f;
            }
        }

        private void CheckTimedEvents()
        {
            if (!isHovered)
                return;

            foreach (var timedEvent in timedEvents)
            {
                if (!timedEvent.hasTriggered && currentHoverTime >= timedEvent.triggerTime)
                {
                    timedEvent.hasTriggered = true;
                    timedEvent.onTrigger.Invoke();
                }
            }
        }

        [ContextMenu("Print Debug Info")]
        private void PrintDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }

        [ContextMenu("Test Enter Hover")]
        private void TestHoverOn()
        {
            ForceHoverOn();
        }

        [ContextMenu("Test Exit Hover")]
        private void TestHoverOff()
        {
            ForceHoverOff();
        }
    }
}
