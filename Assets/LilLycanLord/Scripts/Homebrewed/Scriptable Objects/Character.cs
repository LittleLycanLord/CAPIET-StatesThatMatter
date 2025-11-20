using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;

namespace LilLycanLord_Official
{
    public enum DialogueStyle
    {
        Normal, // Standard dialogue presentation
        Whisper, // Quiet, smaller text
        Shout, // Bold, larger text
        Thought, // Italicized, different color
        Narrator, // Special formatting for narration
        Robotic, // Mechanical/digital style
        Mystical, // Magical/ethereal style
    }

    [System.Serializable]
    public class CharacterAnimationTrigger
    {
        [Tooltip("Name/ID of the animation trigger")]
        public string triggerName;

        [Tooltip("Animation clip or trigger to activate")]
        public string animationClip;

        [Tooltip("When during dialogue this trigger should fire")]
        public float triggerTime = 0f;

        [Tooltip("Should this animation loop during dialogue?")]
        public bool loop = false;
    }

    [System.Serializable]
    public class ConditionalSprite
    {
        [Tooltip("Condition that must be met to use this sprite")]
        public string condition;

        [Tooltip("Sprite to use when condition is met")]
        public Sprite sprite;

        [Tooltip("Priority level (higher numbers take precedence)")]
        public int priority = 0;
    }

    [CreateAssetMenu(fileName = "New Character", menuName = "LilLycanLord/Dialogue/Character")]
    public class Character : ScriptableObject
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

        [Header("Core Identity")]
        [SerializeField]
        [Tooltip("Display name for this character")]
        private string characterName = "Unknown";

        [SerializeField]
        [Tooltip("Unique identifier for referencing this character")]
        private string characterID = "";

        [SerializeField]
        [TextArea(3, 6)]
        [Tooltip("Character biography or notes")]
        private string description = "";

        [Header("Visual System")]
        [SerializeField]
        [SerializedDictionary("Emotion/State", "Sprite")]
        [Tooltip("Different character expressions/emotions for dialogue")]
        private SerializedDictionary<string, Sprite> dialogueSprites = new SerializedDictionary<
            string,
            Sprite
        >
        {
            { "neutral", null },
            { "happy", null },
            { "sad", null },
            { "angry", null },
            { "surprised", null },
            { "confused", null },
        };

        [SerializeField]
        [Tooltip("Default sprite when no emotion is specified")]
        private Sprite defaultSprite;

        [SerializeField]
        [Tooltip("UI theme color for this character")]
        private Color characterColor = Color.white;

        [SerializeField]
        [Tooltip("High-resolution portrait for detailed character views")]
        private Sprite characterPortrait;

        [Header("Audio System")]
        [SerializeField]
        [SerializedDictionary("Voice Type", "Audio Clip")]
        [Tooltip("Different voice clips for various emotional states")]
        private SerializedDictionary<string, AudioClip> voiceClips = new SerializedDictionary<
            string,
            AudioClip
        >
        {
            { "default", null },
            { "happy", null },
            { "sad", null },
            { "angry", null },
        };

        [SerializeField]
        [Range(0.5f, 2.0f)]
        [Tooltip("Base pitch for voice modulation or text-to-speech")]
        private float defaultVoicePitch = 1.0f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Character-specific volume control")]
        private float voiceVolume = 1.0f;

        [Header("Dialogue Behavior")]
        [SerializeField]
        [Range(1f, 100f)]
        [Tooltip("Character-specific text reveal speed (characters per second)")]
        private float typingSpeed = 20f;

        [SerializeField]
        [Tooltip("Presentation style for this character's dialogue")]
        private DialogueStyle dialogueStyle = DialogueStyle.Normal;

        [SerializeField]
        [Tooltip("Color for this character's dialogue text")]
        private Color textColor = Color.white;

        [SerializeField]
        [Tooltip("Optional custom font for this character")]
        private TMP_FontAsset fontOverride;

        [Header("Relationship System")]
        [SerializeField]
        [SerializedDictionary("Character ID", "Relationship Value")]
        [Tooltip("Relationship values with other characters (-100 to 100)")]
        private SerializedDictionary<string, float> relationshipValues =
            new SerializedDictionary<string, float>();

        [SerializeField]
        [Tooltip("Tags for categorizing this character (friend, enemy, merchant, etc.)")]
        private List<string> characterTags = new List<string>();

