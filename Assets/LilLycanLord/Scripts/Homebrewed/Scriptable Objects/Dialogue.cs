using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Character speaking this line (null = use default speaker)")]
        public Character speaker;

        [Tooltip("Raw text with formatting codes")]
        [TextArea(2, 5)]
        public string rawText;

        [Tooltip("Emotion/sprite override for this line")]
        public string emotionOverride = "";

        [Tooltip("Typing speed override for this line")]
        [Range(0f, 100f)]
        public float typingSpeedOverride = -1f; // -1 = use character default

        [Tooltip("Additional pause after this line (seconds)")]
        [Range(0f, 5f)]
        public float pauseAfter = 0f;

        [Tooltip("Audio clip to play with this line")]
        public AudioClip audioOverride;

        // Parsed data (runtime only)
        [System.NonSerialized]
        public string processedText;

        [System.NonSerialized]
        public List<DialogueCommand> commands;

        [System.NonSerialized]
        public bool isParsed = false;
    }

    [System.Serializable]
    public class DialogueCommand
    {
        public DialogueCommandType commandType;
        public string parameter1;
        public string parameter2;
        public float floatValue;
        public int textPosition; // Position in the text where this command should execute
    }

    public enum DialogueCommandType
    {
        // Character Control
        SpeakerChange, // [speaker:characterName]
        EmotionChange, // [emotion:happy]
        AnimationTrigger, // [anim:wave]

        // Text Formatting
        ColorChange, // [color:red] or [color:#FF0000]
        BoldStart, // [bold]
        BoldEnd, // [/bold]
        ItalicStart, // [italic]
        ItalicEnd, // [/italic]
        SizeChange, // [size:20]

        // Timing Control
        Pause, // [pause:2.0]
        SpeedChange, // [speed:fast] or [speed:20]

        // Audio Control
        SoundEffect, // [sound:effect_name]
        VoiceChange, // [voice:angry]

        // Legacy Support
        SpeakerSwitch, // > (original system)
        NameReference, // ## (original system)
    }

    public enum DialogueMode
    {
        Legacy, // Use original two-speaker system
        MultiCharacter, // Use new character-based system
        Hybrid, // Support both systems
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "LilLycanLord/Dialogue/Dialogue")]
    public class Dialogue : ScriptableObject
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

        [Header("Dialogue Mode")]
        [SerializeField]
        [Tooltip("How this dialogue should be processed")]
        private DialogueMode dialogueMode = DialogueMode.Hybrid;

        [Header("Character-Based System")]
        [SerializeField]
        [Tooltip("All characters that can speak in this dialogue")]
        private List<Character> availableCharacters = new List<Character>();

        [SerializeField]
        [Tooltip("Individual dialogue lines with full control")]
        private List<DialogueLine> dialogueLines = new List<DialogueLine>();

        [Header("Legacy Two-Speaker System")]
        [SerializeField]
        [Tooltip("Name of the first speaker (legacy mode)")]
        private string speaker1Name = "";

        [SerializeField]
        [Tooltip("Name of the second speaker (legacy mode)")]
        private string speaker2Name = "";

        [SerializeField]
        [TextArea(5, 15)]
        [Tooltip("Lines for speaker 1 (legacy mode with formatting codes)")]
        private string speaker1Lines = "";

        [SerializeField]
        [TextArea(5, 15)]
        [Tooltip("Lines for speaker 2 (legacy mode with formatting codes)")]
        private string speaker2Lines = "";

        [Header("Dialogue Metadata")]
        [SerializeField]
        [Tooltip("Unique ID for referencing this dialogue")]
        private string dialogueID = "";

        [SerializeField]
        [Tooltip("Category for organizing dialogues")]
        private string category = "General";

        [SerializeField]
        [Tooltip("Priority level (higher = more important)")]
        [Range(0, 10)]
        private int priority = 5;

        [SerializeField]
        [Tooltip("Tags for searching and filtering")]
        private List<string> tags = new List<string>();

        [Header("State Tracking")]
        [SerializeField]
        [Tooltip("Has this dialogue been spoken/read by the player?")]
        private bool spoken = false;

        [SerializeField]
        [Tooltip("How many times this dialogue has been triggered")]
        private int timesTriggered = 0;

        [SerializeField]
        [Tooltip("Can this dialogue be repeated?")]
        private bool repeatable = true;

        [Header("Conditions & Events")]
        [SerializeField]
        [Tooltip("Conditions that must be met for this dialogue to trigger")]
        private List<DialogueCondition> conditions = new List<DialogueCondition>();

        [SerializeField]
        [Tooltip("Events to trigger before dialogue starts")]
        private UnityEvent onDialogueStart;

        [SerializeField]
        [Tooltip("Events to trigger after dialogue ends")]
        private UnityEvent onDialogueEnd;

        [SerializeField]
        [Tooltip("Events to trigger when dialogue is repeated")]
        private UnityEvent onDialogueRepeat;

        [Header("Audio Settings")]
        [SerializeField]
        [SerializedDictionary("Sound Name", "Audio Clip")]
        [Tooltip("Named sound effects that can be triggered during dialogue")]
        private SerializedDictionary<string, AudioClip> soundEffects =
            new SerializedDictionary<string, AudioClip>();

        [SerializeField]
        [Tooltip("Background music for this dialogue")]
        private AudioClip backgroundMusic;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Volume for background music")]
        private float musicVolume = 0.5f;

        // Runtime data
        [System.NonSerialized]
        private List<DialogueLine> parsedLines;

        [System.NonSerialized]
        private bool isParsed = false;

        [System.NonSerialized]
        private System.DateTime lastTriggeredTime;

        [System.NonSerialized]
        private bool isCurrentlyActive = false;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Get the dialogue's display name
        /// </summary>
        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(dialogueID) ? dialogueID : name;
        }

        /// <summary>
        /// Get the dialogue mode
        /// </summary>
        public DialogueMode GetDialogueMode()
        {
            return dialogueMode;
        }

        /// <summary>
        /// Get all available characters for this dialogue
        /// </summary>
        public Character[] GetAvailableCharacters()
        {
            return availableCharacters.ToArray();
        }

        /// <summary>
        /// Get character by name/ID
        /// </summary>
        public Character GetCharacter(string nameOrId)
        {
            return availableCharacters.Find(c =>
                c.GetName() == nameOrId || c.GetID() == nameOrId || c.name == nameOrId
            );
        }

        /// <summary>
        /// Get parsed dialogue lines ready for presentation
        /// </summary>
        public List<DialogueLine> GetParsedLines()
        {
            if (!isParsed)
                ParseDialogue();

            return parsedLines ?? new List<DialogueLine>();
        }

        /// <summary>
        /// Get speaker 1 name (legacy system)
        /// </summary>
        public string GetSpeaker1Name()
        {
            return speaker1Name;
        }

        /// <summary>
        /// Get speaker 2 name (legacy system)
        /// </summary>
        public string GetSpeaker2Name()
        {
            return speaker2Name;
        }

        /// <summary>
        /// Get speaker 1 lines (legacy system)
        /// </summary>
        public string GetSpeaker1Lines()
        {
            return speaker1Lines;
        }

        /// <summary>
        /// Get speaker 2 lines (legacy system)
        /// </summary>
        public string GetSpeaker2Lines()
        {
            return speaker2Lines;
        }

        /// <summary>
        /// Check if dialogue can be triggered
        /// </summary>
        public bool CanTrigger()
        {
            // Check if already spoken and not repeatable
            if (spoken && !repeatable)
                return false;

            // Check if has content
            if (!HasContent())
            {
                Debug.LogWarning($"Dialogue '{GetDisplayName()}': No content to display");
                return false;
            }

            // Check conditions
            foreach (var condition in conditions)
            {
                if (!condition.IsConditionMet())
                {
                    Debug.Log(
                        $"Dialogue '{GetDisplayName()}': Condition '{condition.conditionName}' not met"
                    );
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if dialogue has content
        /// </summary>
        public bool HasContent()
        {
            if (dialogueMode == DialogueMode.MultiCharacter || dialogueMode == DialogueMode.Hybrid)
            {
                if (dialogueLines != null && dialogueLines.Count > 0)
                {
                    foreach (var line in dialogueLines)
                    {
                        if (!string.IsNullOrEmpty(line.rawText))
                            return true;
                    }
                }
            }

            if (dialogueMode == DialogueMode.Legacy || dialogueMode == DialogueMode.Hybrid)
            {
                return !string.IsNullOrEmpty(speaker1Lines) || !string.IsNullOrEmpty(speaker2Lines);
            }

            return false;
        }

        /// <summary>
        /// Trigger this dialogue
        /// </summary>
        public void TriggerDialogue()
        {
            // This would integrate with your dialogue manager
            Debug.Log($"Triggering dialogue: {GetDisplayName()}");
            OnDialogueTriggered();
        }

        /// <summary>
        /// Mark dialogue as spoken
        /// </summary>
        public void MarkAsSpoken()
        {
            bool wasSpoken = spoken;
            spoken = true;
            timesTriggered++;
            lastTriggeredTime = System.DateTime.Now;

            if (wasSpoken && timesTriggered > 1)
            {
                onDialogueRepeat?.Invoke();
            }

            Debug.Log(
                $"Dialogue '{GetDisplayName()}': Marked as spoken (triggered {timesTriggered} times)"
            );
        }

        /// <summary>
        /// Reset spoken status
        /// </summary>
        public void ResetSpokenStatus()
        {
            spoken = false;
            timesTriggered = 0;
            Debug.Log($"Dialogue '{GetDisplayName()}': Status reset");
        }

        /// <summary>
        /// Check if dialogue has specific tag
        /// </summary>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// Add a tag to this dialogue
        /// </summary>
        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
                tags.Add(tag);
        }

        /// <summary>
        /// Remove a tag from this dialogue
        /// </summary>
        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }

        /// <summary>
        /// Check if dialogue is in specific category
        /// </summary>
        public bool IsInCategory(string categoryName)
        {
            return string.Equals(category, categoryName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get sound effect by name
        /// </summary>
        public AudioClip GetSoundEffect(string soundName)
        {
            if (soundEffects.ContainsKey(soundName))
                return soundEffects[soundName];

            return null;
        }

        /// <summary>
        /// Get background music
        /// </summary>
        public AudioClip GetBackgroundMusic()
        {
            return backgroundMusic;
        }

        /// <summary>
        /// Get music volume
        /// </summary>
        public float GetMusicVolume()
        {
            return musicVolume;
        }

        /// <summary>
        /// Get dialogue priority
        /// </summary>
        public int GetPriority()
        {
            return priority;
        }

        /// <summary>
        /// Get spoken status
        /// </summary>
        public bool IsSpoken()
        {
            return spoken;
        }

        /// <summary>
        /// Get times triggered
        /// </summary>
        public int GetTimesTriggered()
        {
            return timesTriggered;
        }

        /// <summary>
        /// Check if currently active
        /// </summary>
        public bool IsActive()
        {
            return isCurrentlyActive;
        }

        /// <summary>
        /// Get estimated reading time
        /// </summary>
        public float GetEstimatedReadingTime()
        {
            int totalWords = 0;

            var lines = GetParsedLines();
            foreach (var line in lines)
            {
                totalWords += CountWords(line.processedText ?? line.rawText);
            }

            // Rough estimate: 200 words per minute = ~3.3 words per second
            return totalWords / 3.3f;
        }

        /// <summary>
        /// Get dialogue information
        /// </summary>
        public string GetInfo()
        {
            return $"Dialogue: {GetDisplayName()} | Mode: {dialogueMode} | Category: {category} | "
                + $"Priority: {priority} | Spoken: {spoken} | Times: {timesTriggered} | "
                + $"Characters: {availableCharacters.Count} | Lines: {dialogueLines.Count} | "
                + $"Est. Time: {GetEstimatedReadingTime():F1}s | Tags: [{string.Join(", ", tags)}]";
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        /// <summary>
        /// Parse dialogue content into structured lines with commands
        /// </summary>
        private void ParseDialogue()
        {
            parsedLines = new List<DialogueLine>();

            if (dialogueMode == DialogueMode.MultiCharacter || dialogueMode == DialogueMode.Hybrid)
            {
                // Parse character-based dialogue lines
                foreach (var line in dialogueLines)
                {
                    if (!string.IsNullOrEmpty(line.rawText))
                    {
                        var parsedLine = ParseDialogueLine(line);
                        parsedLines.Add(parsedLine);
                    }
                }
            }

            if (
                dialogueMode == DialogueMode.Legacy
                || (dialogueMode == DialogueMode.Hybrid && parsedLines.Count == 0)
            )
            {
                // Parse legacy two-speaker format
                ParseLegacyDialogue();
            }

            isParsed = true;
        }

        /// <summary>
        /// Parse a single dialogue line with formatting codes
        /// </summary>
        private DialogueLine ParseDialogueLine(DialogueLine originalLine)
        {
            var parsedLine = new DialogueLine
            {
                speaker = originalLine.speaker,
                rawText = originalLine.rawText,
                emotionOverride = originalLine.emotionOverride,
                typingSpeedOverride = originalLine.typingSpeedOverride,
                pauseAfter = originalLine.pauseAfter,
                audioOverride = originalLine.audioOverride,
                commands = new List<DialogueCommand>(),
            };

            string text = originalLine.rawText;
            string processedText = "";
            int currentPosition = 0;

            // Regex patterns for different formatting codes
            var patterns = new Dictionary<string, DialogueCommandType>
            {
                { @"\[speaker:([^\]]+)\]", DialogueCommandType.SpeakerChange },
                { @"\[emotion:([^\]]+)\]", DialogueCommandType.EmotionChange },
                { @"\[anim:([^\]]+)\]", DialogueCommandType.AnimationTrigger },
                { @"\[color:([^\]]+)\]", DialogueCommandType.ColorChange },
                { @"\[bold\]", DialogueCommandType.BoldStart },
                { @"\[/bold\]", DialogueCommandType.BoldEnd },
                { @"\[italic\]", DialogueCommandType.ItalicStart },
                { @"\[/italic\]", DialogueCommandType.ItalicEnd },
                { @"\[size:([^\]]+)\]", DialogueCommandType.SizeChange },
                { @"\[pause:([^\]]+)\]", DialogueCommandType.Pause },
                { @"\[speed:([^\]]+)\]", DialogueCommandType.SpeedChange },
                { @"\[sound:([^\]]+)\]", DialogueCommandType.SoundEffect },
                { @"\[voice:([^\]]+)\]", DialogueCommandType.VoiceChange },
                { @">", DialogueCommandType.SpeakerSwitch },
                { @"##", DialogueCommandType.NameReference },
            };

            // Find and parse all formatting codes
            var allMatches = new List<(Match match, DialogueCommandType type)>();

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(text, pattern.Key);
                foreach (Match match in matches)
                {
                    allMatches.Add((match, pattern.Value));
                }
            }

            // Sort matches by position
            allMatches.Sort((a, b) => a.match.Index.CompareTo(b.match.Index));

            int textOffset = 0;
            foreach (var (match, commandType) in allMatches)
            {
                // Add text before the command
                int beforeLength = match.Index - currentPosition;
                if (beforeLength > 0)
                {
                    processedText += text.Substring(currentPosition, beforeLength);
                }

                // Create command
                var command = new DialogueCommand
                {
                    commandType = commandType,
                    textPosition = processedText.Length - textOffset,
                };

                // Extract parameters based on command type
                if (match.Groups.Count > 1)
                {
                    command.parameter1 = match.Groups[1].Value;

                    // Parse numeric values if needed
                    if (
                        commandType == DialogueCommandType.Pause
                        || commandType == DialogueCommandType.SizeChange
                    )
                    {
                        if (float.TryParse(command.parameter1, out float floatVal))
                            command.floatValue = floatVal;
                    }
                }

                // Handle special cases
                switch (commandType)
                {
                    case DialogueCommandType.NameReference:
                        // Replace ## with speaker name
                        string replacementName = GetReplacementName(parsedLine.speaker);
                        processedText += replacementName;
                        break;

                    case DialogueCommandType.SpeakerSwitch:
                        // Handle > speaker switching (legacy support)
                        break;

                    default:
                        // Most commands don't add text to the final output
                        textOffset += match.Length;
                        break;
                }

                parsedLine.commands.Add(command);
                currentPosition = match.Index + match.Length;
            }

            // Add remaining text
            if (currentPosition < text.Length)
            {
                processedText += text.Substring(currentPosition);
            }

            parsedLine.processedText = processedText;
            parsedLine.isParsed = true;

            return parsedLine;
        }

        /// <summary>
        /// Parse legacy two-speaker format into dialogue lines
        /// </summary>
        private void ParseLegacyDialogue()
        {
            var speaker1Character =
                GetCharacter(speaker1Name) ?? CreateTemporaryCharacter(speaker1Name);
            var speaker2Character =
                GetCharacter(speaker2Name) ?? CreateTemporaryCharacter(speaker2Name);

            // Combine and parse both speaker texts
            var allLines = new List<string>();
            var speakerAssignments = new List<bool>(); // true = speaker1, false = speaker2

            if (!string.IsNullOrEmpty(speaker1Lines))
            {
                var lines = speaker1Lines.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var line in lines)
                {
                    allLines.Add(line);
                    speakerAssignments.Add(true);
                }
            }

            if (!string.IsNullOrEmpty(speaker2Lines))
            {
                var lines = speaker2Lines.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var line in lines)
                {
                    allLines.Add(line);
                    speakerAssignments.Add(false);
                }
            }

            // Process lines in order, handling speaker switches
            for (int i = 0; i < allLines.Count; i++)
            {
                string lineText = allLines[i];
                bool isFromSpeaker1 = speakerAssignments[i];

                // Check for speaker switch marker
                if (lineText.EndsWith(">"))
                {
                    lineText = lineText.Substring(0, lineText.Length - 1).Trim();
                    // Speaker will switch after this line
                }

                // Handle name replacement
                string otherSpeakerName = isFromSpeaker1 ? speaker2Name : speaker1Name;
                lineText = lineText.Replace("##", otherSpeakerName ?? "Unknown");

                // Create dialogue line
                var dialogueLine = new DialogueLine
                {
                    speaker = isFromSpeaker1 ? speaker1Character : speaker2Character,
                    rawText = lineText,
                    emotionOverride = "neutral",
                };

                var parsedLine = ParseDialogueLine(dialogueLine);
                parsedLines.Add(parsedLine);
            }
        }

        /// <summary>
        /// Get replacement name for ## references
        /// </summary>
        private string GetReplacementName(Character speaker)
        {
            if (speaker != null)
                return speaker.GetName();

            // Fallback logic for legacy mode
            return "Unknown";
        }

        /// <summary>
        /// Create a temporary character for legacy mode
        /// </summary>
        private Character CreateTemporaryCharacter(string name)
        {
            // This would create a temporary character object
            // In practice, you might want to cache these or handle differently
            var tempCharacter = ScriptableObject.CreateInstance<Character>();
            tempCharacter.name = name ?? "Unknown";
            return tempCharacter;
        }

        /// <summary>
        /// Called when dialogue is triggered
        /// </summary>
        private void OnDialogueTriggered()
        {
            timesTriggered++;
            lastTriggeredTime = System.DateTime.Now;
            OnDialogueStart();
        }

        /// <summary>
        /// Called when dialogue starts
        /// </summary>
        internal void OnDialogueStart()
        {
            isCurrentlyActive = true;
            onDialogueStart?.Invoke();
        }

        /// <summary>
        /// Called when dialogue ends
        /// </summary>
        internal void OnDialogueEnd()
        {
            isCurrentlyActive = false;
            MarkAsSpoken();
            onDialogueEnd?.Invoke();
        }

        /// <summary>
        /// Count words in text
        /// </summary>
        private int CountWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            string[] words = text.Split(
                new char[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries
            );
            return words.Length;
        }

        /// <summary>
        /// Validate dialogue data in editor
        /// </summary>
        private void OnValidate()
        {
            // Ensure dialogue ID is set
            if (string.IsNullOrEmpty(dialogueID))
                dialogueID = name.Replace(" ", "_").ToLower();

            // Validate priority range
            priority = Mathf.Clamp(priority, 0, 10);

            // Clear parsed data when edited
            isParsed = false;
            parsedLines = null;
        }
    }

    [System.Serializable]
    public class DialogueCondition
    {
        [SerializeField]
        [Tooltip("Name/description of this condition")]
        public string conditionName = "New Condition";

        [SerializeField]
        [Tooltip("Type of condition to check")]
        public DialogueConditionType conditionType = DialogueConditionType.AlwaysTrue;

        [SerializeField]
        [Tooltip("String parameter for the condition")]
        public string stringParameter = "";

        [SerializeField]
        [Tooltip("Integer parameter for the condition")]
        public int intParameter = 0;

        [SerializeField]
        [Tooltip("Float parameter for the condition")]
        public float floatParameter = 0f;

        [SerializeField]
        [Tooltip("Boolean parameter for the condition")]
        public bool boolParameter = true;

        /// <summary>
        /// Check if this condition is met
        /// </summary>
        public bool IsConditionMet()
        {
            switch (conditionType)
            {
                case DialogueConditionType.AlwaysTrue:
                    return true;

                case DialogueConditionType.AlwaysFalse:
                    return false;

                case DialogueConditionType.GameEventExists:
                    // Check if a specific game event exists or has been triggered
                    // Note: This would need GameEventManager reference
                    return true; // Placeholder - implement with your event system

                case DialogueConditionType.TimesTriggeredLessThan:
                    // This would need reference to the dialogue itself
                    return true; // Placeholder

                case DialogueConditionType.CustomCondition:
                    // Override this for custom condition logic
                    return EvaluateCustomCondition();

                case DialogueConditionType.CharacterRelationship:
                    // Check relationship between characters
                    return EvaluateRelationshipCondition();

                case DialogueConditionType.CharacterTag:
                    // Check if character has specific tag
                    return EvaluateTagCondition();

                default:
                    return true;
            }
        }

        /// <summary>
        /// Evaluate custom condition logic
        /// </summary>
        protected virtual bool EvaluateCustomCondition()
        {
            // Implement custom condition logic here
            return boolParameter;
        }

        /// <summary>
        /// Evaluate relationship condition
        /// </summary>
        private bool EvaluateRelationshipCondition()
        {
            // This would check character relationships
            // Example: "player_relationship > 50"
            return true; // Placeholder
        }

        /// <summary>
        /// Evaluate tag condition
        /// </summary>
        private bool EvaluateTagCondition()
        {
            // This would check if characters have specific tags
            // Example: "player has_tag:friendly"
            return true; // Placeholder
        }
    }

    public enum DialogueConditionType
    {
        AlwaysTrue,
        AlwaysFalse,
        GameEventExists,
        TimesTriggeredLessThan,
        CustomCondition,
        CharacterRelationship,
        CharacterTag,
    }
}
