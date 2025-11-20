using System;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Interface for classes that can send signals through the GameEventManager.
    /// Provides a clean contract for implementing the observer pattern in game systems.
    /// </summary>
    public interface IHasSignals
    {
        /// <summary>
        /// Called during initialization to set up all signal subscriptions.
        /// Subscribe to events from GameEventManager here.
        /// </summary>
        void InitializeSignals();

        /// <summary>
        /// Called during cleanup to remove all signal subscriptions.
        /// Unsubscribe from events to prevent memory leaks.
        /// </summary>
        void CleanupSignals();
    }

    /// <summary>
    /// Abstract base class providing common functionality for classes that need to work with signals.
    /// Handles automatic signal lifecycle management.
    /// </summary>
    public abstract class HasSignalsBase : MonoBehaviour, IHasSignals
    {
        [Header("Signal Management")]
        [SerializeField]
        [Tooltip("Automatically initialize signals on Start")]
        private bool autoInitializeSignals = true;

        [SerializeField]
        [Tooltip("Automatically cleanup signals on Destroy")]
        private bool autoCleanupSignals = true;

        [SerializeField]
        [Tooltip("Log signal initialization and cleanup events")]
        private bool debugSignals = false;

        private bool signalsInitialized = false;

        protected virtual void Start()
        {
            if (autoInitializeSignals && !signalsInitialized)
            {
                InitializeSignals();
            }
        }

        protected virtual void OnDestroy()
        {
            if (autoCleanupSignals && signalsInitialized)
            {
                CleanupSignals();
            }
        }

        public virtual void InitializeSignals()
        {
            if (signalsInitialized)
            {
                if (debugSignals)
                    Debug.LogWarning($"{gameObject.name}: Signals already initialized", this);
                return;
            }

            if (debugSignals)
                Debug.Log($"{gameObject.name}: Initializing signals", this);

            OnInitializeSignals();
            signalsInitialized = true;
        }

        public virtual void CleanupSignals()
        {
            if (!signalsInitialized)
            {
                if (debugSignals)
                    Debug.LogWarning($"{gameObject.name}: Signals already cleaned up", this);
                return;
            }

            if (debugSignals)
                Debug.Log($"{gameObject.name}: Cleaning up signals", this);

            OnCleanupSignals();
            signalsInitialized = false;
        }

        /// <summary>
        /// Override this method to implement your signal subscriptions.
        /// Called automatically during InitializeSignals().
        /// </summary>
        protected abstract void OnInitializeSignals();

        /// <summary>
        /// Override this method to implement your signal cleanup.
        /// Called automatically during CleanupSignals().
        /// </summary>
        protected abstract void OnCleanupSignals();

        /// <summary>
        /// Helper method to subscribe to a signal with automatic cleanup tracking.
        /// </summary>
        protected void Subscribe<T>(GameEvent<T> gameEvent, Action<T> callback)
        {
            gameEvent.Subscribe(callback);
        }

        /// <summary>
        /// Helper method to subscribe to a parameterless signal with automatic cleanup tracking.
        /// </summary>
        protected void Subscribe(GameEvent gameEvent, Action callback)
        {
            gameEvent.Subscribe(callback);
        }

        /// <summary>
        /// Helper method to unsubscribe from a signal.
        /// </summary>
        protected void Unsubscribe<T>(GameEvent<T> gameEvent, Action<T> callback)
        {
            gameEvent.Unsubscribe(callback);
        }

        /// <summary>
        /// Helper method to unsubscribe from a parameterless signal.
        /// </summary>
        protected void Unsubscribe(GameEvent gameEvent, Action callback)
        {
            gameEvent.Unsubscribe(callback);
        }

        /// <summary>
        /// Helper method to raise a signal through the GameEventManager.
        /// </summary>
        protected void RaiseSignal<T>(GameEvent<T> gameEvent, T data)
        {
            gameEvent.Raise(data);
        }

        /// <summary>
        /// Helper method to raise a parameterless signal through the GameEventManager.
        /// </summary>
        protected void RaiseSignal(GameEvent gameEvent)
        {
            gameEvent.Raise();
        }

        public bool SignalsInitialized => signalsInitialized;
    }
}
