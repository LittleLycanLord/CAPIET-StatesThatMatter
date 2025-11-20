using System.Collections.Generic;
using System.Linq;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// General DialogueSpeaker that integrates with the modern dialogue system and interaction framework.
    /// Supports both legacy and new dialogue formats, with comprehensive state management.
    /// </summary>
    public class DialogueSpeaker : MonoBehaviour, HasDialogue, IInteractable
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Dialogue Status - Runtime")]
        [SerializeField]
        [Tooltip("Current dialogue being processed")]
        private Dialogue currentDialogue;

        [SerializeField]
        [Tooltip("Total dialogues spoken so far")]
        private int totalDialoguesSpoken = 0;

        [SerializeField]
        [Tooltip("Number of times this speaker has been interacted with")]
        private int interactionCount = 0;

        [SerializeField]
        [Tooltip("Can currently interact with this speaker")]
        private bool canCurrentlyInteract = true;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Space(10)]
        [Header("Dialogue Configuration")]
        [SerializeField]
        [Tooltip("One-time dialogues; dialogues only to be read once.")]
        private List<Dialogue> mainDialogues = new List<Dialogue>();

        [SerializeField]
        [Tooltip("Dialogues that can be repeated after main dialogues are exhausted")]
        private List<Dialogue> repeatingDialogues = new List<Dialogue>();

        [Header("Dialogue Behavior")]
        [SerializeField]
        [Tooltip("Randomize repeating dialogues instead of playing in order")]
        private bool randomizedRepeatingDialogues = true;

        [SerializeField]
        [Tooltip("Priority order for main dialogues (higher = more important)")]
        private bool prioritizeByDialoguePriority = true;

        [SerializeField]
        [Tooltip("Allow interrupting current dialogue with new interaction")]
        private bool allowDialogueInterruption = false;

        [Header("Interaction Settings")]
        [SerializeField]
        [Tooltip("Display name for interaction prompts")]
        private string interactionPrompt = "Talk";

        [SerializeField]
        [Tooltip("Interaction priority (higher = more important)")]
        [Range(0, 20)]
        private int interactionPriority = 5;

        [SerializeField]
        [Tooltip("Hold duration required for interaction (-1 = use global setting)")]
        [Range(-1f, 5f)]
        private float holdDuration = -1f;

        [SerializeField]
        [Tooltip("Cooldown time between interactions")]
        [Range(0f, 10f)]
        private float interactionCooldown = 0.5f;

        [Header("Advanced Features")]
        [SerializeField]
        [Tooltip("Conditions that must be met before any dialogue can trigger")]
        private List<DialogueCondition> speakerConditions = new List<DialogueCondition>();

        [SerializeField]
        [Tooltip("Character associated with this speaker")]
        private Character speakerCharacter;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when any dialogue starts")]
        private UnityEvent OnDialogueStarted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when any dialogue ends")]
        private UnityEvent OnDialogueEnded = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when all main dialogues are exhausted")]
        private UnityEvent OnMainDialoguesCompleted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when no dialogue is available")]
        private UnityEvent OnNoDialogueAvailable = new UnityEvent();

        // Internal state
        private float lastInteractionTime = -1f;
        private bool isDialogueActive = false;
        private List<Dialogue> availableMainDialogues = new List<Dialogue>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            InitializeSpeaker();
        }

        void Start()
        {
            RefreshAvailableDialogues();
        }

        void Update()
        {
            UpdateInteractionCooldown();
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeSpeaker()
        {
            // Initialize all dialogues as unspoken
            foreach (Dialogue dialogue in mainDialogues)
            {
                if (dialogue != null)
                {
                    dialogue.ResetSpokenStatus();
                }
            }

            // Subscribe to dialogue manager events if available
            if (DialogueManager.Instance != null)
            {
                // Integration with dialogue events could be added here
            }

            // Register with GameEventManager
            RegisterWithGameEvents();
        }

        private void RegisterWithGameEvents()
        {
            if (GameEventManager.Instance != null)
            {
                // Register speaker-specific events
                GameEventManager.Instance.AddAction(
                    $"Dialogue_ForceNext_{gameObject.name}",
                    new UnityAction(ForceNextDialogue)
                );

                GameEventManager.Instance.AddAction(
                    $"Dialogue_Reset_{gameObject.name}",
                    new UnityAction(ResetAllDialogues)
                );

                GameEventManager.Instance.AddAction(
                    $"Dialogue_Refresh_{gameObject.name}",
                    new UnityAction(RefreshAvailableDialogues)
                );
            }
        }

        private void RefreshAvailableDialogues()
        {
            availableMainDialogues.Clear();

            // Check which main dialogues are available
            foreach (Dialogue dialogue in mainDialogues)
            {
                if (dialogue != null && dialogue.CanTrigger())
                {
                    availableMainDialogues.Add(dialogue);
                }
            }

            // Sort by priority if enabled
            if (prioritizeByDialoguePriority)
            {
                availableMainDialogues.Sort((a, b) => b.GetPriority().CompareTo(a.GetPriority()));
            }

            // Update interaction availability
            UpdateInteractionAvailability();
        }

        private void UpdateInteractionAvailability()
        {
            canCurrentlyInteract = true;

            // Check speaker conditions
            foreach (var condition in speakerConditions)
            {
                if (!condition.IsConditionMet())
                {
                    canCurrentlyInteract = false;
                    break;
                }
            }

            // Check if dialogue is active and interruption is not allowed
            if (isDialogueActive && !allowDialogueInterruption)
            {
                canCurrentlyInteract = false;
            }

            // Check interaction cooldown
            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                canCurrentlyInteract = false;
            }

            // Must have at least some dialogue available
            if (availableMainDialogues.Count == 0 && repeatingDialogues.Count == 0)
            {
                canCurrentlyInteract = false;
            }
        }

        private void UpdateInteractionCooldown()
        {
            UpdateInteractionAvailability();
        }

        //! ╔══════════════════════════╗
        //! ║ IInteractable Interface  ║
        //! ╚══════════════════════════╝

        public void Interact(bool held)
        {
            if (!CanInteract())
            {
                Debug.LogWarning(
                    $"DialogueSpeaker '{gameObject.name}': Interaction attempted but not available"
                );
                return;
            }

            interactionCount++;
            lastInteractionTime = Time.time;

            // Determine if we should use main or repeating dialogues
            bool useRepeating = (NextUnspokenDialogue() == null);

            if (!held) // Only trigger dialogue on press, not hold
            {
                TriggerDialogue(useRepeating);
            }

            // Send interaction events
            GameEventManager.Instance?.SendSignal($"Dialogue_Interacted_{gameObject.name}");
        }

        public void OnInteractionEnter()
        {
            GameEventManager.Instance?.SendSignal($"Dialogue_InteractionEnter_{gameObject.name}");
        }

        public void OnInteractionExit()
        {
            GameEventManager.Instance?.SendSignal($"Dialogue_InteractionExit_{gameObject.name}");
        }

        public bool CanInteract()
        {
            return canCurrentlyInteract;
        }

        public int GetInteractionPriority()
        {
            return interactionPriority;
        }

        public string GetInteractionPrompt()
        {
            // Dynamic prompt based on available dialogues
            if (availableMainDialogues.Count > 0)
            {
                return interactionPrompt;
            }
            else if (repeatingDialogues.Count > 0)
            {
                return $"{interactionPrompt} (Again)";
            }
            else
            {
                return "No Dialogue";
            }
        }

        public float GetHoldDuration()
        {
            return holdDuration;
        }

        //! ╔═══════════════════════╗
        //! ║ HasDialogue Interface ║
        //! ╚═══════════════════════╝

        public void TriggerDialogue(bool repeat)
        {
            RefreshAvailableDialogues();

            Dialogue dialogueToTrigger = null;

            if (!repeat)
            {
                // Use main dialogues
                dialogueToTrigger = NextUnspokenDialogue();

                if (dialogueToTrigger != null)
                {
                    dialogueToTrigger.MarkAsSpoken();
                    totalDialoguesSpoken++;

                    // Check if all main dialogues are now complete
                    if (NextUnspokenDialogue() == null)
                    {
                        OnMainDialoguesCompleted?.Invoke();
                        GameEventManager.Instance?.SendSignal(
                            $"Dialogue_MainCompleted_{gameObject.name}"
                        );
                    }
                }
            }
            else
            {
                // Use repeating dialogues
                if (repeatingDialogues.Count > 0)
                {
                    if (randomizedRepeatingDialogues)
                    {
                        // Filter available repeating dialogues
                        List<Dialogue> availableRepeating = repeatingDialogues
                            .Where(d => d != null && d.CanTrigger())
                            .ToList();

                        if (availableRepeating.Count > 0)
                        {
                            dialogueToTrigger = availableRepeating[
                                Random.Range(0, availableRepeating.Count)
                            ];
                        }
                    }
                    else
                    {
                        // Use first available repeating dialogue
                        dialogueToTrigger = repeatingDialogues.FirstOrDefault(d =>
                            d != null && d.CanTrigger()
                        );
                    }

                    if (dialogueToTrigger != null)
                    {
                        dialogueToTrigger.MarkAsSpoken();
                    }
                }
            }

            // Execute the dialogue
            if (dialogueToTrigger != null)
            {
                ExecuteDialogue(dialogueToTrigger);
            }
            else
            {
                HandleNoDialogueAvailable();
            }
        }

        private void ExecuteDialogue(Dialogue dialogue)
        {
            if (dialogue == null || DialogueManager.Instance == null)
            {
                Debug.LogError(
                    $"DialogueSpeaker '{gameObject.name}': Cannot execute dialogue - null reference"
                );
                return;
            }

            currentDialogue = dialogue;
            isDialogueActive = true;

            // Trigger events
            OnDialogueStarted?.Invoke();
            GameEventManager.Instance?.SendSignal(
                $"Dialogue_Started_{gameObject.name}_{dialogue.name}"
            );

            // Start the dialogue with enhanced manager
            try
            {
                DialogueManager.Instance.TriggerDialogue(dialogue);

                Debug.Log(
                    $"DialogueSpeaker '{gameObject.name}': Triggered dialogue '{dialogue.name}'"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"DialogueSpeaker '{gameObject.name}': Error triggering dialogue - {e.Message}"
                );
                isDialogueActive = false;
                currentDialogue = null;
            }

            // Schedule dialogue end detection
            StartCoroutine(WaitForDialogueEnd());
        }

        private System.Collections.IEnumerator WaitForDialogueEnd()
        {
            // Wait for dialogue to actually start
            yield return null;

            // Wait while dialogue is active
            while (currentDialogue != null && currentDialogue.IsActive())
            {
                yield return null;
            }

            // Dialogue has ended
            OnDialogueComplete();
        }

        private void OnDialogueComplete()
        {
            isDialogueActive = false;
            Dialogue completedDialogue = currentDialogue;
            currentDialogue = null;

            // Trigger events
            OnDialogueEnded?.Invoke();

            if (completedDialogue != null)
            {
                GameEventManager.Instance?.SendSignal(
                    $"Dialogue_Ended_{gameObject.name}_{completedDialogue.name}"
                );
            }

            // Refresh available dialogues for next interaction
            RefreshAvailableDialogues();
        }

        private void HandleNoDialogueAvailable()
        {
            Debug.LogWarning(
                $"DialogueSpeaker '{gameObject.name}': No dialogue available to trigger"
            );

            OnNoDialogueAvailable?.Invoke();
            GameEventManager.Instance?.SendSignal($"Dialogue_NoAvailable_{gameObject.name}");
        }

        private Dialogue NextUnspokenDialogue()
        {
            return availableMainDialogues.FirstOrDefault();
        }

        //! ╔═══════════════════╗
        //! ║ Public Methods    ║
        //! ╚═══════════════════╝

        /// <summary>
        /// Force the next main dialogue to trigger, regardless of conditions
        /// </summary>
        [ContextMenu("Force Next Dialogue")]
        public void ForceNextDialogue()
        {
            var nextDialogue = mainDialogues.FirstOrDefault(d => d != null && !d.IsSpoken());
            if (nextDialogue != null)
            {
                ExecuteDialogue(nextDialogue);
                nextDialogue.MarkAsSpoken();
            }
            else
            {
                Debug.Log(
                    $"DialogueSpeaker '{gameObject.name}': No unspoken main dialogues to force"
                );
            }
        }

        /// <summary>
        /// Reset all dialogue spoken status
        /// </summary>
        [ContextMenu("Reset All Dialogues")]
        public void ResetAllDialogues()
        {
            foreach (Dialogue dialogue in mainDialogues)
            {
                dialogue?.ResetSpokenStatus();
            }

            foreach (Dialogue dialogue in repeatingDialogues)
            {
                dialogue?.ResetSpokenStatus();
            }

            totalDialoguesSpoken = 0;
            interactionCount = 0;
            RefreshAvailableDialogues();

            Debug.Log($"DialogueSpeaker '{gameObject.name}': All dialogues reset");
            GameEventManager.Instance?.SendSignal($"Dialogue_Reset_{gameObject.name}");
        }

        /// <summary>
        /// Add a new main dialogue at runtime
        /// </summary>
        public void AddMainDialogue(Dialogue dialogue)
        {
            if (dialogue != null && !mainDialogues.Contains(dialogue))
            {
                mainDialogues.Add(dialogue);
                RefreshAvailableDialogues();

                Debug.Log(
                    $"DialogueSpeaker '{gameObject.name}': Added new main dialogue '{dialogue.name}'"
                );
            }
        }

        /// <summary>
        /// Add a new repeating dialogue at runtime
        /// </summary>
        public void AddRepeatingDialogue(Dialogue dialogue)
        {
            if (dialogue != null && !repeatingDialogues.Contains(dialogue))
            {
                repeatingDialogues.Add(dialogue);
                RefreshAvailableDialogues();

                Debug.Log(
                    $"DialogueSpeaker '{gameObject.name}': Added new repeating dialogue '{dialogue.name}'"
                );
            }
        }

        /// <summary>
        /// Get comprehensive speaker information
        /// </summary>
        public string GetSpeakerInfo()
        {
            int unspokenMain = mainDialogues.Count(d => d != null && !d.IsSpoken());
            int availableRepeating = repeatingDialogues.Count(d => d != null && d.CanTrigger());

            return $"DialogueSpeaker '{gameObject.name}': "
                + $"Main: {unspokenMain}/{mainDialogues.Count}, "
                + $"Repeating: {availableRepeating}/{repeatingDialogues.Count}, "
                + $"Total Spoken: {totalDialoguesSpoken}, "
                + $"Interactions: {interactionCount}, "
                + $"Can Interact: {canCurrentlyInteract}, "
                + $"Priority: {interactionPriority}";
        }

        /// <summary>
        /// Check if speaker has any available dialogues
        /// </summary>
        public bool HasAvailableDialogue()
        {
            return availableMainDialogues.Count > 0
                || repeatingDialogues.Any(d => d != null && d.CanTrigger());
        }

        /// <summary>
        /// Get the estimated total dialogue time remaining
        /// </summary>
        public float GetEstimatedDialogueTime()
        {
            float totalTime = 0f;

            foreach (var dialogue in availableMainDialogues)
            {
                totalTime += dialogue.GetEstimatedReadingTime();
            }

            // Add one repeating dialogue as estimate
            var firstRepeating = repeatingDialogues.FirstOrDefault(d =>
                d != null && d.CanTrigger()
            );
            if (firstRepeating != null)
            {
                totalTime += firstRepeating.GetEstimatedReadingTime();
            }

            return totalTime;
        }

        /// <summary>
        /// Set interaction cooldown dynamically
        /// </summary>
        public void SetInteractionCooldown(float cooldown)
        {
            interactionCooldown = Mathf.Max(0f, cooldown);
        }

        /// <summary>
        /// Enable or disable this speaker
        /// </summary>
        public void SetSpeakerEnabled(bool enabled)
        {
            canCurrentlyInteract = enabled;

            if (enabled)
            {
                RefreshAvailableDialogues();
            }
        }

        void OnDestroy()
        {
            // Cleanup GameEventManager subscriptions
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.RemoveAction(
                    $"Dialogue_ForceNext_{gameObject.name}",
                    new UnityAction(ForceNextDialogue)
                );

                GameEventManager.Instance.RemoveAction(
                    $"Dialogue_Reset_{gameObject.name}",
                    new UnityAction(ResetAllDialogues)
                );

                GameEventManager.Instance.RemoveAction(
                    $"Dialogue_Refresh_{gameObject.name}",
                    new UnityAction(RefreshAvailableDialogues)
                );
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
