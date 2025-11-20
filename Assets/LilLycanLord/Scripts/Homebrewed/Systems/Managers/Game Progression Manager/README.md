# GameProgressionManager - Complete Guide

## 📖 Overview

The `GameProgressionManager` is a robust singleton system for managing game progression data, save/load functionality, and persistent game state. It uses a flexible key-value system that can store primitives, Unity objects, and complex custom data structures.

---

## 🚀 Quick Start

### 1. Setup

-   Add the `GameProgressionManager` script to any GameObject in your scene
-   The singleton will automatically persist across scenes and handle its own lifecycle

### 2. Basic Usage

```csharp
// Save data
GameProgressionManager.Instance.SaveProgression("player_level", 42);
GameProgressionManager.Instance.SaveProgression("player_name", "Hero");
GameProgressionManager.Instance.SaveProgression("has_sword", true);

// Retrieve data
int level = GameProgressionManager.Instance.GetProgression<int>("player_level");
string name = GameProgressionManager.Instance.GetProgression<string>("player_name");
bool hasSword = GameProgressionManager.Instance.GetProgression<bool>("has_sword");

// Save all to file
GameProgressionManager.Instance.SaveAllProgression();

// Load all from file
GameProgressionManager.Instance.LoadAllProgression();
```

---

## 🛠️ Features

### ✅ Singleton Pattern

-   **Thread-safe** access from anywhere in your code
-   **Auto-creation** if no instance exists
-   **Persistent across scenes** (configurable)
-   **Auto-recreation** if destroyed during runtime (configurable)

### ✅ Data Storage

-   **Primitives**: `int`, `long`, `float`, `string`, `bool`
-   **Unity Vectors**: `Vector2`, `Vector3`, `Quaternion`
-   **Type-safe retrieval** with generic methods

> **⚠️ Note**: Only primitive types and Unity vector types are supported for reliable serialization. Complex Unity objects like `Transform` or `GameObject` are not supported.

### ✅ File Persistence

-   **JSON serialization** for human-readable save files
-   **Automatic directory creation**
-   **Configurable save location** and filename
-   **Error handling** with detailed logging

### ✅ Inspector Integration

-   **SerializedDictionary** shows all progression flags in the inspector
-   **Runtime editing** for debugging
-   **Configurable singleton behavior**

---

## 📋 Core Methods

### Data Management

```csharp
// Save progression data
SaveProgression(string key, object value)

// Retrieve progression data
T GetProgression<T>(string key)

// Check if progression exists
bool HasProgression(string key)

// Remove specific progression
bool RemoveProgression(string key)

// Clear all progression
ClearAllProgression()
```

### File Operations

```csharp
// Save all progression to default file
bool SaveAllProgression()

// Save all progression to specific file
bool SaveAllProgression(string filePath)

// Load all progression from default file
bool LoadAllProgression()

// Load all progression from specific file
bool LoadAllProgression(string filePath)
```

### Utility Methods

```csharp
// Get all progression keys
IEnumerable<string> GetAllProgressionKeys()

// Get count of progression flags
int GetProgressionCount()

// Debug log all progression
LogAllProgression()
```

### Singleton Management

```csharp
// Access the singleton instance
GameProgressionManager.Instance

// Check if instance exists (safe)
GameProgressionManager.HasInstance

// Force instance creation
GameProgressionManager.EnsureInstance()

// Reset singleton state (for editor)
GameProgressionManager.ResetSingletonState()
```

---

## 🎮 Usage Examples

### Basic Progression Tracking

```csharp
public class PlayerController : MonoBehaviour
{
    private void Start()
    {
        // Load player's saved level
        int savedLevel = GameProgressionManager.Instance.GetProgression<int>("player_level");
        if (savedLevel > 0)
        {
            SetPlayerLevel(savedLevel);
        }
    }

    public void LevelUp()
    {
        currentLevel++;
        // Save immediately when player levels up
        GameProgressionManager.Instance.SaveProgression("player_level", currentLevel);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save when game is paused/minimized
            GameProgressionManager.Instance.SaveAllProgression();
        }
    }
}
```

