# Hold Duration System Documentation

The `GeneralPlayerInteraction` system now supports **per-interaction hold duration** override, allowing individual interactables to specify custom hold times that override the global setting.

## Overview

The hold duration system allows:

-   **Global hold duration** setting in `GeneralPlayerInteraction`
-   **Per-interaction hold duration** override via the `IInteractable.GetHoldDuration()` method
-   **Instant interactions** when hold duration is 0
-   **Legacy compatibility** with existing interaction code

## Implementation

### 1. IInteractable Interface Enhancement

The `IInteractable` interface now includes:

```csharp
/// <summary>
/// Get hold duration for this interaction (-1 = use global setting)
/// </summary>
float GetHoldDuration()
{
    return -1f; // Default: use global setting
}
```

### 2. Global Hold Duration Setting

In `GeneralPlayerInteraction`:

```csharp
[SerializeField]
[Tooltip("Default hold duration in seconds (0 = instant interaction)")]
[Range(0.0f, 5.0f)]
private float globalHoldDuration = 0.0f;
```

### 3. Hold Duration Values

| Value  | Behavior                               |
| ------ | -------------------------------------- |
| `-1f`  | Use global hold duration setting       |
| `0f`   | Instant interaction (no hold required) |
| `> 0f` | Custom hold duration in seconds        |

## Usage Examples

### Example 1: Instant Interaction

```csharp
public float GetHoldDuration()
{
    return 0f; // Instant interaction on press
}
```

### Example 2: Custom Hold Duration

```csharp
public float GetHoldDuration()
{
    return 3f; // Requires 3 seconds of holding
}
```

### Example 3: Use Global Setting

```csharp
public float GetHoldDuration()
{
    return -1f; // Use the global hold duration
}
```

### Example 4: Dynamic Hold Duration

```csharp
[SerializeField] private float customHoldDuration = 2f;
[SerializeField] private bool useGlobalSetting = false;

public float GetHoldDuration()
{
    return useGlobalSetting ? -1f : customHoldDuration;
}
```

## Events Broadcasting

The system broadcasts the following GameEventManager events:

| Event                         | When Triggered                           |
| ----------------------------- | ---------------------------------------- |
| `"Interaction_HoldStarted"`   | Player starts holding an interaction key |
| `"Interaction_HoldProgress"`  | During hold progress (every 5 frames)    |
| `"Interaction_HoldCompleted"` | Hold duration completed successfully     |
| `"Interaction_HoldCancelled"` | Player released key before completion    |

### Listening to Hold Events

```csharp
void Start()
{
    GameEventManager.Instance.AddAction("Interaction_HoldStarted", OnHoldStarted);
    GameEventManager.Instance.AddAction("Interaction_HoldProgress", OnHoldProgress);
    GameEventManager.Instance.AddAction("Interaction_HoldCompleted", OnHoldCompleted);
    GameEventManager.Instance.AddAction("Interaction_HoldCancelled", OnHoldCancelled);
}

private void OnHoldStarted()
{
    Debug.Log("Player started holding interaction key");
}

private void OnHoldProgress()
{
    float progress = playerInteraction.GetHoldProgress(); // 0.0 to 1.0
    Debug.Log($"Hold progress: {progress:P1}");
}

private void OnHoldCompleted()
{
    Debug.Log("Hold completed successfully!");
}

private void OnHoldCancelled()
{
    Debug.Log("Hold was cancelled");
}
```

## Public API Methods

### GeneralPlayerInteraction Public Methods

```csharp
/// <summary>
/// Get current hold progress (0.0 to 1.0)
/// </summary>
public float GetHoldProgress()

/// <summary>
/// Check if currently holding an interaction
/// </summary>
public bool IsCurrentlyHolding()

/// <summary>
/// Get the target being held (if any)
/// </summary>
public GameObject GetHoldTarget()
```

## Runtime Debugging

The system provides runtime debugging information in the Inspector:

### Hold System Status

-   **Is Currently Holding**: Shows if player is currently holding
-   **Current Hold Target**: The GameObject being held
-   **Hold Progress**: Visual slider showing hold completion (0-1)

### Debug Mode

Enable `debugMode` in `GeneralPlayerInteraction` for console logging:

-   Hold timer start events
-   Hold timer cancellation with progress
-   Hold completion events

## Integration with UI

You can create UI feedback using the public API:

```csharp
public class HoldProgressUI : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GeneralPlayerInteraction playerInteraction;

    void Update()
    {
        if (playerInteraction.IsCurrentlyHolding())
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = playerInteraction.GetHoldProgress();
        }
        else
        {
            progressSlider.gameObject.SetActive(false);
        }
    }
}
```

## Best Practices

### 1. Consistent Hold Durations

-   Use similar hold durations for similar interaction types
-   Reserve long holds (3+ seconds) for important/dangerous actions
-   Use instant interactions (0s) for simple toggles

### 2. Visual Feedback

-   Provide clear visual feedback during hold progress
-   Use color changes, progress bars, or animations
-   Indicate the expected hold duration in UI

### 3. Audio Feedback

-   Play audio cues when hold starts
-   Consider progress audio (ticking, building intensity)
-   Clear completion/cancellation sounds

### 4. Accessibility

-   Don't make holds too long (max 5 seconds recommended)
-   Provide alternatives for users with motor difficulties
-   Clear visual indication of hold requirements

## Example Complete Implementation

See `ExampleHoldInteractable.cs` for a complete implementation example that demonstrates:

-   Custom hold duration settings
-   Visual feedback during hold
-   Event handling
-   Editor integration
-   Multiple interaction modes

## Backward Compatibility

The system is fully backward compatible:

-   Existing `IInteractable` implementations work unchanged
-   Default `GetHoldDuration()` returns `-1f` (use global)
-   Legacy interaction behavior preserved
-   No breaking changes to existing code

## Performance Considerations

-   Hold progress events are throttled (every 5 frames) to prevent spam
-   Efficient timer management with minimal allocations
-   Thread-safe event broadcasting through GameEventManager
-   Optimized input caching to reduce per-frame overhead
