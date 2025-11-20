using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    [System.Serializable]
    public struct MovementInfo
    {
        public bool IsMoving;
        public bool IsSprinting;
        public bool IsGrounded;
        public bool IsJumping;
        public bool IsFalling;
        public float CurrentSpeed;
        public Vector3 Velocity;
        public float TimeInAir;
        public float CoyoteTimer;
        public int JumpsQueued;
    }

    /// <summary>
    /// Streamlined 3D Player Controller that coordinates all movement systems
    /// This replaces the monolithic General3DPlayerMovement with a modular approach
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(General3DMovementCore))]
    [RequireComponent(typeof(GeneralPlayerInputHandler))]
    public class GeneralPlayerController : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        // Component References
        private CharacterController characterController;
        private General3DMovementCore coreMovement;
        private GeneralPlayerInputHandler inputHandler;
        private General3DJumpSystem jumpSystem;
        private General3DMovementAnimator movementAnimator;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝


        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("System Control")]
        [SerializeField]
        private bool enableAllSystems = true;

        [SerializeField]
        private bool autoSetupComponents = true;

        [Header("Debug & Performance")]
        [SerializeField]
        private bool enableDebugMode = false;

        [SerializeField]
        private bool showPerformanceStats = false;

        [SerializeField]
        private bool autoClearConsoleOnStart = false;

        [Header("Events")]
        public UnityEvent OnSystemsInitialized = new UnityEvent();
        public UnityEvent OnMovementEnabled = new UnityEvent();
        public UnityEvent OnMovementDisabled = new UnityEvent();
        public UnityEvent<MovementInfo> OnMovementStateChanged = new UnityEvent<MovementInfo>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // System State
        public bool IsInitialized { get; private set; }
        public bool SystemsEnabled => enableAllSystems;

        // Performance Tracking
        private float performanceTimer;
        private int frameCount;
        private float lastFPS;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            InitializeComponents();
        }

        void Start()
        {
            InitializeSystems();

            if (autoClearConsoleOnStart)
            {
                ClearDebugLog.ClearWithMessage("Streamlined 3D Player Controller Initialized");
            }
        }

        void Update()
        {
            if (!IsInitialized || !enableAllSystems)
                return;

            UpdatePerformanceTracking();
            UpdateMovementState();
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        private void InitializeComponents()
        {
            // Get required components
            characterController = GetComponent<CharacterController>();
            coreMovement = GetComponent<General3DMovementCore>();
            inputHandler = GetComponent<GeneralPlayerInputHandler>();

            // Get optional components
            jumpSystem = GetComponent<General3DJumpSystem>();
            movementAnimator = GetComponent<General3DMovementAnimator>();

            // Auto-setup missing components if enabled
            if (autoSetupComponents)
            {
                SetupMissingComponents();
            }

            // Validate critical components
            if (characterController == null)
            {
                Debug.LogError(
                    $"Streamlined3DPlayerController: CharacterController missing on {gameObject.name}"
                );
                enabled = false;
                return;
            }

            if (coreMovement == null)
            {
                Debug.LogError(
                    $"Streamlined3DPlayerController: Core3DMovement missing on {gameObject.name}"
                );
                enabled = false;
                return;
            }

            if (inputHandler == null)
            {
                Debug.LogError(
                    $"Streamlined3DPlayerController: GeneralPlayerInputHandler missing on {gameObject.name}"
                );
                enabled = false;
                return;
            }

            if (coreMovement == null)
            {
                Debug.LogError(
                    $"Streamlined3DPlayerController: General3DMovementCore missing on {gameObject.name}"
                );
                enabled = false;
                return;
            }
        }

        private void SetupMissingComponents()
        {
            // Add jump system if missing
            if (jumpSystem == null)
            {
                jumpSystem = gameObject.AddComponent<General3DJumpSystem>();
                Debug.Log("Streamlined3DPlayerController: Auto-added General3DJumpSystem");
            }

            // Add movement animator if missing and we have an Animator
            if (movementAnimator == null && GetComponent<Animator>() != null)
            {
                movementAnimator = gameObject.AddComponent<General3DMovementAnimator>();
                Debug.Log("Streamlined3DPlayerController: Auto-added Movement3DAnimator");
            }
        }

        private void InitializeSystems()
        {
            // Subscribe to system events
            SubscribeToEvents();

            // Configure systems
            ConfigureSystems();

            IsInitialized = true;
            OnSystemsInitialized.Invoke();

            if (enableDebugMode)
            {
                Debug.Log("Streamlined3DPlayerController: All systems initialized successfully");
                LogSystemConfiguration();
            }
        }

        private void SubscribeToEvents()
        {
            // Subscribe to movement events
            if (coreMovement != null)
            {
                coreMovement.OnStartedMoving += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player started moving");
                };
                coreMovement.OnStoppedMoving += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player stopped moving");
                };
                coreMovement.OnStartedSprinting += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player started sprinting");
                };
                coreMovement.OnStoppedSprinting += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player stopped sprinting");
                };
            }

            // Subscribe to jump events
            if (jumpSystem != null)
            {
                jumpSystem.OnJumpStarted += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player jumped");
                };
                jumpSystem.OnLanded += () =>
                {
                    if (enableDebugMode)
                        Debug.Log("Player landed");
                };
                jumpSystem.OnMultiJump += (jumpCount) =>
                {
                    if (enableDebugMode)
                        Debug.Log($"Player multi-jumped (jump #{jumpCount})");
                };
            }
        }

        private void ConfigureSystems()
        {
            // Set up camera reference for enhanced movement
            if (coreMovement != null && Camera.main != null)
            {
                coreMovement.SetCameraReference(Camera.main.transform);
            }

            // Configure character controller settings
            if (characterController != null)
            {
                characterController.minMoveDistance = 0f;
            }
        }

        private void UpdatePerformanceTracking()
        {
            if (!showPerformanceStats)
                return;

            frameCount++;
            performanceTimer += Time.deltaTime;

            if (performanceTimer >= 1.0f)
            {
                lastFPS = frameCount / performanceTimer;
                frameCount = 0;
                performanceTimer = 0f;

                if (enableDebugMode)
                {
                    Debug.Log($"Player Controller Performance: {lastFPS:F1} FPS");
                }
            }
        }

        private void UpdateMovementState()
        {
            // Broadcast movement state changes
            MovementInfo movementInfo = GetMovementInfo();
            OnMovementStateChanged.Invoke(movementInfo);
        }

        private void LogSystemConfiguration()
        {
            Debug.Log($"=== Streamlined3D Player Controller Configuration ===");
            Debug.Log($"Core3DMovement: {(coreMovement != null ? "✓" : "✗")}");
            Debug.Log($"GeneralPlayerInputHandler: {(inputHandler != null ? "✓" : "✗")}");
            Debug.Log($"General3DMovementCore: {(coreMovement != null ? "✓" : "✗")}");
            Debug.Log($"General3DJumpSystem: {(jumpSystem != null ? "✓" : "✗")}");
            Debug.Log($"Movement3DAnimator: {(movementAnimator != null ? "✓" : "✗")}");
            Debug.Log($"CharacterController: {(characterController != null ? "✓" : "✗")}");
            Debug.Log($"================================================");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        // Public API Methods
        public void EnableAllSystems()
        {
            enableAllSystems = true;

            if (inputHandler != null)
                inputHandler.EnableInput();
            if (coreMovement != null)
                coreMovement.EnableMovement();
            if (jumpSystem != null)
                jumpSystem.EnableJumping();
            if (movementAnimator != null)
                movementAnimator.EnableAnimations();

            OnMovementEnabled.Invoke();
        }

        public void DisableAllSystems()
        {
            enableAllSystems = false;

            if (inputHandler != null)
                inputHandler.DisableInput();
            if (coreMovement != null)
                coreMovement.DisableMovement();
            if (jumpSystem != null)
                jumpSystem.DisableJumping();
            if (movementAnimator != null)
                movementAnimator.DisableAnimations();

            OnMovementDisabled.Invoke();
        }

        public MovementInfo GetMovementInfo()
        {
            MovementInfo info = new MovementInfo();

            if (coreMovement != null)
            {
                info.IsMoving = coreMovement.IsMoving;
                info.IsSprinting = coreMovement.IsSprinting;
                info.CurrentSpeed = coreMovement.CurrentSpeed;
            }

            if (jumpSystem != null)
            {
                JumpInfo jumpInfo = jumpSystem.GetJumpInfo();
                info.IsGrounded = jumpInfo.IsGrounded;
                info.IsJumping = jumpInfo.IsJumping;
                info.IsFalling = jumpInfo.IsFalling;
                info.TimeInAir = jumpInfo.TimeInAir;
                info.CoyoteTimer = jumpInfo.CoyoteTimer;
                info.JumpsQueued = jumpInfo.CurrentJumps;
            }

            if (coreMovement != null)
            {
                info.Velocity = coreMovement.Velocity;
            }

            return info;
        }

        public void SetMovementSpeed(float speed)
        {
            if (coreMovement != null)
            {
                // This would need to be implemented in Core3DMovement
                Debug.LogWarning(
                    "SetMovementSpeed: Method needs to be implemented in Core3DMovement"
                );
            }
        }

        public void SetSprintMultiplier(float multiplier)
        {
            if (coreMovement != null)
            {
                coreMovement.SetSprintMultiplier(multiplier);
            }
        }

        public void SetJumpHeight(float height)
        {
            if (jumpSystem != null)
            {
                jumpSystem.SetJumpHeight(height);
            }
        }

        public void Jump()
        {
            if (jumpSystem != null && jumpSystem.CanJump)
            {
                // Trigger jump through input system for consistency
                if (inputHandler != null)
                {
                    inputHandler.ConsumeJumpInput();
                }
            }
        }

        public void ResetPlayer()
        {
            if (inputHandler != null)
            {
                inputHandler.ResetInputState();
            }

            if (jumpSystem != null)
            {
                jumpSystem.ResetJumps();
            }

            if (coreMovement != null)
            {
                coreMovement.Stop();
            }
        }

        // Component Getters
        public General3DMovementCore GetCoreMovement() => coreMovement;

        public GeneralPlayerInputHandler GetInputHandler() => inputHandler;

        public General3DJumpSystem GetJumpSystem() => jumpSystem;

        public General3DMovementAnimator GetMovementAnimator() => movementAnimator;

        public CharacterController GetCharacterController() => characterController;

        void OnValidate()
        {
            // Validation happens here
        }

        void OnDrawGizmosSelected()
        {
            if (enableDebugMode && Application.isPlaying)
            {
                // Draw debug information
                Gizmos.color = Color.blue;
                if (coreMovement != null)
                {
                    Gizmos.DrawRay(transform.position, coreMovement.Velocity);
                }
            }
        }
    }
}
