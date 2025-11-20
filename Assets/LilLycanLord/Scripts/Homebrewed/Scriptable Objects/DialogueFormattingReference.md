# Dialogue System Formatting Codes Reference

## 📚 **Overview**
The enhanced Dialogue system supports rich text formatting through special codes embedded in dialogue text. These codes control character behavior, text appearance, timing, and audio during dialogue playback.

## 🎭 **Character Control Codes**

### Speaker Management
- **`[speaker:characterName]`** - Switch to a specific character
  - Example: `Hello! [speaker:Alice] Oh, hi there!`
  - Switches from current speaker to Alice

- **`[emotion:emotionName]`** - Change character's emotion/sprite
  - Example: `I'm so happy! [emotion:happy]`
  - Uses the character's sprite for "happy" emotion

- **`[anim:animationName]`** - Trigger character animation
  - Example: `Watch this! [anim:wave]`
  - Triggers the "wave" animation for current character

### Audio Control
- **`[voice:voiceType]`** - Change voice clip type
  - Example: `[voice:angry] I'm furious!`
  - Uses the character's "angry" voice clip

- **`[sound:effectName]`** - Play sound effect
  - Example: `[sound:doorbell] Someone's at the door!`
  - Plays the "doorbell" sound effect

## 🎨 **Text Formatting Codes**

### Color Control
- **`[color:colorName]`** - Change text color
  - Named colors: `[color:red]`, `[color:blue]`, `[color:green]`, etc.
  - Hex colors: `[color:#FF0000]` (red), `[color:#00FF00]` (green)
  - Example: `This text is [color:red]red[color:white] and this is white.`

### Text Style
- **`[bold]text[/bold]`** - Make text bold
  - Example: `This is [bold]very important[/bold] information!`

- **`[italic]text[/italic]`** - Make text italic
  - Example: `She thought to herself, [italic]what a strange day[/italic].`

- **`[size:number]`** - Change font size
  - Example: `Normal text [size:24]BIG TEXT[size:12] small text`

## ⏱️ **Timing Control Codes**

### Speed Control
- **`[speed:speedValue]`** - Change typing speed
  - Named speeds: `[speed:slow]`, `[speed:normal]`, `[speed:fast]`
  - Numeric speeds: `[speed:5]`, `[speed:50]` (characters per second)
  - Example: `[speed:slow]I'll speak slowly now... [speed:fast]and now quickly!`

### Pauses
- **`[pause:seconds]`** - Insert pause in dialogue
  - Example: `Wait for it... [pause:2.0] ...surprise!`
  - Pauses for 2 seconds before continuing

## 🔄 **Legacy Support Codes**

### Original System Compatibility
- **`>`** - Speaker switch marker (end of line)
  - Example: Line ends with `Hello there! >`
  - Switches to the other speaker after this line

- **`##`** - Reference other speaker's name
  - Example: `Nice to meet you, ##!`
  - Replaces `##` with the other speaker's name

## 💡 **Usage Examples**

### Basic Formatting
```
Hello [bold]there[/bold]! [emotion:happy]
[color:blue]This text is blue[color:white] and this is white again.
```

### Multi-Character Conversation
```
[speaker:Alice] Hi Bob! [emotion:happy]
[speaker:Bob] [emotion:surprised] Oh, hello Alice!
[speaker:Alice] [voice:whisper] [speed:slow] Can you keep a secret? [pause:1.0]
[speaker:Bob] [emotion:curious] Of course! What is it?
```

### Complex Scene with Effects
```
[speaker:Narrator] [color:gray] [italic]The old door creaked open...[/italic]
[sound:creaking_door] [pause:1.5]
[speaker:Hero] [emotion:scared] [voice:whisper] Who's there?
[sound:footsteps] [pause:0.5]
[speaker:Mystery] [color:red] [bold] [speed:slow] You shouldn't have come here... [/bold]
```

### Legacy Format Example
```
Speaker 1: Hello ##! How are you today? >
Speaker 2: I'm doing well, ##. Thanks for asking!
```

## 🛠️ **Technical Notes**

### Parsing Order
1. Character control codes are processed first
2. Text formatting codes are applied to the visual output
3. Timing codes control playback speed and pauses
4. Legacy codes are converted to new format internally

### Nesting Rules
- Text formatting codes can be nested: `[bold][italic]text[/italic][/bold]`
- Color changes persist until another color is specified
- Speed changes persist until another speed is specified
- Emotions persist until changed or speaker switches

### Error Handling
- Unknown codes are ignored and removed from text
- Invalid parameters default to safe values
- Missing closing tags are automatically closed at line end

## 🎯 **Best Practices**

### Performance
- Avoid excessive formatting changes within single lines
- Use named speeds/colors for consistency across dialogues
- Group related formatting codes together

### Readability
- Use consistent spacing around codes: `text [code] text`
- Comment complex formatting sequences
- Test dialogue with different characters to ensure compatibility

### Accessibility
- Always provide fallback text for sound-only cues
- Don't rely solely on color to convey meaning
- Consider pause lengths for different reading speeds

## 🔧 **Integration with Character System**

### Character Requirements
- Characters must have defined sprites for used emotions
- Voice clips should match voice type codes used in dialogue
- Animation triggers must be set up in character definitions

### Fallbacks
- Missing emotions default to character's default sprite
- Missing voice clips use character's default voice
- Unknown animations are ignored (logged as warnings)

---

*This dialogue system is designed to be flexible and extensible. New formatting codes can be added by extending the `DialogueCommandType` enum and implementing parsing logic in the `ParseDialogueLine` method.*
