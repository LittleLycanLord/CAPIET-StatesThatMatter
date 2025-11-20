using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AYellowpaper.SerializedCollections;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Data container for progression flags that can hold primitive types
    /// Supported types: int, long, float, bool, string, Vector2, Vector3, Quaternion
    /// </summary>
    [System.Serializable]
    public class ProgressionData
    {
        public string dataType;
        public string jsonData;
        public DateTime lastModified;

        public ProgressionData(object value)
        {
            var type = value.GetType();
            dataType = type.FullName;
            lastModified = DateTime.Now;

            // Handle each supported type specifically
            if (type == typeof(int))
            {
                jsonData = JsonUtility.ToJson(new IntWrapper { value = (int)value });
            }
            else if (type == typeof(long))
            {
                jsonData = JsonUtility.ToJson(new LongWrapper { value = (long)value });
            }
            else if (type == typeof(float))
            {
                jsonData = JsonUtility.ToJson(new FloatWrapper { value = (float)value });
            }
            else if (type == typeof(bool))
            {
                jsonData = JsonUtility.ToJson(new BoolWrapper { value = (bool)value });
            }
            else if (type == typeof(string))
            {
                jsonData = JsonUtility.ToJson(new StringWrapper { value = (string)value });
            }
            else if (type == typeof(Vector2))
            {
                jsonData = JsonUtility.ToJson(new Vector2Wrapper { value = (Vector2)value });
            }
            else if (type == typeof(Vector3))
            {
                jsonData = JsonUtility.ToJson(new Vector3Wrapper { value = (Vector3)value });
            }
            else if (type == typeof(Quaternion))
            {
                jsonData = JsonUtility.ToJson(new QuaternionWrapper { value = (Quaternion)value });
            }
            else
            {
                throw new System.ArgumentException(
                    $"Unsupported type for progression data: {type.Name}. Supported types: int, long, float, bool, string, Vector2, Vector3, Quaternion"
                );
            }
        }

        public T GetValue<T>()
        {
            try
            {
                var targetType = typeof(T);

                if (targetType == typeof(int))
                {
                    var wrapper = JsonUtility.FromJson<IntWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(long))
                {
                    var wrapper = JsonUtility.FromJson<LongWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(float))
                {
                    var wrapper = JsonUtility.FromJson<FloatWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(bool))
                {
                    var wrapper = JsonUtility.FromJson<BoolWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(string))
                {
                    var wrapper = JsonUtility.FromJson<StringWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(Vector2))
                {
                    var wrapper = JsonUtility.FromJson<Vector2Wrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(Vector3))
                {
                    var wrapper = JsonUtility.FromJson<Vector3Wrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else if (targetType == typeof(Quaternion))
                {
                    var wrapper = JsonUtility.FromJson<QuaternionWrapper>(jsonData);
                    return (T)(object)wrapper.value;
                }
                else
                {
                    Debug.LogError(
                        $"Unsupported type for GetValue: {targetType.Name}. Supported types: int, long, float, bool, string, Vector2, Vector3, Quaternion"
                    );
                    return default(T);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize progression data: {e.Message}");
                return default(T);
            }
        }

        // Wrapper classes for each supported type
        [System.Serializable]
        private class IntWrapper
        {
            public int value;
        }

        [System.Serializable]
        private class LongWrapper
        {
            public long value;
        }

        [System.Serializable]
        private class FloatWrapper
        {
            public float value;
        }

        [System.Serializable]
        private class BoolWrapper
        {
            public bool value;
        }

        [System.Serializable]
        private class StringWrapper
        {
            public string value;
        }

        [System.Serializable]
        private class Vector2Wrapper
        {
            public Vector2 value;
        }

        [System.Serializable]
        private class Vector3Wrapper
        {
            public Vector3 value;
        }

        [System.Serializable]
        private class QuaternionWrapper
        {
            public Quaternion value;
        }
    }

    /// <summary>
    /// Defines where save files should be located
    /// </summary>
    public enum SaveLocation
    {
        /// <summary>
        /// Relative to executable in builds, relative to Assets folder in editor
        /// </summary>
        RelativeToExecutable,

        /// <summary>
        /// Always use Application.persistentDataPath (platform-specific user data)
        /// </summary>
        PersistentDataPath,

        /// <summary>
        /// Always relative to Assets folder (for easy access during development)
        /// </summary>
        AssetsFolder,
    }

    public class GameProgressionManager : MonoBehaviour
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

        [SerializeField]
        [Tooltip("If true, automatically recreate singleton if destroyed during runtime")]
        private bool autoRecreateOnDestroy = true;

        //* Singleton Instance Management
        private static GameProgressionManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static GameProgressionManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<GameProgressionManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(GameProgressionManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(GameProgressionManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject(
                                "GameProgressionManager"
                            );
                            instance = singletonGameObject.AddComponent<GameProgressionManager>();
                        }
                    }
                    return instance;
                }
            }
        }

        public static bool HasInstance => instance != null && !isBeingDestroyed;

        //* Data Transfer Interface for Singleton Replacement
        protected virtual void TransferDataToNewInstance(GameProgressionManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Transfer progression-specific data
                newInstance.defaultSaveFileName = this.defaultSaveFileName;
                newInstance.saveDirectory = this.saveDirectory;
                newInstance.saveLocation = this.saveLocation;
                newInstance.appendTimestampToFileName = this.appendTimestampToFileName;
                newInstance.enableDebugLogging = this.enableDebugLogging;
                newInstance.progressionFlags = new SerializedDictionary<string, ProgressionData>(
                    this.progressionFlags
                );

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(GameProgressionManager newInstance)
        {
            //* Implement custom data transfer logic here
        }

        //! - - - - - - - - - - -

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Progression Data")]
        [SerializeField, Tooltip("Dictionary containing all progression flags and their data")]
        private SerializedDictionary<string, ProgressionData> progressionFlags =
            new SerializedDictionary<string, ProgressionData>();

        [Header("Save/Load Settings")]
        [SerializeField, Tooltip("Default file name for progression saves")]
        private string defaultSaveFileName = "GameProgression.json";

        [SerializeField, Tooltip("Save files directory relative to chosen location")]
        private string saveDirectory = "Saves";

        [SerializeField]
        [Tooltip("Choose save location: relative to exe in builds, or Assets folder in editor")]
        private SaveLocation saveLocation = SaveLocation.RelativeToExecutable;

        [SerializeField]
        [Tooltip(
            "Append date and time to save file name (e.g., GameProgression_2025-08-21_14-30-45.json)"
        )]
        private bool appendTimestampToFileName = false;

        [SerializeField, Tooltip("Enable debug logging for progression operations")]
        private bool enableDebugLogging = true;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        /// <summary>
        /// Full path to the save directory
        /// </summary>
        private string SaveDirectoryPath
        {
            get
            {
                string basePath;

                switch (saveLocation)
                {
                    case SaveLocation.RelativeToExecutable:
                        // In editor: relative to Assets folder, In build: relative to executable
                        if (Application.isEditor)
                        {
                            basePath = Path.Combine(Application.dataPath, "..", "GameData");
                        }
                        else
                        {
                            basePath = Path.GetDirectoryName(Application.dataPath);
                        }
                        break;

                    case SaveLocation.PersistentDataPath:
                        basePath = Application.persistentDataPath;
                        break;

                    case SaveLocation.AssetsFolder:
                        basePath = Path.Combine(Application.dataPath, "..", "GameData");
                        break;

                    default:
                        basePath = Application.persistentDataPath;
                        break;
                }

                return Path.Combine(basePath, saveDirectory);
            }
        }

        /// <summary>
        /// Full path to the default save file
        /// </summary>
        private string DefaultSaveFilePath
        {
            get
            {
                string fileName = defaultSaveFileName;

                if (appendTimestampToFileName)
                {
                    // Extract name and extension
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);

                    // Create timestamp string
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                    // Combine: name_timestamp.extension
                    fileName = $"{nameWithoutExt}_{timestamp}{extension}";
                }

                return Path.Combine(SaveDirectoryPath, fileName);
            }
        }

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Reset the quitting flag when a new instance is created
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
                    GameProgressionManager oldInstance = instance;
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
            // Ensure save directory exists
            if (!Directory.Exists(SaveDirectoryPath))
            {
                Directory.CreateDirectory(SaveDirectoryPath);
                if (enableDebugLogging)
                    Debug.Log($"Created save directory: {SaveDirectoryPath}");
            }
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

        void Start()
        {
            // Try to load existing progression on start
            if (File.Exists(DefaultSaveFilePath))
            {
                LoadAllProgression();
            }
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        /// <summary>
        /// Save a progression flag with the given key and value
        /// </summary>
        /// <param name="key">Unique identifier for the progression flag</param>
        /// <param name="value">Value to save (primitives, Unity objects, etc.)</param>
        public void SaveProgression(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("SaveProgression: Key cannot be null or empty");
                return;
            }

            if (value == null)
            {
                Debug.LogError($"SaveProgression: Value for key '{key}' cannot be null");
                return;
            }

            try
            {
                progressionFlags[key] = new ProgressionData(value);

                if (enableDebugLogging)
                    Debug.Log($"Saved progression flag: {key} = {value}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save progression flag '{key}': {e.Message}");
            }
        }

        /// <summary>
        /// Get a progression flag value by key
        /// </summary>
        /// <typeparam name="T">Type to cast the value to</typeparam>
        /// <param name="key">Key of the progression flag</param>
        /// <returns>The value cast to type T, or default(T) if not found</returns>
        public T GetProgression<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("GetProgression: Key cannot be null or empty");
                return default(T);
            }

            if (!progressionFlags.ContainsKey(key))
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"Progression flag '{key}' not found");
                return default(T);
            }

            try
            {
                return progressionFlags[key].GetValue<T>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get progression flag '{key}': {e.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Check if a progression flag exists
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the flag exists</returns>
        public bool HasProgression(string key)
        {
            return !string.IsNullOrEmpty(key) && progressionFlags.ContainsKey(key);
        }

        /// <summary>
        /// Remove a progression flag
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>True if the flag was removed</returns>
        public bool RemoveProgression(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            bool removed = progressionFlags.Remove(key);
            if (removed && enableDebugLogging)
                Debug.Log($"Removed progression flag: {key}");

            return removed;
        }

        /// <summary>
        /// Clear all progression flags
        /// </summary>
        public void ClearAllProgression()
        {
            progressionFlags.Clear();
            if (enableDebugLogging)
                Debug.Log("Cleared all progression flags");
        }

        /// <summary>
        /// Save all progression flags to the default JSON file
        /// </summary>
        [ContextMenu("Save All Progression")]
        public bool SaveAllProgression()
        {
            return SaveAllProgression(DefaultSaveFilePath);
        }

        /// <summary>
        /// Save all progression flags to a specific JSON file
        /// </summary>
        /// <param name="filePath">Path to save the file</param>
        public bool SaveAllProgression(string filePath)
        {
            try
            {
                // Convert to a regular dictionary for JSON serialization
                var dataToSave = new Dictionary<string, ProgressionData>(progressionFlags);
                string json = JsonUtility.ToJson(
                    new SerializableDictionaryWrapper(dataToSave),
                    true
                );

                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, json);

                if (enableDebugLogging)
                    Debug.Log($"Saved {progressionFlags.Count} progression flags to: {filePath}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save progression to '{filePath}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load all progression flags from the default JSON file
        /// </summary>
        [ContextMenu("Load All Progression")]
        public bool LoadAllProgression()
        {
            return LoadAllProgression(DefaultSaveFilePath);
        }

        /// <summary>
        /// Load all progression flags from a specific JSON file
        /// </summary>
        /// <param name="filePath">Path to load the file from</param>
        public bool LoadAllProgression(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    if (enableDebugLogging)
                        Debug.LogWarning($"Progression file not found: {filePath}");
                    return false;
                }

                string json = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<SerializableDictionaryWrapper>(json);

                progressionFlags.Clear();
                foreach (var kvp in wrapper.data)
                {
                    progressionFlags[kvp.key] = kvp.value;
                }

                if (enableDebugLogging)
                    Debug.Log(
                        $"Loaded {progressionFlags.Count} progression flags from: {filePath}"
                    );

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load progression from '{filePath}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Open the save directory in the system file explorer
        /// </summary>
        [ContextMenu("📁 Open Save Directory")]
        public void OpenSaveDirectory()
        {
            string directoryPath = SaveDirectoryPath;

            // Ensure directory exists
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    Debug.Log($"Created save directory: {directoryPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create save directory: {e.Message}");
                    return;
                }
            }

            try
            {
                // Platform-specific file explorer opening
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start("explorer.exe", directoryPath.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                System.Diagnostics.Process.Start("open", directoryPath);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                System.Diagnostics.Process.Start("xdg-open", directoryPath);
#else
                Debug.Log($"Save directory: {directoryPath}");
#endif
                Debug.Log($"📁 Opened save directory: {directoryPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open save directory: {e.Message}");
                Debug.Log($"Save directory path: {directoryPath}");
            }
        }

        /// <summary>
        /// Show save file information in the console
        /// </summary>
        [ContextMenu("🔍 Show Save File Info")]
        public void ShowSaveFileInfo()
        {
            string filePath = DefaultSaveFilePath;
            string directoryPath = SaveDirectoryPath;

            Debug.Log("=== Save File Information ===");
            Debug.Log($"Save Location Mode: {saveLocation}");
            Debug.Log($"Save Directory: {directoryPath}");
            Debug.Log($"Save File: {filePath}");
            Debug.Log($"File Exists: {File.Exists(filePath)}");

            if (File.Exists(filePath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    Debug.Log($"File Size: {fileInfo.Length} bytes");
                    Debug.Log($"Last Modified: {fileInfo.LastWriteTime}");
                    Debug.Log($"Created: {fileInfo.CreationTime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to get file info: {e.Message}");
                }
            }

            Debug.Log($"Total Progression Flags: {GetProgressionCount()}");
        }

        /// <summary>
        /// Wrapper class for JSON serialization of dictionary
        /// </summary>
        [System.Serializable]
        private class SerializableDictionaryWrapper
        {
            public List<ProgressionKeyValuePair> data = new List<ProgressionKeyValuePair>();

            public SerializableDictionaryWrapper(Dictionary<string, ProgressionData> dict)
            {
                foreach (var kvp in dict)
                {
                    data.Add(new ProgressionKeyValuePair { key = kvp.Key, value = kvp.Value });
                }
            }
        }

        [System.Serializable]
        private class ProgressionKeyValuePair
        {
            public string key;
            public ProgressionData value;
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
                    $"[{nameof(GameProgressionManager)}] Application paused (entering/exiting play mode)"
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

        private IEnumerator RecreateInstanceNextFrame()
        {
            yield return null; // Wait one frame

            lock (instanceLock)
            {
                if (instance == null && !applicationIsQuitting)
                {
                    Debug.LogWarning(
                        $"[{nameof(GameProgressionManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "GameProgressionManager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<GameProgressionManager>();
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
                Debug.Log($"[{nameof(GameProgressionManager)}] Singleton state reset");
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

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        /// <summary>
        /// Get all progression flag keys
        /// </summary>
        /// <returns>Collection of all progression keys</returns>
        public IEnumerable<string> GetAllProgressionKeys()
        {
            return progressionFlags.Keys;
        }

        /// <summary>
        /// Get the count of progression flags
        /// </summary>
        /// <returns>Number of progression flags</returns>
        public int GetProgressionCount()
        {
            return progressionFlags.Count;
        }

        /// <summary>
        /// Log all current progression flags for debugging
        /// </summary>
        public void LogAllProgression()
        {
            Debug.Log($"=== Progression Flags ({progressionFlags.Count}) ===");
            foreach (var kvp in progressionFlags)
            {
                Debug.Log(
                    $"  {kvp.Key}: {kvp.Value.dataType} (Modified: {kvp.Value.lastModified})"
                );
            }
        }
    }
}
