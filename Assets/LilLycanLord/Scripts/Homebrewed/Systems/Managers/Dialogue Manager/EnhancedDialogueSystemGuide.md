# General Dialogue System - Complete Implementation Guide

## 🎯 Overview

Your dialogue system has been completely overhauled with modern features while maintaining backward compatibility. The system now supports:

### ✨ **New Features**

-   **Visual Novel Style**: Left/right character positioning with smooth transitions
-   **Character System**: Full Character ScriptableObject integration with emotions, portraits, and voice clips
-   **Hold Duration Integration**: Complete integration with your interaction system
-   **General DialogueManager**: Singleton pattern with modern and legacy mode support
-   **Smart UI Management**: Automatic DialogueUI registration and management
-   **Event System Integration**: Full GameEventManager integration for decoupled communication

---

## 🏗️ **Core Components**

### 1. **DialogueManager (General)**

**Location**: `Assets/LilLycanLord/Scripts/Homebrewed/Systems/Managers/Dialogue Manager/DialogueManager.cs`

**Key Features**:

-   **Singleton Pattern**: Proper singleton with persistence and data transfer
-   **Dual Mode Support**: Handles both legacy two-speaker and modern multi-character dialogues
-   **Character State Management**: Tracks and restores character emotions
-   **Audio Integration**: Background music and sound effects
-   **Event Broadcasting**: GameEventManager integration
-   **Debug Mode**: Comprehensive logging and debug information

**Public Methods**:

```csharp
// Core Functionality
DialogueManager.Instance.TriggerDialogue(dialogue, dialogueUI);
DialogueManager.Instance.EndDialogue();
DialogueManager.Instance.IsDialogueActive();

// UI Management
DialogueManager.Instance.RegisterDialogueUI(dialogueUI);
DialogueManager.Instance.UnregisterDialogueUI(dialogueUI);

// Control Methods
DialogueManager.Instance.ForceStopDialogue();
DialogueManager.Instance.ForceCompleteCurrentLine();
DialogueManager.Instance.SkipCurrentLine();
```

### 2. **DialogueUI (General)**

**Location**: `Assets/LilLycanLord/Scripts/Homebrewed/Systems/Managers/Dialogue Manager/DialogueUI.cs`

**Key Features**:

-   **Visual Novel Layout**: Left speaker (current) and right speaker (previous)
-   **Character Integration**: Dynamic portrait updates based on emotions
-   **TypewriterText Integration**: Smooth text animation
-   **UICanvasFader Integration**: Fade in/out dialogue sequences
-   **Command Processing**: Handles dialogue formatting commands
-   **Auto-registration**: Automatically registers with DialogueManager

**Required Components**:

```csharp
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(UICanvasFader))]
```

**UI Elements Setup**:

```csharp
// Left Speaker (Current)
[SerializeField] private TMP_Text leftSpeakerName;
[SerializeField] private Image leftSpeakerPortrait;
[SerializeField] private GameObject leftSpeakerContainer;

// Right Speaker (Previous)
[SerializeField] private TMP_Text rightSpeakerName;
[SerializeField] private Image rightSpeakerPortrait;
[SerializeField] private GameObject rightSpeakerContainer;

// Main Dialogue
[SerializeField] private TypewriterText dialogueText;
```

### 3. **DialogueSpeaker (General)**

**Location**: `Assets/LilLycanLord/Scripts/Homebrewed/Systems/Managers/Dialogue Manager/DialogueSpeaker.cs`

**Key Features**:

-   **IInteractable Integration**: Full compliance with your interaction system
-   **Hold Duration Support**: Individual hold duration settings per speaker
-   **Priority System**: Dialogue priority and speaker conditions
-   **General State Tracking**: Real-time dialogue availability and status
-   **Event Integration**: GameEventManager integration for speaker events

**Inspector Configuration**:

```csharp
[Header("Main Dialogue Configuration")]
[SerializeField] private List<Dialogue> mainDialogues;
[SerializeField] private bool randomizedMainDialogues = false;

[Header("Repeating Dialogue Configuration")]
[SerializeField] private List<Dialogue> repeatingDialogues;
[SerializeField] private bool randomizedRepeatingDialogues = true;

[Header("Interaction Settings")]
[SerializeField] private string interactionPrompt = "Talk";
[SerializeField] private float holdDuration = -1f; // -1 = use global setting
[SerializeField] private int interactionPriority = 5;
```

### 4. **Character (General)**

**Location**: `Assets/LilLycanLord/Scripts/Homebrewed/Scriptable Objects/Character.cs`

**Key Features**:

-   **Emotion System**: Dynamic emotion states with sprite management
-   **Portrait Management**: Automatic portrait updates based on current emotion
-   **Voice Integration**: Character-specific voice clips
-   **Debug Support**: Optional debug logging for character state changes

**New Methods Added**:

