using System;
using System.Collections.Generic;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    [Serializable]
    public class DebugOption
    {
        public string name = "New Debug Option";
        public string text = "New Debug Option";
        public bool enabled = true;
        public Texture texture;
        public string description = "";
        public UnityEvent triggers = new UnityEvent();
    }

    public class DebuggingManager : MonoBehaviour
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
        private static DebuggingManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static DebuggingManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<DebuggingManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(DebuggingManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(DebuggingManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Debugging Manager");
                            instance = singletonGameObject.AddComponent<DebuggingManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(DebuggingManager)}] Found existing singleton instance in scene."
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
        protected virtual void TransferDataToNewInstance(DebuggingManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.debugOptions = this.debugOptions;
                newInstance.debugMode = this.debugMode;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(DebuggingManager newInstance)
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
                    DebuggingManager oldInstance = instance;
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
                Debug.Log(
                    $"[{nameof(DebuggingManager)}] Application paused (entering/exiting play mode)"
                );
                // Don't set applicationIsQuitting here in editor
            }
        }

        void OnDestroy()
        {
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
                        $"[{nameof(DebuggingManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Debugging Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<DebuggingManager>();
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
                Debug.Log($"[{nameof(DebuggingManager)}] Singleton state reset");
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

        //! - - - - - - - - - - -

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Displays")]
        public List<DebugOption> debugOptions = new List<DebugOption>();

        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        bool debugMode = false;

        [Space(10)]
        [Header("Appearance")]
        [SerializeField]
        float originalLeftOffset = 20;

        [SerializeField]
        float originalTopOffset = 20;

        [SerializeField]
        float originalWidth = 150;

        [SerializeField]
        float originalHeight = 30;

        [SerializeField]
        float originalGap = 10;

        [SerializeField]
        float optionsPerColumn = 5;

        float leftOffset = 20;
        float topOffset = 20;
        float width = 150;
        float height = 30;
        float gap = 10;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Start()
        {
            Reset();
        }

        void OnGUI()
        {
            if (!debugMode)
                return;

            int currentRow = 0;
            int currentColumn = 0;
            foreach (DebugOption debugOption in debugOptions)
            {
                if (debugOption.enabled)
                    if (
                        GUI.Button(
                            new Rect(
                                leftOffset + (currentColumn * width) + (gap * currentColumn),
                                topOffset + (currentRow * height) + (gap * currentRow),
                                width,
                                height
                            ),
                            new GUIContent(
                                debugOption.text,
                                debugOption.texture,
                                debugOption.description
                            )
                        )
                    )
                    {
                        debugOption.triggers?.Invoke();
                    }

                currentRow++;
                if (currentRow % optionsPerColumn == 0)
                {
                    currentRow = 0;
                    currentColumn++;
                }
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        [ContextMenu("Reset Debug Options")]
        public void Reset()
        {
            debugOptions.Clear();
            leftOffset = originalLeftOffset;
            topOffset = originalTopOffset;
            width = originalWidth;
            height = originalHeight;
            gap = originalGap;
        }

        [ContextMenu("Test Layout")]
        public void TestLayout()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("You can only preview the layout while the game is running");
                return;
            }

            debugMode = true;
            UnityEvent sampleEvent = new UnityEvent();
            sampleEvent.AddListener(FunctionWithNoParameters);
            Reset();

            for (int row = 0; row < optionsPerColumn; row++)
            {
                for (int column = 0; column < optionsPerColumn; column++)
                {
                    AddDebugOption(
                        "Sample #" + debugOptions.Count,
                        "Sample (" + row + ", " + column + ")",
                        (row + column) % 2 == 0,
                        "This is Sample #" + debugOptions.Count,
                        sampleEvent
                    );
                }
            }
        }

        public void AddDebugOption(string name, string text, UnityEvent triggers)
        {
            if (CheckIfDebugOptionExists(name))
            {
                Debug.Log("A Debug Option named \"" + name + "\" already exists");
                return;
            }
            DebugOption newDebugOption = new DebugOption();
            newDebugOption.name = name;
            newDebugOption.text = text;
            newDebugOption.enabled = true;
            newDebugOption.description = "";
            newDebugOption.triggers = triggers;
            debugOptions.Add(newDebugOption);
        }

        public void AddDebugOption(string name, string text, bool enabled, UnityEvent triggers)
        {
            if (CheckIfDebugOptionExists(name))
            {
                Debug.Log("A Debug Option named \"" + name + "\" already exists");
                return;
            }
            DebugOption newDebugOption = new DebugOption();
            newDebugOption.name = name;
            newDebugOption.text = text;
            newDebugOption.enabled = enabled;
            newDebugOption.description = "";
            newDebugOption.triggers = triggers;
            debugOptions.Add(newDebugOption);
        }

        public void AddDebugOption(
            string name,
            string text,
            string description,
            UnityEvent triggers
        )
        {
            if (CheckIfDebugOptionExists(name))
            {
                Debug.Log("A Debug Option named \"" + name + "\" already exists");
                return;
            }
            DebugOption newDebugOption = new DebugOption();
            newDebugOption.name = name;
            newDebugOption.text = text;
            newDebugOption.enabled = true;
            newDebugOption.description = description;
            newDebugOption.triggers = triggers;
            debugOptions.Add(newDebugOption);
        }

        public void AddDebugOption(
            string name,
            string text,
            bool enabled,
            string description,
            UnityEvent triggers
        )
        {
            if (CheckIfDebugOptionExists(name))
            {
                Debug.Log("A Debug Option named \"" + name + "\" already exists");
                return;
            }
            DebugOption newDebugOption = new DebugOption();
            newDebugOption.name = name;
            newDebugOption.text = text;
            newDebugOption.enabled = enabled;
            newDebugOption.description = description;
            newDebugOption.triggers = triggers;
            debugOptions.Add(newDebugOption);
        }

        void FunctionWithNoParameters()
        {
            Debug.Log("I'm a Sample!");
        }

        public DebugOption GetDebugOption(string name)
        {
            return debugOptions.Find(debugOption => debugOption.name == name);
        }

        public void RemoveDebugOption(string name)
        {
            if (!CheckIfDebugOptionExists(name))
            {
                Debug.Log("There is no Debug Option named \"" + name + "\" to remove");
                return;
            }
            debugOptions.Remove(debugOptions.Find(debugOption => debugOption.name == name));
        }

        bool CheckIfDebugOptionExists(string name)
        {
            return GetDebugOption(name) != null;
        }

        public void SetLeftOffset(float newLeftOffset)
        {
            if (leftOffset < newLeftOffset)
                leftOffset = newLeftOffset;
        }

        public void SetTopOffset(float newTopOffset)
        {
            if (topOffset < newTopOffset)
                topOffset = newTopOffset;
        }

        public void SetWidth(float newWidth)
        {
            if (width < newWidth)
                width = newWidth;
        }

        public void SetHeight(float newHeight)
        {
            if (height < newHeight)
                height = newHeight;
        }

        public void SetGap(float newGap)
        {
            if (gap < newGap)
                gap = newGap;
        }

        public void ToggleSceneInteraction() { }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