### Quest System Integration

```csharp
public class QuestManager : MonoBehaviour
{
    public void CompleteQuest(string questId)
    {
        // Mark quest as completed
        GameProgressionManager.Instance.SaveProgression($"quest_{questId}_completed", true);

        // Save completion timestamp using long
        GameProgressionManager.Instance.SaveProgression($"quest_{questId}_completion_time", DateTime.Now.ToBinary());

        // Save all progression after quest completion
        GameProgressionManager.Instance.SaveAllProgression();
    }

    public bool IsQuestCompleted(string questId)
    {
        return GameProgressionManager.Instance.GetProgression<bool>($"quest_{questId}_completed");
    }

    public DateTime GetQuestCompletionTime(string questId)
    {
        long timeBinary = GameProgressionManager.Instance.GetProgression<long>($"quest_{questId}_completion_time");
        return timeBinary != 0 ? DateTime.FromBinary(timeBinary) : DateTime.MinValue;
    }
}
```

### Timestamp Tracking

```csharp
public class SaveSystem : MonoBehaviour
{
    public void SaveWithTimestamp()
    {
        // Save current timestamp as long
        long currentTime = DateTime.Now.ToBinary();
        GameProgressionManager.Instance.SaveProgression("last_save_time", currentTime);
        
        Debug.Log($"Game saved at: {DateTime.Now}");
    }

    public void LoadLastSaveTime()
    {
        long savedTimeBinary = GameProgressionManager.Instance.GetProgression<long>("last_save_time");
        if (savedTimeBinary != 0)
        {
            DateTime lastSaveTime = DateTime.FromBinary(savedTimeBinary);
            Debug.Log($"Last save was: {lastSaveTime}");
            
            TimeSpan timeSince = DateTime.Now - lastSaveTime;
            Debug.Log($"Time since last save: {timeSince.TotalMinutes:F1} minutes");
        }
    }
}
```

### Individual Stat Tracking

```csharp
public class CharacterManager : MonoBehaviour
{
    public void SavePlayerStats()
    {
        // Save individual stats as primitives
        GameProgressionManager.Instance.SaveProgression("player_strength", 15);
        GameProgressionManager.Instance.SaveProgression("player_dexterity", 12);
        GameProgressionManager.Instance.SaveProgression("player_intelligence", 18);
        GameProgressionManager.Instance.SaveProgression("player_health", 85.5f);
    }

    public void LoadPlayerStats()
    {
        // Load individual stats
        int strength = GameProgressionManager.Instance.GetProgression<int>("player_strength");
        int dexterity = GameProgressionManager.Instance.GetProgression<int>("player_dexterity");
        int intelligence = GameProgressionManager.Instance.GetProgression<int>("player_intelligence");
        float health = GameProgressionManager.Instance.GetProgression<float>("player_health");

        // Apply to your character system
        ApplyStats(strength, dexterity, intelligence, health);
    }

    private void ApplyStats(int str, int dex, int intel, float hp)
    {
        // Apply the loaded stats to your character
    }
}
```

### Position/Rotation Saving

```csharp
public class CheckpointSystem : MonoBehaviour
{
    public void SaveCheckpoint(Transform playerTransform)
    {
        // Save position, rotation, and scale separately
        GameProgressionManager.Instance.SaveProgression("checkpoint_position", playerTransform.position);
        GameProgressionManager.Instance.SaveProgression("checkpoint_rotation", playerTransform.rotation);
        GameProgressionManager.Instance.SaveProgression("checkpoint_scale", playerTransform.localScale);
    }

    public void LoadCheckpoint(Transform playerTransform)
    {
        // Load position
        Vector3 savedPosition = GameProgressionManager.Instance.GetProgression<Vector3>("checkpoint_position");
        if (savedPosition != Vector3.zero)
        {
            playerTransform.position = savedPosition;
        }

        // Load rotation
        Quaternion savedRotation = GameProgressionManager.Instance.GetProgression<Quaternion>("checkpoint_rotation");
        if (savedRotation != Quaternion.identity)
        {
            playerTransform.rotation = savedRotation;
        }

        // Load scale
        Vector3 savedScale = GameProgressionManager.Instance.GetProgression<Vector3>("checkpoint_scale");
        if (savedScale != Vector3.zero)
        {
            playerTransform.localScale = savedScale;
        }
    }
}
```

