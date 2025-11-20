using System;
using System.Collections;
using LilLycanLord_Official;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced combo counter with smooth sliding animations and GameEventManager integration.
    /// Features combo breaking, maximum combo tracking, interval triggers, and per-GameObject event signals.
    /// Supports both immediate and animated combo changes with configurable rates.
    /// </summary>
    [RequireComponent(typeof(CanvasFader))]
    public class ComboCounter : MonoBehaviour, IHasSignals
    {
        [System.Serializable]
        public class ComboChangeEventData
        {
            public int previousCombo;
            public int newCombo;
            public int changeAmount;
            public int maxComboReached;
            public bool wasComboBreak;
            public GameObject source;

            public ComboChangeEventData(
                int prev,
                int newVal,
                int change,
                int maxReached,
                bool brokeCombo,
                GameObject src
            )
            {
                previousCombo = prev;
                newCombo = newVal;
                changeAmount = change;
                maxComboReached = maxReached;
                wasComboBreak = brokeCombo;
                source = src;
            }
        }

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [SerializeField]
        [Tooltip("UICanvasFader component (auto-assigned)")]
        private CanvasFader alphaFader;

        [SerializeField]
        [Tooltip("TextMeshPro component for combo label (auto-assigned)")]
        private TMP_Text comboLabel;

        [SerializeField]
        [Tooltip("TextMeshPro component for displaying combo value (auto-assigned)")]
        private TMP_Text comboText;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Combo Status")]
        [SerializeField]
        [Tooltip("Current displayed combo value")]
        private int currentCombo = 0;

        [SerializeField]
        [Tooltip("Target combo for sliding animations")]
        private int targetCombo = 0;

        [SerializeField]
        [Tooltip("Maximum combo reached this session")]
        private int maxCombo = 0;

        [SerializeField]
        [Tooltip("All-time maximum combo reached")]
        private int allTimeMaxCombo = 0;

        [SerializeField]
        [Tooltip("Combo accumulated since last interval trigger")]
        private int intervalAccumulator = 0;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Display Configuration")]
        [SerializeField]
        [Tooltip("Text to display on combo label")]
        private string labelText = "Combo";

        [SerializeField]
        [Tooltip("Pad combo with leading characters")]
        private bool enablePadding = true;

        [SerializeField]
        [Tooltip("Character to use for padding")]
        private char paddingCharacter = '0';

        [SerializeField]
        [Tooltip("Total number of digits to display")]
        [Range(1, 8)]
        private int totalDigits = 3;

        [Header("Animation Settings")]
        // Sliding feature removed for better performance

        [Header("Combo Behavior")]
        [SerializeField]
        [Tooltip("Maximum allowed combo value (0 = unlimited)")]
        private int maxComboLimit = 0;

        [SerializeField]
        [Tooltip("Auto-break combo after inactivity")]
        private bool enableComboTimeout = false;

        [SerializeField]
        [Tooltip("Time in seconds before combo auto-breaks")]
        private float comboTimeoutDuration = 5f;

        [Header("Interval Triggers")]
        [SerializeField]
        [Tooltip("Trigger event every X combo points accumulated")]
        private int intervalTriggerValue = 10;

        [SerializeField]
        [Tooltip("Enable interval-based event triggering")]
        private bool enableIntervalTriggers = true;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when combo value changes")]
        private UnityEvent<ComboChangeEventData> onComboChanged;

        [SerializeField]
        [Tooltip("Called when combo is increased")]
        private UnityEvent<ComboChangeEventData> onComboAdded;

        [SerializeField]
        [Tooltip("Called when combo is broken/reset")]
        private UnityEvent<ComboChangeEventData> onComboBreak;

        [SerializeField]
        [Tooltip("Called when new maximum combo is reached")]
        private UnityEvent<int> onNewMaxCombo;

        [SerializeField]
        [Tooltip("Called when combo reaches the limit")]
        private UnityEvent onComboLimit;

        [SerializeField]
        [Tooltip("Called at combo intervals")]
        private UnityEvent<int> onComboInterval;

        [SerializeField]
        [Tooltip("Called when combo times out")]
        private UnityEvent onComboTimeout;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // GameEventManager signal keys for this specific GameObject
        private string comboAddedEventKey;
        private string comboBreakEventKey;
        private string comboMaxEventKey;
        private string comboLimitEventKey;
        private string comboTimeoutEventKey;

        // Timeout tracking
        private float lastComboTime;
        private Coroutine timeoutCoroutine;

        // Performance optimization fields
        private int lastDisplayedCombo = -1;
        private string cachedDisplayText;
        private bool needsDisplayUpdate = true;

        /// <summary>Current combo value</summary>
        public int CurrentCombo => currentCombo;

        /// <summary>Target combo for sliding animation</summary>
        public int TargetCombo => targetCombo;

        /// <summary>Maximum combo reached this session</summary>
        public int MaxCombo => maxCombo;

        /// <summary>All-time maximum combo</summary>
        public int AllTimeMaxCombo => allTimeMaxCombo;

        /// <summary>Maximum possible combo value</summary>
        public int MaxComboLimit => maxComboLimit;

        /// <summary>Is combo at the limit</summary>
        public bool IsComboAtLimit => maxComboLimit > 0 && currentCombo >= maxComboLimit;

        /// <summary>Time since last combo activity</summary>
        public float TimeSinceLastCombo => Time.time - lastComboTime;

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            // Get components
            alphaFader = GetComponent<CanvasFader>();

            // Try to find text components
            if (comboLabel == null)
            {
                comboLabel = transform.Find("Label")?.GetComponent<TMP_Text>();
                if (comboLabel == null)
                    comboLabel = GetComponentInChildren<TMP_Text>();
            }

            if (comboText == null)
            {
                comboText = transform.Find("Counter")?.GetComponent<TMP_Text>();
                if (comboText == null && comboLabel != null)
                {
                    // Find the second TMP_Text if label was found first
                    var allTexts = GetComponentsInChildren<TMP_Text>();
                    if (allTexts.Length > 1)
                        comboText = allTexts[1];
                }
            }

            // Initialize values
            targetCombo = currentCombo;
            lastComboTime = Time.time;

            // Initialize GameEventManager signal keys
            InitializeSignals();
        }

        void Update()
        {
            // Update display text only when needed
            if (needsDisplayUpdate)
                UpdateComboDisplay();

            // Check for state changes only when combo changes
            if (currentCombo != lastDisplayedCombo)
            {
                CheckComboStates();
                lastDisplayedCombo = currentCombo;
                needsDisplayUpdate = true;
            }

            // Handle interval triggers
            HandleIntervalTriggers();

            // Handle timeout if enabled
            HandleComboTimeout();
        }

        void OnDestroy()
        {
            CleanupSignals();

            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
        }

        void OnValidate()
        {
            // Clamp values to valid ranges
            totalDigits = Mathf.Max(1, totalDigits);
            intervalTriggerValue = Mathf.Max(1, intervalTriggerValue);
            comboTimeoutDuration = Mathf.Max(0.1f, comboTimeoutDuration);
            maxComboLimit = Mathf.Max(0, maxComboLimit);

            if (maxComboLimit > 0)
            {
                currentCombo = Mathf.Clamp(currentCombo, 0, maxComboLimit);
                targetCombo = Mathf.Clamp(targetCombo, 0, maxComboLimit);
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Add to combo count (single increment)
        /// </summary>
        [ContextMenu("Add Combo")]
        public void AddCombo()
        {
            AddCombo(1);
        }

        /// <summary>
        /// Add multiple points to combo count
        /// </summary>
        public void AddCombo(int comboAmount)
        {
            if (comboAmount <= 0)
                return;

            alphaFader?.FadeIn();

            int previousCombo = currentCombo;
            int newTargetCombo = targetCombo + comboAmount;

            // Apply combo limit if set
            if (maxComboLimit > 0)
                newTargetCombo = Mathf.Min(newTargetCombo, maxComboLimit);

            // Apply change immediately (no sliding)
            currentCombo = newTargetCombo;
            targetCombo = currentCombo;
            needsDisplayUpdate = true; // Force display update for immediate changes

            // Update maximum combo tracking
            if (targetCombo > maxCombo)
            {
                maxCombo = targetCombo;
                onNewMaxCombo?.Invoke(maxCombo);
            }

            if (targetCombo > allTimeMaxCombo)
            {
                allTimeMaxCombo = targetCombo;
            }

            // Update interval accumulator
            if (enableIntervalTriggers)
                intervalAccumulator += comboAmount;

            // Update last combo time
            lastComboTime = Time.time;

            // Reset timeout coroutine
            ResetComboTimeout();

            // Trigger events
            TriggerComboChangeEvents(previousCombo, targetCombo, comboAmount, false);

            // Trigger GameEventManager signals
            GameEventManager.Instance?.SendSignal(comboAddedEventKey);

            // Check for combo limit
            if (IsComboAtLimit)
            {
                onComboLimit?.Invoke();
                GameEventManager.Instance?.SendSignal(comboLimitEventKey);
            }
        }

        /// <summary>
        /// Break the combo (reset to 0)
        /// </summary>
        [ContextMenu("Break Combo")]
        public void BreakCombo()
        {
            if (currentCombo == 0 && targetCombo == 0)
                return;

            alphaFader?.FadeIn();

            int previousCombo = Mathf.Max(currentCombo, targetCombo);

            // Reset combo values
            currentCombo = 0;
            targetCombo = 0;
            intervalAccumulator = 0;
            lastComboTime = Time.time;
            needsDisplayUpdate = true; // Force display update for combo break

            // Stop timeout coroutine
            StopComboTimeout();

            // Trigger events
            TriggerComboChangeEvents(previousCombo, 0, -previousCombo, true);

            // Trigger GameEventManager signals
            GameEventManager.Instance?.SendSignal(comboBreakEventKey);
        }

        /// <summary>
        /// Reset all combo tracking including max combo
        /// </summary>
        [ContextMenu("Reset All Combos")]
        public void ResetAllCombos()
        {
            BreakCombo();
            maxCombo = 0;
        }

        /// <summary>
        /// Reset only the all-time maximum combo
        /// </summary>
        [ContextMenu("Reset All-Time Max")]
        public void ResetAllTimeMaxCombo()
        {
            allTimeMaxCombo = 0;
        }

        /// <summary>
        /// Set combo to a specific value immediately
        /// </summary>
        public void SetComboImmediate(int newCombo)
        {
            alphaFader?.FadeIn();

            int previousCombo = currentCombo;
            newCombo = Mathf.Max(0, newCombo);

            if (maxComboLimit > 0)
                newCombo = Mathf.Min(newCombo, maxComboLimit);

            // Apply change immediately
            currentCombo = newCombo;
            targetCombo = currentCombo;
            needsDisplayUpdate = true; // Force display update for immediate set

            // Update maximums
            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
                onNewMaxCombo?.Invoke(maxCombo);
            }

            if (currentCombo > allTimeMaxCombo)
            {
                allTimeMaxCombo = currentCombo;
            }

            lastComboTime = Time.time;
            ResetComboTimeout();

            // Trigger events
            TriggerComboChangeEvents(
                previousCombo,
                currentCombo,
                currentCombo - previousCombo,
                false
            );
        }

        /// <summary>
        /// Get combo change event data for the current state
        /// </summary>
        public ComboChangeEventData GetCurrentComboData()
        {
            return new ComboChangeEventData(
                currentCombo,
                currentCombo,
                0,
                maxCombo,
                false,
                gameObject
            );
        }

        //* ╔═══════════════════════════╗
        //* ║ Virtual/Overridden Functions ║
        //* ╚═══════════════════════════╝

        /// <summary>
        /// Initialize GameEventManager signals for this specific GameObject
        /// </summary>
        public void InitializeSignals()
        {
            if (GameEventManager.Instance == null)
                return;

            // Register this object as a signal source
            GameEventManager.Instance.RegisterSignalObject(this);

            // Create unique signal keys for this specific GameObject
            string objectId = gameObject.GetInstanceID().ToString();
            comboAddedEventKey = $"ComboCounter_Added_{objectId}";
            comboBreakEventKey = $"ComboCounter_Break_{objectId}";
            comboMaxEventKey = $"ComboCounter_Max_{objectId}";
            comboLimitEventKey = $"ComboCounter_Limit_{objectId}";
            comboTimeoutEventKey = $"ComboCounter_Timeout_{objectId}";
        }

        /// <summary>
        /// Cleanup GameEventManager signals when destroyed
        /// </summary>
        public void CleanupSignals()
        {
            if (GameEventManager.Instance == null)
                return;

            // Unregister this object from GameEventManager
            GameEventManager.Instance.UnregisterSignalObject(this);
        }

        private void UpdateComboDisplay()
        {
            if (comboText == null)
                return;

            // Only update if the combo value has actually changed
            if (currentCombo == lastDisplayedCombo && !needsDisplayUpdate)
                return;

            // Cache the display text to avoid repeated string operations
            if (cachedDisplayText == null || currentCombo != lastDisplayedCombo)
            {
                string displayText = currentCombo.ToString();

                if (enablePadding)
                    displayText = displayText.PadLeft(totalDigits, paddingCharacter);

                cachedDisplayText = displayText;
            }

            comboText.text = cachedDisplayText;

            // Update label if available (only once, not every frame)
            if (comboLabel != null && needsDisplayUpdate)
                comboLabel.text = labelText;

            needsDisplayUpdate = false;
        }

        private void CheckComboStates()
        {
            // Check for new maximum combo
            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
                onNewMaxCombo?.Invoke(maxCombo);
                GameEventManager.Instance?.SendSignal(comboMaxEventKey);
            }

            if (currentCombo > allTimeMaxCombo)
            {
                allTimeMaxCombo = currentCombo;
            }
        }

        private void HandleIntervalTriggers()
        {
            if (!enableIntervalTriggers || intervalTriggerValue <= 0)
                return;

            while (intervalAccumulator >= intervalTriggerValue)
            {
                intervalAccumulator -= intervalTriggerValue;
                onComboInterval?.Invoke(intervalTriggerValue);
            }
        }

        private void HandleComboTimeout()
        {
            if (!enableComboTimeout || currentCombo == 0)
                return;

            if (Time.time - lastComboTime >= comboTimeoutDuration)
            {
                // Trigger timeout
                onComboTimeout?.Invoke();
                GameEventManager.Instance?.SendSignal(comboTimeoutEventKey);

                // Break combo
                BreakCombo();
            }
        }

        private void ResetComboTimeout()
        {
            if (!enableComboTimeout)
                return;

            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            if (currentCombo > 0)
            {
                timeoutCoroutine = StartCoroutine(ComboTimeoutCoroutine());
            }
        }

        private void StopComboTimeout()
        {
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
        }

        private IEnumerator ComboTimeoutCoroutine()
        {
            yield return new WaitForSeconds(comboTimeoutDuration);

            if (currentCombo > 0)
            {
                onComboTimeout?.Invoke();
                GameEventManager.Instance?.SendSignal(comboTimeoutEventKey);
                BreakCombo();
            }
        }

        private void TriggerComboChangeEvents(
            int previousCombo,
            int newCombo,
            int changeAmount,
            bool wasComboBreak
        )
        {
            var eventData = new ComboChangeEventData(
                previousCombo,
                newCombo,
                changeAmount,
                maxCombo,
                wasComboBreak,
                gameObject
            );

            // Trigger Unity Events
            onComboChanged?.Invoke(eventData);

            if (wasComboBreak)
            {
                onComboBreak?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(comboBreakEventKey);
            }
            else if (changeAmount > 0)
            {
                onComboAdded?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(comboAddedEventKey);
            }
        }
    }
}
