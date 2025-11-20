using System;
using System.Collections;
using LilLycanLord_Official;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced score counter with smooth sliding animations and GameEventManager integration.
    /// Features customizable formatting, interval triggers, and per-GameObject event signals.
    /// Supports both immediate and animated score changes with configurable rates.
    /// </summary>
    [RequireComponent(typeof(CanvasFader))]
    public class ScoreCounter : MonoBehaviour, IHasSignals
    {
        [System.Serializable]
        public class ScoreChangeEventData
        {
            public int previousScore;
            public int newScore;
            public int changeAmount;
            public float multiplier;
            public GameObject source;

            public ScoreChangeEventData(
                int prev,
                int newVal,
                int change,
                float mult,
                GameObject src
            )
            {
                previousScore = prev;
                newScore = newVal;
                changeAmount = change;
                multiplier = mult;
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
        [Tooltip("TextMeshPro component for displaying score (auto-assigned)")]
        private TMP_Text scoreText;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Score Status")]
        [SerializeField]
        [Tooltip("Current displayed score value")]
        private int currentScore = 0;

        [SerializeField]
        [Tooltip("Target score for sliding animations")]
        private int targetScore = 0;

        [SerializeField]
        [Tooltip("Score accumulated since last interval trigger")]
        private int intervalAccumulator = 0;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Display Configuration")]
        [SerializeField]
        [Tooltip("Text label to display before score")]
        private string scoreLabel = "Score: ";

        [SerializeField]
        [Tooltip("Pad score with leading characters")]
        private bool enablePadding = true;

        [SerializeField]
        [Tooltip("Character to use for padding")]
        private char paddingCharacter = '0';

        [SerializeField]
        [Tooltip("Total number of digits to display")]
        [Range(1, 12)]
        private int totalDigits = 5;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Enable smooth score sliding animations")]
        private bool enableSliding = true;

        [SerializeField]
        [Tooltip("Rate at which score increases (per second)")]
        private int increaseRate = 500;

        [SerializeField]
        [Tooltip("Rate at which score decreases (per second)")]
        private int decreaseRate = 300;

        [SerializeField]
        [Tooltip("Clamp score values to valid range")]
        private bool clampScoreValues = true;

        [SerializeField]
        [Tooltip("Minimum allowed score value")]
        private int minScore = 0;

        [SerializeField]
        [Tooltip("Maximum allowed score value (calculated from digits if 0)")]
        private int maxScore = 0;

        [Header("Interval Triggers")]
        [SerializeField]
        [Tooltip("Trigger event every X points accumulated")]
        private int intervalTriggerValue = 100;

        [SerializeField]
        [Tooltip("Enable interval-based event triggering")]
        private bool enableIntervalTriggers = false;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when score value changes")]
        private UnityEvent<ScoreChangeEventData> onScoreChanged;

        [SerializeField]
        [Tooltip("Called when score is added/increased")]
        private UnityEvent<ScoreChangeEventData> onScoreAdded;

        [SerializeField]
        [Tooltip("Called when score is reduced")]
        private UnityEvent<ScoreChangeEventData> onScoreDeducted;

        [SerializeField]
        [Tooltip("Called when score reaches maximum")]
        private UnityEvent onScoreMaximum;

        [SerializeField]
        [Tooltip("Called when score reaches minimum")]
        private UnityEvent onScoreMinimum;

        [SerializeField]
        [Tooltip("Called at score intervals")]
        private UnityEvent<int> onScoreInterval;

        [SerializeField]
        [Tooltip("Called when score is reset")]
        private UnityEvent onScoreReset;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // GameEventManager signal keys for this specific GameObject
        private string scoreAddedEventKey;
        private string scoreDeductedEventKey;
        private string scoreMaximumEventKey;
        private string scoreMinimumEventKey;
        private string scoreResetEventKey;

        // Performance optimization fields
        private int lastDisplayedScore = -1;
        private string cachedDisplayText;
        private bool needsDisplayUpdate = true;
        private int cachedMaxScore = -1;

        /// <summary>Current score value</summary>
        public int CurrentScore => currentScore;

        /// <summary>Target score for sliding animation</summary>
        public int TargetScore => targetScore;

        /// <summary>Maximum possible score</summary>
        public int MaxScore
        {
            get
            {
                if (cachedMaxScore == -1)
                    cachedMaxScore = maxScore > 0 ? maxScore : (int)Mathf.Pow(10, totalDigits) - 1;
                return cachedMaxScore;
            }
        }

        /// <summary>Minimum possible score</summary>
        public int MinScore => minScore;

        /// <summary>Is score currently sliding to target</summary>
        public bool IsSliding => enableSliding && currentScore != targetScore;

        /// <summary>Is score at maximum</summary>
        public bool IsScoreMaximum => currentScore >= MaxScore;

        /// <summary>Is score at minimum</summary>
        public bool IsScoreMinimum => currentScore <= MinScore;

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            // Get components
            alphaFader = GetComponent<CanvasFader>();

            // Try to find scoreText component
            if (scoreText == null)
            {
                scoreText = transform.Find("Counter")?.GetComponent<TMP_Text>();
                if (scoreText == null)
                    scoreText = GetComponentInChildren<TMP_Text>();
            }

            // Initialize values
            targetScore = currentScore;

            // Initialize GameEventManager signal keys
            InitializeSignals();
        }

        void Update()
        {
            // Handle score sliding animation
            HandleScoreSliding();

            // Update display text only when needed
            if (needsDisplayUpdate)
                UpdateScoreDisplay();

            // Check for state changes only when score changes
            if (currentScore != lastDisplayedScore)
            {
                CheckScoreStates();
                lastDisplayedScore = currentScore;
                needsDisplayUpdate = true;
            }

            // Handle interval triggers
            HandleIntervalTriggers();
        }

        void OnDestroy()
        {
            CleanupSignals();
        }

        void OnValidate()
        {
            // Clamp values to valid ranges
            totalDigits = Mathf.Max(1, totalDigits);
            increaseRate = Mathf.Max(1, increaseRate);
            decreaseRate = Mathf.Max(1, decreaseRate);
            intervalTriggerValue = Mathf.Max(1, intervalTriggerValue);

            if (clampScoreValues)
            {
                // Invalidate cached max score when totalDigits changes
                cachedMaxScore = -1;

                int calculatedMax = (int)Mathf.Pow(10, totalDigits) - 1;
                int actualMax = maxScore > 0 ? maxScore : calculatedMax;

                currentScore = Mathf.Clamp(currentScore, minScore, actualMax);
                targetScore = Mathf.Clamp(targetScore, minScore, actualMax);
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Add score (positive) or deduct score (negative) with optional multiplier
        /// </summary>
        public void AddScore(int amount, float multiplier = 1f)
        {
            if (amount == 0)
                return;

            alphaFader?.FadeIn();

            int previousScore = currentScore;
            int adjustedChange = Mathf.RoundToInt(amount * multiplier);
            int newTargetScore = targetScore + adjustedChange;

            // Clamp if enabled
            if (clampScoreValues)
                newTargetScore = Mathf.Clamp(newTargetScore, MinScore, MaxScore);

            // Apply change
            if (enableSliding)
            {
                targetScore = newTargetScore;
            }
            else
            {
                currentScore = newTargetScore;
                targetScore = currentScore;
                needsDisplayUpdate = true; // Force display update for immediate changes
            }

            // Update interval accumulator
            if (enableIntervalTriggers && adjustedChange > 0)
                intervalAccumulator += adjustedChange;

            // Trigger events
            TriggerScoreChangeEvents(previousScore, targetScore, adjustedChange, multiplier);

            // Trigger GameEventManager signals
            if (adjustedChange > 0)
                GameEventManager.Instance?.SendSignal(scoreAddedEventKey);
            else
                GameEventManager.Instance?.SendSignal(scoreDeductedEventKey);
        }

        public void DeductScore(int amount, float multiplier = 1f)
        {
            AddScore(-amount, multiplier);
        }

        /// <summary>
        /// Set score to a specific value
        /// </summary>
        public void SetScore(int newScore)
        {
            alphaFader?.FadeIn();

            int previousScore = currentScore;
            int changeAmount = newScore - targetScore;

            // Clamp if enabled
            if (clampScoreValues)
                newScore = Mathf.Clamp(newScore, MinScore, MaxScore);

            // Apply change
            if (enableSliding)
            {
                targetScore = newScore;
            }
            else
            {
                currentScore = newScore;
                targetScore = currentScore;
                needsDisplayUpdate = true; // Force display update for immediate changes
            }

            // Trigger events
            TriggerScoreChangeEvents(previousScore, targetScore, changeAmount, 1f);
        }

        /// <summary>
        /// Set score immediately without sliding animation
        /// </summary>
        public void SetScoreImmediate(int newScore)
        {
            alphaFader?.FadeIn();

            int previousScore = currentScore;
            int changeAmount = newScore - currentScore;

            // Clamp if enabled
            if (clampScoreValues)
                newScore = Mathf.Clamp(newScore, MinScore, MaxScore);

            // Apply change immediately
            currentScore = newScore;
            targetScore = currentScore;
            needsDisplayUpdate = true; // Force display update for immediate changes

            // Trigger events
            TriggerScoreChangeEvents(previousScore, currentScore, changeAmount, 1f);
        }

        /// <summary>
        /// Reset score to minimum value
        /// </summary>
        [ContextMenu("Reset Score")]
        public void ResetScore()
        {
            int previousScore = currentScore;

            currentScore = MinScore;
            targetScore = MinScore;
            intervalAccumulator = 0;
            needsDisplayUpdate = true; // Force display update for reset

            onScoreReset?.Invoke();
            GameEventManager.Instance?.SendSignal(scoreResetEventKey);

            TriggerScoreChangeEvents(previousScore, currentScore, MinScore - previousScore, 1f);
        }

        /// <summary>
        /// Test adding score
        /// </summary>
        [ContextMenu("Test Add Score")]
        public void TestAddScore()
        {
            AddScore(250);
        }

        /// <summary>
        /// Test deducting score
        /// </summary>
        [ContextMenu("Test Deduct Score")]
        public void TestDeductScore()
        {
            DeductScore(150);
        }

        /// <summary>
        /// Get score change event data for the current state
        /// </summary>
        public ScoreChangeEventData GetCurrentScoreData()
        {
            return new ScoreChangeEventData(currentScore, currentScore, 0, 1f, gameObject);
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
            scoreAddedEventKey = $"ScoreCounter_Added_{objectId}";
            scoreDeductedEventKey = $"ScoreCounter_Deducted_{objectId}";
            scoreMaximumEventKey = $"ScoreCounter_Maximum_{objectId}";
            scoreMinimumEventKey = $"ScoreCounter_Minimum_{objectId}";
            scoreResetEventKey = $"ScoreCounter_Reset_{objectId}";
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

        private void HandleScoreSliding()
        {
            if (!enableSliding || currentScore == targetScore)
                return;

            int rate = (targetScore > currentScore) ? increaseRate : decreaseRate;
            int direction = targetScore > currentScore ? 1 : -1;

            currentScore += direction * Mathf.RoundToInt(rate * Time.deltaTime);

            // Clamp to target
            if (direction > 0 && currentScore >= targetScore)
                currentScore = targetScore;
            else if (direction < 0 && currentScore <= targetScore)
                currentScore = targetScore;

            // Mark that display needs updating
            needsDisplayUpdate = true;
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText == null)
                return;

            // Only update if the score value has actually changed
            if (currentScore == lastDisplayedScore && !needsDisplayUpdate)
                return;

            // Cache the display text to avoid repeated string operations
            if (cachedDisplayText == null || currentScore != lastDisplayedScore)
            {
                string displayText = currentScore.ToString();

                if (enablePadding)
                    displayText = displayText.PadLeft(totalDigits, paddingCharacter);

                cachedDisplayText = scoreLabel + displayText;
            }

            scoreText.text = cachedDisplayText;
            needsDisplayUpdate = false;
        }

        private void CheckScoreStates()
        {
            // Check for score maximum state
            if (IsScoreMaximum)
            {
                onScoreMaximum?.Invoke();
                GameEventManager.Instance?.SendSignal(scoreMaximumEventKey);
            }

            // Check for score minimum state
            if (IsScoreMinimum)
            {
                onScoreMinimum?.Invoke();
                GameEventManager.Instance?.SendSignal(scoreMinimumEventKey);
            }
        }

        private void HandleIntervalTriggers()
        {
            if (!enableIntervalTriggers || intervalTriggerValue <= 0)
                return;

            while (intervalAccumulator >= intervalTriggerValue)
            {
                intervalAccumulator -= intervalTriggerValue;
                onScoreInterval?.Invoke(intervalTriggerValue);
            }
        }

        private void TriggerScoreChangeEvents(
            int previousScore,
            int newScore,
            int changeAmount,
            float multiplier
        )
        {
            if (changeAmount == 0)
                return;

            var eventData = new ScoreChangeEventData(
                previousScore,
                newScore,
                changeAmount,
                multiplier,
                gameObject
            );

            // Trigger Unity Events
            onScoreChanged?.Invoke(eventData);

            if (changeAmount > 0)
            {
                onScoreAdded?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(scoreAddedEventKey);
            }
            else
            {
                onScoreDeducted?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(scoreDeductedEventKey);
            }
        }
    }
}
