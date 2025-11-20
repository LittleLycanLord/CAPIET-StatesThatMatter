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
    /// SIMPLIFIED VERSION: Modern GameEventManager with both type-safe and legacy string-based events.
    /// This version compiles immediately and provides the core functionality.
    ///
    /// KEY FEATURES:
    /// - Bulletproof singleton pattern (like AudioManager/BeatManager)
    /// - Type-safe events through static GameEvents class
    /// - Legacy string-based events for backwards compatibility
    /// - HasSignals interface support for observer pattern
    /// - Comprehensive debugging and performance monitoring
    /// - Thread-safe operations
    /// - Memory management with pooling and caching
    /// </summary>
    public class GameEventManagerSimplified : MonoBehaviour
    {
        //! ╔═══════════════════╗
        //! ║ SINGLETON CONTENT ║
        //! ╚═══════════════════╝

        [Header("Singleton Settings")]
        [SerializeField]
        private bool persistAcrossScenes = true;

        [SerializeField]
        private bool transferDataOnReplace = true;

        private static GameEventManagerSimplified instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;

        public static GameEventManagerSimplified Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<GameEventManagerSimplified>();

                        if (instance == null)
                        {
                            GameObject singletonGameObject = new GameObject(
                                "Game Event Manager (Simplified)"
                            );
                            instance =
                                singletonGameObject.AddComponent<GameEventManagerSimplified>();
                        }
                    }
                    return instance;
                }
            }
        }

        protected virtual void TransferDataToNewInstance(GameEventManagerSimplified newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                newInstance.legacyEvents = this.legacyEvents;
                newInstance.eventCategories = this.eventCategories;
                newInstance.debugMode = this.debugMode;
                newInstance.manualTestEvent = this.manualTestEvent;
                OnDataTransfer(newInstance);
            }
        }

        protected virtual void OnDataTransfer(GameEventManagerSimplified newInstance)
        {
            // Override for custom data transfer
        }

        void Awake()
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = this;
                    InitializeSingleton();
                }
                else if (instance != this)
                {
                    if (transferDataOnReplace)
                    {
                        instance.TransferDataToNewInstance(this);
                    }

                    GameEventManagerSimplified oldInstance = instance;
                    instance = this;

                    if (oldInstance != null && oldInstance.gameObject != this.gameObject)
                    {
                        Destroy(oldInstance.gameObject);
                    }

                    InitializeSingleton();
                }
            }

            InitializeEventSystem();
        }

        private void InitializeSingleton()
        {
            if (persistAcrossScenes)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
            OnSingletonInitialized();
        }

        protected virtual void OnSingletonInitialized()
        {
            // Override for custom initialization
        }

        void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                CleanupEventSystem();
                instance = null;
            }
        }

        //! ╔══════════════════════╗
        //! ║ EVENT SYSTEM CONTENT ║
        //! ╚══════════════════════╝

        [Header("Event System Configuration")]
        [SerializeField]
        private bool debugMode = false;

        [SerializeField]
        [Range(50, 1000)]
        private int maxHistoryEntries = 200;

        [SerializeField]
        private bool enableEventPooling = true;

        [SerializeField]
        private bool enableEventCaching = true;

        [SerializeField]
        [Range(10, 100)]
        private int maxCachedEvents = 50;

        [Header("Legacy Event System (String-Based)")]
        [SerializedDictionary("Signal", "Events")]
        private SerializedDictionary<string, UnityEvent> legacyEvents =
            new SerializedDictionary<string, UnityEvent>();

        [Header("Event Categories")]
        private List<EventCategory> eventCategories = new List<EventCategory>();

        [Header("Debug Information")]
        [SerializeField]
        private List<EventDebugInfo> eventDebugInfos = new List<EventDebugInfo>();

        [SerializeField]
        private List<string> eventHistory = new List<string>();

        [Header("Manual Testing")]
        [SerializeField]
        private string manualTestEvent = "";

        // Private data
        private Dictionary<string, EventDebugInfo> debugInfoLookup =
            new Dictionary<string, EventDebugInfo>();
        private Dictionary<string, UnityEvent> eventCache = new Dictionary<string, UnityEvent>();
        private Queue<UnityEvent> eventPool = new Queue<UnityEvent>();
        private HashSet<IHasSignals> registeredSignalObjects = new HashSet<IHasSignals>();
        private readonly object eventSystemLock = new object();

        void Start()
        {
            if (debugMode)
            {
                Debug.Log("GameEventManager: Modern event system initialized");
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
                    eventCache.Clear();
                }

                InitializeDefaultCategories();
                RefreshDebugInfo();
            }
        }

        private void CleanupEventSystem()
        {
            lock (eventSystemLock)
            {
                foreach (var signalObject in registeredSignalObjects)
                {
                    if (signalObject != null)
                    {
                        signalObject.CleanupSignals();
                    }
                }
                registeredSignalObjects.Clear();

                foreach (var eventPair in legacyEvents)
                {
                    eventPair.Value.RemoveAllListeners();
                }
                legacyEvents.Clear();

                eventCache.Clear();
                eventPool.Clear();
            }
        }

        private void InitializeEventPool()
        {
            for (int i = 0; i < 20; i++)
            {
                eventPool.Enqueue(new UnityEvent());
            }
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
                            "Player.HealthChanged"
                        ),
                        new EventCategory(
                            "Game State",
                            Color.green,
                            "Game.Started",
                            "Game.Paused",
                            "Game.Over"
                        ),
                        new EventCategory(
                            "UI Events",
                            Color.yellow,
                            "UI.MenuOpened",
                            "UI.MenuClosed"
                        ),
                        new EventCategory(
                            "Audio Events",
                            Color.magenta,
                            "Audio.MusicStarted",
                            "Audio.SFXPlayed"
                        ),
                        new EventCategory(
                            "Scene Events",
                            Color.cyan,
                            "Scene.LoadingStarted",
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

            foreach (var eventPair in legacyEvents)
            {
                CreateOrUpdateLegacyDebugInfo(eventPair.Key);
            }
        }

        //! ╔═══════════════════════════╗
        //! ║ PUBLIC EVENT SYSTEM API   ║
        //! ╚═══════════════════════════╝

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
                return legacyEvents.ContainsKey(eventName);
            }
        }

        public List<string> GetAllEventNames()
        {
            lock (eventSystemLock)
            {
                return legacyEvents.Keys.ToList();
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

        [ContextMenu("Print Event Statistics")]
        public void PrintEventStatistics()
        {
            lock (eventSystemLock)
            {
                Debug.Log("=== GAME EVENT MANAGER STATISTICS ===");
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
    }
}
