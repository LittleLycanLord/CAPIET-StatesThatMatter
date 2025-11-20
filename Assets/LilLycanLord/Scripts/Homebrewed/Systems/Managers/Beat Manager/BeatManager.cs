using System;
using System.Collections;
using System.Collections.Generic;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    [Serializable]
    public class Rhythm
    {
        [Header("Rhythm Configuration")]
        public string name = "";

        [Tooltip("BPM for this rhythm (overrides sound BPM if > 0)")]
        public float BPM = 120f;

        [Tooltip("Beat subdivisions - 1=quarter notes, 2=eighth notes, 4=sixteenth notes")]
        [Range(0.25f, 4f)]
        public float steps = 1f;

        [Header("Events")]
        public UnityEvent triggers = new UnityEvent();

        [Header("Runtime Data - Debug Only")]
        [SerializeField]
        bool isActive = false;

        [SerializeField]
        float nextBeatTime = 0f;

        [SerializeField]
        int beatCount = 0;

        // Internal state
        public bool removeNextInterval = false;
        Coroutine beatCoroutine;

        public float GetBeatInterval()
        {
            return 60f / (BPM * steps);
        }

        public void SetActive(bool active)
        {
            if (isActive == active)
                return;

            isActive = active;

            if (isActive)
            {
                StartBeatCoroutine();
            }
            else
            {
                StopBeatCoroutine();
            }
        }

        public bool IsActive => isActive;

        void StartBeatCoroutine()
        {
            if (beatCoroutine == null && BeatManager.Instance != null)
            {
                nextBeatTime = Time.time;
                beatCount = 0;
                beatCoroutine = BeatManager.Instance.StartCoroutine(BeatCoroutine());
            }
        }

        void StopBeatCoroutine()
        {
            if (beatCoroutine != null && BeatManager.Instance != null)
            {
                BeatManager.Instance.StopCoroutine(beatCoroutine);
                beatCoroutine = null;
            }
        }

        IEnumerator BeatCoroutine()
        {
            while (isActive && !removeNextInterval)
            {
                float currentTime = Time.time;

                if (currentTime >= nextBeatTime)
                {
                    ExecuteBeat();
                    nextBeatTime += GetBeatInterval();
                    beatCount++;
                }

                yield return null; // Check every frame for precision
            }

            beatCoroutine = null;
        }

        public void ExecuteBeat()
        {
            triggers?.Invoke();
        }

        public void Reset()
        {
            SetActive(false);
            name = "";
            BPM = 120f;
            steps = 1f;
            triggers.RemoveAllListeners();
            removeNextInterval = false;
            beatCount = 0;
            nextBeatTime = 0f;
        }

        public void SyncToCurrentTime()
        {
            nextBeatTime = Time.time;
            beatCount = 0;
        }
    }

    [Serializable]
    public class AutoRhythm : Rhythm
    {
        [Header("Auto Sync Configuration")]
        [Tooltip("Sound to automatically sync to")]
        public string soundName = "";

        [Tooltip("Use sound's BPM instead of manual BPM")]
        public bool useSoundBPM = true;

        [Header("Runtime Tracking - Debug Only")]
        [SerializeField]
        bool soundIsPlaying = false;

        [SerializeField]
        List<AudioSource> trackedAudioSources = new List<AudioSource>();

        public void UpdateTracking()
        {
            if (string.IsNullOrEmpty(soundName) || AudioManager.Instance == null)
                return;

            // Get current audio sources playing this sound
            List<AudioSource> currentSources = AudioManager.Instance.GetAudioSourcesPlayingSound(
                soundName
            );
            bool shouldBeActive = currentSources.Count > 0;

            // Update tracked sources
            trackedAudioSources = currentSources;
            soundIsPlaying = shouldBeActive;

            // Update BPM from sound if needed
            if (
                useSoundBPM
                && shouldBeActive
                && AudioManager.Instance.GetAllSoundNames().Contains(soundName)
            )
            {
                // Try to get BPM from sound library
                foreach (var sound in AudioManager.Instance.GetAllSoundNames())
                {
                    if (sound == soundName)
                    {
                        // Note: You'd need to expose a way to get Sound objects from AudioManager
                        // For now, we'll keep the manually set BPM
                        break;
                    }
                }
            }

            // Activate/deactivate based on sound playing state
            if (shouldBeActive && !IsActive)
            {
                SyncToCurrentTime(); // Sync to current time when starting
                SetActive(true);
            }
            else if (!shouldBeActive && IsActive)
            {
                SetActive(false);
            }
        }

        public new void Reset()
        {
            base.Reset();
            soundName = "";
            useSoundBPM = true;
            soundIsPlaying = false;
            trackedAudioSources.Clear();
        }
    }

    [DefaultExecutionOrder(-99)] // Initializes after AudioManager initializes
    public class BeatManager : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Debug Info - Runtime Only")]
        [SerializeField]
        int activeRhythmsCount = 0;

        [SerializeField]
        int activeAutoRhythmsCount = 0;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Beat Configuration")]
        [Tooltip("Default BPM when no specific BPM is provided")]
        [Range(60f, 200f)]
        public float defaultBPM = 120f;

        [Header("Rhythms")]
        [Tooltip("Manual rhythms - always active when enabled")]
        public List<Rhythm> manualRhythms = new List<Rhythm>();

        [Tooltip("Auto rhythms - sync automatically to playing sounds")]
        public List<AutoRhythm> autoRhythms = new List<AutoRhythm>();

        [Header("Performance")]
        [Tooltip("How often to check for sound changes (in seconds)")]
        [Range(0.1f, 1f)]
        public float autoRhythmUpdateRate = 0.1f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

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
        private static BeatManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static BeatManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<BeatManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(BeatManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(BeatManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Beat Manager");
                            instance = singletonGameObject.AddComponent<BeatManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(BeatManager)}] Found existing singleton instance in scene."
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
        protected virtual void TransferDataToNewInstance(BeatManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.defaultBPM = this.defaultBPM;
                newInstance.manualRhythms = this.manualRhythms;
                newInstance.autoRhythms = this.autoRhythms;
                newInstance.autoRhythmUpdateRate = this.autoRhythmUpdateRate;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(BeatManager newInstance)
        {
            //* Implement custom data transfer logic here
        }

        //! - - - - - - - - - - -

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

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
                    BeatManager oldInstance = instance;
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
            InitializeBeatManager();
        }

        void OnApplicationQuit()
        {
            lock (instanceLock)
            {
                applicationIsQuitting = true;
                isBeingDestroyed = true;
            }
        }

        void OnDestroy()
        {
            StopAllRhythms();
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

        void OnApplicationPause(bool pauseStatus)
        {
            // In editor, this is called when entering/exiting play mode
            if (pauseStatus && Application.isEditor)
            {
                Debug.Log(
                    $"[{nameof(BeatManager)}] Application paused (entering/exiting play mode)"
                );
                // Don't set applicationIsQuitting here in editor
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
                        $"[{nameof(BeatManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Beat Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<BeatManager>();
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
                Debug.Log($"[{nameof(BeatManager)}] Singleton state reset");
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

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        private void InitializeBeatManager()
        {
            StartCoroutine(AutoRhythmUpdateLoop());
            Debug.Log("BeatManager: Initialized and ready");
        }

        private IEnumerator AutoRhythmUpdateLoop()
        {
            while (true)
            {
                UpdateAutoRhythms();
                UpdateDebugInfo();
                yield return new WaitForSeconds(autoRhythmUpdateRate);
            }
        }

        private void UpdateAutoRhythms()
        {
            foreach (AutoRhythm autoRhythm in autoRhythms)
            {
                if (autoRhythm != null && !autoRhythm.removeNextInterval)
                {
                    autoRhythm.UpdateTracking();
                }
            }

            // Clean up removed rhythms
            autoRhythms.RemoveAll(r => r.removeNextInterval);
        }

        private void UpdateDebugInfo()
        {
            activeRhythmsCount = 0;
            activeAutoRhythmsCount = 0;

            foreach (var rhythm in manualRhythms)
                if (rhythm != null && rhythm.IsActive)
                    activeRhythmsCount++;

            foreach (var autoRhythm in autoRhythms)
                if (autoRhythm != null && autoRhythm.IsActive)
                    activeAutoRhythmsCount++;
        }

        /// <summary>
        /// Creates a manual rhythm that runs at the specified BPM
        /// </summary>
        public Rhythm CreateManualRhythm(
            string rhythmName,
            float bpm,
            UnityAction beatAction,
            float steps = 1f
        )
        {
            Rhythm rhythm = new Rhythm();
            rhythm.name = rhythmName;
            rhythm.BPM = bpm > 0 ? bpm : defaultBPM;
            rhythm.steps = steps;
            rhythm.triggers.AddListener(beatAction);

            manualRhythms.Add(rhythm);
            Debug.Log($"BeatManager: Created manual rhythm '{rhythmName}' at {rhythm.BPM} BPM");
            return rhythm;
        }

        /// <summary>
        /// Creates an auto rhythm that syncs to a specific sound
        /// </summary>
        public AutoRhythm CreateAutoRhythm(
            string rhythmName,
            string soundName,
            UnityAction beatAction,
            float customBPM = -1,
            float steps = 1f
        )
        {
            AutoRhythm autoRhythm = new AutoRhythm();
            autoRhythm.name = rhythmName;
            autoRhythm.soundName = soundName;
            autoRhythm.useSoundBPM = customBPM <= 0;
            autoRhythm.BPM = customBPM > 0 ? customBPM : defaultBPM;
            autoRhythm.steps = steps;
            autoRhythm.triggers.AddListener(beatAction);

            autoRhythms.Add(autoRhythm);
            Debug.Log(
                $"BeatManager: Created auto rhythm '{rhythmName}' synced to sound '{soundName}'"
            );
            return autoRhythm;
        }

        /// <summary>
        /// Starts a manual rhythm
        /// </summary>
        public void StartRhythm(string rhythmName)
        {
            Rhythm rhythm = manualRhythms.Find(r => r.name == rhythmName);
            if (rhythm != null)
            {
                rhythm.SetActive(true);
                Debug.Log($"BeatManager: Started rhythm '{rhythmName}'");
            }
            else
            {
                Debug.LogWarning($"BeatManager: Rhythm '{rhythmName}' not found");
            }
        }

        /// <summary>
        /// Stops a rhythm (manual or auto)
        /// </summary>
        public void StopRhythm(string rhythmName)
        {
            // Check manual rhythms
            Rhythm manualRhythm = manualRhythms.Find(r => r.name == rhythmName);
            if (manualRhythm != null)
            {
                manualRhythm.SetActive(false);
                Debug.Log($"BeatManager: Stopped manual rhythm '{rhythmName}'");
                return;
            }

            // Check auto rhythms
            AutoRhythm autoRhythm = autoRhythms.Find(r => r.name == rhythmName);
            if (autoRhythm != null)
            {
                autoRhythm.SetActive(false);
                Debug.Log($"BeatManager: Stopped auto rhythm '{rhythmName}'");
                return;
            }

            Debug.LogWarning($"BeatManager: Rhythm '{rhythmName}' not found");
        }

        /// <summary>
        /// Removes a rhythm completely
        /// </summary>
        public void RemoveRhythm(string rhythmName)
        {
            // Try manual rhythms
            Rhythm manualRhythm = manualRhythms.Find(r => r.name == rhythmName);
            if (manualRhythm != null)
            {
                manualRhythm.SetActive(false);
                manualRhythms.Remove(manualRhythm);
                Debug.Log($"BeatManager: Removed manual rhythm '{rhythmName}'");
                return;
            }

            // Try auto rhythms
            AutoRhythm autoRhythm = autoRhythms.Find(r => r.name == rhythmName);
            if (autoRhythm != null)
            {
                autoRhythm.SetActive(false);
                autoRhythms.Remove(autoRhythm);
                Debug.Log($"BeatManager: Removed auto rhythm '{rhythmName}'");
                return;
            }

            Debug.LogWarning($"BeatManager: Rhythm '{rhythmName}' not found");
        }

        /// <summary>
        /// Stops all active rhythms
        /// </summary>
        [ContextMenu("Stop All Rhythms")]
        public void StopAllRhythms()
        {
            foreach (var rhythm in manualRhythms)
                rhythm?.SetActive(false);

            foreach (var autoRhythm in autoRhythms)
                autoRhythm?.SetActive(false);

            Debug.Log("BeatManager: Stopped all rhythms");
        }

        /// <summary>
        /// Starts all manual rhythms
        /// </summary>
        [ContextMenu("Start All Manual Rhythms")]
        public void StartAllManualRhythms()
        {
            foreach (var rhythm in manualRhythms)
                rhythm?.SetActive(true);

            Debug.Log("BeatManager: Started all manual rhythms");
        }

        /// <summary>
        /// Gets the beat interval for a given BPM and steps
        /// </summary>
        public float GetBeatInterval(float bpm, float steps = 1f)
        {
            return 60f / (bpm * steps);
        }

        /// <summary>
        /// Waits for one beat at the specified BPM
        /// </summary>
        public IEnumerator WaitForBeat(float bpm = -1, float steps = 1f)
        {
            float actualBPM = bpm > 0 ? bpm : defaultBPM;
            float waitTime = GetBeatInterval(actualBPM, steps);
            yield return new WaitForSeconds(waitTime);
        }

        /// <summary>
        /// Gets all rhythm names (manual and auto)
        /// </summary>
        public List<string> GetAllRhythmNames()
        {
            List<string> names = new List<string>();

            foreach (var rhythm in manualRhythms)
                if (rhythm != null)
                    names.Add(rhythm.name);

            foreach (var autoRhythm in autoRhythms)
                if (autoRhythm != null)
                    names.Add(autoRhythm.name);

            return names;
        }

        /// <summary>
        /// Checks if AudioManager is available
        /// </summary>
        public bool IsAudioManagerAvailable()
        {
            return AudioManager.Instance != null;
        }

        [ContextMenu("Debug: Print All Rhythms")]
        public void DebugPrintAllRhythms()
        {
            Debug.Log(
                $"BeatManager: {manualRhythms.Count} manual rhythms, {autoRhythms.Count} auto rhythms"
            );

            foreach (var rhythm in manualRhythms)
            {
                if (rhythm != null)
                    Debug.Log(
                        $"  Manual: '{rhythm.name}' - {rhythm.BPM} BPM - {(rhythm.IsActive ? "Active" : "Inactive")}"
                    );
            }

            foreach (var autoRhythm in autoRhythms)
            {
                if (autoRhythm != null)
                    Debug.Log(
                        $"  Auto: '{autoRhythm.name}' -> '{autoRhythm.soundName}' - {autoRhythm.BPM} BPM - {(autoRhythm.IsActive ? "Active" : "Inactive")}"
                    );
            }
        }
    }
}
