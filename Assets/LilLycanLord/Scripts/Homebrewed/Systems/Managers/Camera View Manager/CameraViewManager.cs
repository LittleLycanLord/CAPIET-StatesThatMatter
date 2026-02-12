using System;
using System.Collections;
using System.Collections.Generic;
using LilLycanLord_Official;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    [Serializable]
    public class View
    {
        public CinemachineCamera cinemachineCamera;

        public float cinematicEffect;
        public CinemachineBlendDefinition blendMode;
    }

    public class CameraViewManager : MonoBehaviour
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
        private static CameraViewManager instance;
        private static readonly object instanceLock = new object();
        private static bool applicationIsQuitting = false;
        private static bool isBeingDestroyed = false;

        public static CameraViewManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    // Always try to provide an instance, even during application quit
                    if (instance == null && !isBeingDestroyed)
                    {
                        instance = FindFirstObjectByType<CameraViewManager>();

                        if (instance == null)
                        {
                            // Don't create during application quit unless explicitly needed
                            if (applicationIsQuitting)
                            {
                                Debug.LogWarning(
                                    $"[{nameof(CameraViewManager)}] Attempted to access singleton during application quit. Returning null."
                                );
                                return null;
                            }

                            Debug.Log(
                                $"[{nameof(CameraViewManager)}] Creating new singleton instance at runtime."
                            );
                            GameObject singletonGameObject = new GameObject("Camera View Manager");
                            instance = singletonGameObject.AddComponent<CameraViewManager>();
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(CameraViewManager)}] Found existing singleton instance in scene."
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
        protected virtual void TransferDataToNewInstance(CameraViewManager newInstance)
        {
            if (newInstance != null && transferDataOnReplace)
            {
                //* Transfer critical data here
                newInstance.views = new List<View>(this.views);
                newInstance.defaultViewChangeSpeed = this.defaultViewChangeSpeed;
                newInstance.cinematicEffect = this.cinematicEffect;
                newInstance.currentView = this.currentView;
                newInstance.persistAcrossScenes = this.persistAcrossScenes;
                newInstance.transferDataOnReplace = this.transferDataOnReplace;
                newInstance.autoRecreateOnDestroy = this.autoRecreateOnDestroy;

                //* Call custom data transfer method
                OnDataTransfer(newInstance);
            }
        }

        //* Override this method in derived classes for custom data transfer
        protected virtual void OnDataTransfer(CameraViewManager newInstance)
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
                    CameraViewManager oldInstance = instance;
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
                    $"[{nameof(CameraViewManager)}] Application paused (entering/exiting play mode)"
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
                        $"[{nameof(CameraViewManager)}] Singleton was destroyed during runtime. Auto-recreating..."
                    );
                    GameObject singletonGameObject = new GameObject(
                        "Camera View Manager (Auto-Recreated)"
                    );
                    instance = singletonGameObject.AddComponent<CameraViewManager>();
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
                Debug.Log($"[{nameof(CameraViewManager)}] Singleton state reset");
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
        [SerializeField]
        CinemachineCamera currentView;
        public float cinematicEffect;

        [Space(10)]
        [Header("Cinemachine Setup")]
        [SerializeField]
        CinemachineBrain cinemachineBrain;

        [SerializeField]
        CinemachineCamera startingView;

        public List<View> views = new List<View>();

        [Header("View Transition Settings")]
        [SerializeField]
        [Range(0.1f, 10.0f)]
        float defaultViewChangeSpeed = 2.0f;

        [Tooltip("Smooth cinematic bar transitions")]
        [SerializeField]
        private bool smoothCinematicBars = true;

        [Tooltip("Speed of cinematic bar size changes")]
        [SerializeField]
        [Range(0.1f, 5.0f)]
        private float cinematicBarSpeed = 2.0f;

        [Header("Performance Settings")]
        [Tooltip("Use cached view lookups for better performance")]
        [SerializeField]
        private bool enableViewCaching = true;

        [Header("Controls")]
        [SerializeField]
        string targetViewName;

        [SerializeField]
        float viewChangeSpeed = 0.0f;

        Image topCinematicBar;
        Image bottomCinematicBar;

        //* Private Fields for Performance
        private Dictionary<string, View> viewLookup = new Dictionary<string, View>();
        private Dictionary<CinemachineCamera, View> cameraViewLookup =
            new Dictionary<CinemachineCamera, View>();
        private float targetCinematicEffect = 0f;
        private bool isTransitioning = false;
        private Coroutine currentTransition = null;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Start()
        {
            InitializeCameraSystem();
        }

        void Update()
        {
            //* General cinematic bar update with null checking and smooth animations
            if (topCinematicBar != null && bottomCinematicBar != null)
            {
                UpdateCinematicBars();
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void InitializeCameraSystem()
        {
            if (cinemachineBrain == null)
            {
                cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
                if (cinemachineBrain == null)
                {
                    Debug.LogError("CameraViewManager: No CinemachineBrain found in scene");
                    return;
                }
            }

            if (startingView == null)
            {
                Debug.LogError("CameraViewManager: Starting View is missing");
                return;
            }

            //* Initialize view lookup cache
            BuildViewLookupCache();

            //* Set up starting view
            currentView = startingView;
            foreach (View view in views)
            {
                if (view.cinemachineCamera != currentView)
                    view.cinemachineCamera.gameObject.SetActive(false);
            }

            //* Initialize cinematic bars
            InitializeCinematicBars();

            //* Set default values and apply starting view's cinematic effect
            viewChangeSpeed = defaultViewChangeSpeed;
            
            // Find the starting view's cinematic effect and set bars immediately
            View startingViewData = FindViewByCamera(startingView);
            if (startingViewData != null)
            {
                cinematicEffect = startingViewData.cinematicEffect;
                targetCinematicEffect = startingViewData.cinematicEffect;
                
                // Immediately set the cinematic bar sizes
                if (topCinematicBar != null && bottomCinematicBar != null)
                {
                    float barSize = cinematicEffect * 10.0f;
                    topCinematicBar.rectTransform.sizeDelta = new Vector2(
                        topCinematicBar.rectTransform.sizeDelta.x,
                        barSize
                    );
                    bottomCinematicBar.rectTransform.sizeDelta = new Vector2(
                        bottomCinematicBar.rectTransform.sizeDelta.x,
                        barSize
                    );
                }
            }
            else
            {
                targetCinematicEffect = cinematicEffect;
            }

            Debug.Log(
                $"CameraViewManager: Initialized with {views.Count} views, starting with '{currentView.name}'"
            );
        }

        private void BuildViewLookupCache()
        {
            if (!enableViewCaching)
                return;

            viewLookup.Clear();
            cameraViewLookup.Clear();

            foreach (View view in views)
            {
                if (view.cinemachineCamera != null)
                {
                    string viewName = view.cinemachineCamera.name;
                    if (!viewLookup.ContainsKey(viewName))
                    {
                        viewLookup[viewName] = view;
                        cameraViewLookup[view.cinemachineCamera] = view;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"CameraViewManager: Duplicate view name '{viewName}' found!"
                        );
                    }
                }
            }
        }

        private void InitializeCinematicBars()
        {
            try
            {
                Transform cinematicBarsParent = transform.Find("Cinematic Bars");
                if (cinematicBarsParent == null)
                {
                    Debug.LogWarning(
                        "CameraViewManager: 'Cinematic Bars' UI element not found. Cinematic effects disabled."
                    );
                    return;
                }

                topCinematicBar = cinematicBarsParent.Find("Top")?.GetComponent<Image>();
                bottomCinematicBar = cinematicBarsParent.Find("Bottom")?.GetComponent<Image>();

                if (topCinematicBar == null || bottomCinematicBar == null)
                {
                    Debug.LogWarning(
                        "CameraViewManager: Top or Bottom cinematic bar not found. Cinematic effects disabled."
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(
                    $"CameraViewManager: Error initializing cinematic bars: {e.Message}"
                );
            }
        }

        private void UpdateCinematicBars()
        {
            float targetBarSize = cinematicEffect * 10.0f;

            if (smoothCinematicBars)
            {
                //* Smooth animation for cinematic bars
                float currentTopSize = topCinematicBar.rectTransform.sizeDelta.y;
                float currentBottomSize = bottomCinematicBar.rectTransform.sizeDelta.y;

                float newTopSize = Mathf.Lerp(
                    currentTopSize,
                    targetBarSize,
                    Time.deltaTime * cinematicBarSpeed
                );
                float newBottomSize = Mathf.Lerp(
                    currentBottomSize,
                    targetBarSize,
                    Time.deltaTime * cinematicBarSpeed
                );

                topCinematicBar.rectTransform.sizeDelta = new Vector2(
                    topCinematicBar.rectTransform.sizeDelta.x,
                    newTopSize
                );
                bottomCinematicBar.rectTransform.sizeDelta = new Vector2(
                    bottomCinematicBar.rectTransform.sizeDelta.x,
                    newBottomSize
                );
            }
            else
            {
                //* Instant cinematic bar update
                topCinematicBar.rectTransform.sizeDelta = new Vector2(
                    topCinematicBar.rectTransform.sizeDelta.x,
                    targetBarSize
                );
                bottomCinematicBar.rectTransform.sizeDelta = new Vector2(
                    bottomCinematicBar.rectTransform.sizeDelta.x,
                    targetBarSize
                );
            }
        }

        public View GetCurrentView()
        {
            return FindViewByCamera(currentView);
        }

        public string GetCurrentViewName()
        {
            return currentView?.name ?? "None";
        }

        public bool HasView(string viewName)
        {
            return FindCameraByName(viewName) != null;
        }

        public string[] GetAllViewNames()
        {
            List<string> viewNames = new List<string>();
            foreach (View view in views)
            {
                if (view.cinemachineCamera != null)
                {
                    viewNames.Add(view.cinemachineCamera.name);
                }
            }
            return viewNames.ToArray();
        }

        public void RefreshViewCache()
        {
            BuildViewLookupCache();
            Debug.Log("CameraViewManager: View cache refreshed");
        }

        public void SetCinematicEffect(float value)
        {
            cinematicEffect = Mathf.Clamp01(value);
        }

        public void StopAllTransitions()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
            isTransitioning = false;
            Debug.Log("CameraViewManager: All transitions stopped");
        }

        private CinemachineCamera FindCameraByName(string cameraName)
        {
            if (enableViewCaching && viewLookup.ContainsKey(cameraName))
            {
                return viewLookup[cameraName].cinemachineCamera;
            }

            //* Fallback to traditional search if not cached
            foreach (View view in views)
            {
                if (view.cinemachineCamera != null && view.cinemachineCamera.name == cameraName)
                {
                    return view.cinemachineCamera;
                }
            }

            return null;
        }

        private View FindViewByCamera(CinemachineCamera camera)
        {
            if (enableViewCaching && cameraViewLookup.ContainsKey(camera))
            {
                return cameraViewLookup[camera];
            }

            //* Fallback to traditional search if not cached
            foreach (View view in views)
            {
                if (view.cinemachineCamera == camera)
                {
                    return view;
                }
            }

            return null;
        }

        public void SetView(CinemachineCamera view, float changeSpeed)
        {
            if (ViewExists(view) == null)
            {
                View newView = new View();
                newView.cinemachineCamera = view;
                newView.cinematicEffect = 0.0f;
                views.Add(newView);
                targetViewName = newView.cinemachineCamera.name;
            }
            viewChangeSpeed = changeSpeed;
            ChangeToView();
        }

        public void SetView(string viewName, float changeSpeed)
        {
            targetViewName = viewName;
            viewChangeSpeed = changeSpeed;
            ChangeToView();
        }

        public void ChangeToView(string viewName)
        {
            targetViewName = viewName;
            viewChangeSpeed = defaultViewChangeSpeed;
            ChangeToView();
        }

        [ContextMenu("Change To View")]
        public void ChangeToView()
        {
            if (ViewExists(targetViewName) == null)
                return;

            Debug.Log("Switching to " + targetViewName + " View");
            View targetView = ViewExists(targetViewName);
            currentView.gameObject.SetActive(false);
            cinemachineBrain.DefaultBlend = targetView.blendMode;
            cinemachineBrain.DefaultBlend.Time = viewChangeSpeed;
            StartCoroutine(CinematicEffect());
            currentView = targetView.cinemachineCamera;
            currentView.gameObject.SetActive(true);
        }

        IEnumerator CinematicEffect()
        {
            // Code to do before function
            float startValue = cinematicEffect;
            float timeElapsed = 0;
            while (timeElapsed < cinemachineBrain.DefaultBlend.Time)
            {
                cinematicEffect = Mathf.Lerp(
                    startValue,
                    ViewExists(targetViewName).cinematicEffect,
                    timeElapsed / cinemachineBrain.DefaultBlend.Time
                );
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            // Code to do after function
        }

        View ViewExists(string viewName)
        {
            foreach (View view in views)
            {
                if (view.cinemachineCamera.name == viewName)
                    return view;
            }
            Debug.LogWarning(name + " has no View called \"" + viewName + "\"");
            return null;
        }

        View ViewExists(CinemachineCamera viewCamera)
        {
            foreach (View view in views)
            {
                if (view.cinemachineCamera == viewCamera)
                    return view;
            }
            return null;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
