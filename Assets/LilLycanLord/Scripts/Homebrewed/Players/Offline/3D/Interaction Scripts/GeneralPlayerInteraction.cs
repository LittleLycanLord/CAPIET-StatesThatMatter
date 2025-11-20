using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        /// <param name="held">True if interaction button is being held</param>
        void Interact(bool held);

        /// <summary>
        /// Called when player enters interaction range
        /// </summary>
        void OnInteractionEnter() { }

        /// <summary>
        /// Called when player exits interaction range
        /// </summary>
        void OnInteractionExit() { }

        /// <summary>
        /// Check if this object can currently be interacted with
        /// </summary>
        bool CanInteract()
        {
            return true;
        }

        /// <summary>
        /// Get interaction priority (higher = more important)
        /// </summary>
        int GetInteractionPriority()
        {
            return 0;
        }

        /// <summary>
        /// Get display name for interaction prompts
        /// </summary>
        string GetInteractionPrompt()
        {
            return "Interact";
        }

        /// <summary>
        /// Get hold duration for this interaction (-1 = use global setting)
        /// </summary>
        float GetHoldDuration()
        {
            return -1f;
        }
    }

    /// <summary>
    /// Legacy interface support for backward compatibility
    /// </summary>
    public interface Interaction : IInteractable
    {
        // Automatically inherits IInteractable methods
    }

    public enum InteractionMethod
    {
        Raycast, // Ray-based detection
        Sphere, // Sphere overlap detection
        Capsule, // Capsule overlap detection
        Box, // Box overlap detection
    }

    public enum InteractionMode
    {
        Raycast, // Use raycast detection only
        Sphere, // Use sphere overlap detection only
        Hybrid, // Use both raycast and sphere detection
        Smart, // Automatically choose best method based on context
    }

    public enum InteractionPriority
    {
        Low = 0,
        Normal = 5,
        High = 10,
        Critical = 20,
    }

    [System.Serializable]
    public struct InteractionData
    {
        public GameObject GameObject;
        public Interaction Interactable;
        public float Distance;
        public Vector3 HitPoint;
        public InteractionMethod Method;
    }

    [System.Serializable]
    public struct InteractionInfo
    {
        public bool IsInteractionAvailable;
        public GameObject CurrentTarget;
        public float DistanceToTarget;
        public GameObject[] AvailableInteractions;
        public GameObject LastInteractedObject;
        public float InteractionCooldown;
    }

    public class GeneralPlayerInteraction : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        private GeneralInputManager inputManager;
        public Transform interactionPoint;

        [Header("Interaction Settings")]
        [SerializeField]
        private bool enableInteraction = true;

        [SerializeField]
        private LayerMask interactableLayer = 1;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float interactionRange = 0.99f;

        [SerializeField]
        private InteractionMode interactionMode = InteractionMode.Raycast;

        [SerializeField]
        private bool enableInteractionBuffering = true;

        [SerializeField]
        [Range(0.1f, 1.0f)]
        private float interactionBufferTime = 0.3f;

        [SerializeField]
        [Tooltip("Default hold duration in seconds (0 = instant interaction)")]
        [Range(0.0f, 5.0f)]
        private float globalHoldDuration = 0.0f;

        [Header("Advanced Detection")]
        [SerializeField]
        private bool enableSphereDetection = false;

        [SerializeField]
        [Range(0.1f, 5.0f)]
        private float sphereRadius = 1.0f;

        [SerializeField]
        private bool enableMultipleInteractions = false;

        [SerializeField]
        [Range(1, 10)]
        private int maxInteractions = 3;

        [SerializeField]
        private bool prioritizeClosest = true;

        [Header("Visual Feedback")]
        [SerializeField]
        private bool showDebugRays = true;

        [SerializeField]
        private Color rayColor = Color.green;

        [SerializeField]
        private Color hitColor = Color.red;

        [SerializeField]
        private bool enableInteractionUI = true;

        [SerializeField]
        private bool debugMode = false;

        [Header("Runtime State Display")]
        [SerializeField]
        private GameObject lastInteractedObject;

        [SerializeField]
        private GameObject currentTarget;

        [SerializeField]
        private List<GameObject> availableInteractions = new List<GameObject>();

        [SerializeField]
        private float distanceToTarget;

        [SerializeField]
        private bool interactionAvailable;

        [SerializeField]
        private float interactionCooldown;

        [Header("Hold System Status")]
        [SerializeField]
        private bool isCurrentlyHolding;

        [SerializeField]
        private GameObject currentHoldTarget;

        [SerializeField]
        [Range(0f, 1f)]
        private float holdProgress;

        // Interaction buffering
        private Queue<float> interactionBuffer = new Queue<float>();
        private float lastInteractionTime;

        // Performance optimization
        private bool cachedInteractionInput;
        private bool cachedHoldInput;
        private float lastInputCacheTime;

        // Cooldown system
        private float interactionCooldownTimer;
        private Dictionary<GameObject, float> objectCooldowns = new Dictionary<GameObject, float>();

        // Multi-interaction support
        private List<InteractionData> detectedInteractions = new List<InteractionData>();

        // Hold duration system
        private float holdTimer;
        private bool isHolding;
        private GameObject holdingTarget;
        private float requiredHoldDuration;

        [Header("Interaction Events")]
        public UnityEvent<GameObject> OnInteractionStarted = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnInteractionEnded = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnInteractionTargetChanged = new UnityEvent<GameObject>();
        public UnityEvent<List<GameObject>> OnAvailableInteractionsChanged =
            new UnityEvent<List<GameObject>>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void OnEnable()
        {
            RegisterGameEvents();
        }

        void OnDisable()
        {
            UnregisterGameEvents();
        }

        void Awake()
        {
            InitializeComponents();
        }

        void Start()
        {
            InitializeInteractionSystem();
        }

        void Update()
        {
            if (!enableInteraction)
                return;

            UpdateInteractionCooldowns();
            UpdateInteractionDetection();
            ProcessInteractionInput();
        }

        void OnDrawGizmosSelected()
        {
            if (showDebugRays)
            {
                DrawInteractionGizmos();
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeComponents()
        {
            inputManager = GeneralInputManager.Instance;

            if (interactionPoint == null)
            {
                interactionPoint = transform;
            }
        }

        private void InitializeInteractionSystem()
        {
            if (enableInteractionBuffering && inputManager != null)
            {
                inputManager.SetInputBufferTime(interactionBufferTime);
                inputManager.OnInputPressed.AddListener(OnInputPressed);
                inputManager.OnInputBuffered.AddListener(OnInputBuffered);
            }

            Debug.Log($"GeneralPlayerInteraction: Initialized for {gameObject.name}");
        }

        private void RegisterGameEvents()
        {
            if (GameEventManager.Instance != null)
            {
                // Core player control events
                GameEventManager.Instance.AddAction(
                    "Enable Player",
                    new UnityAction(EnablePlayerInteraction)
                );
                GameEventManager.Instance.AddAction(
                    "Disable Player",
                    new UnityAction(DisablePlayerInteraction)
                );

                // Interaction-specific events
                GameEventManager.Instance.AddAction(
                    "Enable Player Interactions",
                    new UnityAction(EnablePlayerInteraction)
                );
                GameEventManager.Instance.AddAction(
                    "Disable Player Interactions",
                    new UnityAction(DisablePlayerInteraction)
                );

                // Advanced interaction events
                GameEventManager.Instance.AddAction(
                    "Force Interaction",
                    new UnityAction(ForceCurrentInteraction)
                );
                GameEventManager.Instance.AddAction(
                    "Clear Interaction Targets",
                    new UnityAction(ClearAllInteractions)
                );
                GameEventManager.Instance.AddAction(
                    "Refresh Interactions",
                    new UnityAction(RefreshInteractionDetection)
                );
            }
        }

        private void UnregisterGameEvents()
        {
            if (GameEventManager.Instance != null)
            {
                // Core player control events
                GameEventManager.Instance.RemoveAction(
                    "Enable Player",
                    new UnityAction(EnablePlayerInteraction)
                );
                GameEventManager.Instance.RemoveAction(
                    "Disable Player",
                    new UnityAction(DisablePlayerInteraction)
                );

                // Interaction-specific events
                GameEventManager.Instance.RemoveAction(
                    "Enable Player Interactions",
                    new UnityAction(EnablePlayerInteraction)
                );
                GameEventManager.Instance.RemoveAction(
                    "Disable Player Interactions",
                    new UnityAction(DisablePlayerInteraction)
                );

                // Advanced interaction events
                GameEventManager.Instance.RemoveAction(
                    "Force Interaction",
                    new UnityAction(ForceCurrentInteraction)
                );
                GameEventManager.Instance.RemoveAction(
                    "Clear Interaction Targets",
                    new UnityAction(ClearAllInteractions)
                );
                GameEventManager.Instance.RemoveAction(
                    "Refresh Interactions",
                    new UnityAction(RefreshInteractionDetection)
                );
            }

            if (inputManager != null)
            {
                inputManager.OnInputPressed.RemoveListener(OnInputPressed);
                inputManager.OnInputBuffered.RemoveListener(OnInputBuffered);
            }
        }

        private void UpdateInteractionDetection()
        {
            detectedInteractions.Clear();

            switch (interactionMode)
            {
                case InteractionMode.Raycast:
                    DetectRaycastInteractions();
                    break;

                case InteractionMode.Sphere:
                    DetectSphereInteractions();
                    break;

                case InteractionMode.Hybrid:
                    DetectRaycastInteractions();
                    if (enableSphereDetection)
                    {
                        DetectSphereInteractions();
                    }
                    break;
            }

            ProcessDetectedInteractions();
        }

        private void DetectRaycastInteractions()
        {
            Vector3 rayOrigin = interactionPoint.position;
            Vector3 rayDirection = interactionPoint.TransformDirection(Vector3.forward);

            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, rayDirection * interactionRange, rayColor);
            }

            if (
                Physics.Raycast(
                    rayOrigin,
                    rayDirection,
                    out RaycastHit hit,
                    interactionRange,
                    interactableLayer
                )
            )
            {
                if (hit.collider.TryGetComponent<Interaction>(out var interactable))
                {
                    var interactionData = new InteractionData
                    {
                        GameObject = hit.collider.gameObject,
                        Interactable = interactable,
                        Distance = hit.distance,
                        HitPoint = hit.point,
                        Method = InteractionMethod.Raycast,
                    };

                    detectedInteractions.Add(interactionData);

                    if (showDebugRays)
                    {
                        Debug.DrawRay(rayOrigin, rayDirection * hit.distance, hitColor);
                    }
                }
            }
        }

        private void DetectSphereInteractions()
        {
            Collider[] overlapping = Physics.OverlapSphere(
                interactionPoint.position,
                sphereRadius,
                interactableLayer
            );

            foreach (var collider in overlapping)
            {
                if (collider.TryGetComponent<Interaction>(out var interactable))
                {
                    float distance = Vector3.Distance(
                        interactionPoint.position,
                        collider.transform.position
                    );

                    var interactionData = new InteractionData
                    {
                        GameObject = collider.gameObject,
                        Interactable = interactable,
                        Distance = distance,
                        HitPoint = collider.transform.position,
                        Method = InteractionMethod.Sphere,
                    };

                    detectedInteractions.Add(interactionData);
                }
            }
        }

        private void ProcessDetectedInteractions()
        {
            // Remove duplicates and sort by distance if needed
            if (prioritizeClosest)
            {
                detectedInteractions.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            }

            // Limit to max interactions
            if (!enableMultipleInteractions && detectedInteractions.Count > 0)
            {
                detectedInteractions = new List<InteractionData> { detectedInteractions[0] };
            }
            else if (detectedInteractions.Count > maxInteractions)
            {
                detectedInteractions = detectedInteractions.GetRange(0, maxInteractions);
            }

            // Update current target
            GameObject newTarget =
                detectedInteractions.Count > 0 ? detectedInteractions[0].GameObject : null;

            if (newTarget != currentTarget)
            {
                currentTarget = newTarget;
                OnInteractionTargetChanged?.Invoke(currentTarget);
            }

            // Update available interactions list
            availableInteractions.Clear();
            foreach (var interaction in detectedInteractions)
            {
                availableInteractions.Add(interaction.GameObject);
            }

            OnAvailableInteractionsChanged?.Invoke(availableInteractions);

            // Update state
            interactionAvailable = detectedInteractions.Count > 0;
            distanceToTarget = interactionAvailable ? detectedInteractions[0].Distance : 0f;

            // Update hold system display
            isCurrentlyHolding = isHolding;
            currentHoldTarget = holdingTarget;
            holdProgress = GetHoldProgress();
        }

        private void ProcessInteractionInput()
        {
            if (inputManager == null || !interactionAvailable)
            {
                ResetHoldTimer();
                return;
            }

            // Update cooldown
            if (interactionCooldownTimer > 0)
            {
                interactionCooldownTimer -= Time.deltaTime;
                ResetHoldTimer();
                return;
            }

            // Get cached input for performance
            bool interactPressed = GetCachedInteractionInput(false);
            bool interactHeld = GetCachedInteractionInput(true);

            var targetInteraction = detectedInteractions[0];
            var targetGameObject = targetInteraction.GameObject;

            // Get hold duration for this interaction
            float holdDuration = targetInteraction.Interactable.GetHoldDuration();
            if (holdDuration < 0)
                holdDuration = globalHoldDuration;

            // Process instant interaction (no hold required)
            if (holdDuration <= 0 && interactPressed)
            {
                ExecuteInteraction(false);
                ResetHoldTimer();
                return;
            }

            // Process hold-based interaction
            if (holdDuration > 0)
            {
                if (interactPressed)
                {
                    // Start holding
                    StartHoldTimer(targetGameObject, holdDuration);
                }
                else if (interactHeld && isHolding && holdingTarget == targetGameObject)
                {
                    // Continue holding
                    holdTimer += Time.deltaTime;

                    // Broadcast progress update
                    if (Time.frameCount % 5 == 0) // Every 5 frames to avoid spam
                    {
                        GameEventManager.Instance.SendSignal("Interaction_HoldProgress");
                    }

                    // Check if hold duration is complete
                    if (holdTimer >= requiredHoldDuration)
                    {
                        // Broadcast hold completed event
                        GameEventManager.Instance.SendSignal("Interaction_HoldCompleted");

                        ExecuteInteraction(true);
                        ResetHoldTimer();
                    }
                }
                else if (!interactHeld)
                {
                    // Released early - reset hold
                    ResetHoldTimer();
                }
            }
            else
            {
                // Legacy behavior for held input without duration requirement
                if (interactHeld)
                {
                    ExecuteInteraction(true);
                }
            }
        }

        private void ExecuteInteraction(bool held)
        {
            if (detectedInteractions.Count == 0)
                return;

            var targetInteraction = detectedInteractions[0];

            // Check object-specific cooldown
            if (
                objectCooldowns.ContainsKey(targetInteraction.GameObject)
                && objectCooldowns[targetInteraction.GameObject] > 0
            )
            {
                return;
            }

            try
            {
                // Execute the interaction
                targetInteraction.Interactable.Interact(held);

                // Update tracking
                lastInteractedObject = targetInteraction.GameObject;
                lastInteractionTime = Time.time;

                // Set cooldowns
                interactionCooldownTimer = 0.1f; // Global cooldown
                objectCooldowns[targetInteraction.GameObject] = 0.5f; // Object-specific cooldown

                // Trigger events
                if (!held) // Only trigger start/end events for press, not hold
                {
                    OnInteractionStarted?.Invoke(targetInteraction.GameObject);
                    StartCoroutine(TriggerInteractionEnd(targetInteraction.GameObject));
                }

                Debug.Log(
                    $"{name} interacted with {targetInteraction.GameObject.name} (held: {held}, method: {targetInteraction.Method})"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"Error executing interaction with {targetInteraction.GameObject.name}: {e.Message}"
                );
            }
        }

        private void StartHoldTimer(GameObject target, float duration)
        {
            isHolding = true;
            holdingTarget = target;
            holdTimer = 0f;
            requiredHoldDuration = duration;

            // Broadcast hold start event
            GameEventManager.Instance.SendSignal("Interaction_HoldStarted");

            if (debugMode)
            {
                Debug.Log($"Started hold timer for {target.name} with duration {duration}s");
            }
        }

        private void ResetHoldTimer()
        {
            if (isHolding)
            {
                // Broadcast hold cancelled event
                GameEventManager.Instance.SendSignal("Interaction_HoldCancelled");

                if (debugMode)
                {
                    float progress = holdTimer / requiredHoldDuration;
                    Debug.Log(
                        $"Hold timer cancelled for {holdingTarget?.name} at {progress:P1} progress"
                    );
                }
            }

            isHolding = false;
            holdingTarget = null;
            holdTimer = 0f;
            requiredHoldDuration = 0f;
        }

        /// <summary>
        /// Get current hold progress (0.0 to 1.0)
        /// </summary>
        public float GetHoldProgress()
        {
            if (!isHolding || requiredHoldDuration <= 0)
                return 0f;

            return Mathf.Clamp01(holdTimer / requiredHoldDuration);
        }

        /// <summary>
        /// Check if currently holding an interaction
        /// </summary>
        public bool IsCurrentlyHolding()
        {
            return isHolding;
        }

        /// <summary>
        /// Get the target being held (if any)
        /// </summary>
        public GameObject GetHoldTarget()
        {
            return holdingTarget;
        }

        private bool GetCachedInteractionInput(bool held)
        {
            if (Time.time - lastInputCacheTime < 0.016f) // ~60fps caching
            {
                return held ? cachedHoldInput : cachedInteractionInput;
            }

            cachedInteractionInput = inputManager.GetPlayerInteractInput(false);
            cachedHoldInput = inputManager.GetPlayerInteractInput(true);
            lastInputCacheTime = Time.time;

            return held ? cachedHoldInput : cachedInteractionInput;
        }

        private void UpdateInteractionCooldowns()
        {
            var keys = new List<GameObject>(objectCooldowns.Keys);
            foreach (var key in keys)
            {
                if (objectCooldowns[key] > 0)
                {
                    objectCooldowns[key] -= Time.deltaTime;
                }
                else
                {
                    objectCooldowns.Remove(key);
                }
            }
        }

        private IEnumerator TriggerInteractionEnd(GameObject target)
        {
            yield return null; // Wait one frame
            OnInteractionEnded?.Invoke(target);
        }

        private void DrawInteractionGizmos()
        {
            if (interactionPoint == null)
                return;

            // Draw interaction range
            Gizmos.color = rayColor;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRange);

            // Draw sphere detection if enabled
            if (enableSphereDetection)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(interactionPoint.position, sphereRadius);
            }

            // Draw current target
            if (currentTarget != null)
            {
                Gizmos.color = hitColor;
                Gizmos.DrawLine(interactionPoint.position, currentTarget.transform.position);
            }
        }

        private void OnInputPressed(string inputName)
        {
            if (inputName == "Interact" && enableInteractionBuffering)
            {
                interactionBuffer.Enqueue(Time.time);
            }
        }

        private void OnInputBuffered(string inputName)
        {
            if (inputName == "Interact")
            {
                Debug.Log("Interact input buffered for responsive controls");
            }
        }

        public void EnablePlayerInteraction()
        {
            enableInteraction = true;
            Debug.Log("Player Interaction Enabled!");
        }

        public void DisablePlayerInteraction()
        {
            enableInteraction = false;
            Debug.Log("Player Interaction Disabled!");
        }

        public void SetInteractionRange(float range)
        {
            interactionRange = Mathf.Clamp(range, 0.1f, 10.0f);
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            interactionMode = mode;
        }

        public void ForceInteraction(GameObject target, bool held = false)
        {
            if (target != null && target.TryGetComponent<Interaction>(out var interactable))
            {
                interactable.Interact(held);
                lastInteractedObject = target;
                lastInteractionTime = Time.time;

                Debug.Log($"Forced interaction with {target.name}");
            }
        }

        public InteractionInfo GetInteractionInfo()
        {
            return new InteractionInfo
            {
                IsInteractionAvailable = interactionAvailable,
                CurrentTarget = currentTarget,
                DistanceToTarget = distanceToTarget,
                AvailableInteractions = availableInteractions.ToArray(),
                LastInteractedObject = lastInteractedObject,
                InteractionCooldown = interactionCooldownTimer,
            };
        }

        public void ClearCooldowns()
        {
            objectCooldowns.Clear();
            interactionCooldownTimer = 0f;
        }

        /// <summary>
        /// Force interaction with current target (triggered by GameEventManager)
        /// </summary>
        public void ForceCurrentInteraction()
        {
            if (currentTarget != null && interactionAvailable)
            {
                ExecuteInteraction(false);
                Debug.Log($"Forced interaction with current target: {currentTarget.name}");

                // Send GameEventManager signal
                GameEventManager.Instance?.SendSignal($"Interaction_Forced_{currentTarget.name}");
            }
            else
            {
                Debug.LogWarning("No current interaction target available for forced interaction");
            }
        }

        /// <summary>
        /// Clear all current interaction targets (triggered by GameEventManager)
        /// </summary>
        public void ClearAllInteractions()
        {
            detectedInteractions.Clear();
            availableInteractions.Clear();
            currentTarget = null;
            interactionAvailable = false;
            distanceToTarget = 0f;

            Debug.Log("All interactions cleared");

            // Send GameEventManager signal
            GameEventManager.Instance?.SendSignal("Interactions_Cleared");

            // Trigger UI update events
            OnInteractionTargetChanged?.Invoke(null);
            OnAvailableInteractionsChanged?.Invoke(availableInteractions);
        }

        /// <summary>
        /// Force refresh interaction detection (triggered by GameEventManager)
        /// </summary>
        public void RefreshInteractionDetection()
        {
            // Force immediate interaction detection update
            UpdateInteractionDetection();

            Debug.Log(
                $"Interaction detection refreshed - Found {detectedInteractions.Count} interactions"
            );

            // Send GameEventManager signal with interaction count
            GameEventManager.Instance?.SendSignal(
                $"Interactions_Refreshed_{detectedInteractions.Count}"
            );
        }

        /// <summary>
        /// Enhanced interaction execution with GameEventManager integration
        /// </summary>
        private void ExecuteInteractionWithEvents(InteractionData interactionData, bool held)
        {
            var target = interactionData.GameObject;
            var interactable = interactionData.Interactable;

            // Pre-interaction events
            OnInteractionStarted?.Invoke(target);
            GameEventManager.Instance?.SendSignal($"Interaction_Started_{target.name}");

            // Execute the actual interaction
            try
            {
                interactable.Interact(held);

                // Update state
                lastInteractedObject = target;
                lastInteractionTime = Time.time;
                interactionCooldownTimer = interactionBufferTime;

                // Set object-specific cooldown
                objectCooldowns[target] = interactionBufferTime;

                // Success events
                GameEventManager.Instance?.SendSignal($"Interaction_Success_{target.name}");
                Debug.Log($"Successfully interacted with {target.name} (held: {held})");
            }
            catch (System.Exception e)
            {
                // Error handling
                Debug.LogError($"Error during interaction with {target.name}: {e.Message}");
                GameEventManager.Instance?.SendSignal($"Interaction_Error_{target.name}");
            }

            // Post-interaction events
            StartCoroutine(TriggerInteractionEnd(target));
        }

        /// <summary>
        /// Get detailed interaction statistics for debugging
        /// </summary>
        public Dictionary<string, object> GetInteractionStats()
        {
            return new Dictionary<string, object>
            {
                ["IsEnabled"] = enableInteraction,
                ["DetectionMode"] = interactionMode.ToString(),
                ["InteractionRange"] = interactionRange,
                ["SphereRadius"] = sphereRadius,
                ["LayerMask"] = interactableLayer.value,
                ["CurrentTargets"] = detectedInteractions.Count,
                ["AvailableInteractions"] = availableInteractions.Count,
                ["LastInteractionTime"] = lastInteractionTime,
                ["CooldownTimer"] = interactionCooldownTimer,
                ["BufferTime"] = interactionBufferTime,
                ["ActiveCooldowns"] = objectCooldowns.Count,
                ["InteractionPoint"] = interactionPoint != null ? interactionPoint.name : "None",
            };
        }

        /// <summary>
        /// Send comprehensive interaction state to GameEventManager
        /// </summary>
        public void BroadcastInteractionState()
        {
            var stats = GetInteractionStats();

            // Send individual signals for important state changes
            GameEventManager.Instance?.SendSignal(
                $"Interaction_State_Available_{interactionAvailable}"
            );
            GameEventManager.Instance?.SendSignal(
                $"Interaction_State_TargetCount_{detectedInteractions.Count}"
            );
            GameEventManager.Instance?.SendSignal($"Interaction_State_Mode_{interactionMode}");

            if (currentTarget != null)
            {
                GameEventManager.Instance?.SendSignal(
                    $"Interaction_State_CurrentTarget_{currentTarget.name}"
                );
            }

            Debug.Log($"Interaction state broadcasted: {stats.Count} properties sent");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
