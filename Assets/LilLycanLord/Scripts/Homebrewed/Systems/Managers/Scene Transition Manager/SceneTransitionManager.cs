using System;
using System.Collections.Generic;
using LilLycanLord_Official;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Slider))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class SceneTransition : MonoBehaviour
    {
        // [Header("Displays")]

        [Space(10)]
        [Header("Fields")]
        public UnityEvent beforeTriggers;

        public UnityEvent afterTriggers;

        [SerializeField]
        protected TMP_Text transitionDebugText;

        [SerializeField]
        protected TMP_Text loadingIndicator;

        [SerializeField]
        protected TMP_Text loadingText;

        [HideInInspector]
        public string transitionName;

        [HideInInspector]
        public Image background;

        [HideInInspector]
        public Animator animator;

        [HideInInspector]
        public Slider slider;

        [HideInInspector]
        public CanvasGroup canvasGroup;

        void Awake()
        {
            InitializeSceneTransition();
        }

        protected void InitializeSceneTransition()
        {
            transitionName = name;
            background = GetComponent<Image>();
            animator = GetComponent<Animator>();
            slider = GetComponent<Slider>();
            slider.fillRect = background.rectTransform;
            canvasGroup = GetComponent<CanvasGroup>();

            if (transitionDebugText == null && SceneTransitionManager.Instance.debugMode)
            {
                GameObject transitionDebugTextObject = new GameObject("Transition Debug Text");
                transitionDebugText = transitionDebugTextObject.AddComponent<TMP_Text>();
                transitionDebugText.text = "";
            }
        }

        protected virtual void UpdateDebugText()
        {
            transitionDebugText.gameObject.SetActive(SceneTransitionManager.Instance.debugMode);
            transitionDebugText.text =
                "Loading to \"" + SceneTransitionManager.Instance.targetSceneName + "\"...";
        }

        public abstract void PrimeTransition();
        public abstract void SwitchScene();
        public abstract void CleanUpTransition();
    }

    [Serializable]
    public class SceneTriggers
    {
        public Scene scene;
        public UnityEvent onEnter;
        public UnityEvent onExit;
    }

    /// <summary>
    /// Scene Transition Manager - Handles smooth transitions between scenes with visual effects.
    ///
    /// ⚠️ IMPORTANT SCENE ARCHITECTURE WARNING:
    /// Due to singleton replacement behavior, placing SceneTransitionManager in multiple scenes
    /// can cause transition interruption bugs. Here's what happens:
    ///
    /// BUG SCENARIO:
    /// 1. Scene A: SceneTransitionManager starts crossfade transition to Scene B
    /// 2. Scene B: Loads and contains its own SceneTransitionManager
    /// 3. PROBLEM: Singleton replacement destroys Scene A's manager mid-transition
    /// 4. RESULT: Active coroutines/animations stop → transition appears abruptly
    ///
    /// RECOMMENDED SOLUTIONS:
    /// • OPTION 1: Keep SceneTransitionManager in a persistent "ManagerScene" only
    /// • OPTION 2: Use DontDestroyOnLoad and place in first scene only
    /// • OPTION 3: Only put transition effects (Crossfade, etc.) in individual scenes
    /// • OPTION 4: Ensure target scenes don't contain SceneTransitionManager
    ///
    /// TECHNICAL DETAILS:
    /// - Singleton replacement happens in Awake() before transitions can complete
    /// - TransferDataToNewInstance() doesn't transfer runtime state (coroutines, timing)
    /// - Active WaitForFadeOutComplete() coroutines are destroyed with old instance
    /// - New instance has no knowledge of ongoing transition state
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
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
        private static SceneTransitionManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static SceneTransitionManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<SceneTransitionManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(SceneTransitionManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(SceneTransitionManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject(
                                "Scene Transition Manager"
                            );
                            instance = singletonGameObject.AddComponent<SceneTransitionManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(SceneTransitionManager)}] Found existing singleton instance in scene."
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
        protected virtual void TransferDataToNewInstance(SceneTransitionManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.targetSceneName = this.targetSceneName;
                newInstance.selectedTransitionName = this.selectedTransitionName;
                newInstance.debugMode = this.debugMode;
                newInstance.delay = this.delay;
                newInstance.enteringDuration = this.enteringDuration;
                newInstance.exitingDuration = this.exitingDuration;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(SceneTransitionManager newInstance)
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

                    //* Keep the old instance, destroy the new duplicate
                    Debug.LogWarning($"[{nameof(SceneTransitionManager)}] Duplicate instance detected. Keeping the original instance and destroying the new one.");
                    Destroy(this.gameObject);
                    return;
                }
            }

            //* - - - - - Non - Singleton Awake Content - - - - -
            InitializeTransitionSystem();
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
                    $"[{nameof(SceneTransitionManager)}] Application paused (entering/exiting play mode)"
                );
                // Don't set applicationIsQuitting here in editor
            }
        }

        void OnDestroy()
        {
            //* Cleanup scene event subscriptions
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            //* Stop any ongoing transitions
            StopAllTransitions();

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
                        $"[{nameof(SceneTransitionManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Scene Transition Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<SceneTransitionManager>();
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
                Debug.Log($"[{nameof(SceneTransitionManager)}] Singleton state reset");
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

        private void InitializeTransitionSystem()
        {
            //* Build transition cache for performance
            BuildTransitionCache();

            //* Initialize scene change detection
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Debug.Log("SceneTransitionManager: Transition system initialized");
        }

        private void BuildTransitionCache()
        {
            transitionLookup.Clear();

            foreach (SceneTransition transition in transitions)
            {
                if (transition != null && !string.IsNullOrEmpty(transition.transitionName))
                {
                    if (!transitionLookup.ContainsKey(transition.transitionName))
                    {
                        transitionLookup[transition.transitionName] = transition;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"SceneTransitionManager: Duplicate transition name '{transition.transitionName}' found!"
                        );
                    }
                }
            }

            isTransitionCacheBuilt = true;
            Debug.Log(
                $"SceneTransitionManager: Transition cache built with {transitionLookup.Count} transitions"
            );
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                Debug.Log($"SceneTransitionManager: Scene '{scene.name}' loaded");
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"SceneTransitionManager: Scene '{scene.name}' unloaded");
        }

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
        public bool isTransitioning { get; private set; } = false;
        public List<SceneTransition> transitions = new List<SceneTransition>();

        [SerializeField]
        SceneTransition selectedTransition;

        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        List<SceneTriggers> sceneTriggers = new List<SceneTriggers>();

        public bool debugMode = false;
        public string targetSceneName = "";
        public string selectedTransitionName = "";

        [Space(10)]
        [Header("Timing")]
        public float delay = 0.0f;
        public float enteringDuration = 0.0f;
        public float exitingDuration = 0.0f;

        string previousSceneName;
        float GUIWidth = 220;
        bool debugModeDrawn = false;

        //* Performance Enhancement Fields
        private Dictionary<string, SceneTransition> transitionLookup =
            new Dictionary<string, SceneTransition>();
        private bool isTransitionCacheBuilt = false;
        private Coroutine currentTransitionCoroutine = null;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Start()
        {
            //* General transition discovery and caching
            InitializeTransitions();
        }

        void Update()
        {
            if (debugMode)
            {
                ShowDebugButtons();
                debugModeDrawn = true;
            }
            else
            {
                CleanUpDebugButtons();
                debugModeDrawn = false;
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeTransitions()
        {
            Transform transitionsParent = transform.Find("Transitions");
            if (transitionsParent == null)
            {
                Debug.LogWarning(
                    "SceneTransitionManager: 'Transitions' child object not found. Creating one..."
                );
                GameObject transitionsObject = new GameObject("Transitions");
                transitionsObject.transform.SetParent(transform);
                transitionsParent = transitionsObject.transform;
            }

            transitions.Clear();
            foreach (Transform transition in transitionsParent)
            {
                SceneTransition sceneTransition = transition.GetComponent<SceneTransition>();
                if (sceneTransition != null)
                {
                    transitions.Add(sceneTransition);
                    Debug.Log(
                        $"SceneTransitionManager: Found transition '{sceneTransition.transitionName}'"
                    );
                }
            }

            //* Build performance cache
            if (!isTransitionCacheBuilt)
            {
                BuildTransitionCache();
            }

            Debug.Log($"SceneTransitionManager: Initialized with {transitions.Count} transitions");
        }

        void ShowDebugButtons()
        {
            if (debugModeDrawn)
                return;

            DebuggingManager.Instance.SetWidth(GUIWidth);

            UnityEvent loadSceneWithTransition = new UnityEvent();
            loadSceneWithTransition.AddListener(LoadSceneWithTransition);
            DebuggingManager.Instance.AddDebugOption(
                "LoadSceneWithTransition",
                "Load Scene With Transition",
                "Loads the Target Scene with the selected Transition",
                loadSceneWithTransition
            );

            UnityEvent loadSceneWithoutTransition = new UnityEvent();
            loadSceneWithoutTransition.AddListener(LoadSceneWithoutTransition);
            DebuggingManager.Instance.AddDebugOption(
                "LoadSceneWithoutTransition",
                "Load Scene Without Transition",
                "Loads the Target Scene",
                loadSceneWithoutTransition
            );

            UnityEvent addToCurrentScene = new UnityEvent();
            addToCurrentScene.AddListener(LoadSceneWithoutTransition);
            DebuggingManager.Instance.AddDebugOption(
                "AddToCurrentScene",
                "Add To Current Scene",
                "Adds the Target Scene onto the Current Scene",
                addToCurrentScene
            );

            UnityEvent returnToPreviousScene = new UnityEvent();
            returnToPreviousScene.AddListener(ReturnToPreviousScene);
            DebuggingManager.Instance.AddDebugOption(
                "ReturnToPreviousScene",
                "Return To Previous Scene",
                "Return tp the previously Loaded Scene",
                returnToPreviousScene
            );
        }

        void CleanUpDebugButtons()
        {
            if (!debugModeDrawn)
                return;
            DebuggingManager.Instance.RemoveDebugOption("LoadSceneWithTransition");
            DebuggingManager.Instance.RemoveDebugOption("LoadSceneWithoutTransition");
            DebuggingManager.Instance.RemoveDebugOption("AddToCurrentScene");
            DebuggingManager.Instance.RemoveDebugOption("ReturnToPreviousScene");
        }

        public void LoadSceneWithTransition()
        {
            if (targetSceneName == "")
            {
                Debug.LogError(name + " has no Target Scene");
                return;
            }
            if (selectedTransitionName == "")
            {
                Debug.LogWarning(
                    name + " has no Selected Transition; switching with no transition"
                );
                LoadSceneWithoutTransition();
                return;
            }
            if (!isTransitioning)
            {
                previousSceneName = SceneManager.GetActiveScene().name;
                selectedTransition = FindTransition(selectedTransitionName);
                Debug.Log("Transitioning to \"" + targetSceneName + "\"...");
                Invoke("TriggerTransition", delay);
            }
            else
                Debug.LogWarning(name + " is already transitioning to " + targetSceneName);
        }

        void TriggerTransition()
        {
            selectedTransition.PrimeTransition();
            ActivateTriggers(false);
            FlagTransition(true);
        }

        public void LoadSceneWithTransition(string targetScene)
        {
            targetSceneName = targetScene;
            LoadSceneWithTransition();
        }

        public void LoadSceneWithTransition(string targetScene, string transitionName)
        {
            targetSceneName = targetScene;
            selectedTransitionName = transitionName;
            LoadSceneWithTransition();
        }

        public void LoadSceneWithoutTransition()
        {
            if (isTransitioning)
                return;
            previousSceneName = SceneManager.GetActiveScene().name;
            ActivateTriggers(false);
            LoadToTargetScene();
            ActivateTriggers(true);
        }

        public void LoadToTargetScene()
        {
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
            if (selectedTransitionName != "" && selectedTransition != null)
                EndTransition();
        }

        public void AddToCurrentScene()
        {
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Additive);
        }

        public void AddToCurrentScene(string transitionName)
        {
            targetSceneName = transitionName;
            AddToCurrentScene();
        }

        public void ReturnToPreviousScene()
        {
            if (isTransitioning)
                return;
            targetSceneName = previousSceneName;
            LoadSceneWithTransition();
        }

        public void EndTransition()
        {
            ActivateTriggers(true);
            FlagTransition(false);

            targetSceneName = "";
        }
        
        /// <summary>
        /// Plays a transition effect (fade in/out) without loading a new scene.
        /// Useful for visual transitions like teleporting, respawning, etc.
        /// </summary>
        /// <param name="transitionName">Name of the transition to play</param>
        /// <param name="onMidpoint">Optional callback to execute when fully faded in (at midpoint)</param>
        public void PlayTransitionEffect(string transitionName, System.Action onMidpoint = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitionManager: Cannot play transition effect while already transitioning");
                return;
            }
            
            SceneTransition transition = FindTransition(transitionName);
            if (transition == null)
            {
                Debug.LogError($"SceneTransitionManager: Cannot play transition effect - transition '{transitionName}' not found");
                return;
            }
            
            // Store current transition
            selectedTransition = transition;
            selectedTransitionName = transitionName;
            
            // Start the effect coroutine
            StartCoroutine(PlayTransitionEffectCoroutine(onMidpoint));
        }
        
        /// <summary>
        /// Coroutine that handles the transition effect sequence
        /// </summary>
        private System.Collections.IEnumerator PlayTransitionEffectCoroutine(System.Action onMidpoint)
        {
            Debug.Log($"SceneTransitionManager: Playing transition effect '{selectedTransitionName}'");
            
            // Mark as transitioning
            FlagTransition(true);
            
            // Phase 1: Prime transition (fade in)
            selectedTransition.PrimeTransition();
            
            // Wait for fade in duration
            float fadeInDuration = exitingDuration > 0 ? exitingDuration : 1f;
            
            // Try to get duration from Crossfade if it's the selected transition
            Crossfade crossfade = selectedTransition as Crossfade;
            if (crossfade != null)
            {
                // Access private field via reflection or use a reasonable default
                // Since we can't access private fields directly, use the exitingDuration from manager
                fadeInDuration = exitingDuration > 0 ? exitingDuration : 1f;
            }
            
            Debug.Log($"SceneTransitionManager: Waiting {fadeInDuration}s for fade in");
            yield return new WaitForSeconds(fadeInDuration);
            
            // Phase 2: Execute midpoint callback
            if (onMidpoint != null)
            {
                Debug.Log("SceneTransitionManager: Executing midpoint callback");
                try
                {
                    onMidpoint.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"SceneTransitionManager: Error in midpoint callback - {e.Message}");
                }
            }
            
            // Phase 3: Clean up transition (fade out)
            selectedTransition.CleanUpTransition();
            
            // Wait for fade out duration (CleanUpTransition will call EndTransition when done)
            float fadeOutDuration = enteringDuration > 0 ? enteringDuration : 1f;
            Debug.Log($"SceneTransitionManager: Transition effect will complete in {fadeOutDuration}s");
            
            // Note: CleanUpTransition internally waits and calls EndTransition,
            // so we don't need to call it here
        }

        /// <param name="transitionName">Name of the transition to find</param>
        /// <returns>SceneTransition if found, null otherwise</returns>
        SceneTransition FindTransition(string transitionName)
        {
            if (string.IsNullOrEmpty(transitionName))
            {
                Debug.LogError("SceneTransitionManager: Transition name cannot be null or empty");
                return null;
            }

            //* Use cache for performance if available
            if (isTransitionCacheBuilt && transitionLookup.ContainsKey(transitionName))
            {
                return transitionLookup[transitionName];
            }

            //* Fallback to traditional search if cache miss
            SceneTransition foundTransition = transitions.Find(transition =>
                transition.transitionName == transitionName
            );

            if (foundTransition == null)
            {
                Debug.LogError($"SceneTransitionManager: Transition '{transitionName}' not found");
                return null;
            }

            //* Add to cache for future lookups
            if (isTransitionCacheBuilt && !transitionLookup.ContainsKey(transitionName))
            {
                transitionLookup[transitionName] = foundTransition;
            }

            return foundTransition;
        }

        void ActivateTriggers(bool onEnter)
        {
            List<SceneTriggers> triggers = new List<SceneTriggers>();
            if (onEnter)
            {
                if (selectedTransitionName != "" && selectedTransition != null)
                    if (selectedTransition.afterTriggers != null)
                        selectedTransition.afterTriggers?.Invoke();
                if (
                    sceneTriggers.Contains(
                        sceneTriggers.Find(sceneTrigger =>
                            sceneTrigger.scene.name == targetSceneName
                        )
                    )
                )
                    triggers = sceneTriggers.FindAll(sceneTrigger =>
                        sceneTrigger.scene.name == targetSceneName
                    );
            }
            else
            {
                if (selectedTransitionName != "" && selectedTransition != null)
                    if (selectedTransition.beforeTriggers != null)
                        selectedTransition.beforeTriggers?.Invoke();
                if (
                    sceneTriggers.Contains(
                        sceneTriggers.Find(sceneTrigger =>
                            sceneTrigger.scene.name == targetSceneName
                        )
                    )
                )
                    triggers = sceneTriggers.FindAll(sceneTrigger =>
                        sceneTrigger.scene.name == previousSceneName
                    );
            }

            if (triggers.Count > 0)
                foreach (SceneTriggers sceneTrigger in triggers)
                    if (onEnter)
                        sceneTrigger.onEnter?.Invoke();
                    else
                        sceneTrigger.onExit?.Invoke();
        }

        void FlagTransition(bool flag)
        {
            isTransitioning = flag;
            if (selectedTransitionName == "")
                return;

            selectedTransition.animator.SetBool("isTransitioning", isTransitioning);
        }

        public string[] GetAllTransitionNames()
        {
            List<string> transitionNames = new List<string>();
            foreach (SceneTransition transition in transitions)
            {
                if (transition != null && !string.IsNullOrEmpty(transition.transitionName))
                {
                    transitionNames.Add(transition.transitionName);
                }
            }
            return transitionNames.ToArray();
        }

        public bool HasTransition(string transitionName)
        {
            return FindTransition(transitionName) != null;
        }

        public SceneTransition GetCurrentTransition()
        {
            return selectedTransition;
        }

        public string GetCurrentTransitionName()
        {
            return selectedTransitionName ?? "None";
        }

        public void StopAllTransitions()
        {
            if (currentTransitionCoroutine != null)
            {
                StopCoroutine(currentTransitionCoroutine);
                currentTransitionCoroutine = null;
            }

            if (selectedTransition != null)
            {
                selectedTransition.animator.SetBool("isTransitioning", false);
            }

            isTransitioning = false;
            Debug.Log("SceneTransitionManager: All transitions stopped");
        }

        public void RefreshTransitionCache()
        {
            BuildTransitionCache();
            Debug.Log("SceneTransitionManager: Transition cache refreshed");
        }

        public string GetTransitionStats()
        {
            return $"Transitions: {transitions.Count}, Cached: {transitionLookup.Count}, IsTransitioning: {isTransitioning}";
        }

        public void LoadSceneWithTransition(
            string targetScene,
            string transitionName,
            float customDelay,
            float customEnterDuration = -1f,
            float customExitDuration = -1f
        )
        {
            targetSceneName = targetScene;
            selectedTransitionName = transitionName;

            //* Apply custom timings if provided
            if (customDelay >= 0f)
                delay = customDelay;
            if (customEnterDuration >= 0f)
                enteringDuration = customEnterDuration;
            if (customExitDuration >= 0f)
                exitingDuration = customExitDuration;

            LoadSceneWithTransition();
        }

        public void QuickTransitionTo(string targetScene)
        {
            //* Use first available transition or create a basic one
            if (transitions.Count > 0)
            {
                LoadSceneWithTransition(targetScene, transitions[0].transitionName);
            }
            else
            {
                Debug.LogWarning(
                    "SceneTransitionManager: No transitions available, loading without transition"
                );
                LoadSceneWithoutTransition();
            }
        }
    }

    //* ╔════════════════════════════════╗
    //* ║ Virtual / Overridden Functions ║
    //* ╚════════════════════════════════╝
}
