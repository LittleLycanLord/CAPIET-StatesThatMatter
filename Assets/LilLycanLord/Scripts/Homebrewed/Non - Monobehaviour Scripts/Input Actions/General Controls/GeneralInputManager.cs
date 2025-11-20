using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LilLycanLord_Official
{
    [System.Serializable]
    public struct InputHistoryEntry
    {
        public string InputName;
        public string Action;
        public float Timestamp;
        public int FrameCount;
    }

    [System.Serializable]
    public struct InputProfile
    {
        public string Name;
        public float Deadzone;
        public float MouseSensitivity;
        public float ScrollSensitivity;
    }

    [DefaultExecutionOrder(-101)]
    public class GeneralInputManager : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        private GeneralControls controls;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Input State Display")]
        [SerializeField]
        private Vector2 cachedWASDInput;

        [SerializeField]
        private bool cachedSprintInput;

        [SerializeField]
        private bool cachedJumpInput;

        [SerializeField]
        private bool cachedInteractInput;

        [SerializeField]
        private bool cachedPrimaryMouse;

        [SerializeField]
        private bool cachedSecondaryMouse;

        [SerializeField]
        private bool cachedMMBInput;

        [SerializeField]
        private float cachedScrollInput;

        [SerializeField]
        private Vector2 cachedMouseMovement;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        private static GeneralInputManager instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;
        public static GeneralInputManager Instance
        {
            get
            {
                // In editor, don't block instance creation when stopping play mode
                if (applicationIsQuitting && !Application.isEditor)
                {
                    Debug.LogWarning(
                        "GeneralInputManager instance requested during application quit. Returning null."
                    );
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null && (!applicationIsQuitting || Application.isEditor))
                    {
                        instance = FindFirstObjectByType<GeneralInputManager>();
                        if (instance == null)
                        {
                            // Only create new instance if we're in play mode or editor
                            if (Application.isPlaying)
                            {
                                GameObject singletonObject = new GameObject(
                                    "General Input Manager"
                                );
                                instance = singletonObject.AddComponent<GeneralInputManager>();
                                instance.InitializeSingleton();

                                Debug.Log("GeneralInputManager: Created new singleton instance");
                            }
                        }
                    }
                    return instance;
                }
            }
        }
        public static bool HasInstance =>
            instance != null && (!applicationIsQuitting || Application.isEditor);

        private void InitializeSingleton()
        {
            if (persistAcrossScenes)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        void Awake()
        {
            // Reset the quitting flag when a new instance is created
            applicationIsQuitting = false;

            if (instance == null)
            {
                instance = this;
                InitializeSingleton();
                InitializeInputSystem();
            }
            else if (instance != this)
            {
                if (transferDataOnDuplicate && instance != null)
                {
                    TransferDataToExistingInstance();
                }
                Destroy(gameObject);
                return;
            }
        }

        void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // In editor, this is called when entering/exiting play mode
            if (pauseStatus && Application.isEditor)
            {
                Debug.Log("GeneralInputManager: Application paused (entering/exiting play mode)");
                // Don't set applicationIsQuitting here in editor
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                CleanupInputSystem();
                instance = null;

                // Only set quitting flag if we're actually quitting the application
                // Not just destroying this instance
                if (Application.isPlaying && !Application.isEditor)
                {
                    applicationIsQuitting = true;
                }
            }
        }

        private void TransferDataToExistingInstance()
        {
            if (instance == null)
                return;

            //* Transfer settings
            instance.persistAcrossScenes = persistAcrossScenes;
            instance.deadzone = deadzone;
            instance.mouseSensitivity = mouseSensitivity;
            instance.scrollSensitivity = scrollSensitivity;
            instance.inputBufferTime = inputBufferTime;
            instance.comboTimeWindow = comboTimeWindow;
            instance.updateFrequency = updateFrequency;
            instance.customUpdateInterval = customUpdateInterval;
            instance.enableInputLocking = enableInputLocking;
            instance.enableInputHistory = enableInputHistory;
            instance.historySize = historySize;
            instance.debugMode = debugMode;
            instance.showDebugOverlay = showDebugOverlay;
        }

        [Header("Singleton Settings")]
        [SerializeField]
        private bool persistAcrossScenes = true;

        [SerializeField]
        private bool transferDataOnDuplicate = true;

        [Header("Input Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float deadzone = 0.2f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float mouseSensitivity = 1f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float scrollSensitivity = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float inputBufferTime = 0.2f;

        [SerializeField]
        [Range(0.1f, 2f)]
        private float comboTimeWindow = 0.5f;

        [Header("Performance")]
        [SerializeField]
        private InputUpdateFrequency updateFrequency = InputUpdateFrequency.EveryFrame;

        [SerializeField]
        [Range(0.016f, 0.5f)]
        private float customUpdateInterval = 0.033f; // ~30 FPS

        [SerializeField]
        private bool enableInputLocking = true;

        [SerializeField]
        private bool enableInputHistory = true;

        [SerializeField]
        [Range(10, 100)]
        private int historySize = 30;

        [Header("Debug")]
        [SerializeField]
        private bool debugMode = false;

        [SerializeField]
        private bool showDebugOverlay = false;

        // Input State Caching
        private Dictionary<string, object> inputCache = new Dictionary<string, object>();
        private Dictionary<string, float> inputHoldDurations = new Dictionary<string, float>();
        private Dictionary<string, float> inputLastPressTime = new Dictionary<string, float>();

        // Input Buffering
        private Dictionary<string, Queue<float>> inputBuffers =
            new Dictionary<string, Queue<float>>();

        // Input Locking
        private HashSet<string> lockedInputs = new HashSet<string>();
        private bool globalInputLock = false;

        // Input History
        private Queue<InputHistoryEntry> inputHistory = new Queue<InputHistoryEntry>();

        // Performance
        private float lastUpdateTime = 0f;
        private int framesSinceLastUpdate = 0;

        // Events
        public UnityEvent<string> OnInputPressed = new UnityEvent<string>();
        public UnityEvent<string> OnInputReleased = new UnityEvent<string>();
        public UnityEvent<string, float> OnInputHeld = new UnityEvent<string, float>();
        public UnityEvent<string> OnInputBuffered = new UnityEvent<string>();

        // Input Profiles
        private Dictionary<string, InputProfile> inputProfiles =
            new Dictionary<string, InputProfile>();
        private string currentProfile = "default";

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Start()
        {
            InitializeInputProfiles();
            StartCoroutine(UpdateInputSystem());
        }

        void Update()
        {
            if (debugMode)
            {
                UpdateDisplayValues();
            }

            if (updateFrequency == InputUpdateFrequency.EveryFrame)
            {
                UpdateInputCache();
                ProcessInputBuffers();
                UpdateHoldDurations();
            }
        }

        void OnEnable()
        {
            if (controls != null)
            {
                controls.Enable();
                RegisterInputCallbacks();
            }
        }

        void OnDisable()
        {
            if (controls != null)
            {
                UnregisterInputCallbacks();
                controls.Disable();
            }
        }

        void OnGUI()
        {
            if (showDebugOverlay && debugMode)
            {
                DrawDebugOverlay();
            }
        }

        private void InitializeInputSystem()
        {
            try
            {
                controls = new GeneralControls();
                InitializeInputCache();
                InitializeInputBuffers();
                InitializeHoldDurations();

                if (enableInputHistory)
                {
                    inputHistory = new Queue<InputHistoryEntry>();
                }

                Debug.Log("GeneralInputManager: Input system initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"GeneralInputManager: Failed to initialize input system - {e.Message}"
                );
            }
        }

        private void CleanupInputSystem()
        {
            if (controls != null)
            {
                UnregisterInputCallbacks();
                controls.Disable();
                controls.Dispose();
                controls = null;
            }

            inputCache?.Clear();
            inputHoldDurations?.Clear();
            inputLastPressTime?.Clear();
            inputBuffers?.Clear();
            lockedInputs?.Clear();
            inputHistory?.Clear();
            inputProfiles?.Clear();
        }

        private IEnumerator UpdateInputSystem()
        {
            while (this != null)
            {
                if (updateFrequency == InputUpdateFrequency.Custom)
                {
                    UpdateInputCache();
                    ProcessInputBuffers();
                    UpdateHoldDurations();
                    yield return new WaitForSeconds(customUpdateInterval);
                }
                else if (updateFrequency == InputUpdateFrequency.FixedUpdate)
                {
                    yield return new WaitForFixedUpdate();
                    UpdateInputCache();
                    ProcessInputBuffers();
                    UpdateHoldDurations();
                }
                else
                {
                    yield return null;
                }
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        public Vector2 GetPlayerWASDInput()
        {
            if (IsInputLocked("WASD") || controls == null)
                return Vector2.zero;

            Vector2 input = controls.General_Keyboard.Walking.ReadValue<Vector2>();
            return ApplyDeadzone(input);
        }

        public bool GetPlayerSprintInput(bool held = false)
        {
            if (IsInputLocked("Sprint") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Keyboard.Sprinting.IsPressed();
                UpdateInputHoldDuration("Sprint", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Keyboard.Sprinting.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("Sprint");
                TriggerInputEvent("Sprint", true);
            }

            return wasPressed || IsInputBuffered("Sprint");
        }

        public bool GetPlayerJumpInput(bool held = false)
        {
            if (IsInputLocked("Jump") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Keyboard.Jumping.IsPressed();
                UpdateInputHoldDuration("Jump", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Keyboard.Jumping.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("Jump");
                TriggerInputEvent("Jump", true);
            }

            return wasPressed || IsInputBuffered("Jump");
        }

        public bool GetPlayerInteractInput(bool held = false)
        {
            if (IsInputLocked("Interact") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Keyboard.Interacting.IsPressed();
                UpdateInputHoldDuration("Interact", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Keyboard.Interacting.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("Interact");
                TriggerInputEvent("Interact", true);
            }

            return wasPressed || IsInputBuffered("Interact");
        }

        public bool GetPlayerPrimaryMouse(bool held = false)
        {
            if (IsInputLocked("PrimaryMouse") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Mouse.Primary.IsPressed();
                UpdateInputHoldDuration("PrimaryMouse", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Mouse.Primary.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("PrimaryMouse");
                TriggerInputEvent("PrimaryMouse", true);
            }

            return wasPressed || IsInputBuffered("PrimaryMouse");
        }

        public bool GetPlayerSecondaryMouse(bool held = false)
        {
            if (IsInputLocked("SecondaryMouse") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Mouse.Secondary.IsPressed();
                UpdateInputHoldDuration("SecondaryMouse", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Mouse.Secondary.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("SecondaryMouse");
                TriggerInputEvent("SecondaryMouse", true);
            }

            return wasPressed || IsInputBuffered("SecondaryMouse");
        }

        public bool GetPlayerMMB(bool held = false)
        {
            if (IsInputLocked("MMB") || controls == null)
                return false;

            if (held)
            {
                bool isPressed = controls.General_Mouse.MMB.IsPressed();
                UpdateInputHoldDuration("MMB", isPressed);
                return isPressed;
            }

            bool wasPressed = controls.General_Mouse.MMB.WasPressedThisFrame();
            if (wasPressed)
            {
                BufferInput("MMB");
                TriggerInputEvent("MMB", true);
            }

            return wasPressed || IsInputBuffered("MMB");
        }

        public float GetPlayerScroll()
        {
            if (IsInputLocked("Scroll") || controls == null)
                return 0f;

            float scroll = controls.General_Mouse.Scroll.ReadValue<float>();
            return scroll * scrollSensitivity;
        }

        public Vector2 GetPlayerMouseMovement()
        {
            if (IsInputLocked("MouseMovement") || controls == null)
                return Vector2.zero;

            Vector2 movement = controls.General_Mouse.MouseMovement.ReadValue<Vector2>();
            return movement * mouseSensitivity;
        }

        public float GetInputHoldDuration(string inputName)
        {
            return inputHoldDurations.ContainsKey(inputName) ? inputHoldDurations[inputName] : 0f;
        }

        public bool GetInputDoubleTap(string inputName)
        {
            if (inputLastPressTime.ContainsKey(inputName))
            {
                float timeSinceLastPress = Time.time - inputLastPressTime[inputName];
                return timeSinceLastPress <= comboTimeWindow;
            }
            return false;
        }

        public InputAction GetRawInputAction(string actionName)
        {
            if (controls == null)
                return null;

            return actionName.ToLower() switch
            {
                "walking" or "wasd" => controls.General_Keyboard.Walking,
                "jumping" or "jump" => controls.General_Keyboard.Jumping,
                "sprinting" or "sprint" => controls.General_Keyboard.Sprinting,
                "interacting" or "interact" => controls.General_Keyboard.Interacting,
                "primary" or "primarymouse" => controls.General_Mouse.Primary,
                "secondary" or "secondarymouse" => controls.General_Mouse.Secondary,
                "mmb" or "middlemouse" => controls.General_Mouse.MMB,
                "scroll" => controls.General_Mouse.Scroll,
                "mousemovement" or "mouse" => controls.General_Mouse.MouseMovement,
                _ => null,
            };
        }

        public void LockInput(string inputName)
        {
            if (enableInputLocking)
            {
                lockedInputs.Add(inputName);
                Debug.Log($"Input '{inputName}' locked");
            }
        }

        public void UnlockInput(string inputName)
        {
            if (lockedInputs.Remove(inputName))
            {
                Debug.Log($"Input '{inputName}' unlocked");
            }
        }

        public void LockAllInputs()
        {
            globalInputLock = true;
            Debug.Log("All inputs locked globally");
        }

        public void UnlockAllInputs()
        {
            globalInputLock = false;
            lockedInputs.Clear();
            Debug.Log("All inputs unlocked");
        }

        public bool IsInputLocked(string inputName)
        {
            return globalInputLock || (enableInputLocking && lockedInputs.Contains(inputName));
        }

        public InputHistoryEntry[] GetInputHistory()
        {
            return enableInputHistory ? inputHistory.ToArray() : new InputHistoryEntry[0];
        }

        public void ClearInputHistory()
        {
            inputHistory?.Clear();
        }

        public void SetDeadzone(float newDeadzone)
        {
            deadzone = Mathf.Clamp01(newDeadzone);
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
        }

        public void SetScrollSensitivity(float sensitivity)
        {
            scrollSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
        }

        public void SetInputBufferTime(float bufferTime)
        {
            inputBufferTime = Mathf.Clamp01(bufferTime);
        }

        private Vector2 ApplyDeadzone(Vector2 input)
        {
            if (input.magnitude < deadzone)
                return Vector2.zero;

            return input.normalized * ((input.magnitude - deadzone) / (1f - deadzone));
        }

        private void InitializeInputCache()
        {
            inputCache = new Dictionary<string, object>();
        }

        private void InitializeInputBuffers()
        {
            inputBuffers = new Dictionary<string, Queue<float>>();
            string[] inputNames =
            {
                "Jump",
                "Sprint",
                "Interact",
                "PrimaryMouse",
                "SecondaryMouse",
                "MMB",
            };

            foreach (string inputName in inputNames)
            {
                inputBuffers[inputName] = new Queue<float>();
            }
        }

        private void InitializeHoldDurations()
        {
            inputHoldDurations = new Dictionary<string, float>();
            inputLastPressTime = new Dictionary<string, float>();
        }

        private void UpdateInputCache()
        {
            if (controls == null)
                return;

            inputCache["WASD"] = GetPlayerWASDInput();
            inputCache["Sprint"] = GetPlayerSprintInput(true);
            inputCache["Jump"] = GetPlayerJumpInput(true);
            inputCache["Interact"] = GetPlayerInteractInput(true);
            inputCache["PrimaryMouse"] = GetPlayerPrimaryMouse(true);
            inputCache["SecondaryMouse"] = GetPlayerSecondaryMouse(true);
            inputCache["MMB"] = GetPlayerMMB(true);
            inputCache["Scroll"] = GetPlayerScroll();
            inputCache["MouseMovement"] = GetPlayerMouseMovement();
        }

        private void UpdateDisplayValues()
        {
            cachedWASDInput = GetPlayerWASDInput();
            cachedSprintInput = GetPlayerSprintInput(true);
            cachedJumpInput = GetPlayerJumpInput(true);
            cachedInteractInput = GetPlayerInteractInput(true);
            cachedPrimaryMouse = GetPlayerPrimaryMouse(true);
            cachedSecondaryMouse = GetPlayerSecondaryMouse(true);
            cachedMMBInput = GetPlayerMMB(true);
            cachedScrollInput = GetPlayerScroll();
            cachedMouseMovement = GetPlayerMouseMovement();
        }

        private void BufferInput(string inputName)
        {
            if (inputBuffers.ContainsKey(inputName))
            {
                inputBuffers[inputName].Enqueue(Time.time);
                OnInputBuffered?.Invoke(inputName);

                if (enableInputHistory)
                {
                    AddToInputHistory(inputName, "Buffered");
                }
            }
        }

        private bool IsInputBuffered(string inputName)
        {
            if (!inputBuffers.ContainsKey(inputName))
                return false;

            var buffer = inputBuffers[inputName];
            while (buffer.Count > 0 && Time.time - buffer.Peek() > inputBufferTime)
            {
                buffer.Dequeue();
            }

            if (buffer.Count > 0)
            {
                buffer.Dequeue();
                return true;
            }

            return false;
        }

        private void ProcessInputBuffers()
        {
            foreach (var buffer in inputBuffers.Values)
            {
                while (buffer.Count > 0 && Time.time - buffer.Peek() > inputBufferTime)
                {
                    buffer.Dequeue();
                }
            }
        }

        private void UpdateInputHoldDuration(string inputName, bool isPressed)
        {
            if (isPressed)
            {
                if (inputHoldDurations.ContainsKey(inputName))
                {
                    inputHoldDurations[inputName] += Time.deltaTime;
                }
                else
                {
                    inputHoldDurations[inputName] = Time.deltaTime;
                }

                OnInputHeld?.Invoke(inputName, inputHoldDurations[inputName]);
            }
            else
            {
                inputHoldDurations[inputName] = 0f;
            }
        }

        private void UpdateHoldDurations()
        {
            // This method is called by the update system
        }

        private static void ResetPerformanceMetrics()
        {
            if (Instance != null)
            {
                Instance.inputCache.Clear();
                Instance.inputHoldDurations.Clear();
                Instance.inputLastPressTime.Clear();
                Instance.inputBuffers.Clear();
                Instance.inputProfiles.Clear();
            }

            Debug.Log("<color=cyan>[Input Manager]</color> Performance metrics reset");
        }

        /// <summary>
        /// Reset the singleton state - useful for editor play mode transitions
        /// </summary>
        public static void ResetSingletonState()
        {
            applicationIsQuitting = false;
            if (instance != null)
            {
                Debug.Log("GeneralInputManager: Singleton state reset");
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

        private void TriggerInputEvent(string inputName, bool pressed)
        {
            if (pressed)
            {
                OnInputPressed?.Invoke(inputName);
                inputLastPressTime[inputName] = Time.time;

                if (enableInputHistory)
                {
                    AddToInputHistory(inputName, "Pressed");
                }
            }
            else
            {
                OnInputReleased?.Invoke(inputName);

                if (enableInputHistory)
                {
                    AddToInputHistory(inputName, "Released");
                }
            }
        }

        private void AddToInputHistory(string inputName, string action)
        {
            if (!enableInputHistory)
                return;

            var entry = new InputHistoryEntry
            {
                InputName = inputName,
                Action = action,
                Timestamp = Time.time,
                FrameCount = Time.frameCount,
            };

            inputHistory.Enqueue(entry);

            while (inputHistory.Count > historySize)
            {
                inputHistory.Dequeue();
            }
        }

        private void InitializeInputProfiles()
        {
            // Default profile is already active
            inputProfiles["default"] = new InputProfile
            {
                Name = "Default",
                Deadzone = deadzone,
                MouseSensitivity = mouseSensitivity,
                ScrollSensitivity = scrollSensitivity,
            };
        }

        private void RegisterInputCallbacks()
        {
            // Register callbacks for input actions if needed
        }

        private void UnregisterInputCallbacks()
        {
            // Unregister callbacks for input actions if needed
        }

        private void DrawDebugOverlay()
        {
            if (!debugMode || !showDebugOverlay)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

#if UNITY_EDITOR
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUILayout.Label("=== Input Manager Debug ===", headerStyle);
#else
            GUILayout.Label("=== Input Manager Debug ===", GUI.skin.label);
#endif
            GUILayout.Space(5);

            // Console clear buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Console", GUILayout.Width(100)))
            {
                ClearDebugLog.ClearWithMessage("Manual clear from Input Manager");
            }
            if (GUILayout.Button("Clear All", GUILayout.Width(70)))
            {
                ClearDebugLog.Clear();
                ResetPerformanceMetrics();
                Debug.Log(
                    "<color=yellow>[Input Manager]</color> Console cleared and metrics reset"
                );
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.Label($"WASD: {cachedWASDInput}");
            GUILayout.Label($"Sprint: {cachedSprintInput}");
            GUILayout.Label($"Jump: {cachedJumpInput}");
            GUILayout.Label($"Interact: {cachedInteractInput}");
            GUILayout.Label($"Mouse Primary: {cachedPrimaryMouse}");
            GUILayout.Label($"Mouse Secondary: {cachedSecondaryMouse}");
            GUILayout.Label($"Mouse Middle: {cachedMMBInput}");
            GUILayout.Label($"Scroll: {cachedScrollInput:F2}");
            GUILayout.Label($"Mouse Movement: {cachedMouseMovement}");

            GUILayout.Space(10);
            GUILayout.Label($"Locked Inputs: {lockedInputs.Count}");
            GUILayout.Label($"Global Lock: {globalInputLock}");
            GUILayout.Label($"Buffer Count: {inputBuffers.Values.Sum(b => b.Count)}");
            GUILayout.Label($"History Count: {inputHistory.Count}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        //* ╔═══════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚═══════════════════════════════╝
    }

    public enum InputUpdateFrequency
    {
        EveryFrame, //* Update every Update() call
        FixedUpdate, //* Update every FixedUpdate() call
        Custom, //* Update at custom interval
    }
}