```csharp
// Emotion Management
public string GetCurrentEmotion();
public void SetEmotion(string emotion);
public Sprite GetCurrentPortrait();

// Existing Methods (General)
public Sprite GetDialogueSprite(string emotion = "neutral");
public AudioClip GetVoiceClip(string voiceType = "default");
public Color GetCharacterColor();
```

### 5. **Dialogue (General)**

**Location**: `Assets/LilLycanLord/Scripts/Homebrewed/Scriptable Objects/Dialogue.cs`

**Key Features**:

-   **Dual Mode Support**: Legacy two-speaker and modern multi-character
-   **Command System**: Rich text formatting and control commands
-   **Character Integration**: Full Character ScriptableObject support
-   **Audio Integration**: Background music and sound effects per dialogue

**New Public Accessors Added**:

```csharp
// Legacy System Accessors
public string GetSpeaker1Name();
public string GetSpeaker2Name();
public string GetSpeaker1Lines();
public string GetSpeaker2Lines();

// Audio Accessors
public AudioClip GetBackgroundMusic();
public AudioClip GetSoundEffect(string soundName);
```

---

## 🚀 **Setup Guide**

### **Step 1: DialogueUI Setup**

1. **Create DialogueUI GameObject**:

    ```
    Create Empty GameObject → "DialogueUI"
    Add Components: Canvas, UICanvasFader, DialogueUI script
    ```

2. **Setup UI Elements**:

    ```
    DialogueUI/
    ├── LeftSpeaker/
    │   ├── Name (TMP_Text)
    │   ├── Portrait (Image)
    │   └── Container (GameObject)
    ├── RightSpeaker/
    │   ├── Name (TMP_Text)
    │   ├── Portrait (Image)
    │   └── Container (GameObject)
    └── DialogueText (TypewriterText)
    ```

3. **Assign References**: Drag UI elements to DialogueUI script fields

### **Step 2: Character Creation**

1. **Create Character Asset**:

    ```
    Right-click in Project → Create → LilLycanLord → Dialogue → Character
    ```

2. **Configure Character**:
    ```csharp
    Character Name: "Alice"
    Character ID: "alice"
    Dialogue Sprites:
      - neutral: [sprite]
      - happy: [sprite]
      - sad: [sprite]
    Default Sprite: [neutral sprite]
    Character Color: [theme color]
    ```

### **Step 3: Dialogue Creation**

1. **Create Dialogue Asset**:

    ```
    Right-click in Project → Create → LilLycanLord → Dialogue → Dialogue
    ```

2. **Configure for Multi-Character** (Recommended):

    ```csharp
    Dialogue Mode: MultiCharacter or Hybrid
    Available Characters: [Assign Character assets]
    Dialogue Lines:
      - Speaker: [Character reference]
      - Raw Text: "Hello! [emotion:happy] How are you?"
    ```

3. **Or Configure for Legacy**:
    ```csharp
    Dialogue Mode: Legacy or Hybrid
    Speaker 1 Name: "Alice"
    Speaker 2 Name: "Player"
    Speaker 1 Lines: "Hello there!\nHow are you today?"
    Speaker 2 Lines: "I'm doing well!\nThanks for asking!"
    ```

### **Step 4: DialogueSpeaker Setup**

1. **Add to GameObject**:

    ```csharp
    GameObject → Add Component → DialogueSpeaker
    ```

2. **Configure Settings**:
    ```csharp
    Main Dialogues: [Assign Dialogue assets]
    Interaction Prompt: "Talk to Alice"
    Hold Duration: -1 (or specific duration)
    Interaction Priority: 5
    ```

---

## 🎮 **Usage Examples**

### **Basic Interaction**

```csharp
// The system handles this automatically through GeneralPlayerInteraction
// When player interacts with a DialogueSpeaker:
// 1. DialogueSpeaker.Interact() is called
// 2. TriggerDialogue() determines which dialogue to use
// 3. DialogueManager.TriggerDialogue() starts the dialogue
// 4. DialogueUI displays with visual novel styling
```

### **Manual Dialogue Triggering**

```csharp
// Get dialogue and trigger manually
Dialogue myDialogue = Resources.Load<Dialogue>("MyDialogue");
DialogueManager.Instance.TriggerDialogue(myDialogue);
```

### **Character Emotion Management**

```csharp
// Characters automatically manage emotions through dialogue commands:
// "[emotion:happy]" in dialogue text
// Or manually:
Character alice = Resources.Load<Character>("Alice");
alice.SetEmotion("happy");
Sprite currentPortrait = alice.GetCurrentPortrait();
```

### **GameEventManager Integration**