        [Header("Advanced Features")]
        [SerializeField]
        [SerializedDictionary("Language Code", "Localized Name")]
        [Tooltip("Character names in different languages")]
        private SerializedDictionary<string, string> localizedNames = new SerializedDictionary<
            string,
            string
        >
        {
            { "en", "" },
            { "es", "" },
            { "fr", "" },
        };

        [SerializeField]
        [Tooltip("Animation triggers that can be activated during dialogue")]
        private List<CharacterAnimationTrigger> animationTriggers =
            new List<CharacterAnimationTrigger>();

        [SerializeField]
        [Tooltip("Sprites that change based on game state or conditions")]
        private List<ConditionalSprite> conditionalSprites = new List<ConditionalSprite>();

        [Header("Debug & Editor")]
        [SerializeField]
        [Tooltip("Show debug information in dialogue system")]
        private bool enableDebugMode = false;

        // Runtime state
        [System.NonSerialized]
        private string currentEmotion = "neutral";

        [System.NonSerialized]
        private Sprite currentPortrait;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Get the character's display name
        /// </summary>
        public string GetName()
        {
            return characterName;
        }

        /// <summary>
        /// Get localized name for the specified language
        /// </summary>
        public string GetLocalizedName(string languageCode = "en")
        {
            if (
                localizedNames.ContainsKey(languageCode)
                && !string.IsNullOrEmpty(localizedNames[languageCode])
            )
                return localizedNames[languageCode];

            return characterName;
        }

        /// <summary>
        /// Get the character's unique ID
        /// </summary>
        public string GetID()
        {
            return string.IsNullOrEmpty(characterID) ? name : characterID;
        }

        /// <summary>
        /// Get character description
        /// </summary>
        public string GetDescription()
        {
            return description;
        }

        /// <summary>
        /// Get sprite for specific emotion/state
        /// </summary>
        public Sprite GetDialogueSprite(string emotion = "neutral")
        {
            if (dialogueSprites.ContainsKey(emotion) && dialogueSprites[emotion] != null)
                return dialogueSprites[emotion];

            return defaultSprite;
        }

        /// <summary>
        /// Get sprite with conditional logic applied
        /// </summary>
        public Sprite GetConditionalSprite(string emotion = "neutral")
        {
            // Check conditional sprites first (sorted by priority)
            var sortedConditionals = new List<ConditionalSprite>(conditionalSprites);
            sortedConditionals.Sort((a, b) => b.priority.CompareTo(a.priority));

            foreach (var conditional in sortedConditionals)
            {
                if (EvaluateCondition(conditional.condition) && conditional.sprite != null)
                    return conditional.sprite;
            }

            // Fall back to regular dialogue sprite
            return GetDialogueSprite(emotion);
        }

        /// <summary>
        /// Get all available emotions/states
        /// </summary>
        public string[] GetAvailableEmotions()
        {
            var emotions = new List<string>();
            foreach (var kvp in dialogueSprites)
            {
                if (kvp.Value != null)
                    emotions.Add(kvp.Key);
            }
            return emotions.ToArray();
        }

        /// <summary>
        /// Get voice clip for specific emotion
        /// </summary>
        public AudioClip GetVoiceClip(string voiceType = "default")
        {
            if (voiceClips.ContainsKey(voiceType) && voiceClips[voiceType] != null)
                return voiceClips[voiceType];

            if (voiceClips.ContainsKey("default"))
                return voiceClips["default"];

            return null;
        }

        /// <summary>
        /// Get character's theme color
        /// </summary>
        public Color GetCharacterColor()
        {
            return characterColor;
        }

        /// <summary>
        /// Get character portrait
        /// </summary>
        public Sprite GetPortrait()
        {
            return characterPortrait;
        }

        /// <summary>
        /// Get current emotion state
        /// </summary>
        public string GetCurrentEmotion()
        {
            return currentEmotion ?? "neutral";
        }

        /// <summary>
        /// Set current emotion and update portrait
        /// </summary>
        public void SetEmotion(string emotion)
        {
            if (string.IsNullOrEmpty(emotion))
                emotion = "neutral";

            currentEmotion = emotion;
            currentPortrait = GetDialogueSprite(emotion);

            if (enableDebugMode)
            {
                Debug.Log($"Character '{characterName}': Set emotion to '{emotion}'");
            }
        }