---

## ⚙️ Configuration

### Singleton Settings (Inspector)

-   **Persist Across Scenes**: Keep singleton alive between scene changes
-   **Transfer Data On Replace**: Transfer data when replacing singleton instances
-   **Auto Recreate On Destroy**: Automatically recreate if destroyed during runtime

### Save/Load Settings (Inspector)

-   **Default Save File Name**: Name of the JSON save file (default: "GameProgression.json")
-   **Save Directory**: Directory relative to chosen location (default: "Saves")
-   **Save Location**: Choose where files are saved:
    -   **Relative To Executable**: Next to exe in builds, in GameData folder in editor (recommended)
    -   **Persistent Data Path**: Platform-specific user data folder (standard Unity location)
    -   **Assets Folder**: Always in GameData folder relative to project (development only)
-   **Append Timestamp To File Name**: Automatically add date/time to save files
    -   **Enabled**: `GameProgression_2025-08-21_14-30-45.json`
    -   **Disabled**: `GameProgression.json` (overwrites previous saves)
-   **Enable Debug Logging**: Enable detailed logging for debugging

### Quick Access (Context Menu)

Right-click the GameProgressionManager component in the Inspector for these options:
-   **Save All Progression**: Manually save all progression data
-   **Load All Progression**: Manually load progression data
-   **📁 Open Save Directory**: Open the save folder in your file explorer
-   **🔍 Show Save File Info**: Display save file details in the console

### Save Location Details

#### Relative To Executable (Recommended)
- **In Editor**: `ProjectFolder/GameData/Saves/`
- **In Build**: `ExecutableFolder/GameData/Saves/`
- **Best for**: Distribution with easy access to save files

#### Persistent Data Path
- **Windows**: `%USERPROFILE%/AppData/LocalLow/CompanyName/ProductName/`
- **Mac**: `~/Library/Application Support/CompanyName/ProductName/`
- **Linux**: `~/.config/unity3d/CompanyName/ProductName/`
- **Best for**: Standard user data storage

#### Assets Folder
- **Always**: `ProjectFolder/GameData/Saves/`
- **Best for**: Development and testing only

---

## 📁 File Structure

### Save File Location

```
{Application.persistentDataPath}/Saves/GameProgression.json
```

### Save File Format (JSON)

```json
{
	"data": [
		{
			"key": "player_level",
			"value": {
				"dataType": "System.Int32",
				"jsonData": "{\"value\":42}",
				"lastModified": "2025-08-21T10:30:00.000Z"
			}
		},
		{
			"key": "player_name",
			"value": {
				"dataType": "System.String",
				"jsonData": "{\"value\":\"Hero\"}",
				"lastModified": "2025-08-21T10:30:00.000Z"
			}
		}
	]
}
```

---

## 🐛 Debugging

### Inspector Debugging

-   View all progression flags in the SerializedDictionary
-   Edit values at runtime for testing
-   Monitor data types and modification times

### Console Debugging

```csharp
// Log all current progression
GameProgressionManager.Instance.LogAllProgression();

// Check if specific progression exists
if (GameProgressionManager.Instance.HasProgression("player_level"))
{
    Debug.Log("Player level found!");
}

// Get progression count
int count = GameProgressionManager.Instance.GetProgressionCount();
Debug.Log($"Total progression flags: {count}");
```

### File Management Examples

