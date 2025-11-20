using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    public interface HasDialogue
    {
        public void TriggerDialogue(bool repeat) { }
    }

    /// <summary>
    /// General DialogueManager that handles modern dialogue system with Character support.
    /// Manages dialogue sequences, character state, and UI communication.
    /// Supports both legacy two-speaker system and new multi-character dialogues.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        //! ╔═══════════════════╗
        //! ║ SINGLETON CONTENT ║
        //! ╚═══════════════════╝

        //* Singleton Configuration
        [Header("Singleton Settings")]
        [SerializeField]
        private bool persistAcrossScenes = true;

        [SerializeField]
        private bool transferDataOnReplace = true;

        [
            SerializeField,
            Tooltip("If true, automatically recreate singleton if destroyed during runtime")
        ]
        private bool autoRecreateOnDestroy = true;

        //* Singleton Instance Management
        private static DialogueManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static DialogueManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<DialogueManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(DialogueManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(DialogueManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Dialogue Manager");
                            instance = singletonGameObject.AddComponent<DialogueManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(DialogueManager)}] Found existing singleton instance in scene."
                            );
                        }
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Check if singleton instance exists without creating one
        /// </summary>
        public static bool HasInstance
        {
            get
            {
                lock (instanceLock)
                {
                    return instance != null && !isBeingDestroyed;
                }
            }
        }

        //* Data Transfer Interface for Singleton Replacement
        protected virtual void TransferDataToNewInstance(DialogueManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.currentDialogue = this.currentDialogue;
                newInstance.isDialogueActive = this.isDialogueActive;
                newInstance.currentDialogueUI = this.currentDialogueUI;
                newInstance.registeredDialogueUIs = this.registeredDialogueUIs;
                newInstance.activeCharacters = this.activeCharacters;
                newInstance.debugMode = this.debugMode;
                newInstance.enablePlayerDisabling = this.enablePlayerDisabling;
                newInstance.enableEventIntegration = this.enableEventIntegration;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(DialogueManager newInstance)
        {
            //* Implement custom data transfer logic here
        }

        void Awake()
        {
            // Reset applicationIsQuitting flag in case it was set from previous play session
            applicationIsQuitting = false;

            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = this;
                    InitializeSingleton();
                }
                else if (instance != this)
                {
                    //* Transfer data from existing instance if enabled
                    if (transferDataOnReplace)
                    {
                        instance.TransferDataToNewInstance(this);
                    }

                    //* Destroy the old instance and replace it
                    DialogueManager oldInstance = instance;
                    instance = this;

                    if (oldInstance != null && oldInstance.gameObject != this.gameObject)
                    {
                        isBeingDestroyed = true;
                        Destroy(oldInstance.gameObject);
                    }

                    InitializeSingleton();
                }
            }

            //* - - - - - Non - Singleton Awake Content - - - - -
            InitializeDialogueManager();
        }

        private void InitializeSingleton()
        {
            if (persistAcrossScenes)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }

            //* Mark as not being destroyed
            isBeingDestroyed = false;

            //* Call initialization hook
            OnSingletonInitialized();
        }

        //* Override this method for custom initialization logic
        protected virtual void OnSingletonInitialized()
        {
            //* Implement custom initialization here
        }

        void OnApplicationQuit()
        {
            lock (instanceLock)
            {
                applicationIsQuitting = true;
                isBeingDestroyed = true;
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // In editor, this is called when entering/exiting play mode
            if (pauseStatus && Application.isEditor)
            {
                Debug.Log(
                    $"[{nameof(DialogueManager)}] Application paused (entering/exiting play mode)"
                );
                // Don't set applicationIsQuitting here in editor
            }
        }

        void OnDestroy()
        {
            CleanupDialogueManager();
            lock (instanceLock)
            {
                if (instance == this)
                {
                    // Only nullify if we're not auto-recreating and not during app quit
                    if (!autoRecreateOnDestroy || applicationIsQuitting)
                    {
                        instance = null;
                        isBeingDestroyed = true;
                    }
                    else if (autoRecreateOnDestroy && !applicationIsQuitting)
                    {
                        // Schedule recreation on next frame
                        StartCoroutine(RecreateInstanceNextFrame());
                    }

                    // Only set quitting flag if we're actually quitting the application
                    // Not just destroying this instance
                    if (Application.isPlaying && !Application.isEditor)
                    {
                        applicationIsQuitting = true;
                    }
                }
            }
        }

        private System.Collections.IEnumerator RecreateInstanceNextFrame()
        {
            yield return null; // Wait one frame

            lock (instanceLock)
            {
                if (instance == null && !applicationIsQuitting)
                {
                    Debug.LogWarning(
                        $"[{nameof(DialogueManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Dialogue Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<DialogueManager>();
                }
            }
        }

        //* Public method to ensure singleton exists (call this if you're paranoid)
        public static void EnsureInstance()
        {
            var dummy = Instance; // This will create it if it doesn't exist
        }

        /// <summary>
        /// Reset the singleton state - useful for editor play mode transitions
        /// </summary>
        public static void ResetSingletonState()
        {
            applicationIsQuitting = false;
            if (instance != null)
            {
                Debug.Log($"[{nameof(DialogueManager)}] Singleton state reset");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            // Reset the quitting flag when scripts reload in editor
            applicationIsQuitting = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticState()
        {
            // Reset static state when entering play mode
            applicationIsQuitting = false;
            instance = null;
        }
#endif

        //! - - - - - - - - - - -

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Runtime State - Debug Only")]
        [SerializeField]
        [Tooltip("Currently active dialogue")]
        private Dialogue currentDialogue;

        [SerializeField]
        [Tooltip("Is a dialogue currently being displayed")]
        private bool isDialogueActive = false;

        [SerializeField]
        [Tooltip("Current dialogue UI being used")]
        private DialogueUI currentDialogueUI;

        [SerializeField]
        [Tooltip("All currently active characters in dialogue")]
        private List<Character> activeCharacters = new List<Character>();

        [SerializeField]
        [Tooltip("Current dialogue line index")]
        private int currentLineIndex = 0;

        [SerializeField]
        [Tooltip("Characters that have spoken in current dialogue")]
        private List<Character> spokesCharacters = new List<Character>();

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Header("DialogueUI Assignment")]
        [SerializeField]
        [Tooltip("Default DialogueUI to use for dialogues")]
        private DialogueUI defaultDialogueUI;

        [SerializeField]
        [Tooltip("All registered dialogue UIs in the scene")]
        private List<DialogueUI> registeredDialogueUIs = new List<DialogueUI>();

        [Header("Dialogue Settings")]
        [SerializeField]
        [Tooltip("Allow interrupting current dialogue with new dialogue")]
        private bool allowDialogueInterruption = false;

        [SerializeField]
        [Tooltip("Automatically disable player movement during dialogue")]
        private bool enablePlayerDisabling = true;

        [SerializeField]
        [Tooltip("Send game events for dialogue state changes")]
        private bool enableEventIntegration = true;

        [SerializeField]
        [Tooltip("Delay before re-enabling player after dialogue ends")]
        [Range(0f, 2f)]
        private float playerReEnableDelay = 0.1f;

        [Header("Character Management")]
        [SerializeField]
        [Tooltip("Maximum number of characters to track simultaneously")]
        [Range(2, 10)]
        private int maxActiveCharacters = 6;

        [SerializeField]
        [Tooltip("Reset character emotions after dialogue ends")]
        private bool resetCharacterEmotionsAfterDialogue = false;

        [Header("Audio Integration")]
        [SerializeField]
        [Tooltip("Play background music from dialogue")]
        private bool enableBackgroundMusic = true;

        [SerializeField]
        [Tooltip("Fade out music when dialogue starts")]
        private bool fadeOutMusicDuringDialogue = false;

        [SerializeField]
        [Tooltip("Music fade duration")]
        [Range(0f, 5f)]
        private float musicFadeDuration = 1f;

        [Header("Debug Settings")]
        [SerializeField]
        [Tooltip("Enable detailed debug logging")]
        private bool debugMode = false;

        [SerializeField]
        [Tooltip("Show dialogue parsing information")]
        private bool showParsingDebug = false;

        [SerializeField]
        [Tooltip("Log character state changes")]
        private bool logCharacterStateChanges = false;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when any dialogue starts")]
        private UnityEvent<Dialogue> onDialogueStarted = new UnityEvent<Dialogue>();

        [SerializeField]
        [Tooltip("Called when any dialogue ends")]
        private UnityEvent<Dialogue> onDialogueEnded = new UnityEvent<Dialogue>();

        [SerializeField]
        [Tooltip("Called when dialogue is interrupted")]
        private UnityEvent<Dialogue> onDialogueInterrupted = new UnityEvent<Dialogue>();

        [SerializeField]
        [Tooltip("Called when character starts speaking")]
        private UnityEvent<Character> onCharacterSpeakingStarted = new UnityEvent<Character>();

        [SerializeField]
        [Tooltip("Called when character stops speaking")]
        private UnityEvent<Character> onCharacterSpeakingStopped = new UnityEvent<Character>();

        // Internal state
        private Coroutine currentDialogueCoroutine;
        private Dictionary<Character, string> characterPreviousEmotions =
            new Dictionary<Character, string>();
        private List<string> previousPlayingMusic = new List<string>();

        // Legacy compatibility
        private bool speaker1 = true; // For legacy two-speaker mode
        private List<string> currentSpeaker1Lines = new List<string>();
        private List<string> currentSpeaker2Lines = new List<string>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Start()
        {
            RegisterGameEvents();
            FindAndRegisterDialogueUIs();
        }

        void Update()
        {
            // Handle legacy dialogue progression (for backward compatibility)
            if (
                isDialogueActive
                && currentDialogue != null
                && currentDialogue.GetDialogueMode() == DialogueMode.Legacy
            )
            {
                HandleLegacyDialogueProgression();
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeDialogueManager()
        {
            // Initialize collections
            activeCharacters = new List<Character>();
            spokesCharacters = new List<Character>();
            registeredDialogueUIs = new List<DialogueUI>();
            characterPreviousEmotions = new Dictionary<Character, string>();
            previousPlayingMusic = new List<string>();

            // Reset state
            isDialogueActive = false;
            currentDialogue = null;
            currentDialogueUI = null;
            currentLineIndex = 0;

            Debug.Log("DialogueManager: Initialized and ready");
        }

        private void RegisterGameEvents()
        {
            if (!enableEventIntegration || GameEventManager.Instance == null)
                return;

            GameEventManager.Instance.AddAction("Dialogue_ForceStop", ForceStopDialogue);
            GameEventManager.Instance.AddAction("Dialogue_ForceComplete", ForceCompleteCurrentLine);
            GameEventManager.Instance.AddAction("Dialogue_SkipLine", SkipCurrentLine);

            Debug.Log("DialogueManager: Game events registered");
        }

        private void CleanupDialogueManager()
        {
            // Stop any active dialogue
            if (isDialogueActive)
                ForceStopDialogue();

            // Cleanup game events
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.RemoveAction("Dialogue_ForceStop", ForceStopDialogue);
                GameEventManager.Instance.RemoveAction(
                    "Dialogue_ForceComplete",
                    ForceCompleteCurrentLine
                );
                GameEventManager.Instance.RemoveAction("Dialogue_SkipLine", SkipCurrentLine);
            }

            Debug.Log("DialogueManager: Cleaned up");
        }

        private void FindAndRegisterDialogueUIs()
        {
            registeredDialogueUIs.Clear();

            // Find all DialogueUI components in the scene
            DialogueUI[] foundUIs = FindObjectsByType<DialogueUI>(FindObjectsSortMode.None);
            registeredDialogueUIs.AddRange(foundUIs);

            // Set default if not assigned and available
            if (defaultDialogueUI == null && registeredDialogueUIs.Count > 0)
            {
                defaultDialogueUI = registeredDialogueUIs[0];
            }

            DebugLog($"Found and registered {registeredDialogueUIs.Count} DialogueUI components");
        }

        //! ╔═══════════════════════╗
        //! ║ Core Dialogue API     ║
        //! ╚═══════════════════════╝

        /// <summary>
        /// Start a dialogue sequence
        /// </summary>
        /// <param name="dialogue">Dialogue to display</param>
        /// <param name="dialogueUI">Specific UI to use (null = use default)</param>
        public void TriggerDialogue(Dialogue dialogue, DialogueUI dialogueUI = null)
        {
            if (dialogue == null)
            {
                Debug.LogError("DialogueManager: Cannot trigger null dialogue");
                return;
            }

            // Check if dialogue can be triggered
            if (!dialogue.CanTrigger())
            {
                DebugLog(
                    $"Dialogue '{dialogue.GetDisplayName()}' cannot be triggered - conditions not met"
                );
                return;
            }

            // Handle interruption
            if (isDialogueActive && !allowDialogueInterruption)
            {
                DebugLog(
                    $"Dialogue '{dialogue.GetDisplayName()}' blocked - another dialogue is active and interruption is disabled"
                );
                return;
            }

            if (isDialogueActive)
            {
                DebugLog($"Interrupting current dialogue with '{dialogue.GetDisplayName()}'");
                InterruptCurrentDialogue();
            }

            // Get dialogue UI
            DialogueUI targetUI = dialogueUI ?? defaultDialogueUI;
            if (targetUI == null)
            {
                Debug.LogError("DialogueManager: No DialogueUI available for dialogue display");
                return;
            }

            // Start dialogue
            StartDialogue(dialogue, targetUI);
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public void TriggerDialogue(Dialogue dialogue)
        {
            TriggerDialogue(dialogue, null);
        }

        private void StartDialogue(Dialogue dialogue, DialogueUI dialogueUI)
        {
            DebugLog(
                $"Starting dialogue: '{dialogue.GetDisplayName()}' (Mode: {dialogue.GetDialogueMode()})"
            );

            // Set state
            currentDialogue = dialogue;
            currentDialogueUI = dialogueUI;
            currentLineIndex = 0;
            isDialogueActive = true;

            // Clear previous state
            activeCharacters.Clear();
            spokesCharacters.Clear();

            // Handle player disabling
            if (enablePlayerDisabling)
            {
                DisablePlayer();
            }

            // Handle background music
            if (enableBackgroundMusic)
            {
                HandleDialogueMusic(dialogue, true);
            }

            // Trigger dialogue start event
            dialogue.OnDialogueStart();
            onDialogueStarted?.Invoke(dialogue);

            // Send game events
            if (enableEventIntegration)
            {
                SendGameEvent("Dialogue_Started", dialogue.GetDisplayName());
            }

            // Start appropriate dialogue mode
            switch (dialogue.GetDialogueMode())
            {
                case DialogueMode.MultiCharacter:
                case DialogueMode.Hybrid:
                    StartModernDialogue(dialogue, dialogueUI);
                    break;

                case DialogueMode.Legacy:
                    StartLegacyDialogue(dialogue, dialogueUI);
                    break;

                default:
                    Debug.LogError(
                        $"DialogueManager: Unknown dialogue mode {dialogue.GetDialogueMode()}"
                    );
                    break;
            }
        }

        private void StartModernDialogue(Dialogue dialogue, DialogueUI dialogueUI)
        {
            DebugLog($"Starting modern dialogue with {dialogue.GetParsedLines().Count} lines");

            // Get all characters in this dialogue
            var allCharacters = dialogue.GetAvailableCharacters();
            foreach (var character in allCharacters)
            {
                if (character != null && !activeCharacters.Contains(character))
                {
                    activeCharacters.Add(character);
                    SaveCharacterState(character);
                }
            }

            // Start dialogue display through UI
            if (dialogueUI != null)
            {
                dialogueUI.StartDialogue(dialogue);
            }
            else
            {
                Debug.LogError("DialogueManager: DialogueUI is null, cannot display dialogue");
                EndDialogue();
            }
        }

        private void StartLegacyDialogue(Dialogue dialogue, DialogueUI dialogueUI)
        {
            DebugLog("Starting legacy dialogue mode");

            // Parse legacy dialogue format
            speaker1 = true;
            currentSpeaker1Lines = dialogue
                .GetSpeaker1Lines()
                .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            currentSpeaker2Lines = dialogue
                .GetSpeaker2Lines()
                .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();

            // Clean up empty lines
            currentSpeaker1Lines = currentSpeaker1Lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Distinct()
                .ToList();
            currentSpeaker2Lines = currentSpeaker2Lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Distinct()
                .ToList();

            // Start legacy dialogue coroutine
            if (currentDialogueCoroutine != null)
                StopCoroutine(currentDialogueCoroutine);

            currentDialogueCoroutine = StartCoroutine(
                LegacyDialogueCoroutine(dialogue, dialogueUI)
            );
        }

        private void InterruptCurrentDialogue()
        {
            if (currentDialogue != null)
            {
                DebugLog($"Interrupting dialogue: '{currentDialogue.GetDisplayName()}'");

                onDialogueInterrupted?.Invoke(currentDialogue);

                if (enableEventIntegration)
                {
                    SendGameEvent("Dialogue_Interrupted", currentDialogue.GetDisplayName());
                }
            }

            // Stop current dialogue processing
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
                currentDialogueCoroutine = null;
            }

            // Clean up UI
            if (currentDialogueUI != null)
            {
                currentDialogueUI.StopDialogue();
            }

            // Reset state but don't re-enable player yet (new dialogue will handle it)
            ResetDialogueState(false);
        }

        /// <summary>
        /// End the current dialogue
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive || currentDialogue == null)
                return;

            DebugLog($"Ending dialogue: '{currentDialogue.GetDisplayName()}'");

            // Trigger dialogue end event
            currentDialogue.OnDialogueEnd();
            onDialogueEnded?.Invoke(currentDialogue);

            // Send game events
            if (enableEventIntegration)
            {
                SendGameEvent("Dialogue_Ended", currentDialogue.GetDisplayName());
            }

            // Handle background music
            if (enableBackgroundMusic)
            {
                HandleDialogueMusic(currentDialogue, false);
            }

            // Reset character states
            if (resetCharacterEmotionsAfterDialogue)
            {
                RestoreCharacterStates();
            }

            // Stop UI display
            if (currentDialogueUI != null)
            {
                currentDialogueUI.StopDialogue();
            }

            // Reset state and re-enable player
            ResetDialogueState(true);
        }

        private void ResetDialogueState(bool enablePlayer)
        {
            // Stop any running coroutines
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
                currentDialogueCoroutine = null;
            }

            // Clear state
            currentDialogue = null;
            currentDialogueUI = null;
            currentLineIndex = 0;
            isDialogueActive = false;

            // Clear character collections
            activeCharacters.Clear();
            spokesCharacters.Clear();

            // Re-enable player if requested
            if (enablePlayer && enablePlayerDisabling)
            {
                StartCoroutine(EnablePlayerAfterDelay());
            }
        }

        //! ╔══════════════════════╗
        //! ║ Character Management ║
        //! ╚══════════════════════╝

        private void SaveCharacterState(Character character)
        {
            if (character == null)
                return;

            // Save current emotion state
            string currentEmotion = character.GetCurrentEmotion();
            if (!string.IsNullOrEmpty(currentEmotion))
            {
                characterPreviousEmotions[character] = currentEmotion;
            }

            if (logCharacterStateChanges)
            {
                DebugLog(
                    $"Saved state for character '{character.GetName()}': emotion '{currentEmotion}'"
                );
            }
        }

        private void RestoreCharacterStates()
        {
            foreach (var kvp in characterPreviousEmotions)
            {
                Character character = kvp.Key;
                string previousEmotion = kvp.Value;

                if (character != null && !string.IsNullOrEmpty(previousEmotion))
                {
                    character.SetEmotion(previousEmotion);

                    if (logCharacterStateChanges)
                    {
                        DebugLog(
                            $"Restored character '{character.GetName()}' to emotion '{previousEmotion}'"
                        );
                    }
                }
            }

            characterPreviousEmotions.Clear();
        }

        //! ╔═════════════════════╗
        //! ║ Audio Integration   ║
        //! ╚═════════════════════╝

        private void HandleDialogueMusic(Dialogue dialogue, bool isStarting)
        {
            if (dialogue == null || AudioManager.Instance == null)
                return;

            if (isStarting)
            {
                // Handle music at dialogue start
                var backgroundMusic = dialogue.GetBackgroundMusic();
                if (backgroundMusic != null)
                {
                    // Save currently playing music
                    if (fadeOutMusicDuringDialogue)
                    {
                        // This would require AudioManager to support getting currently playing music
                        DebugLog("Fading out current music for dialogue");
                    }

                    // Play dialogue background music using the clip name
                    string musicName = backgroundMusic.name;
                    if (!string.IsNullOrEmpty(musicName))
                    {
                        AudioManager.Instance.Play(musicName, gameObject);
                        DebugLog($"Playing dialogue background music: {musicName}");
                    }
                }
            }
            else
            {
                // Handle music at dialogue end
                if (fadeOutMusicDuringDialogue && previousPlayingMusic.Count > 0)
                {
                    // Restore previous music
                    foreach (string musicName in previousPlayingMusic)
                    {
                        if (!string.IsNullOrEmpty(musicName))
                        {
                            AudioManager.Instance.Play(musicName, gameObject);
                            DebugLog($"Restoring previous music: {musicName}");
                        }
                    }
                    previousPlayingMusic.Clear();
                }
            }
        }

        //! ╔══════════════════════╗
        //! ║ Legacy Compatibility ║
        //! ╚══════════════════════╝

        private void HandleLegacyDialogueProgression()
        {
            // This would be handled by DialogueInputManager or similar
            // For now, just a placeholder for legacy support
        }

        private IEnumerator LegacyDialogueCoroutine(Dialogue dialogue, DialogueUI dialogueUI)
        {
            DebugLog("Starting legacy dialogue coroutine");

            // Show UI
            dialogueUI.RevealUI();
            yield return new WaitForSeconds(dialogueUI.fadeInDuration);

            // Process all lines in alternating speaker fashion
            int totalLines = Mathf.Max(currentSpeaker1Lines.Count, currentSpeaker2Lines.Count);
            for (int i = 0; i < totalLines; i++)
            {
                string dialogueText = "";
                string speakerName = "";

                if (speaker1 && i < currentSpeaker1Lines.Count)
                {
                    dialogueText = currentSpeaker1Lines[i];
                    speakerName = dialogue.GetSpeaker1Name();
                }
                else if (!speaker1 && i < currentSpeaker2Lines.Count)
                {
                    dialogueText = currentSpeaker2Lines[i];
                    speakerName = dialogue.GetSpeaker2Name();
                }

                if (!string.IsNullOrEmpty(dialogueText))
                {
                    // Set speaker name
                    if (dialogueUI.speakerName != null)
                    {
                        dialogueUI.speakerName.text = speakerName;
                    }

                    // Set dialogue text
                    if (dialogueUI.dialogue != null)
                    {
                        dialogueUI.dialogue.SetText(dialogueText);
                    }

                    // Wait for dialogue to complete
                    yield return null;
                    if (dialogueUI.dialogue != null)
                    {
                        yield return new WaitUntil(() => dialogueUI.dialogue.ready);
                    }

                    // Wait for input to continue
                    bool waitingForInput = true;
                    while (waitingForInput)
                    {
                        if (
                            DialogueInputManager.Instance != null
                            && DialogueInputManager.Instance.GetPlayerConfirmDialogue(false)
                        )
                        {
                            waitingForInput = false;
                        }
                        yield return null;
                    }
                }

                // Alternate speaker for next line
                speaker1 = !speaker1;
            }

            // End dialogue
            EndDialogue();
        }

        //! ╔═══════════════════════╗
        //! ║ Player Control        ║
        //! ╚═══════════════════════╝

        private void DisablePlayer()
        {
            if (enableEventIntegration && GameEventManager.Instance != null)
            {
                GameEventManager.Instance.SendSignal("Disable Player");
                DebugLog("Player disabled for dialogue");
            }
        }

        private IEnumerator EnablePlayerAfterDelay()
        {
            yield return new WaitForSeconds(playerReEnableDelay);

            if (enableEventIntegration && GameEventManager.Instance != null)
            {
                GameEventManager.Instance.SendSignal("Enable Player");
                DebugLog("Player re-enabled after dialogue");
            }
        }

        //! ╔═══════════════════════╗
        //! ║ Event Handling        ║
        //! ╚═══════════════════════╝

        private void SendGameEvent(string eventName, string parameter = "")
        {
            if (enableEventIntegration && GameEventManager.Instance != null)
            {
                GameEventManager.Instance.SendSignal(eventName);

                if (!string.IsNullOrEmpty(parameter))
                {
                    GameEventManager.Instance.SendSignal($"{eventName}_{parameter}");
                }
            }
        }

        //! ╔═══════════════════════╗
        //! ║ Public Control API    ║
        //! ╚═══════════════════════╝

        /// <summary>
        /// Force stop current dialogue immediately
        /// </summary>
        [ContextMenu("Force Stop Dialogue")]
        public void ForceStopDialogue()
        {
            if (!isDialogueActive)
                return;

            DebugLog("Force stopping dialogue");
            EndDialogue();
        }

        /// <summary>
        /// Force complete the current dialogue line
        /// </summary>
        [ContextMenu("Force Complete Current Line")]
        public void ForceCompleteCurrentLine()
        {
            if (!isDialogueActive || currentDialogueUI == null)
                return;

            currentDialogueUI.ForceCompleteCurrentLine();
            DebugLog("Force completed current dialogue line");
        }

        /// <summary>
        /// Skip to the next dialogue line
        /// </summary>
        [ContextMenu("Skip Current Line")]
        public void SkipCurrentLine()
        {
            if (!isDialogueActive || currentDialogueUI == null)
                return;

            currentDialogueUI.ProgressToNextLine();
            DebugLog("Skipped to next dialogue line");
        }

        /// <summary>
        /// Register a new DialogueUI for use
        /// </summary>
        public void RegisterDialogueUI(DialogueUI dialogueUI)
        {
            if (dialogueUI != null && !registeredDialogueUIs.Contains(dialogueUI))
            {
                registeredDialogueUIs.Add(dialogueUI);

                if (defaultDialogueUI == null)
                {
                    defaultDialogueUI = dialogueUI;
                }

                DebugLog($"Registered new DialogueUI: {dialogueUI.name}");
            }
        }

        /// <summary>
        /// Unregister a DialogueUI
        /// </summary>
        public void UnregisterDialogueUI(DialogueUI dialogueUI)
        {
            if (dialogueUI != null && registeredDialogueUIs.Contains(dialogueUI))
            {
                registeredDialogueUIs.Remove(dialogueUI);

                if (defaultDialogueUI == dialogueUI)
                {
                    defaultDialogueUI = registeredDialogueUIs.FirstOrDefault();
                }

                DebugLog($"Unregistered DialogueUI: {dialogueUI.name}");
            }
        }

        /// <summary>
        /// Check if a dialogue is currently active
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        /// <summary>
        /// Get the current active dialogue
        /// </summary>
        public Dialogue GetCurrentDialogue()
        {
            return currentDialogue;
        }

        /// <summary>
        /// Get all characters currently in dialogue
        /// </summary>
        public List<Character> GetActiveCharacters()
        {
            return new List<Character>(activeCharacters);
        }

        /// <summary>
        /// Get comprehensive dialogue manager information
        /// </summary>
        public string GetDialogueManagerInfo()
        {
            return $"DialogueManager: "
                + $"Active: {isDialogueActive}, "
                + $"Dialogue: {currentDialogue?.GetDisplayName() ?? "None"}, "
                + $"UI: {currentDialogueUI?.name ?? "None"}, "
                + $"Characters: {activeCharacters.Count}, "
                + $"Registered UIs: {registeredDialogueUIs.Count}";
        }

        //! ╔══════════════════════╗
        //! ║ Debug Utilities      ║
        //! ╚══════════════════════╝

        private void DebugLog(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[DialogueManager] {message}");
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