        /// <summary>
        /// Get current portrait sprite based on emotion
        /// </summary>
        public Sprite GetCurrentPortrait()
        {
            if (currentPortrait != null)
                return currentPortrait;

            return GetDialogueSprite(GetCurrentEmotion());
        }

        /// <summary>
        /// Get typing speed for this character
        /// </summary>
        public float GetTypingSpeed()
        {
            return typingSpeed;
        }

        /// <summary>
        /// Get dialogue style
        /// </summary>
        public DialogueStyle GetDialogueStyle()
        {
            return dialogueStyle;
        }

        /// <summary>
        /// Get text color for dialogue
        /// </summary>
        public Color GetTextColor()
        {
            return textColor;
        }

        /// <summary>
        /// Get custom font override
        /// </summary>
        public TMP_FontAsset GetFontOverride()
        {
            return fontOverride;
        }

        /// <summary>
        /// Get voice pitch
        /// </summary>
        public float GetVoicePitch()
        {
            return defaultVoicePitch;
        }

        /// <summary>
        /// Get voice volume
        /// </summary>
        public float GetVoiceVolume()
        {
            return voiceVolume;
        }

        /// <summary>
        /// Get relationship value with another character
        /// </summary>
        public float GetRelationshipValue(string otherCharacterID)
        {
            if (relationshipValues.ContainsKey(otherCharacterID))
                return relationshipValues[otherCharacterID];

            return 0f; // Neutral relationship
        }

        /// <summary>
        /// Set relationship value with another character
        /// </summary>
        public void SetRelationshipValue(string otherCharacterID, float value)
        {
            value = Mathf.Clamp(value, -100f, 100f);
            relationshipValues[otherCharacterID] = value;
        }

        /// <summary>
        /// Modify relationship value with another character
        /// </summary>
        public void ModifyRelationship(string otherCharacterID, float delta)
        {
            float currentValue = GetRelationshipValue(otherCharacterID);
            SetRelationshipValue(otherCharacterID, currentValue + delta);
        }

        /// <summary>
        /// Check if character has a specific tag
        /// </summary>
        public bool HasTag(string tag)
        {
            return characterTags.Contains(tag);
        }

        /// <summary>
        /// Add a tag to this character
        /// </summary>
        public void AddTag(string tag)
        {
            if (!characterTags.Contains(tag))
                characterTags.Add(tag);
        }

        /// <summary>
        /// Remove a tag from this character
        /// </summary>
        public void RemoveTag(string tag)
        {
            characterTags.Remove(tag);
        }

        /// <summary>
        /// Get all character tags
        /// </summary>
        public string[] GetTags()
        {
            return characterTags.ToArray();
        }

        /// <summary>
        /// Get animation triggers for this character
        /// </summary>
        public CharacterAnimationTrigger[] GetAnimationTriggers()
        {
            return animationTriggers.ToArray();
        }

        /// <summary>
        /// Get specific animation trigger by name
        /// </summary>
        public CharacterAnimationTrigger GetAnimationTrigger(string triggerName)
        {
            return animationTriggers.Find(trigger => trigger.triggerName == triggerName);
        }

        /// <summary>
        /// Check if debug mode is enabled
        /// </summary>
        public bool IsDebugModeEnabled()
        {
            return enableDebugMode;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        /// <summary>
        /// Evaluate a condition string (placeholder for future condition system)
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            // This is a placeholder for a more sophisticated condition evaluation system
            // You could integrate this with your game's state management system

            if (string.IsNullOrEmpty(condition))
                return false;

            // Example simple conditions:
            // "health_low" - check if character health is low
            // "relationship_high:player" - check if relationship with player is high
            // "has_item:key" - check if character has a specific item

            // For now, return false as a safe default
            return false;
        }

        /// <summary>
        /// Validate character data (called in editor)
        /// </summary>
        private void OnValidate()
        {
            // Ensure character ID is set
            if (string.IsNullOrEmpty(characterID))
                characterID = name.Replace(" ", "_").ToLower();

            // Ensure character name is set
            if (string.IsNullOrEmpty(characterName))
                characterName = name;

            // Clamp values
            typingSpeed = Mathf.Clamp(typingSpeed, 1f, 100f);
            defaultVoicePitch = Mathf.Clamp(defaultVoicePitch, 0.5f, 2.0f);
            voiceVolume = Mathf.Clamp01(voiceVolume);
        }
    }
}
