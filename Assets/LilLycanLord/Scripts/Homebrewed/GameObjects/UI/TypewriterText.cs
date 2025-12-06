using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using LilLycanLord_Official;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LilLycanLord_Official
{
    public enum SkipMode
    {
        FastForward, // Speeds up the typewriting effect
        SkipForward, // Shows all text immediately, but can be resumed
        SkipPage, // Shows all text immediately and marks as complete
    }

    public enum TypewriteMode
    {
        PerCharacter, // Traditional character-by-character reveal
        PerWord, // Word-by-word reveal
    }

    public enum RevealMode
    {
        Automatic, // Uses coroutine-based typewriting
        Manual, // Uses progress slider for manual control
        Hybrid, // Allows both automatic and manual control
    }

    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterText : MonoBehaviour
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

        [HideInInspector]
        public TMP_Text textBox;
        Coroutine typewriterCoroutine;

        [Header("Text State")]
        public string currentText;
        public string[] currentWords;
        public int totalCharacters;
        public int totalWords;

        [Header("Progress Control")]
        [Range(0f, 1f)]
        [Tooltip(
            "Manual progress slider (0 = no text, 1 = full text). Only active in Manual/Hybrid mode."
        )]
        public float textProgress = 0f;

        [Min(0)]
        [Tooltip("Current visible character index")]
        public int visibleCharacterIndex = 0;

        [Min(0)]
        [Tooltip("Current visible word index")]
        public int visibleWordIndex = 0;

        [Header("Mode Settings")]
        public RevealMode revealMode = RevealMode.Automatic;
        public TypewriteMode typewriteMode = TypewriteMode.PerCharacter;
        public SkipMode skipMode = SkipMode.SkipPage;

        [Header("Typewriting Settings")]
        [SerializeField]
        [Tooltip("Characters revealed per second")]
        float charactersPerSecond = 20.0f;

        [SerializeField]
        [Tooltip("Words revealed per second (when using PerWord mode)")]
        float wordsPerSecond = 5.0f;

        [SerializeField]
        [Tooltip("Speed multiplier when fast forwarding")]
        float fastForwardMultiplier = 5.0f;

        [Header("Timing Settings")]
        [SerializeField]
        [Tooltip("Delay before onCompletion event")]
        float completionDelay = 0.1f;

        [SerializeField]
        [Tooltip("When skipping, will punctuation marks still have a delay?")]
        bool respectPunctuationDuringSkip = false;

        [SerializeField]
        [Tooltip("Additional delay between words when using PerWord mode")]
        float wordSeparatorDelay = 0.1f;

        [Header("Punctuation Delays")]
        [SerializedDictionary("Punctuation Mark", "Delay")]
        public SerializedDictionary<char, float> punctuationDelays = new SerializedDictionary<
            char,
            float
        >
        {
            { '.', 0.5f },
            { '?', 0.5f },
            { '!', 0.5f },
            { ',', 0.25f },
            { ';', 0.25f },
            { ':', 0.25f },
        };

        [Header("Events")]
        [SerializeField]
        UnityEvent onCompletion;

        [SerializeField]
        UnityEvent onCharacterReveal;

        [SerializeField]
        UnityEvent onWordReveal;

        [SerializeField]
        UnityEvent onSkipTriggered;

        [Header("State")]
        public bool isFastForwarding { get; private set; } = false;
        public bool isSkipping { get; private set; } = false;

        [Tooltip("Is the current text finished or can it be replaced?")]
        public bool ready { get; private set; } = true;

        // Internal timing variables
        private bool skipForward = false;
        private WaitForSeconds characterDelay;
        private WaitForSeconds wordDelay;
        private WaitForSeconds fastForwardCharacterDelay;
        private WaitForSeconds fastForwardWordDelay;
        private float lastProgressValue = 0f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void OnEnable()
        {
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.AddAction("Skip Forward", SkipForward);
                GameEventManager.Instance.AddAction("Skip Page", SkipPage);
                GameEventManager.Instance.AddAction("Fast Forward Start", StartFastForward);
                GameEventManager.Instance.AddAction("Fast Forward End", EndFastForward);
            }
        }

        void OnDisable()
        {
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.RemoveAction("Skip Forward", SkipForward);
                GameEventManager.Instance.RemoveAction("Skip Page", SkipPage);
                GameEventManager.Instance.RemoveAction("Fast Forward Start", StartFastForward);
                GameEventManager.Instance.RemoveAction("Fast Forward End", EndFastForward);
            }
        }

        void Awake()
        {
            textBox = GetComponent<TMP_Text>();
        }

        void Start()
        {
            InitializeTimingValues();

            // Initialize progress tracking
            lastProgressValue = textProgress;
        }

        void Update()
        {
            // Handle manual/hybrid progress control
            if (
                (revealMode == RevealMode.Manual || revealMode == RevealMode.Hybrid)
                && Mathf.Abs(textProgress - lastProgressValue) > 0.001f
            )
            {
                UpdateManualProgress();
                lastProgressValue = textProgress;
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Initialize all timing-related WaitForSeconds objects
        /// </summary>
        private void InitializeTimingValues()
        {
            characterDelay = new WaitForSeconds(1f / charactersPerSecond);
            wordDelay = new WaitForSeconds(1f / wordsPerSecond);
            fastForwardCharacterDelay = new WaitForSeconds(
                1f / (charactersPerSecond * fastForwardMultiplier)
            );
            fastForwardWordDelay = new WaitForSeconds(
                1f / (wordsPerSecond * fastForwardMultiplier)
            );
        }

        /// <summary>
        /// Set new text and start typewriting effect
        /// </summary>
        /// <param name="newText">The text to display</param>
        public void SetText(string newText)
        {
            if (!ready && revealMode == RevealMode.Automatic)
                return;

            // Stop any existing typewriting
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            // Initialize text data
            currentText = newText;
            textBox.text = newText;
            textBox.ForceMeshUpdate();

            // Parse words for word-based typewriting
            currentWords = ParseTextIntoWords(newText);
            totalCharacters = textBox.textInfo.characterCount;
            totalWords = currentWords.Length;

            // Reset state
            ResetTypewriterState();

            // Start appropriate reveal mode
            switch (revealMode)
            {
                case RevealMode.Automatic:
                    ready = false;
                    typewriterCoroutine = StartCoroutine(AutomaticTypewrite());
                    break;

                case RevealMode.Manual:
                    ready = true;
                    UpdateManualProgress();
                    break;

                case RevealMode.Hybrid:
                    ready = false;
                    typewriterCoroutine = StartCoroutine(HybridTypewrite());
                    break;
            }
        }

        /// <summary>
        /// Parse text into words while preserving spaces and punctuation
        /// </summary>
        private string[] ParseTextIntoWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string[0];

            var words = new List<string>();
            var currentWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    if (!string.IsNullOrEmpty(currentWord))
                    {
                        words.Add(currentWord);
                        currentWord = "";
                    }
                    words.Add(c.ToString()); // Add space as separate "word"
                }
                else
                {
                    currentWord += c;
                }
            }

            if (!string.IsNullOrEmpty(currentWord))
                words.Add(currentWord);

            return words.ToArray();
        }

        /// <summary>
        /// Reset all typewriter state variables
        /// </summary>
        private void ResetTypewriterState()
        {
            visibleCharacterIndex = 0;
            visibleWordIndex = 0;
            textProgress = 0f;
            lastProgressValue = 0f;
            isFastForwarding = false;
            isSkipping = false;
            skipForward = false;
            textBox.maxVisibleCharacters = 0;
        }

        /// <summary>
        /// Update text display based on manual progress slider
        /// </summary>
        private void UpdateManualProgress()
        {
            if (string.IsNullOrEmpty(currentText))
                return;

            textProgress = Mathf.Clamp01(textProgress);

            if (typewriteMode == TypewriteMode.PerCharacter)
            {
                int targetCharacters = Mathf.RoundToInt(totalCharacters * textProgress);
                textBox.maxVisibleCharacters = targetCharacters;
                visibleCharacterIndex = targetCharacters;
            }
            else
            {
                int targetWords = Mathf.RoundToInt(totalWords * textProgress);
                visibleWordIndex = targetWords;
                UpdateWordBasedDisplay();
            }

            // Check for completion
            if (textProgress >= 1f && !ready)
            {
                ready = true;
                onCompletion?.Invoke();
            }
        }

        /// <summary>
        /// Update display when using word-based typewriting
        /// </summary>
        private void UpdateWordBasedDisplay()
        {
            if (currentWords == null || currentWords.Length == 0)
                return;

            string displayText = "";
            for (int i = 0; i < Mathf.Min(visibleWordIndex, currentWords.Length); i++)
            {
                displayText += currentWords[i];
            }

            textBox.text = displayText;
            textBox.maxVisibleCharacters = displayText.Length;
        }

        /// <summary>
        /// Start fast forward mode
        /// </summary>
        public void StartFastForward()
        {
            isFastForwarding = true;
        }

        /// <summary>
        /// End fast forward mode
        /// </summary>
        public void EndFastForward()
        {
            isFastForwarding = false;
        }

        /// <summary>
        /// Skip forward - show remaining text immediately but allow resume
        /// </summary>
        public void SkipForward()
        {
            if (skipMode != SkipMode.SkipForward)
                return;

            skipForward = true;
            isSkipping = true;
            onSkipTriggered?.Invoke();
        }

        /// <summary>
        /// Skip page - show all text and mark as complete
        /// </summary>
        public void SkipPage()
        {
            if (skipMode != SkipMode.SkipPage)
                return;

            // Stop typewriting immediately
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            // Show all text
            textBox.maxVisibleCharacters = totalCharacters;
            textProgress = 1f;
            visibleCharacterIndex = totalCharacters;
            visibleWordIndex = totalWords;

            // Mark as complete
            ready = true;
            isSkipping = true;
            onSkipTriggered?.Invoke();
            onCompletion?.Invoke();
        }

        /// <summary>
        /// Get current completion percentage (0-1)
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (totalCharacters == 0)
                return 1f;

            return (float)visibleCharacterIndex / totalCharacters;
        }

        /// <summary>
        /// Check if typewriting is currently active
        /// </summary>
        public bool IsTypewriting()
        {
            return typewriterCoroutine != null && !ready;
        }

        /// <summary>
        /// Force complete the current text immediately
        /// </summary>
        public void ForceComplete()
        {
            SkipPage();
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        /// <summary>
        /// Automatic typewriting coroutine - traditional typewriter effect
        /// </summary>
        private IEnumerator AutomaticTypewrite()
        {
            textBox.maxVisibleCharacters = 0;
            visibleCharacterIndex = 0;
            visibleWordIndex = 0;

            if (typewriteMode == TypewriteMode.PerCharacter)
            {
                yield return StartCoroutine(AutomaticTypewriteCharacters());
            }
            else
            {
                yield return StartCoroutine(AutomaticTypewriteWords());
            }

            // Completion
            yield return new WaitForSeconds(completionDelay);
            ready = true;
            onCompletion?.Invoke();
        }

        /// <summary>
        /// Character-by-character automatic typewriting
        /// </summary>
        private IEnumerator AutomaticTypewriteCharacters()
        {
            TMP_TextInfo textInfo = textBox.textInfo;

            while (visibleCharacterIndex < totalCharacters)
            {
                // Handle skipping
                if (skipForward)
                {
                    textBox.maxVisibleCharacters = totalCharacters;
                    visibleCharacterIndex = totalCharacters;
                    textProgress = 1f;
                    skipForward = false;
                    yield break;
                }

                // Reveal next character
                textBox.maxVisibleCharacters = visibleCharacterIndex + 1;
                onCharacterReveal?.Invoke();

                // Update progress
                textProgress = (float)visibleCharacterIndex / totalCharacters;

                // Get current character for punctuation delays
                char currentCharacter = textInfo.characterInfo[visibleCharacterIndex].character;

                // Calculate delay
                WaitForSeconds delay = GetCharacterDelay(currentCharacter);
                yield return delay;

                visibleCharacterIndex++;
            }
        }

        /// <summary>
        /// Word-by-word automatic typewriting
        /// </summary>
        private IEnumerator AutomaticTypewriteWords()
        {
            while (visibleWordIndex < totalWords)
            {
                // Handle skipping
                if (skipForward)
                {
                    visibleWordIndex = totalWords;
                    textProgress = 1f;
                    UpdateWordBasedDisplay();
                    skipForward = false;
                    yield break;
                }

                // Reveal next word
                visibleWordIndex++;
                UpdateWordBasedDisplay();
                onWordReveal?.Invoke();

                // Update progress
                textProgress = (float)visibleWordIndex / totalWords;

                // Word separator delay
                if (visibleWordIndex < totalWords && wordSeparatorDelay > 0f)
                {
                    float actualDelay = isFastForwarding
                        ? wordSeparatorDelay / fastForwardMultiplier
                        : wordSeparatorDelay;
                    yield return new WaitForSeconds(actualDelay);
                }

                // Main word delay
                WaitForSeconds delay = isFastForwarding ? fastForwardWordDelay : wordDelay;
                yield return delay;
            }
        }

        /// <summary>
        /// Hybrid typewriting - allows both automatic progression and manual control
        /// </summary>
        private IEnumerator HybridTypewrite()
        {
            float autoProgress = 0f;

            while (autoProgress < 1f && textProgress < 1f)
            {
                // Handle skipping
                if (skipForward)
                {
                    textProgress = 1f;
                    autoProgress = 1f;
                    UpdateManualProgress();
                    skipForward = false;
                    break;
                }

                // Auto-advance progress
                float deltaTime = Time.deltaTime;
                if (isFastForwarding)
                    deltaTime *= fastForwardMultiplier;

                if (typewriteMode == TypewriteMode.PerCharacter)
                    autoProgress += deltaTime * charactersPerSecond / totalCharacters;
                else
                    autoProgress += deltaTime * wordsPerSecond / totalWords;

                // Use the maximum of auto progress and manual progress
                float newProgress = Mathf.Max(autoProgress, textProgress);

                if (newProgress != textProgress)
                {
                    textProgress = Mathf.Clamp01(newProgress);
                    UpdateManualProgress();

                    // Trigger appropriate events
                    if (typewriteMode == TypewriteMode.PerCharacter)
                        onCharacterReveal?.Invoke();
                    else
                        onWordReveal?.Invoke();
                }

                yield return null;
            }

            // Ensure completion
            textProgress = 1f;
            UpdateManualProgress();

            yield return new WaitForSeconds(completionDelay);
            ready = true;
            onCompletion?.Invoke();
        }

        /// <summary>
        /// Get appropriate delay for character based on punctuation and current state
        /// </summary>
        private WaitForSeconds GetCharacterDelay(char character)
        {
            // Base delay
            WaitForSeconds baseDelay = isFastForwarding
                ? fastForwardCharacterDelay
                : characterDelay;

            // Check for punctuation delays (only if not skipping or if respecting punctuation during skip)
            if (
                (!isSkipping || respectPunctuationDuringSkip)
                && punctuationDelays.ContainsKey(character)
            )
            {
                float punctuationDelay = punctuationDelays[character];
                if (isFastForwarding)
                    punctuationDelay /= fastForwardMultiplier;

                return new WaitForSeconds(punctuationDelay);
            }

            return baseDelay;
        }

        /// <summary>
        /// Update timing values when settings change (useful for runtime adjustments)
        /// </summary>
        public void RefreshTimingValues()
        {
            InitializeTimingValues();
        }

        /// <summary>
        /// Pause the current typewriting effect
        /// </summary>
        public void PauseTypewriting()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
        }

        /// <summary>
        /// Resume typewriting from current position
        /// </summary>
        public void ResumeTypewriting()
        {
            if (!ready && revealMode == RevealMode.Automatic && typewriterCoroutine == null)
            {
                typewriterCoroutine = StartCoroutine(AutomaticTypewrite());
            }
        }

        /// <summary>
        /// Set progress directly (useful for external control)
        /// </summary>
        public void SetProgress(float progress)
        {
            textProgress = Mathf.Clamp01(progress);
            UpdateManualProgress();
        }
    }
}
