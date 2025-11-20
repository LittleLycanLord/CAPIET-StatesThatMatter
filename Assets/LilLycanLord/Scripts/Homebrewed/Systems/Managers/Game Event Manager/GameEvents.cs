using System;
using System.Collections.Generic;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Base class for all game events. Provides common functionality for the event system.
    /// </summary>
    [System.Serializable]
    public abstract class BaseGameEvent
    {
        [SerializeField]
        protected string eventName;

        [SerializeField]
        protected string description;

        [SerializeField]
        protected bool debugMode = false;

        public string EventName => eventName;
        public string Description => description;
        public abstract int ListenerCount { get; }
        public abstract void ClearAllListeners();

        protected BaseGameEvent(string name, string desc = "")
        {
            eventName = name;
            description = desc;
        }
    }

    /// <summary>
    /// Generic game event that can carry data of type T.
    /// Thread-safe and provides comprehensive debugging support.
    /// </summary>
    [System.Serializable]
    public class GameEvent<T> : BaseGameEvent
    {
        private event Action<T> OnEventRaised;
        private readonly object lockObject = new object();

        [SerializeField]
        private int triggerCount = 0;

        [SerializeField]
        private float lastTriggeredTime = 0f;

        public override int ListenerCount
        {
            get
            {
                lock (lockObject)
                {
                    return OnEventRaised?.GetInvocationList().Length ?? 0;
                }
            }
        }

        public int TriggerCount => triggerCount;
        public float LastTriggeredTime => lastTriggeredTime;

        public GameEvent(string name, string description = "")
            : base(name, description) { }

        /// <summary>
        /// Subscribe a callback to this event.
        /// </summary>
        public void Subscribe(Action<T> callback)
        {
            if (callback == null)
            {
                Debug.LogWarning(
                    $"GameEvent<{typeof(T).Name}> '{eventName}': Attempted to subscribe null callback"
                );
                return;
            }

            lock (lockObject)
            {
                OnEventRaised += callback;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent<{typeof(T).Name}> '{eventName}': Subscribed {callback.Method.Name} (Total listeners: {ListenerCount})"
                    );
                }
            }
        }

        /// <summary>
        /// Unsubscribe a callback from this event.
        /// </summary>
        public void Unsubscribe(Action<T> callback)
        {
            if (callback == null)
            {
                Debug.LogWarning(
                    $"GameEvent<{typeof(T).Name}> '{eventName}': Attempted to unsubscribe null callback"
                );
                return;
            }

            lock (lockObject)
            {
                OnEventRaised -= callback;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent<{typeof(T).Name}> '{eventName}': Unsubscribed {callback.Method.Name} (Total listeners: {ListenerCount})"
                    );
                }
            }
        }

        /// <summary>
        /// Raise the event with the provided data.
        /// </summary>
        public void Raise(T data)
        {
            lock (lockObject)
            {
                triggerCount++;
                lastTriggeredTime = Time.time;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent<{typeof(T).Name}> '{eventName}': Raised with data '{data}' (Listeners: {ListenerCount}, Trigger count: {triggerCount})"
                    );
                }

                try
                {
                    OnEventRaised?.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"GameEvent<{typeof(T).Name}> '{eventName}': Exception during event invocation: {e}"
                    );
                }
            }
        }

        public override void ClearAllListeners()
        {
            lock (lockObject)
            {
                int listenerCount = ListenerCount;
                OnEventRaised = null;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent<{typeof(T).Name}> '{eventName}': Cleared all {listenerCount} listeners"
                    );
                }
            }
        }

        /// <summary>
        /// Get a list of all listener method names (for debugging).
        /// </summary>
        public List<string> GetListenerNames()
        {
            lock (lockObject)
            {
                var names = new List<string>();
                if (OnEventRaised != null)
                {
                    foreach (var del in OnEventRaised.GetInvocationList())
                    {
                        names.Add($"{del.Target?.GetType().Name}.{del.Method.Name}");
                    }
                }
                return names;
            }
        }
    }

    /// <summary>
    /// Game event without parameters. Optimized for simple notifications.
    /// </summary>
    [System.Serializable]
    public class GameEvent : BaseGameEvent
    {
        private event Action OnEventRaised;
        private readonly object lockObject = new object();

        [SerializeField]
        private int triggerCount = 0;

        [SerializeField]
        private float lastTriggeredTime = 0f;

        public override int ListenerCount
        {
            get
            {
                lock (lockObject)
                {
                    return OnEventRaised?.GetInvocationList().Length ?? 0;
                }
            }
        }

        public int TriggerCount => triggerCount;
        public float LastTriggeredTime => lastTriggeredTime;

        public GameEvent(string name, string description = "")
            : base(name, description) { }

        /// <summary>
        /// Subscribe a callback to this event.
        /// </summary>
        public void Subscribe(Action callback)
        {
            if (callback == null)
            {
                Debug.LogWarning($"GameEvent '{eventName}': Attempted to subscribe null callback");
                return;
            }

            lock (lockObject)
            {
                OnEventRaised += callback;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent '{eventName}': Subscribed {callback.Method.Name} (Total listeners: {ListenerCount})"
                    );
                }
            }
        }

        /// <summary>
        /// Unsubscribe a callback from this event.
        /// </summary>
        public void Unsubscribe(Action callback)
        {
            if (callback == null)
            {
                Debug.LogWarning(
                    $"GameEvent '{eventName}': Attempted to unsubscribe null callback"
                );
                return;
            }

            lock (lockObject)
            {
                OnEventRaised -= callback;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent '{eventName}': Unsubscribed {callback.Method.Name} (Total listeners: {ListenerCount})"
                    );
                }
            }
        }

        /// <summary>
        /// Raise the event.
        /// </summary>
        public void Raise()
        {
            lock (lockObject)
            {
                triggerCount++;
                lastTriggeredTime = Time.time;

                if (debugMode)
                {
                    Debug.Log(
                        $"GameEvent '{eventName}': Raised (Listeners: {ListenerCount}, Trigger count: {triggerCount})"
                    );
                }

                try
                {
                    OnEventRaised?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"GameEvent '{eventName}': Exception during event invocation: {e}"
                    );
                }
            }
        }

        public override void ClearAllListeners()
        {
            lock (lockObject)
            {
                int listenerCount = ListenerCount;
                OnEventRaised = null;

                if (debugMode)
                {
                    Debug.Log($"GameEvent '{eventName}': Cleared all {listenerCount} listeners");
                }
            }
        }

        /// <summary>
        /// Get a list of all listener method names (for debugging).
        /// </summary>
        public List<string> GetListenerNames()
        {
            lock (lockObject)
            {
                var names = new List<string>();
                if (OnEventRaised != null)
                {
                    foreach (var del in OnEventRaised.GetInvocationList())
                    {
                        names.Add($"{del.Target?.GetType().Name}.{del.Method.Name}");
                    }
                }
                return names;
            }
        }
    }
}
