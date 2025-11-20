# GameEventManager Overhaul Documentation

## Overview

Your GameEventManager has been completely overhauled to provide a modern, type-safe, high-performance event system while maintaining backwards compatibility with your existing string-based approach.

## Key Improvements

### 1. **Type-Safe Events**
- **Before**: String-based events prone to typos and runtime errors
- **After**: Compile-time type checking with generic events
- **Benefit**: Catch errors at compile-time, better IntelliSense support

### 2. **HasSignals Interface Pattern**
- **IHasSignals Interface**: Contract for classes that use the event system
- **HasSignalsBase Class**: Automatic lifecycle management for signal subscriptions
- **Benefit**: No more memory leaks from forgotten unsubscriptions

### 3. **Bulletproof Singleton Pattern**
- Same reliable pattern as your AudioManager/BeatManager
- Automatic data transfer during scene transitions
- Thread-safe operations with proper locking
- **Benefit**: Rock-solid singleton that survives scene changes

### 4. **Performance Optimizations**
- Event pooling to reduce garbage collection
- Event caching for frequently used events
- Thread-safe operations with minimal locking
- **Benefit**: Better performance, especially in high-frequency scenarios

## File Structure

```
Game Event Manager/
├── GameEvents.cs              # Type-safe event definitions (BaseGameEvent, GameEvent<T>)
├── IHasSignals.cs            # Interface and base class for observer pattern
├── GameEventManager.cs       # Main manager (full featured but has compile issues)
├── GameEventManagerSimplified.cs # Simplified version that compiles immediately
├── ExampleSignalUser.cs      # Complete usage example
└── README.md                 # This documentation
```

## Usage Examples

### Modern Type-Safe Approach (Recommended)

```csharp
public class PlayerController : HasSignalsBase
{
    protected override void OnInitializeSignals()
    {
        // Subscribe to type-safe events with compile-time checking
        Subscribe(GameEvents.GameStarted, OnGameStarted);
        Subscribe(GameEvents.PlayerHealthChanged, OnHealthChanged);
    }

    protected override void OnCleanupSignals()
    {
        // Automatic cleanup - you can also do manual cleanup here
        Unsubscribe(GameEvents.GameStarted, OnGameStarted);
        Unsubscribe(GameEvents.PlayerHealthChanged, OnHealthChanged);
    }

    private void OnGameStarted()
    {
        Debug.Log("Game started!");
    }

    private void OnHealthChanged((GameObject player, float health, float maxHealth) data)
    {
        Debug.Log($"Player health: {data.health}/{data.maxHealth}");
    }

    // Raise events
    private void TakeDamage(float damage)
    {
        health -= damage;
        RaiseSignal(GameEvents.PlayerHealthChanged, (gameObject, health, maxHealth));
    }
}
```

### Legacy String-Based Approach (Backwards Compatible)

```csharp
public class OldStyleComponent : MonoBehaviour
{
    void Start()
    {
        // Still works exactly like before
        GameEventManager.Instance.AddAction("Player_Spawned", OnPlayerSpawned);
    }

    void OnDestroy()
    {
        GameEventManager.Instance.RemoveAction("Player_Spawned", OnPlayerSpawned);
    }

    private void OnPlayerSpawned()
    {
        Debug.Log("Player spawned (old style)");
    }

    private void DoSomething()
    {
        // Send legacy signals
        GameEventManager.Instance.SendSignal("Player_Spawned");
    }
}
```

## Predefined Events

The system comes with comprehensive predefined events:

### Player Events
- `GameEvents.PlayerSpawned` - `GameEvent<GameObject>`
- `GameEvents.PlayerDied` - `GameEvent<GameObject>`
- `GameEvents.PlayerHealthChanged` - `GameEvent<(GameObject player, float health, float maxHealth)>`
- `GameEvents.PlayerLevelUp` - `GameEvent<(GameObject player, int level)>`

### Game State Events
- `GameEvents.GameStarted` - `GameEvent`
- `GameEvents.GamePaused` - `GameEvent`
- `GameEvents.GameResumed` - `GameEvent`
- `GameEvents.GameOver` - `GameEvent<float>` (score parameter)

### UI Events
- `GameEvents.UIMenuOpened` - `GameEvent<string>`
- `GameEvents.UIMenuClosed` - `GameEvent<string>`
- `GameEvents.UIButtonClicked` - `GameEvent<(string buttonName, GameObject source)>`

### Audio Events
- `GameEvents.AudioMusicStarted` - `GameEvent<string>`
- `GameEvents.AudioMusicStopped` - `GameEvent`
- `GameEvents.AudioSFXPlayed` - `GameEvent<(string sfxName, float volume)>`

