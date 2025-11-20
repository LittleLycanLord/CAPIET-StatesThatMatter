using System.Collections;
using LilLycanLord_Official;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    /// <summary>
    /// General DialogueUI with visual novel-style character presentation.
    /// Supports left/right speaker positioning, Character sprites, emotions, and smooth transitions.
    /// Integrates with UICanvasFader for smooth dialogue sequence fading.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasFader))]
    public class DialogueUI : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        private CanvasFader canvasFader;
        private Canvas canvas;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Runtime State - Debug Only")]
        [SerializeField]
        [Tooltip("Currently active dialogue")]
        private Dialogue currentDialogue;

        [SerializeField]
        [Tooltip("Current dialogue line being displayed")]
        private DialogueLine currentDialogueLine;

        [SerializeField]
        [Tooltip("Character currently speaking (left side)")]
        private Character currentSpeaker;

        [SerializeField]
        [Tooltip("Previous character (right side)")]
        private Character previousSpeaker;

        [SerializeField]
        [Tooltip("Current line index in dialogue")]
        private int currentLineIndex = 0;

        [SerializeField]
        [Tooltip("Is dialogue UI currently active")]
        private bool isDialogueActive = false;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Core Dialogue Components")]
        [SerializeField]
        [Tooltip("Main dialogue text display")]
        private TypewriterText dialogueText;

        [Header("Left Speaker (Current)")]
        [SerializeField]
        [Tooltip("Left speaker name display")]
        private TMP_Text leftSpeakerName;

        [SerializeField]
        [Tooltip("Left speaker character portrait")]
        private Image leftSpeakerPortrait;

        [SerializeField]
        [Tooltip("Container for left speaker elements")]
        private GameObject leftSpeakerContainer;

        [Header("Right Speaker (Previous)")]
        [SerializeField]
        [Tooltip("Right speaker name display")]
        private TMP_Text rightSpeakerName;

        [SerializeField]
        [Tooltip("Right speaker character portrait")]
        private Image rightSpeakerPortrait;

        [SerializeField]
        [Tooltip("Container for right speaker elements")]
        private GameObject rightSpeakerContainer;

        [Header("Legacy Support")]
        [SerializeField]
        [Tooltip("Legacy speaker name (for backward compatibility)")]
        private TMP_Text legacySpeakerName;

        [Header("Visual Settings")]
        [SerializeField]
        [Tooltip("Default portrait sprite when character has none")]
        private Sprite defaultPortraitSprite;

        [SerializeField]
        [Tooltip("Fade duration for speaker transitions")]
        [Range(0.1f, 2f)]
        private float speakerTransitionDuration = 0.3f;

        [SerializeField]
        [Tooltip("Alpha for inactive speaker")]
        [Range(0.3f, 0.9f)]
        private float inactiveSpeakerAlpha = 0.6f;

        [SerializeField]
        [Tooltip("Scale for active speaker")]
        [Range(1f, 1.2f)]
        private float activeSpeakerScale = 1.05f;

        [Header("Audio Settings")]
        [SerializeField]
        [Tooltip("Play character voice when they speak")]
        private bool enableCharacterVoices = true;

        [SerializeField]
        [Tooltip("Play sound effects from dialogue commands")]
        private bool enableDialogueSoundEffects = true;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Animate speaker portraits")]
        private bool animatePortraits = true;

        [SerializeField]
        [Tooltip("Portrait animation curve")]
        private AnimationCurve portraitAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when dialogue UI is revealed")]
        private UnityEvent onDialogueUIShown = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when dialogue UI is hidden")]
        private UnityEvent onDialogueUIHidden = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when speaker changes")]
        private UnityEvent<Character> onSpeakerChanged = new UnityEvent<Character>();

        [SerializeField]
        [Tooltip("Called when dialogue line changes")]
        private UnityEvent<DialogueLine> onDialogueLineChanged = new UnityEvent<DialogueLine>();

        // Internal state
        private Coroutine speakerTransitionCoroutine;
        private bool isInitialized = false;

        // Legacy compatibility
        public TypewriterText dialogue => dialogueText;
        public TMP_Text speakerName => legacySpeakerName;
        public float fadeInDuration => canvasFader != null ? 0.75f : 1f;
        public float fadeOutDuration => canvasFader != null ? 1f : 1f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            InitializeComponents();
        }

        void Start()
        {
            InitializeUI();
        }

        void Update()
        {
            // Handle dialogue progression input
            HandleDialogueInput();
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeComponents()
        {
            // Get required components
            canvasFader = GetComponent<CanvasFader>();
            canvas = GetComponent<Canvas>();

            // Auto-assign dialogue text if not set
            if (dialogueText == null)
                dialogueText = GetComponentInChildren<TypewriterText>();

            // Validate critical components
            if (dialogueText == null)
            {
                Debug.LogError(
                    $"DialogueUI '{name}': No TypewriterText component found! Dialogue text display will not work."
                );
            }

            if (canvasFader == null)
            {
                Debug.LogError(
                    $"DialogueUI '{name}': No UICanvasFader component found! Manual CanvasGroup fallback will be used."
                );
            }

            Debug.Log($"DialogueUI '{name}': Components initialized");
        }

        private void InitializeUI()
        {
            // Set initial state
            isDialogueActive = false;
            currentLineIndex = 0;
            currentDialogue = null;
            currentDialogueLine = null;

            // Initialize speaker containers
            InitializeSpeakerContainers();

            // Start hidden
            if (canvasFader != null)
            {
                canvasFader.SetAlphaImmediate(0f);
            }

            isInitialized = true;
            Debug.Log($"DialogueUI '{name}': UI initialized and ready");
        }

        private void InitializeSpeakerContainers()
        {
            // Initialize left speaker
            if (leftSpeakerContainer != null)
            {
                SetSpeakerContainerActive(leftSpeakerContainer, false);
                if (leftSpeakerPortrait != null && defaultPortraitSprite != null)
                    leftSpeakerPortrait.sprite = defaultPortraitSprite;
            }

            // Initialize right speaker
            if (rightSpeakerContainer != null)
            {
                SetSpeakerContainerActive(rightSpeakerContainer, false);
                if (rightSpeakerPortrait != null && defaultPortraitSprite != null)
                    rightSpeakerPortrait.sprite = defaultPortraitSprite;
            }

            Debug.Log("DialogueUI: Speaker containers initialized");
        }

        private void HandleDialogueInput()
        {
            if (!isDialogueActive || currentDialogue == null)
                return;

            // Handle dialogue progression through DialogueInputManager
            if (DialogueInputManager.Instance != null)
            {
                if (
                    DialogueInputManager.Instance.GetPlayerConfirmDialogue(false)
                    && dialogueText != null
                    && dialogueText.ready
                )
                {
                    ProgressToNextLine();
                }

                // Handle skip inputs
                if (DialogueInputManager.Instance.GetPlayerSkip(false))
                {
                    if (dialogueText != null)
                        dialogueText.ForceComplete();
                }
            }
        }

        //! ╔═══════════════════════╗
        //! ║ Public Dialogue API   ║
        //! ╚═══════════════════════╝

        /// <summary>
        /// Start displaying a dialogue sequence
        /// </summary>
        public void StartDialogue(Dialogue dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogError("DialogueUI: Cannot start null dialogue");
                return;
            }

            currentDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;

            // Clear previous speakers
            currentSpeaker = null;
            previousSpeaker = null;

            // Clear all text before fading in (prevents showing old text)
            ClearAllTextDisplays();

            Debug.Log($"DialogueUI: Starting dialogue '{dialogue.GetDisplayName()}'");

            // Show the UI
            RevealUI(() =>
            {
                // Start the first line
                DisplayNextDialogueLine();
            });
        }

        /// <summary>
        /// Stop the current dialogue sequence
        /// </summary>
        public void StopDialogue()
        {
            if (!isDialogueActive)
                return;

            Debug.Log($"DialogueUI: Stopping dialogue '{currentDialogue?.GetDisplayName()}'");

            isDialogueActive = false;

            // Hide the UI
            HideUI(() =>
            {
                // Clear current dialogue data
                currentDialogue = null;
                currentDialogueLine = null;
                currentLineIndex = 0;
                currentSpeaker = null;
                previousSpeaker = null;

                // Clear UI elements
                ClearAllSpeakerDisplays();
                if (dialogueText != null)
                    dialogueText.SetText("");
            });
        }

        /// <summary>
        /// Display the next line in the current dialogue
        /// </summary>
        public void DisplayNextDialogueLine()
        {
            if (currentDialogue == null)
                return;

            var lines = currentDialogue.GetParsedLines();

            if (currentLineIndex >= lines.Count)
            {
                // Dialogue complete
                CompleteDialogue();
                return;
            }

            currentDialogueLine = lines[currentLineIndex];
            DisplayDialogueLine(currentDialogueLine);

            // Trigger events
            onDialogueLineChanged?.Invoke(currentDialogueLine);

            currentLineIndex++;
        }

        /// <summary>
        /// Progress to the next dialogue line.
        /// If text is still typing, completes it first (standard visual novel behavior).
        /// </summary>
        public void ProgressToNextLine()
        {
            if (!isDialogueActive)
                return;

            // Check if text is still typing
            if (dialogueText != null && !dialogueText.ready)
            {
                // First click: Complete the current line
                dialogueText.ForceComplete();
                return;
            }

            // Second click (or text already complete): Advance to next line
            DisplayNextDialogueLine();
        }

        /// <summary>
        /// Display a specific dialogue line
        /// </summary>
        public void DisplayDialogueLine(DialogueLine dialogueLine)
        {
            if (dialogueLine == null)
            {
                Debug.LogWarning("DialogueUI: Cannot display null dialogue line");
                return;
            }

            // Handle speaker changes
            HandleSpeakerChange(dialogueLine.speaker);

            // Display the text
            string textToDisplay = ProcessDialogueText(dialogueLine);
            if (dialogueText != null)
            {
                dialogueText.SetText(textToDisplay);
            }

            // Handle dialogue commands
            ProcessDialogueCommands(dialogueLine);

            // Play character voice if enabled
            if (enableCharacterVoices && dialogueLine.speaker != null)
            {
                PlayCharacterVoice(dialogueLine.speaker, dialogueLine.audioOverride);
            }

            Debug.Log(
                $"DialogueUI: Displaying line from {dialogueLine.speaker?.GetName() ?? "Unknown"}: {textToDisplay.Substring(0, Mathf.Min(textToDisplay.Length, 50))}..."
            );
        }

        private void HandleSpeakerChange(Character newSpeaker)
        {
            if (newSpeaker == currentSpeaker)
                return; // No change needed

            // Move current speaker to previous
            if (currentSpeaker != null)
                previousSpeaker = currentSpeaker;

            // Set new current speaker
            currentSpeaker = newSpeaker;

            // Update speaker displays
            UpdateSpeakerDisplays();

            // Trigger speaker change event
            onSpeakerChanged?.Invoke(currentSpeaker);
        }

        private void UpdateSpeakerDisplays()
        {
            // Check if dialogue has only one character
            bool hasOnlyOneCharacter =
                currentDialogue != null && currentDialogue.GetAvailableCharacters().Length <= 1;

            // Update left speaker (current)
            if (currentSpeaker != null)
            {
                SetSpeakerDisplay(
                    leftSpeakerName,
                    leftSpeakerPortrait,
                    leftSpeakerContainer,
                    currentSpeaker,
                    true
                );
            }
            else
            {
                SetSpeakerContainerActive(leftSpeakerContainer, false);
            }

            // Update right speaker (previous) - hide if only one character
            if (previousSpeaker != null && !hasOnlyOneCharacter)
            {
                SetSpeakerDisplay(
                    rightSpeakerName,
                    rightSpeakerPortrait,
                    rightSpeakerContainer,
                    previousSpeaker,
                    false
                );
            }
            else
            {
                SetSpeakerContainerActive(rightSpeakerContainer, false);
            }

            // Update legacy speaker name for backward compatibility
            if (legacySpeakerName != null && currentSpeaker != null)
            {
                legacySpeakerName.text = currentSpeaker.GetName();
            }

            // Start speaker transition animation
            if (animatePortraits)
            {
                StartSpeakerTransition();
            }
        }

        private void SetSpeakerDisplay(
            TMP_Text nameText,
            Image portraitImage,
            GameObject container,
            Character character,
            bool isActive
        )
        {
            if (character == null)
            {
                SetSpeakerContainerActive(container, false);
                return;
            }

            // Set name
            if (nameText != null)
                nameText.text = character.GetName();

            // Set portrait
            if (portraitImage != null)
            {
                Sprite portraitSprite = character.GetCurrentPortrait();
                if (portraitSprite != null)
                    portraitImage.sprite = portraitSprite;
                else if (defaultPortraitSprite != null)
                    portraitImage.sprite = defaultPortraitSprite;
            }

            // Show container
            SetSpeakerContainerActive(container, true);

            // Set visual state based on active/inactive
            SetSpeakerVisualState(container, isActive);
        }

        private void SetSpeakerContainerActive(GameObject container, bool active)
        {
            if (container != null)
                container.SetActive(active);
        }

        private void SetSpeakerVisualState(GameObject container, bool isActive)
        {
            if (container == null)
                return;

            CanvasGroup containerGroup = container.GetComponent<CanvasGroup>();
            if (containerGroup == null)
                containerGroup = container.AddComponent<CanvasGroup>();

            // Set alpha based on active state
            float targetAlpha = isActive ? 1f : inactiveSpeakerAlpha;
            containerGroup.alpha = targetAlpha;

            // Set scale based on active state
            float targetScale = isActive ? activeSpeakerScale : 1f;
            container.transform.localScale = Vector3.one * targetScale;
        }

        private void StartSpeakerTransition()
        {
            if (speakerTransitionCoroutine != null)
                StopCoroutine(speakerTransitionCoroutine);

            speakerTransitionCoroutine = StartCoroutine(SpeakerTransitionCoroutine());
        }

        private IEnumerator SpeakerTransitionCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < speakerTransitionDuration)
            {
                float progress = elapsed / speakerTransitionDuration;
                float curveValue = portraitAnimationCurve.Evaluate(progress);

                // Apply transition effects here if needed
                // (Currently visual state is set immediately, but this allows for smooth transitions)

                elapsed += Time.deltaTime;
                yield return null;
            }

            speakerTransitionCoroutine = null;
        }

        private string ProcessDialogueText(DialogueLine dialogueLine)
        {
            if (dialogueLine == null)
                return "";

            // Use processed text if available, otherwise use raw text
            string text = !string.IsNullOrEmpty(dialogueLine.processedText)
                ? dialogueLine.processedText
                : dialogueLine.rawText;

            // Handle character name replacements and other processing
            if (currentDialogue != null)
            {
                // Replace character name tokens (e.g., ##)
                text = ProcessNameReplacements(text);
            }

            return text ?? "";
        }

        private string ProcessNameReplacements(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Replace ## with previous speaker's name (legacy support)
            if (previousSpeaker != null)
            {
                text = text.Replace("##", previousSpeaker.GetName());
            }

            // Add more replacement logic here as needed

            return text;
        }

        private void ProcessDialogueCommands(DialogueLine dialogueLine)
        {
            if (dialogueLine?.commands == null)
                return;

            foreach (var command in dialogueLine.commands)
            {
                ProcessDialogueCommand(command);
            }
        }

        private void ProcessDialogueCommand(DialogueCommand command)
        {
            if (command == null)
                return;

            switch (command.commandType)
            {
                case DialogueCommandType.EmotionChange:
                    HandleEmotionChange(command.parameter1);
                    break;

                case DialogueCommandType.SoundEffect:
                    HandleSoundEffect(command.parameter1);
                    break;

                case DialogueCommandType.AnimationTrigger:
                    HandleAnimationTrigger(command.parameter1);
                    break;

                case DialogueCommandType.Pause:
                    HandlePause(command.floatValue);
                    break;

                // Add more command handling as needed

                default:
                    Debug.Log($"DialogueUI: Unhandled dialogue command: {command.commandType}");
                    break;
            }
        }

        private void HandleEmotionChange(string emotionName)
        {
            if (currentSpeaker == null || string.IsNullOrEmpty(emotionName))
                return;

            // Change current speaker's emotion/portrait
            currentSpeaker.SetEmotion(emotionName);

            // Update the portrait display
            if (leftSpeakerPortrait != null)
            {
                Sprite newPortrait = currentSpeaker.GetCurrentPortrait();
                if (newPortrait != null)
                    leftSpeakerPortrait.sprite = newPortrait;
            }

            Debug.Log(
                $"DialogueUI: Changed {currentSpeaker.GetName()}'s emotion to '{emotionName}'"
            );
        }

        private void HandleSoundEffect(string soundName)
        {
            if (!enableDialogueSoundEffects || string.IsNullOrEmpty(soundName))
                return;

            // Play sound effect through AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(soundName, gameObject);
            }

            // Also check dialogue's sound effects
            if (currentDialogue != null)
            {
                var soundClip = currentDialogue.GetSoundEffect(soundName);
                if (soundClip != null && AudioManager.Instance != null)
                {
                    // Create a temporary sound and play it
                    // This would require extending AudioManager to support direct AudioClip playback
                    Debug.Log($"DialogueUI: Playing sound effect '{soundName}' from dialogue");
                }
            }
        }

        private void HandleAnimationTrigger(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
                return;

            // Trigger character animation
            if (currentSpeaker != null)
            {
                // This would require Character to have animation support
                Debug.Log(
                    $"DialogueUI: Triggering animation '{animationName}' for {currentSpeaker.GetName()}"
                );
            }
        }

        private void HandlePause(float duration)
        {
            if (duration <= 0f)
                return;

            // Pause dialogue text progression
            if (dialogueText != null)
            {
                dialogueText.PauseTypewriting();
                StartCoroutine(ResumeAfterPause(duration));
            }
        }

        private IEnumerator ResumeAfterPause(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (dialogueText != null)
                dialogueText.ResumeTypewriting();
        }

        private void PlayCharacterVoice(Character character, AudioClip audioOverride)
        {
            if (character == null)
                return;

            AudioClip voiceClip = audioOverride ?? character.GetVoiceClip();
            if (voiceClip != null && AudioManager.Instance != null)
            {
                // Play character voice
                AudioManager.Instance.Play(voiceClip.name, gameObject);
            }
        }

        private void CompleteDialogue()
        {
            Debug.Log($"DialogueUI: Dialogue '{currentDialogue?.GetDisplayName()}' completed");

            // Trigger completion events
            if (currentDialogue != null)
            {
                currentDialogue.OnDialogueEnd();
            }

            // Stop dialogue
            StopDialogue();
        }

        private void ClearAllSpeakerDisplays()
        {
            SetSpeakerContainerActive(leftSpeakerContainer, false);
            SetSpeakerContainerActive(rightSpeakerContainer, false);

            if (legacySpeakerName != null)
                legacySpeakerName.text = "";
        }

        /// <summary>
        /// Clear all text displays (dialogue text and speaker names)
        /// Call this before fading in to prevent showing previous dialogue
        /// </summary>
        private void ClearAllTextDisplays()
        {
            // Clear dialogue text
            if (dialogueText != null)
                dialogueText.SetText("");

            // Clear speaker names
            if (leftSpeakerName != null)
                leftSpeakerName.text = "";

            if (rightSpeakerName != null)
                rightSpeakerName.text = "";

            if (legacySpeakerName != null)
                legacySpeakerName.text = "";

            // Clear speaker containers (hide them)
            ClearAllSpeakerDisplays();
        }

        //! ╔══════════════════════╗
        //! ║ Legacy Compatibility ║
        //! ╚══════════════════════╝

        /// <summary>
        /// Legacy reveal method for backward compatibility
        /// </summary>
        [ContextMenu("Reveal UI")]
        public void RevealUI()
        {
            RevealUI(null);
        }

        /// <summary>
        /// Show the dialogue UI with optional callback
        /// </summary>
        public void RevealUI(System.Action onComplete = null)
        {
            if (canvasFader != null)
            {
                canvasFader.FadeIn(onComplete);
            }
            else
            {
                // Fallback for legacy compatibility
                StartCoroutine(LegacyFadeIn(onComplete));
            }

            onDialogueUIShown?.Invoke();
        }

        /// <summary>
        /// Legacy hide method for backward compatibility
        /// </summary>
        [ContextMenu("Hide UI")]
        public void HideUI()
        {
            HideUI(null);
        }

        /// <summary>
        /// Hide the dialogue UI with optional callback
        /// </summary>
        public void HideUI(System.Action onComplete = null)
        {
            if (canvasFader != null)
            {
                canvasFader.FadeOut(onComplete);
            }
            else
            {
                // Fallback for legacy compatibility
                StartCoroutine(LegacyFadeOut(onComplete));
            }

            onDialogueUIHidden?.Invoke();
        }

        private IEnumerator LegacyFadeIn(System.Action onComplete = null)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            float timeElapsed = 0;
            while (timeElapsed < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, timeElapsed / fadeInDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1.0f;

            onComplete?.Invoke();
        }

        private IEnumerator LegacyFadeOut(System.Action onComplete = null)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            float timeElapsed = 0;
            while (timeElapsed < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, timeElapsed / fadeOutDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0.0f;

            onComplete?.Invoke();
        }

        //! ╔═══════════════════════╗
        //! ║ Public Utility API    ║
        //! ╚═══════════════════════╝

        /// <summary>
        /// Check if dialogue UI is currently active
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        /// <summary>
        /// Get current dialogue being displayed
        /// </summary>
        public Dialogue GetCurrentDialogue()
        {
            return currentDialogue;
        }

        /// <summary>
        /// Get current dialogue line being displayed
        /// </summary>
        public DialogueLine GetCurrentDialogueLine()
        {
            return currentDialogueLine;
        }

        /// <summary>
        /// Get current speaker (left side)
        /// </summary>
        public Character GetCurrentSpeaker()
        {
            return currentSpeaker;
        }

        /// <summary>
        /// Get previous speaker (right side)
        /// </summary>
        public Character GetPreviousSpeaker()
        {
            return previousSpeaker;
        }

        /// <summary>
        /// Force complete current dialogue line
        /// </summary>
        public void ForceCompleteCurrentLine()
        {
            if (dialogueText != null)
                dialogueText.ForceComplete();
        }

        /// <summary>
        /// Get dialogue UI debug information
        /// </summary>
        public string GetDialogueUIInfo()
        {
            return $"DialogueUI '{name}': "
                + $"Active: {isDialogueActive}, "
                + $"Dialogue: {currentDialogue?.GetDisplayName() ?? "None"}, "
                + $"Line: {currentLineIndex}, "
                + $"Current Speaker: {currentSpeaker?.GetName() ?? "None"}, "
                + $"Previous Speaker: {previousSpeaker?.GetName() ?? "None"}";
        }
    }
}
