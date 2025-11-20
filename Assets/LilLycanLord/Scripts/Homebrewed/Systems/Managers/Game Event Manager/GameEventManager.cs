using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Centralized event definitions for the game. Use these predefined events for common game systems.
    /// Create ScriptableObject instances for custom events or use the GameEventManager's dynamic event creation.
    /// </summary>
    public static class GameEvents
    {
        // Player Events
        public static readonly GameEvent<GameObject> PlayerSpawned = new(
            "Player.Spawned",
            "Called when a player is spawned in the scene"
        );
        public static readonly GameEvent<GameObject> PlayerDied = new(
            "Player.Died",
            "Called when a player dies"
        );
        public static readonly GameEvent<(
            GameObject player,
            float health,
            float maxHealth
        )> PlayerHealthChanged = new("Player.HealthChanged", "Called when player health changes");
        public static readonly GameEvent<(GameObject player, int level)> PlayerLevelUp = new(
            "Player.LevelUp",
            "Called when player levels up"
        );

        // Game State Events
        public static readonly GameEvent GameStarted = new(
            "Game.Started",
            "Called when the game starts"
        );
        public static readonly GameEvent GamePaused = new(
            "Game.Paused",
            "Called when the game is paused"
        );
        public static readonly GameEvent GameResumed = new(
            "Game.Resumed",
            "Called when the game resumes from pause"
        );
        public static readonly GameEvent<float> GameOver = new(
            "Game.Over",
            "Called when the game ends (score parameter)"
        );

        // UI Events
        public static readonly GameEvent<string> UIMenuOpened = new(
            "UI.MenuOpened",
            "Called when a menu is opened"
        );
        public static readonly GameEvent<string> UIMenuClosed = new(
            "UI.MenuClosed",
            "Called when a menu is closed"
        );
        public static readonly GameEvent<(string buttonName, GameObject source)> UIButtonClicked =
            new("UI.ButtonClicked", "Called when a UI button is clicked");

        // Audio Events
        public static readonly GameEvent<string> AudioMusicStarted = new(
            "Audio.MusicStarted",
            "Called when music starts playing"
        );
        public static readonly GameEvent AudioMusicStopped = new(
            "Audio.MusicStopped",
            "Called when music stops"
        );
        public static readonly GameEvent<(string sfxName, float volume)> AudioSFXPlayed = new(
            "Audio.SFXPlayed",
            "Called when a sound effect is played"
        );

        // Scene Events
        public static readonly GameEvent<string> SceneLoadingStarted = new(
            "Scene.LoadingStarted",
            "Called when scene loading begins"
        );
        public static readonly GameEvent<string> SceneLoadingFinished = new(
            "Scene.LoadingFinished",
            "Called when scene loading completes"
        );
        public static readonly GameEvent<(
            string fromScene,
            string toScene
        )> SceneTransitionStarted = new(
            "Scene.TransitionStarted",
            "Called when scene transition begins"
        );

        // Custom Events
        public static readonly GameEvent<object> CustomEventExample = new(
            "Custom.Example",
            "Example custom event"
        );

        /// <summary>
        /// Get all predefined game events for debugging and management purposes.
        /// </summary>
        public static BaseGameEvent[] GetAllEvents()
        {
            return new BaseGameEvent[]
            {
                PlayerSpawned,
                PlayerDied,
                PlayerHealthChanged,
                PlayerLevelUp,
                GameStarted,
                GamePaused,
                GameResumed,
                GameOver,
                UIMenuOpened,
                UIMenuClosed,
                UIButtonClicked,
                AudioMusicStarted,
                AudioMusicStopped,
                AudioSFXPlayed,
                SceneLoadingStarted,
                SceneLoadingFinished,
                SceneTransitionStarted,
                CustomEventExample,
            };
        }
    }

    /// <summary>
    /// Event category system for organizing and managing game events.
    /// Allows grouping related events and controlling their behavior collectively.
    /// </summary>
    [System.Serializable]
    public class EventCategory
    {
        [SerializeField]
        public string categoryName;

        [SerializeField]
        public Color categoryColor = Color.white;

        [SerializeField]
        public bool isEnabled = true;

        [SerializeField]
        public List<string> eventNames = new List<string>();

        [SerializeField]
        [Range(0f, 5f)]
        public float cooldownTime = 0f;

        [SerializeField]
        public bool logEvents = false;

        private float lastEventTime = 0f;

        public bool CanTriggerEvent()
        {
            return Time.time - lastEventTime >= cooldownTime;
        }

        public void RegisterEventTrigger()
        {
            lastEventTime = Time.time;
        }

        public EventCategory(string name, Color color, params string[] eventNames)
        {
            categoryName = name;
            categoryColor = color;
            this.eventNames.AddRange(eventNames);
        }
    }

    /// <summary>
    /// General debug information for tracking event system performance and behavior.
    /// </summary>
    [System.Serializable]
    public class EventDebugInfo
    {
        public string eventName;
        public string eventType;
        public int listenerCount;
        public float lastTriggeredTime;
        public int triggerCount;
        public bool isActive;
        public float averageTimeBetweenTriggers;
        public List<string> listenerDetails = new List<string>();

        private List<float> triggerTimes = new List<float>();
        private const int MAX_TRIGGER_HISTORY = 10;

        public void RecordTrigger()
        {
            float currentTime = Time.time;
            triggerCount++;
            lastTriggeredTime = currentTime;

            triggerTimes.Add(currentTime);
            if (triggerTimes.Count > MAX_TRIGGER_HISTORY)
            {
                triggerTimes.RemoveAt(0);
            }

            CalculateAverageTime();
        }

        private void CalculateAverageTime()
        {
            if (triggerTimes.Count <= 1)
            {
                averageTimeBetweenTriggers = 0f;
                return;
            }

            float totalTime = 0f;
            for (int i = 1; i < triggerTimes.Count; i++)
            {
                totalTime += triggerTimes[i] - triggerTimes[i - 1];
            }

            averageTimeBetweenTriggers = totalTime / (triggerTimes.Count - 1);
        }
    }

    /// <summary>
    /// Modern, type-safe GameEventManager that provides centralized event management for Unity games.
    /// Features include: bulletproof singleton pattern, type-safe events, performance optimization,
    /// comprehensive debugging, and easy integration with the HasSignals pattern.
    ///
    /// ARCHITECTURE NOTES:
    /// - Uses both predefined static events (GameEvents class) and dynamic runtime events
    /// - Thread-safe operations with proper locking mechanisms
    /// - Automatic memory management with event pooling and cleanup
    /// - Comprehensive debugging and profiling capabilities
    /// - Singleton replacement handling for scene transitions
    /// </summary>
    public class GameEventManager : MonoBehaviour
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
        private static GameEventManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static GameEventManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<GameEventManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(GameEventManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(GameEventManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Game Event Manager");
                            instance = singletonGameObject.AddComponent<GameEventManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(GameEventManager)}] Found existing singleton instance in scene."
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
        protected virtual void TransferDataToNewInstance(GameEventManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.legacyEvents = this.legacyEvents;
                newInstance.dynamicEvents = this.dynamicEvents;
                newInstance.eventCategories = this.eventCategories;
                newInstance.debugMode = this.debugMode;
                newInstance.manualTestEvent = this.manualTestEvent;
                newInstance.enableEventPooling = this.enableEventPooling;
                newInstance.enableEventCaching = this.enableEventCaching;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(GameEventManager newInstance)
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
                    GameEventManager oldInstance = instance;
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
            InitializeEventSystem();
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
                Debug.Log($"[{nameof(GameEventManager)}] Application paused (entering/exiting play mode)");
                // Don't set applicationIsQuitting here in editor
            }
        }

        void OnDestroy()
        {
            CleanupEventSystem();
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
                        $"[{nameof(GameEventManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject("Game Event Manager (Auto-Recreated)");
                    instance = singletonGameObject.AddComponent<GameEventManager>();
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
                Debug.Log($"[{nameof(GameEventManager)}] Singleton state reset");
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

        //! ╔══════════════════════╗
        //! ║ EVENT SYSTEM CONTENT ║
        //! ╚══════════════════════╝

        [Header("Event System Configuration")]
        [SerializeField]
        [Tooltip("Enable detailed debug logging for event operations")]
        private bool debugMode = false;

        [SerializeField]
        [Tooltip("Maximum number of events to track in history")]
        [Range(50, 1000)]
        private int maxHistoryEntries = 200;

        [SerializeField]
        [Tooltip("Enable event pooling for better performance")]
        private bool enableEventPooling = true;

        [SerializeField]
        [Tooltip("Cache frequently used events")]
        private bool enableEventCaching = true;

        [SerializeField]
        [Tooltip("Maximum number of events to cache")]
        [Range(10, 100)]
        private int maxCachedEvents = 50;

        [SerializeField]
        [Tooltip("Automatically cleanup unused events")]
        private bool autoCleanupUnusedEvents = true;

        [SerializeField]
        [Tooltip("Time interval for automatic cleanup (seconds)")]
        [Range(30f, 300f)]
        private float cleanupInterval = 60f;

        [Header("Legacy Event System (String-Based)")]
        [SerializeField]
        [Tooltip("Legacy string-based events for backwards compatibility")]
        [SerializedDictionary("Signal", "Events")]
        private SerializedDictionary<string, UnityEvent> legacyEvents =
            new SerializedDictionary<string, UnityEvent>();

        [Header("Dynamic Event System (Type-Safe)")]
        [SerializeField]
        [Tooltip("Runtime-created type-safe events")]
        private List<object> dynamicEvents = new List<object>();

        [Header("Event Categories")]
        [SerializeField]
        [Tooltip("Organize events into categories for better management")]
        private List<EventCategory> eventCategories = new List<EventCategory>();

        [Header("Debug Information")]
        [SerializeField]
        [Tooltip("Real-time debug information about events")]
        private List<EventDebugInfo> eventDebugInfos = new List<EventDebugInfo>();

        [SerializeField]
        [Tooltip("Recent event history for debugging")]
        private List<string> eventHistory = new List<string>();

        [Header("Manual Testing")]
        [SerializeField]
        [Tooltip("Event name for manual testing")]
        private string manualTestEvent = "";

        //* Private Event System Data
        private Dictionary<string, BaseGameEvent> eventRegistry =
            new Dictionary<string, BaseGameEvent>();
        private Dictionary<string, EventDebugInfo> debugInfoLookup =
            new Dictionary<string, EventDebugInfo>();
        private Dictionary<string, UnityEvent> eventCache = new Dictionary<string, UnityEvent>();
        private Queue<UnityEvent> eventPool = new Queue<UnityEvent>();
        private HashSet<IHasSignals> registeredSignalObjects = new HashSet<IHasSignals>();

        private float lastCleanupTime = 0f;
        private readonly object eventSystemLock = new object();

        void Start()
        {
            InitializePredefinedEvents();
        }

        void Update()
        {
            if (autoCleanupUnusedEvents && Time.time - lastCleanupTime > cleanupInterval)
            {
                PerformAutomaticCleanup();
                lastCleanupTime = Time.time;
            }
        }

        private void InitializeEventSystem()
        {
            lock (eventSystemLock)
            {
                if (enableEventPooling)
                {
                    InitializeEventPool();
                }

                if (enableEventCaching)
                {
                    InitializeEventCache();
                }

                InitializeDefaultCategories();
                RefreshDebugInfo();
            }
        }

        private void CleanupEventSystem()
        {
            lock (eventSystemLock)
            {
                // Cleanup all registered signal objects
                foreach (var signalObject in registeredSignalObjects)
                {
                    if (signalObject != null)
                    {
                        signalObject.CleanupSignals();
                    }
                }
                registeredSignalObjects.Clear();

                // Clear all events
                foreach (var eventPair in eventRegistry)
                {
                    eventPair.Value.ClearAllListeners();
                }
                eventRegistry.Clear();

                // Clear legacy events
                foreach (var eventPair in legacyEvents)
                {
                    eventPair.Value.RemoveAllListeners();
                }
                legacyEvents.Clear();

                // Clear caches and pools
                eventCache.Clear();
                eventPool.Clear();
            }
        }

        private void InitializePredefinedEvents()
        {
            lock (eventSystemLock)
            {
                // Register all predefined events from GameEvents class
                var predefinedEvents = typeof(GameEvents).GetFields(
                    System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Static
                        | System.Reflection.BindingFlags.GetField
                );

                foreach (var field in predefinedEvents)
                {
                    if (
                        field.FieldType.IsSubclassOf(typeof(BaseGameEvent))
                        || field.FieldType == typeof(BaseGameEvent)
                    )
                    {
                        var eventObj = field.GetValue(null) as BaseGameEvent;
                        if (eventObj != null)
                        {
                            RegisterEvent(eventObj);
                        }
                    }
                }

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEventManager: Registered {eventRegistry.Count} predefined events"
                    );
                }
            }
        }

        private void InitializeEventPool()
        {
            for (int i = 0; i < 20; i++)
            {
                eventPool.Enqueue(new UnityEvent());
            }
        }

        private void InitializeEventCache()
        {
            eventCache.Clear();
        }

        private void InitializeDefaultCategories()
        {
            if (eventCategories.Count == 0)
            {
                eventCategories.AddRange(
                    new[]
                    {
                        new EventCategory(
                            "Player Events",
                            Color.blue,
                            "Player.Spawned",
                            "Player.Died",
                            "Player.HealthChanged",
                            "Player.LevelUp"
                        ),
                        new EventCategory(
                            "Game State",
                            Color.green,
                            "Game.Started",
                            "Game.Paused",
                            "Game.Resumed",
                            "Game.Over"
                        ),
                        new EventCategory(
                            "UI Events",
                            Color.yellow,
                            "UI.MenuOpened",
                            "UI.MenuClosed",
                            "UI.ButtonClicked"
                        ),
                        new EventCategory(
                            "Audio Events",
                            Color.magenta,
                            "Audio.MusicStarted",
                            "Audio.MusicStopped",
                            "Audio.SFXPlayed"
                        ),
                        new EventCategory(
                            "Scene Events",
                            Color.cyan,
                            "Scene.LoadingStarted",
                            "Scene.LoadingFinished",
                            "Scene.TransitionStarted"
                        ),
                    }
                );
            }
        }

        private void RefreshDebugInfo()
        {
            eventDebugInfos.Clear();
            debugInfoLookup.Clear();

            foreach (var eventPair in eventRegistry)
            {
                CreateOrUpdateDebugInfo(eventPair.Key, eventPair.Value);
            }

            foreach (var eventPair in legacyEvents)
            {
                CreateOrUpdateLegacyDebugInfo(eventPair.Key);
            }
        }

        //! ╔═══════════════════════════╗
        //! ║ PUBLIC EVENT SYSTEM API   ║
        //! ╚═══════════════════════════╝

        /// <summary>
        /// Register a type-safe event in the event system.
        /// </summary>
        public void RegisterEvent(BaseGameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Debug.LogWarning("GameEventManager: Cannot register null event");
                return;
            }

            lock (eventSystemLock)
            {
                if (eventRegistry.ContainsKey(gameEvent.EventName))
                {
                    if (debugMode)
                        Debug.LogWarning(
                            $"GameEventManager: Event '{gameEvent.EventName}' already registered, skipping"
                        );
                    return;
                }

                eventRegistry[gameEvent.EventName] = gameEvent;
                CreateOrUpdateDebugInfo(gameEvent.EventName, gameEvent);

                if (debugMode)
                    Debug.Log($"GameEventManager: Registered event '{gameEvent.EventName}'");
            }
        }

        /// <summary>
        /// Unregister a type-safe event from the event system.
        /// </summary>
        public void UnregisterEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            lock (eventSystemLock)
            {
                if (eventRegistry.ContainsKey(eventName))
                {
                    eventRegistry[eventName].ClearAllListeners();
                    eventRegistry.Remove(eventName);

                    if (debugInfoLookup.ContainsKey(eventName))
                    {
                        var debugInfo = debugInfoLookup[eventName];
                        debugInfoLookup.Remove(eventName);
                        eventDebugInfos.Remove(debugInfo);
                    }

                    if (debugMode)
                        Debug.Log($"GameEventManager: Unregistered event '{eventName}'");
                }
            }
        }

        /// <summary>
        /// Get a registered event by name. Returns null if not found.
        /// </summary>
        public T GetEvent<T>(string eventName)
            where T : BaseGameEvent
        {
            lock (eventSystemLock)
            {
                if (eventRegistry.TryGetValue(eventName, out BaseGameEvent gameEvent))
                {
                    return gameEvent as T;
                }
                return null;
            }
        }

        /// <summary>
        /// Register an object that implements IHasSignals for automatic lifecycle management.
        /// </summary>
        public void RegisterSignalObject(IHasSignals signalObject)
        {
            if (signalObject == null)
                return;

            lock (eventSystemLock)
            {
                registeredSignalObjects.Add(signalObject);
                signalObject.InitializeSignals();

                if (debugMode)
                    Debug.Log(
                        $"GameEventManager: Registered signal object {signalObject.GetType().Name}"
                    );
            }
        }

        /// <summary>
        /// Unregister an object that implements IHasSignals.
        /// </summary>
        public void UnregisterSignalObject(IHasSignals signalObject)
        {
            if (signalObject == null)
                return;

            lock (eventSystemLock)
            {
                if (registeredSignalObjects.Contains(signalObject))
                {
                    signalObject.CleanupSignals();
                    registeredSignalObjects.Remove(signalObject);

                    if (debugMode)
                        Debug.Log(
                            $"GameEventManager: Unregistered signal object {signalObject.GetType().Name}"
                        );
                }
            }
        }

        //! ╔══════════════════════════╗
        //! ║ LEGACY EVENT SYSTEM API  ║
        //! ╚══════════════════════════╝

        /// <summary>
        /// Add a listener to a legacy string-based event (backwards compatibility).
        /// </summary>
        public void AddAction(string signalName, UnityAction action)
        {
            if (string.IsNullOrEmpty(signalName) || action == null)
            {
                Debug.LogWarning("GameEventManager: Cannot add null signal name or action");
                return;
            }

            lock (eventSystemLock)
            {
                if (!IsEventCategoryEnabled(signalName))
                {
                    if (debugMode)
                        Debug.LogWarning($"Event category for '{signalName}' is disabled");
                    return;
                }

                UnityEvent targetEvent = GetOrCreateLegacyEvent(signalName);
                targetEvent.AddListener(action);

                CreateOrUpdateLegacyDebugInfo(signalName);

                if (debugMode)
                    Debug.Log(
                        $"'{action.Method.Name}' is now listening for '{signalName}' (Listeners: {GetLegacyListenerCount(signalName)})"
                    );
            }
        }

        /// <summary>
        /// Remove a listener from a legacy string-based event.
        /// </summary>
        public void RemoveAction(string signalName, UnityAction action)
        {
            if (string.IsNullOrEmpty(signalName) || action == null)
                return;

            lock (eventSystemLock)
            {
                if (legacyEvents.ContainsKey(signalName))
                {
                    legacyEvents[signalName].RemoveListener(action);
                    if (debugMode)
                        Debug.Log(
                            $"'{action.Method.Name}' has stopped listening for '{signalName}'"
                        );
                }
            }
        }

        /// <summary>
        /// Send a legacy string-based signal.
        /// </summary>
        public void SendSignal(string signalName)
        {
            if (string.IsNullOrEmpty(signalName))
            {
                Debug.LogWarning("GameEventManager: Cannot send signal with null or empty name");
                return;
            }

            lock (eventSystemLock)
            {
                if (!IsEventCategoryEnabled(signalName))
                {
                    if (debugMode)
                        Debug.LogWarning($"Signal '{signalName}' not sent - category is disabled");
                    return;
                }

                if (legacyEvents.ContainsKey(signalName) && legacyEvents[signalName] != null)
                {
                    AddToEventHistory(signalName);

                    if (debugInfoLookup.ContainsKey(signalName))
                    {
                        debugInfoLookup[signalName].RecordTrigger();
                    }

                    if (debugMode)
                        Debug.Log(
                            $"Signal Sent: '{signalName}' (Listeners: {GetLegacyListenerCount(signalName)})"
                        );

                    legacyEvents[signalName]?.Invoke();
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"No event found for signal '{signalName}'");
                }
            }
        }

        //! ╔═══════════════════════╗
        //! ║ UTILITY AND DEBUG API ║
        //! ╚═══════════════════════╝

        public bool EventExists(string eventName)
        {
            lock (eventSystemLock)
            {
                return eventRegistry.ContainsKey(eventName) || legacyEvents.ContainsKey(eventName);
            }
        }

        public List<string> GetAllEventNames()
        {
            lock (eventSystemLock)
            {
                var allNames = new List<string>();
                allNames.AddRange(eventRegistry.Keys);
                allNames.AddRange(legacyEvents.Keys);
                return allNames;
            }
        }

        public List<string> GetEventsByCategory(string categoryName)
        {
            var category = eventCategories.FirstOrDefault(cat => cat.categoryName == categoryName);
            return category?.eventNames ?? new List<string>();
        }

        public void SetCategoryEnabled(string categoryName, bool enabled)
        {
            var category = eventCategories.FirstOrDefault(cat => cat.categoryName == categoryName);
            if (category != null)
            {
                category.isEnabled = enabled;
                if (debugMode)
                    Debug.Log($"Category '{categoryName}' {(enabled ? "enabled" : "disabled")}");
            }
        }

        [ContextMenu("Manual Test Event")]
        void ManualTestEvent()
        {
            if (!string.IsNullOrEmpty(manualTestEvent))
            {
                SendSignal(manualTestEvent);
            }
        }

        [ContextMenu("Clear Event History")]
        public void ClearEventHistory()
        {
            eventHistory.Clear();
            if (debugMode)
                Debug.Log("Event history cleared");
        }

        [ContextMenu("Clean Up Unused Events")]
        public void CleanUpUnusedEvents()
        {
            PerformAutomaticCleanup();
        }

        [ContextMenu("Print Event Statistics")]
        public void PrintEventStatistics()
        {
            lock (eventSystemLock)
            {
                Debug.Log("=== GAME EVENT MANAGER STATISTICS ===");
                Debug.Log($"Type-Safe Events: {eventRegistry.Count}");
                Debug.Log($"Legacy Events: {legacyEvents.Count}");
                Debug.Log($"Cached Events: {eventCache.Count}");
                Debug.Log($"Pooled Events: {eventPool.Count}");
                Debug.Log($"Event Categories: {eventCategories.Count}");
                Debug.Log($"History Entries: {eventHistory.Count}");
                Debug.Log($"Registered Signal Objects: {registeredSignalObjects.Count}");

                foreach (var debugInfo in eventDebugInfos)
                {
                    Debug.Log(
                        $"Event '{debugInfo.eventName}' ({debugInfo.eventType}): {debugInfo.listenerCount} listeners, triggered {debugInfo.triggerCount} times"
                    );
                }
            }
        }

        //! ╔═══════════════════╗
        //! ║ PRIVATE UTILITIES ║
        //! ╚═══════════════════╝

        private UnityEvent GetOrCreateLegacyEvent(string signalName)
        {
            if (enableEventCaching && eventCache.ContainsKey(signalName))
            {
                return eventCache[signalName];
            }

            if (legacyEvents.ContainsKey(signalName))
            {
                var existingEvent = legacyEvents[signalName];

                if (enableEventCaching && eventCache.Count < maxCachedEvents)
                {
                    eventCache[signalName] = existingEvent;
                }

                return existingEvent;
            }

            UnityEvent newEvent;
            if (enableEventPooling && eventPool.Count > 0)
            {
                newEvent = eventPool.Dequeue();
                newEvent.RemoveAllListeners();
            }
            else
            {
                newEvent = new UnityEvent();
            }

            legacyEvents.Add(signalName, newEvent);

            if (enableEventCaching && eventCache.Count < maxCachedEvents)
            {
                eventCache[signalName] = newEvent;
            }

            return newEvent;
        }

        private bool IsEventCategoryEnabled(string signalName)
        {
            var category = eventCategories.FirstOrDefault(cat =>
                cat.eventNames.Contains(signalName)
            );
            return category?.isEnabled ?? true;
        }

        private int GetLegacyListenerCount(string signalName)
        {
            if (legacyEvents.ContainsKey(signalName))
            {
                var eventField = typeof(UnityEvent).GetField(
                    "m_Calls",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                if (eventField != null)
                {
                    var calls = eventField.GetValue(legacyEvents[signalName]);
                    var countProperty = calls?.GetType().GetProperty("Count");
                    return (int)(countProperty?.GetValue(calls) ?? 0);
                }
            }
            return 0;
        }

        private void CreateOrUpdateDebugInfo(string eventName, BaseGameEvent gameEvent)
        {
            if (!debugInfoLookup.ContainsKey(eventName))
            {
                var debugInfo = new EventDebugInfo
                {
                    eventName = eventName,
                    eventType = gameEvent.GetType().Name,
                    listenerCount = gameEvent.ListenerCount,
                    lastTriggeredTime = 0f,
                    triggerCount = 0,
                    isActive = true,
                };

                debugInfoLookup[eventName] = debugInfo;
                eventDebugInfos.Add(debugInfo);
            }
            else
            {
                var debugInfo = debugInfoLookup[eventName];
                debugInfo.listenerCount = gameEvent.ListenerCount;
                debugInfo.eventType = gameEvent.GetType().Name;
            }
        }

        private void CreateOrUpdateLegacyDebugInfo(string signalName)
        {
            if (!debugInfoLookup.ContainsKey(signalName))
            {
                var debugInfo = new EventDebugInfo
                {
                    eventName = signalName,
                    eventType = "Legacy UnityEvent",
                    listenerCount = GetLegacyListenerCount(signalName),
                    lastTriggeredTime = 0f,
                    triggerCount = 0,
                    isActive = true,
                };

                debugInfoLookup[signalName] = debugInfo;
                eventDebugInfos.Add(debugInfo);
            }
            else
            {
                var debugInfo = debugInfoLookup[signalName];
                debugInfo.listenerCount = GetLegacyListenerCount(signalName);
            }
        }

        private void AddToEventHistory(string signalName)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            string historyEntry = $"[{timestamp}] {signalName}";

            eventHistory.Insert(0, historyEntry);

            while (eventHistory.Count > maxHistoryEntries)
            {
                eventHistory.RemoveAt(eventHistory.Count - 1);
            }
        }

        private void PerformAutomaticCleanup()
        {
            lock (eventSystemLock)
            {
                var eventsToRemove = new List<string>();

                foreach (var eventPair in legacyEvents)
                {
                    if (GetLegacyListenerCount(eventPair.Key) == 0)
                    {
                        eventsToRemove.Add(eventPair.Key);
                    }
                }

                foreach (var eventName in eventsToRemove)
                {
                    if (enableEventPooling)
                    {
                        eventPool.Enqueue(legacyEvents[eventName]);
                    }

                    legacyEvents.Remove(eventName);
                    eventCache.Remove(eventName);

                    if (debugInfoLookup.ContainsKey(eventName))
                    {
                        var debugInfo = debugInfoLookup[eventName];
                        debugInfoLookup.Remove(eventName);
                        eventDebugInfos.Remove(debugInfo);
                    }
                }

                if (debugMode && eventsToRemove.Count > 0)
                    Debug.Log(
                        $"GameEventManager: Automatically cleaned up {eventsToRemove.Count} unused events"
                    );
            }
        }
    }
}