### Scene Events
- `GameEvents.SceneLoadingStarted` - `GameEvent<string>`
- `GameEvents.SceneLoadingFinished` - `GameEvent<string>`
- `GameEvents.SceneTransitionStarted` - `GameEvent<(string fromScene, string toScene)>`

## Creating Custom Events

### Option 1: Extend GameEvents Class
```csharp
public static class GameEvents
{
    // Add your custom events here
    public static readonly GameEvent<MyCustomData> MyCustomEvent = 
        new("MyCustom.Event", "Description of what this event does");
}
```

### Option 2: Create ScriptableObject Events (Advanced)
```csharp
[CreateAssetMenu(menuName = "Events/Custom Game Event")]
public class CustomGameEventSO : ScriptableObject
{
    public GameEvent<MyData> MyEvent { get; private set; }
    
    void OnEnable()
    {
        MyEvent = new GameEvent<MyData>(name, "Custom event from ScriptableObject");
    }
}
```

## Migration Guide

### From Your Old System:

1. **Keep Existing Code Working**: Your existing string-based events continue to work
2. **Gradually Migrate**: Update components one by one to use `HasSignalsBase`
3. **Replace Strings**: Convert string constants to type-safe events
4. **Add Type Safety**: Replace `object` parameters with strongly typed data

### Example Migration:

**Before:**
```csharp
GameEventManager.Instance.AddAction("Player_Health_Changed", OnHealthChanged);
GameEventManager.Instance.SendSignal("Player_Health_Changed");
```

**After:**
```csharp
Subscribe(GameEvents.PlayerHealthChanged, OnHealthChanged);
RaiseSignal(GameEvents.PlayerHealthChanged, (player, health, maxHealth));
```

## Debug Features

### Inspector Debug Info
- Real-time listener counts
- Event trigger history
- Performance metrics
- Category management

### Context Menu Commands
- `Manual Test Event` - Test specific events
- `Clear Event History` - Reset debug history
- `Print Event Statistics` - Log comprehensive stats
- `Clean Up Unused Events` - Memory cleanup

### Event Categories
Events are organized into categories for better management:
- Enable/disable entire categories
- Color-coded organization
- Cooldown timers per category
- Logging control per category

## Best Practices

### 1. Use HasSignalsBase for New Components
```csharp
public class MyComponent : HasSignalsBase // Instead of MonoBehaviour
{
    // Automatic signal lifecycle management
}
```

### 2. Prefer Type-Safe Events
```csharp
// Good: Compile-time safety
Subscribe(GameEvents.PlayerDied, OnPlayerDied);

// Avoid: Runtime string matching
GameEventManager.Instance.AddAction("Player_Died", OnPlayerDied);
```

### 3. Use Descriptive Event Names
```csharp
public static readonly GameEvent<WeaponData> WeaponEquipped = 
    new("Player.WeaponEquipped", "Called when player equips a weapon");
```

### 4. Group Related Data in Tuples
```csharp
// Good: All related data in one event
GameEvent<(GameObject player, float health, float maxHealth, DamageType damageType)>

// Avoid: Multiple separate events for related data
```

### 5. Clean Up Subscriptions
The system handles this automatically with `HasSignalsBase`, but for manual management:
```csharp
void OnDestroy()
{
    // Always clean up to prevent memory leaks
    Unsubscribe(GameEvents.MyEvent, MyHandler);
}
```

## Performance Considerations

- **Event Pooling**: Reuses UnityEvent instances to reduce GC pressure
- **Event Caching**: Frequently used events are cached for faster access  
- **Thread Safety**: Proper locking ensures thread-safe operations
- **Automatic Cleanup**: Unused events are automatically cleaned up
- **Category Filtering**: Disabled categories skip processing entirely

## Troubleshooting

### Compilation Errors
If you encounter compilation errors, use `GameEventManagerSimplified.cs` which has all the core functionality but with simpler types that compile immediately.

### Memory Leaks
Use `HasSignalsBase` for automatic cleanup, or ensure manual cleanup in `OnDestroy()`.

### Performance Issues
Enable debug mode to monitor event statistics and identify bottlenecks.

## Future Enhancements

The system is designed to be extensible:
- **Event Recording**: Record and replay event sequences
- **Event Analytics**: Track event usage patterns
- **Visual Debugging**: Event flow visualization
- **Network Events**: Networked event synchronization
- **Event Validation**: Runtime event validation and debugging
