using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Audio Manager that handles all sound playback in the game.
    /// Plays sounds by attaching AudioSources to GameObjects dynamically.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AudioManager : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Audio Tracking - Runtime Data")]
        [Tooltip("All currently active audio instances in the scene")]
        [SerializeField]
        private List<AudioInstance> activeAudioInstances = new List<AudioInstance>();

        [Tooltip("Dictionary mapping sound names to their Sound objects for quick lookup")]
        [SerializeField]
        private SerializedDictionary<string, Sound> soundLookup =
            new SerializedDictionary<string, Sound>();

        [Tooltip("Dictionary mapping GameObjects to their playing AudioSources")]
        [SerializeField]
        private SerializedDictionary<GameObject, List<AudioSource>> gameObjectAudioSources =
            new SerializedDictionary<GameObject, List<AudioSource>>();

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Sound Library")]
        [Tooltip("All sounds that can be played by the Audio Manager")]
        [SerializeField]
        private List<Sound> soundLibrary = new List<Sound>();

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

        [SerializeField]
        [Tooltip("If true, automatically recreate singleton if destroyed during runtime")]
        private bool autoRecreateOnDestroy = true;

        //* Singleton Instance Management
        private static AudioManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static AudioManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<AudioManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(AudioManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(AudioManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Audio Manager");
                            instance = singletonGameObject.AddComponent<AudioManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(AudioManager)}] Found existing singleton instance in scene."
                            );
                        }
                    }
                    return instance;
                }
            }
        }

        public static bool HasInstance => instance != null && !isBeingDestroyed;

        //* Data Transfer Interface for Singleton Replacement
        protected virtual void TransferDataToNewInstance(AudioManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.soundLibrary = this.soundLibrary;
                newInstance.activeAudioInstances = this.activeAudioInstances;
                newInstance.soundLookup = this.soundLookup;
                newInstance.gameObjectAudioSources = this.gameObjectAudioSources;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(AudioManager newInstance)
        {
            //* Implement custom data transfer logic here
        }

        //! - - - - - - - - - - -

        [System.Serializable]
        public class AudioInstance
        {
            public string soundName;
            public GameObject targetGameObject;
            public AudioSource audioSource;
            public float startTime;
            public bool isFading;
            public Coroutine fadeCoroutine;

            public AudioInstance(string name, GameObject target, AudioSource source)
            {
                soundName = name;
                targetGameObject = target;
                audioSource = source;
                startTime = Time.time;
                isFading = false;
                fadeCoroutine = null;
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
                    AudioManager oldInstance = instance;
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
            InitializeSoundLookup();
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
            InitializeSoundLookup();
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
                    $"[{nameof(AudioManager)}] Application paused (entering/exiting play mode)"
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
                        $"[{nameof(AudioManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Audio Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<AudioManager>();
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
                Debug.Log($"[{nameof(AudioManager)}] Singleton state reset");
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

        private void InitializeSoundLookup()
        {
            soundLookup.Clear();

            foreach (Sound sound in soundLibrary)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.name))
                {
                    if (!soundLookup.ContainsKey(sound.name))
                    {
                        soundLookup[sound.name] = sound;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"AudioManager: Duplicate sound name '{sound.name}' found in library!"
                        );
                    }
                }
            }

            Debug.Log($"AudioManager: Initialized with {soundLookup.Count} sounds");
        }

        /// <summary>
        /// Plays a sound by name, attaching AudioSource to the specified GameObject
        /// </summary>
        /// <param name="soundName">Name of the sound to play</param>
        /// <param name="target">GameObject to attach AudioSource to (null = use AudioManager)</param>
        /// <returns>The created AudioSource component</returns>
        public AudioSource Play(string soundName, GameObject target = null)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning("AudioManager: Sound name is null or empty");
                return null;
            }

            if (!soundLookup.ContainsKey(soundName))
            {
                Debug.LogWarning($"AudioManager: Sound '{soundName}' not found in library");
                return null;
            }

            Sound sound = soundLookup[soundName];
            return PlaySoundInternal(sound, target, PlayMode.Normal);
        }

        /// <summary>
        /// Plays a sound with fade in effect
        /// </summary>
        public AudioSource PlayWithFadeIn(
            string soundName,
            float fadeInDuration = 1f,
            GameObject target = null
        )
        {
            if (string.IsNullOrEmpty(soundName) || !soundLookup.ContainsKey(soundName))
                return null;

            Sound sound = soundLookup[soundName];
            AudioSource audioSource = PlaySoundInternal(sound, target, PlayMode.FadeIn);

            if (audioSource != null)
            {
                StartCoroutine(FadeInCoroutine(audioSource, fadeInDuration, soundName));
            }

            return audioSource;
        }

        /// <summary>
        /// Plays a sound with fade out effect at the end of the clip
        /// </summary>
        public AudioSource PlayWithFadeOut(
            string soundName,
            float fadeOutDuration = 1f,
            GameObject target = null
        )
        {
            if (string.IsNullOrEmpty(soundName) || !soundLookup.ContainsKey(soundName))
                return null;

            Sound sound = soundLookup[soundName];
            AudioSource audioSource = PlaySoundInternal(sound, target, PlayMode.FadeOut);

            if (audioSource != null)
            {
                // Calculate when to start fading out (clip length - fade duration)
                float clipLength = sound.audioClip.length;
                float fadeOutStart = Mathf.Max(0f, clipLength - fadeOutDuration);

                StartCoroutine(
                    FadeOutCoroutine(audioSource, fadeOutStart, fadeOutDuration, soundName)
                );
            }

            return audioSource;
        }

        /// <summary>
        /// Plays a sound with both fade in and fade out effects
        /// Fades in at start, then fades out at the end of the clip
        /// </summary>
        public AudioSource PlayWithFadeInAndOut(
            string soundName,
            float fadeInDuration = 1f,
            float fadeOutDuration = 1f,
            GameObject target = null
        )
        {
            if (string.IsNullOrEmpty(soundName) || !soundLookup.ContainsKey(soundName))
                return null;

            Sound sound = soundLookup[soundName];
            AudioSource audioSource = PlaySoundInternal(sound, target, PlayMode.FadeInAndOut);

            if (audioSource != null)
            {
                // Calculate when to start fading out (clip length - fade out duration)
                float clipLength = sound.audioClip.length;
                float fadeOutStart = Mathf.Max(fadeInDuration, clipLength - fadeOutDuration);

                StartCoroutine(
                    FadeInAndOutCoroutine(
                        audioSource,
                        fadeInDuration,
                        fadeOutStart,
                        fadeOutDuration,
                        soundName
                    )
                );
            }

            return audioSource;
        }

        private enum PlayMode
        {
            Normal,
            FadeIn,
            FadeOut,
            FadeInAndOut,
        }

        private AudioSource PlaySoundInternal(Sound sound, GameObject target, PlayMode playMode)
        {
            if (sound == null || sound.audioClip == null)
            {
                Debug.LogWarning("AudioManager: Sound or AudioClip is null");
                return null;
            }

            // Use AudioManager as target if none provided
            if (target == null)
                target = gameObject;

            // Create AudioSource component
            AudioSource audioSource = target.AddComponent<AudioSource>();

            // Configure AudioSource with Sound properties
            ConfigureAudioSource(audioSource, sound);

            // Track the audio instance
            AudioInstance instance = new AudioInstance(sound.name, target, audioSource);
            activeAudioInstances.Add(instance);

            // Track AudioSource by GameObject
            if (!gameObjectAudioSources.ContainsKey(target))
                gameObjectAudioSources[target] = new List<AudioSource>();

            gameObjectAudioSources[target].Add(audioSource);

            // Start playing
            audioSource.Play();

            // Start cleanup coroutine
            StartCoroutine(WaitForSoundCompletion(instance));

            Debug.Log($"AudioManager: Playing '{sound.name}' on {target.name}");
            return audioSource;
        }

        private void ConfigureAudioSource(AudioSource audioSource, Sound sound)
        {
            audioSource.clip = sound.audioClip;
            audioSource.loop = sound.loop;
            audioSource.pitch = sound.pitch;
            audioSource.volume = sound.volume;
            audioSource.spatialBlend = sound.spatialBlend;
            audioSource.priority = sound.priority;
            audioSource.dopplerLevel = sound.dopplerLevel;
            audioSource.reverbZoneMix = sound.reverbZoneMix;
            audioSource.playOnAwake = false;
        }

        /// <summary>
        /// Stops a specific AudioSource instance
        /// </summary>
        /// <param name="audioSource">The AudioSource to stop</param>
        /// <param name="fadeOutDuration">Duration to fade out (0 = immediate stop)</param>
        public void Stop(AudioSource audioSource, float fadeOutDuration = 0f)
        {
            if (audioSource == null)
                return;

            if (fadeOutDuration <= 0f)
            {
                // Immediate stop
                audioSource.Stop();
                CleanupAudioSource(audioSource);
            }
            else
            {
                // Fade out stop
                StartCoroutine(FadeOutAndStopCoroutine(audioSource, fadeOutDuration));
            }
        }

        /// <summary>
        /// Stops all AudioSources in the scene
        /// </summary>
        [ContextMenu("Stop All Sounds")]
        public void StopAll()
        {
            // Stop all active audio instances
            for (int i = activeAudioInstances.Count - 1; i >= 0; i--)
            {
                if (activeAudioInstances[i].audioSource != null)
                {
                    activeAudioInstances[i].audioSource.Stop();
                    CleanupAudioSource(activeAudioInstances[i].audioSource);
                }
            }

            activeAudioInstances.Clear();
            gameObjectAudioSources.Clear();

            Debug.Log("AudioManager: Stopped all sounds");
        }

        /// <summary>
        /// Stops all instances of a specific sound
        /// </summary>
        /// <param name="soundName">Name of the sound to stop</param>
        /// <param name="fadeOutDuration">Duration to fade out (0 = immediate stop)</param>
        public void StopSound(string soundName, float fadeOutDuration = 0f)
        {
            if (string.IsNullOrEmpty(soundName))
                return;

            for (int i = activeAudioInstances.Count - 1; i >= 0; i--)
            {
                if (activeAudioInstances[i].soundName == soundName)
                {
                    if (fadeOutDuration <= 0f)
                    {
                        // Immediate stop
                        activeAudioInstances[i].audioSource.Stop();
                        CleanupAudioSource(activeAudioInstances[i].audioSource);
                    }
                    else
                    {
                        // Fade out stop
                        StartCoroutine(
                            FadeOutAndStopCoroutine(
                                activeAudioInstances[i].audioSource,
                                fadeOutDuration
                            )
                        );
                    }
                }
            }

            Debug.Log(
                $"AudioManager: Stopped all instances of '{soundName}' {(fadeOutDuration > 0f ? $"with {fadeOutDuration}s fade" : "immediately")}"
            );
        }

        /// <summary>
        /// Stops all AudioSources attached to a specific GameObject
        /// </summary>
        /// <param name="target">GameObject to stop all sounds on</param>
        /// <param name="fadeOutDuration">Duration to fade out (0 = immediate stop)</param>
        public void StopAllOnGameObject(GameObject target, float fadeOutDuration = 0f)
        {
            if (target == null || !gameObjectAudioSources.ContainsKey(target))
                return;

            List<AudioSource> audioSources = gameObjectAudioSources[target];

            for (int i = audioSources.Count - 1; i >= 0; i--)
            {
                if (audioSources[i] != null)
                {
                    if (fadeOutDuration <= 0f)
                    {
                        // Immediate stop
                        audioSources[i].Stop();
                        CleanupAudioSource(audioSources[i]);
                    }
                    else
                    {
                        // Fade out stop
                        StartCoroutine(FadeOutAndStopCoroutine(audioSources[i], fadeOutDuration));
                    }
                }
            }

            Debug.Log(
                $"AudioManager: Stopped all sounds on {target.name} {(fadeOutDuration > 0f ? $"with {fadeOutDuration}s fade" : "immediately")}"
            );
        }

        /// <summary>
        /// Finds all GameObjects that are currently playing a specific sound
        /// </summary>
        public List<GameObject> FindGameObjectsPlayingSound(string soundName)
        {
            List<GameObject> result = new List<GameObject>();

            if (string.IsNullOrEmpty(soundName))
                return result;

            foreach (AudioInstance instance in activeAudioInstances)
            {
                if (
                    instance.soundName == soundName
                    && instance.targetGameObject != null
                    && !result.Contains(instance.targetGameObject)
                )
                {
                    result.Add(instance.targetGameObject);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all AudioSources currently playing a specific sound
        /// </summary>
        public List<AudioSource> GetAudioSourcesPlayingSound(string soundName)
        {
            List<AudioSource> result = new List<AudioSource>();

            if (string.IsNullOrEmpty(soundName))
                return result;

            foreach (AudioInstance instance in activeAudioInstances)
            {
                if (instance.soundName == soundName && instance.audioSource != null)
                {
                    result.Add(instance.audioSource);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a specific sound is currently playing
        /// </summary>
        public bool IsPlaying(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                return false;

            foreach (AudioInstance instance in activeAudioInstances)
            {
                if (
                    instance.soundName == soundName
                    && instance.audioSource != null
                    && instance.audioSource.isPlaying
                )
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator FadeInCoroutine(
            AudioSource audioSource,
            float duration,
            string soundName
        )
        {
            if (audioSource == null)
                yield break;

            float targetVolume = audioSource.volume;
            audioSource.volume = 0f;

            float elapsed = 0f;
            while (elapsed < duration && audioSource != null)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            if (audioSource != null)
                audioSource.volume = targetVolume;
        }

        private IEnumerator FadeOutCoroutine(
            AudioSource audioSource,
            float delay,
            float duration,
            string soundName
        )
        {
            if (audioSource == null)
                yield break;

            // Wait for delay
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (audioSource == null)
                yield break;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration && audioSource != null)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                CleanupAudioSource(audioSource);
            }
        }

        private IEnumerator FadeInAndOutCoroutine(
            AudioSource audioSource,
            float fadeInDuration,
            float fadeOutStartTime,
            float fadeOutDuration,
            string soundName
        )
        {
            if (audioSource == null)
                yield break;

            // Fade In
            yield return StartCoroutine(FadeInCoroutine(audioSource, fadeInDuration, soundName));

            // Wait until it's time to start fading out
            if (fadeOutStartTime > 0f && audioSource != null)
                yield return new WaitForSeconds(fadeOutStartTime - fadeInDuration);

            // Fade Out
            yield return StartCoroutine(
                FadeOutCoroutine(audioSource, 0f, fadeOutDuration, soundName)
            );
        }

        private IEnumerator FadeOutAndStopCoroutine(AudioSource audioSource, float fadeOutDuration)
        {
            if (audioSource == null)
                yield break;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            // Fade out the volume
            while (elapsed < fadeOutDuration && audioSource != null && audioSource.isPlaying)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            // Stop and cleanup
            if (audioSource != null)
            {
                audioSource.Stop();
                CleanupAudioSource(audioSource);
            }
        }

        private IEnumerator WaitForSoundCompletion(AudioInstance instance)
        {
            if (instance.audioSource == null)
                yield break;

            // Wait while sound is playing (unless it's looping)
            while (instance.audioSource != null && instance.audioSource.isPlaying)
            {
                yield return null;
            }

            // Clean up if AudioSource still exists and isn't fading
            if (instance.audioSource != null && !instance.isFading)
            {
                CleanupAudioSource(instance.audioSource);
            }
        }

        private void CleanupAudioSource(AudioSource audioSource)
        {
            if (audioSource == null)
                return;

            // Find and remove from active instances
            for (int i = activeAudioInstances.Count - 1; i >= 0; i--)
            {
                if (activeAudioInstances[i].audioSource == audioSource)
                {
                    activeAudioInstances.RemoveAt(i);
                    break;
                }
            }

            // Remove from GameObject tracking
            GameObject target = audioSource.gameObject;
            if (gameObjectAudioSources.ContainsKey(target))
            {
                gameObjectAudioSources[target].Remove(audioSource);
                if (gameObjectAudioSources[target].Count == 0)
                {
                    gameObjectAudioSources.Remove(target);
                }
            }

            // Destroy the AudioSource component
            if (audioSource != null)
            {
                Destroy(audioSource);
            }
        }

        /// <summary>
        /// Gets the number of currently active audio instances
        /// </summary>
        public int GetActiveAudioCount()
        {
            return activeAudioInstances.Count;
        }

        /// <summary>
        /// Gets all sound names in the library
        /// </summary>
        public List<string> GetAllSoundNames()
        {
            return new List<string>(soundLookup.Keys);
        }

        /// <summary>
        /// Refreshes the sound lookup dictionary (call after modifying soundLibrary in editor)
        /// </summary>
        [ContextMenu("Refresh Sound Library")]
        public void RefreshSoundLibrary()
        {
            InitializeSoundLookup();
        }

        [ContextMenu("Debug: Print Active Audio Instances")]
        public void DebugPrintActiveInstances()
        {
            Debug.Log($"AudioManager: {activeAudioInstances.Count} active audio instances:");

            foreach (AudioInstance instance in activeAudioInstances)
            {
                string status =
                    instance.audioSource != null && instance.audioSource.isPlaying
                        ? "Playing"
                        : "Stopped";
                Debug.Log($"- {instance.soundName} on {instance.targetGameObject.name} ({status})");
            }
        }

        [ContextMenu("Debug: Print Sound Library")]
        public void DebugPrintSoundLibrary()
        {
            Debug.Log($"AudioManager: Sound Library contains {soundLibrary.Count} sounds:");

            foreach (Sound sound in soundLibrary)
            {
                if (sound != null)
                    Debug.Log($"- {sound.name}");
                else
                    Debug.Log("- [NULL SOUND]");
            }
        }
    }
}