```csharp
public class SaveFileManager : MonoBehaviour
{
    private void Start()
    {
        // Get current save file location info
        ShowCurrentSaveLocation();
    }

    [ContextMenu("Show Save Location")]
    public void ShowCurrentSaveLocation()
    {
        // Access context menu functions programmatically
        GameProgressionManager.Instance.ShowSaveFileInfo();
    }

    [ContextMenu("Open Save Folder")]
    public void OpenSaveFolder()
    {
        // Open the save directory in file explorer
        GameProgressionManager.Instance.OpenSaveDirectory();
    }

    public void CreateBackup()
    {
        // Save to a custom backup file
        string backupPath = Path.Combine(
            Application.persistentDataPath, 
            "Backups", 
            $"backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json"
        );
        
        GameProgressionManager.Instance.SaveAllProgression(backupPath);
        Debug.Log($"Backup created: {backupPath}");
    }
}
```

### Timestamped Save Files

```csharp
public class SaveManager : MonoBehaviour
{
    public void EnableTimestampedSaves()
    {
        // Enable automatic timestamping for unique save files
        // This must be set in the Inspector or during Awake/Start
        // Each save will create a new file: GameProgression_2025-08-21_14-30-45.json
        
        GameProgressionManager.Instance.SaveAllProgression();
        Debug.Log("Saved with timestamp - no files overwritten!");
    }

    public void CreateVersionedSave()
    {
        // Even with timestamping disabled, you can create custom timestamped saves
        string customPath = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            $"GameProgression_v{Application.version}_{DateTime.Now:yyyy-MM-dd}.json"
        );
        
        GameProgressionManager.Instance.SaveAllProgression(customPath);
        Debug.Log($"Version save created: {customPath}");
    }
}
```
}
```

### Testing Keyboard Shortcuts (GameProgressionExample)

-   **S**: Save example progression
-   **L**: Load example progression
-   **P**: Save all progression to file
-   **O**: Load all progression from file

---

## ⚠️ Important Notes

### Data Type Consistency

-   Always use the same type when saving/loading progression data
-   Type mismatches will return `default(T)` and log an error

### Unity Object Limitations

-   `Transform` and `GameObject` references are converted to serializable data
-   Original object references are not preserved across save/load
-   Use transform data to restore positions, rotations, scales

### Performance Considerations

-   Don't call `SaveAllProgression()` every frame
-   Use individual `SaveProgression()` calls for frequent updates
-   Consider batching save operations

### Thread Safety

-   The singleton is thread-safe for access
-   File operations should be called from the main thread

### Timestamped Save Files

-   **Format**: `FileName_YYYY-MM-DD_HH-mm-ss.extension`
-   **Example**: `GameProgression_2025-08-21_14-30-45.json`
-   **Loading**: Timestamped files must be loaded manually using custom file paths
-   **Storage**: Each save creates a new file, so manage disk space accordingly

---

## 🔧 Advanced Usage

### Custom Data Transfer

```csharp
public class CustomProgressionManager : GameProgressionManager
{
    protected override void OnDataTransfer(GameProgressionManager newInstance)
    {
        // Custom data transfer logic when singleton is replaced
        Debug.Log("Transferring custom progression data...");

        // Transfer any additional custom data here
        base.OnDataTransfer(newInstance);
    }

    protected override void OnSingletonInitialized()
    {
        // Custom initialization logic
        Debug.Log("Custom progression manager initialized!");

        // Perform any setup specific to your game
        base.OnSingletonInitialized();
    }
}
```

### Multiple Save Files

```csharp
public class SaveSlotManager : MonoBehaviour
{
    public void SaveToSlot(int slotNumber)
    {
        string fileName = $"GameProgression_Slot{slotNumber}.json";
        GameProgressionManager.Instance.SaveAllProgression(fileName);
    }

    public void LoadFromSlot(int slotNumber)
    {
        string fileName = $"GameProgression_Slot{slotNumber}.json";
        GameProgressionManager.Instance.LoadAllProgression(fileName);
    }
}
```

---

## 📦 Dependencies

-   **AYellowpaper.SerializedCollections**: For inspector-friendly dictionaries
-   **Newtonsoft.Json**: For robust JSON serialization (built into Unity)
-   **Unity 2020.3+**: For modern Unity features

---

## 🤝 Support

For issues, questions, or feature requests, refer to the example script `GameProgressionExample.cs` which demonstrates all core functionality and usage patterns.