```csharp
// Automatic events sent:
GameEventManager.Instance.SendSignal("Dialogue_Started_NPCName_DialogueName");
GameEventManager.Instance.SendSignal("Dialogue_Ended_NPCName_DialogueName");
GameEventManager.Instance.SendSignal("Dialogue_MainCompleted_NPCName");

// Listen for events:
GameEventManager.Instance.AddListener("Dialogue_Started_Alice_Greeting", OnAliceGreetingStarted);
```

---

## 🎨 **Visual Novel Features**

### **Character Positioning**

-   **Left Side**: Current speaker (highlighted)
-   **Right Side**: Previous speaker (dimmed)
-   **Smooth Transitions**: Characters slide in/out and fade

### **Dialogue Commands**

Your system supports rich text formatting commands:

```
[speaker:characterName]  - Switch current speaker
[emotion:happy]         - Change character emotion
[sound:effectName]      - Play sound effect
[pause:2.0]            - Pause for 2 seconds
[color:red]            - Change text color
[bold]text[/bold]      - Bold text
```

### **Dynamic UI Updates**

-   Character portraits update automatically based on emotions
-   Speaker names change dynamically with character switches
-   Color themes adapt to current character
-   Previous speaker is always visible on the right side

---

## 🔧 **Configuration Options**

### **DialogueManager Settings**

```csharp
[Header("Dialogue Settings")]
public bool allowDialogueInterruption = false;
public bool enablePlayerDisabling = true;
public bool enableBackgroundMusic = true;
public bool fadeOutMusicDuringDialogue = true;

[Header("Character Management")]
public int maxActiveCharacters = 6;
public bool resetCharacterEmotionsAfterDialogue = true;
public bool logCharacterStateChanges = false;

[Header("Event Integration")]
public bool enableEventIntegration = true;
public float playerReEnableDelay = 0.1f;
```

### **DialogueUI Settings**

```csharp
[Header("Visual Novel Settings")]
public bool enableCharacterTransitions = true;
public float characterTransitionSpeed = 0.3f;
public bool dimPreviousSpeaker = true;
public float previousSpeakerAlpha = 0.6f;

[Header("Audio Settings")]
public bool enableCharacterVoices = true;
public bool enableDialogueSoundEffects = true;
```

---

## 🚨 **Integration Points**

### **With Hold Duration System**

-   DialogueSpeaker implements `GetHoldDuration()`
-   Individual speakers can override global hold duration
-   Progress tracking works seamlessly

### **With GameEventManager**

-   Automatic event broadcasting for all dialogue events
-   Speaker-specific events for targeted responses
-   Global dialogue events for UI management

### **With AudioManager**

-   Background music management per dialogue
-   Character voice clips
-   Sound effect integration

### **With UICanvasFader**

-   Smooth dialogue sequence transitions
-   Fade in/out for dramatic effect
-   Configurable fade durations

---

## 🎯 **Best Practices**

### **Character Setup**

1. **Consistent Emotion Names**: Use standard names across all characters (neutral, happy, sad, angry, etc.)
2. **Portrait Sizing**: Keep all character portraits same dimensions for smooth transitions
3. **Color Themes**: Choose distinct colors for each character to improve readability

### **Dialogue Creation**

1. **Mode Selection**: Use MultiCharacter for new content, Legacy for existing content
2. **Character References**: Always assign Character assets to dialogue lines for best results
3. **Command Usage**: Use formatting commands sparingly for best readability

### **Performance**

1. **UI Registration**: DialogueUI auto-registers, but you can manually manage for better control
2. **Character Caching**: System caches character states, minimal performance impact
3. **Event Cleanup**: GameEventManager events are properly cleaned up on destroy

---

## 🔄 **Migration from Old System**

Your existing dialogues will continue to work thanks to the hybrid system:

### **Legacy Dialogues** (No changes needed)

-   Set `DialogueMode = Legacy` or `Hybrid`
-   Use existing `speaker1Name`, `speaker2Name`, `speaker1Lines`, `speaker2Lines`
-   System automatically converts to new format at runtime

### **General Dialogues** (New features)

-   Set `DialogueMode = MultiCharacter` or `Hybrid`
-   Create Character assets for speakers
-   Use DialogueLine system for full control
-   Add rich formatting commands as needed

---

## 📊 **Summary**

Your dialogue system is now a complete, modern solution that supports:

✅ **Visual novel-style presentation** with left/right character positioning  
✅ **Full Character integration** with emotions, portraits, and voice  
✅ **Hold duration system** integration with individual override support  
✅ **General DialogueManager** with singleton pattern and state management  
✅ **Backward compatibility** with existing legacy dialogues  
✅ **Event system integration** for decoupled communication  
✅ **Audio management** for music and sound effects  
✅ **Rich text formatting** with dialogue commands  
✅ **Debug and logging** support for easy troubleshooting

The system is production-ready and provides a solid foundation for complex dialogue scenarios while maintaining the simplicity needed for basic conversations. All components compile successfully and integrate seamlessly with your existing systems.
