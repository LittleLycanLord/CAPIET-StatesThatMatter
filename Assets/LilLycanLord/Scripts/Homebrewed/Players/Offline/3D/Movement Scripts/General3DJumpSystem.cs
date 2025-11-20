using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// General 3D jump system that integrates with PlayerInputHandler and General3DMovementCore
    /// Handles advanced jumping mechanics, coyote time, and jump buffering
    /// Uses movement core for ground detection and velocity manipulation
    /// </summary>
    [RequireComponent(typeof(GeneralPlayerInputHandler))]
    [RequireComponent(typeof(General3DMovementCore))]
    public class General3DJumpSystem : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        // Component References
        private General3DMovementCore coreMovement;
        private GeneralPlayerInputHandler inputHandler;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Space(10)]
        [Header("Jump Settings")]
        [SerializeField]
        private bool enableJumping = true;

        [SerializeField]
        [Range(1, 5)]
        private int maxJumps = 1;

        [SerializeField]
        [Range(1f, 20f)]
        private float jumpHeight = 2f;

        [SerializeField]
        [Range(0.5f, 2f)]
        private float jumpModifier = 1f;

        [Header("Advanced Jump Mechanics")]
        [SerializeField]
        private bool enableVariableJumpHeight = true;

        [SerializeField]
        [Range(0.5f, 2f)]
        private float lowJumpMultiplier = 1.5f;

        [SerializeField]
        private bool enableCoyoteTime = true;

        [SerializeField]
        [Range(0.1f, 1f)]
        private float coyoteTime = 0.5f;

        [SerializeField]
        private bool enableJumpBuffer = false;

        [SerializeField]
        [Range(0.0f, 1f)]
        private float jumpBufferTime = 0.0f;

        [Header("Air Control")]
        [SerializeField]
        [Range(0f, 1f)]
        private float airControl = 0.5f;

        [SerializeField]
        [Range(0.5f, 3f)]
        private float fallMultiplier = 2.5f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // Jump State
        public bool IsJumping { get; private set; }
        public bool IsFalling { get; private set; }
        public bool IsGrounded => coreMovement != null ? coreMovement.IsGrounded : false;
        public int CurrentJumps { get; private set; }
        public int RemainingJumps => maxJumps - CurrentJumps;
        public float TimeInAir { get; private set; }
        public float CoyoteTimer { get; private set; }
        public bool CanJump =>
            (IsGrounded || CoyoteTimer > 0f || CurrentJumps < maxJumps) && enableJumping;

        // Internal State
        private bool wasGrounded;
        private float jumpBufferTimer;
        private bool jumpBuffered;
        private Vector3 lastVelocity;
        private float jumpStartTime;

        // Events
        public System.Action OnJumpStarted;
        public System.Action OnJumpPeak;
        public System.Action OnLanded;
        public System.Action<int> OnMultiJump;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            inputHandler = GetComponent<GeneralPlayerInputHandler>();
            coreMovement = GetComponent<General3DMovementCore>();
        }

        void Start()
        {
            // Subscribe to input events
            if (inputHandler != null)
            {
                inputHandler.OnJumpPressed += OnJumpPressed;
                inputHandler.OnJumpReleased += OnJumpReleased;
            }
        }

        void Update()
        {
            if (!enableJumping)
                return;

            UpdateGroundState();
            UpdateJumpState();
            UpdateCoyoteTime();
            UpdateJumpBuffer();
            ProcessJumpInput();
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void UpdateGroundState()
        {
            wasGrounded = IsGrounded;

            // Handle landing - IsGrounded is now provided by movement core
            if (IsGrounded && !wasGrounded)
            {
                OnLanded?.Invoke();
                CurrentJumps = 0;
                IsJumping = false;
                TimeInAir = 0f;
            }

            // Update time in air
            if (!IsGrounded)
            {
                TimeInAir += Time.deltaTime;
            }
            else
            {
                TimeInAir = 0f;
            }
        }

        private void UpdateJumpState()
        {
            if (coreMovement == null)
                return;

            Vector3 currentVelocity = coreMovement.Velocity;

            // Detect jump start
            if (!IsJumping && currentVelocity.y > 0.1f && !IsGrounded)
            {
                IsJumping = true;
                jumpStartTime = Time.time;
                OnJumpStarted?.Invoke();
            }

            // Detect jump peak
            if (IsJumping && currentVelocity.y <= 0f && lastVelocity.y > 0f)
            {
                OnJumpPeak?.Invoke();
            }

            // Update falling state
            IsFalling = !IsGrounded && currentVelocity.y < -0.1f;

            // Apply enhanced gravity for better jump feel
            if (enableVariableJumpHeight && IsFalling)
            {
                ApplyEnhancedGravity();
            }

            lastVelocity = currentVelocity;
        }

        private void UpdateCoyoteTime()
        {
            if (!enableCoyoteTime)
            {
                CoyoteTimer = 0f;
                return;
            }

            if (IsGrounded)
            {
                CoyoteTimer = coyoteTime;
            }
            else if (CoyoteTimer > 0f)
            {
                CoyoteTimer -= Time.deltaTime;
            }
        }

        private void UpdateJumpBuffer()
        {
            if (!enableJumpBuffer)
            {
                jumpBuffered = false;
                jumpBufferTimer = 0f;
                return;
            }

            if (jumpBuffered && jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;

                if (jumpBufferTimer <= 0f)
                {
                    jumpBuffered = false;
                }
            }
        }

        private void ProcessJumpInput()
        {
            // Process buffered jump
            if (jumpBuffered && CanJump)
            {
                ExecuteJump();
                jumpBuffered = false;
                jumpBufferTimer = 0f;
            }

            // Variable jump height
            if (enableVariableJumpHeight && IsJumping && !inputHandler.IsJumpHeld)
            {
                ApplyVariableJumpHeight();
            }
        }

        private void OnJumpPressed()
        {
            if (!enableJumping)
                return;

            if (CanJump)
            {
                ExecuteJump();
            }
            else if (enableJumpBuffer)
            {
                // Buffer the jump input
                jumpBuffered = true;
                jumpBufferTimer = jumpBufferTime;

                // Consume the input in the input handler
                inputHandler.ConsumeJumpInput();
            }
        }

        private void OnJumpReleased()
        {
            // Handle jump release for variable jump height
        }

        private void ExecuteJump()
        {
            if (coreMovement == null)
                return;

            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y) * jumpModifier;

            // Apply jump force
            coreMovement.SetVerticalVelocity(jumpVelocity);

            // Update jump state
            if (IsGrounded || CoyoteTimer > 0f)
            {
                // First jump (ground jump)
                CurrentJumps = 1;
                CoyoteTimer = 0f; // Consume coyote time
            }
            else
            {
                // Multi-jump
                CurrentJumps++;
                OnMultiJump?.Invoke(CurrentJumps);
            }

            IsJumping = true;
            jumpStartTime = Time.time;
            OnJumpStarted?.Invoke();
        }

        private void ApplyEnhancedGravity()
        {
            if (coreMovement == null)
                return;

            Vector3 currentVelocity = coreMovement.Velocity;
            float extraGravity = Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            coreMovement.AddImpulse(new Vector3(0, extraGravity, 0));
        }

        private void ApplyVariableJumpHeight()
        {
            if (coreMovement == null)
                return;

            Vector3 currentVelocity = coreMovement.Velocity;
            if (currentVelocity.y > 0f)
            {
                float extraGravity = Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
                coreMovement.AddImpulse(new Vector3(0, extraGravity, 0));
            }
        }

        public void SetJumpHeight(float height)
        {
            jumpHeight = Mathf.Clamp(height, 1f, 20f);
        }

        public void SetMaxJumps(int jumps)
        {
            maxJumps = Mathf.Clamp(jumps, 1, 5);
        }

        public void ResetJumps()
        {
            CurrentJumps = 0;
        }

        public void EnableJumping()
        {
            enableJumping = true;
        }

        public void DisableJumping()
        {
            enableJumping = false;
        }

        public JumpInfo GetJumpInfo()
        {
            return new JumpInfo
            {
                IsJumping = IsJumping,
                IsFalling = IsFalling,
                IsGrounded = IsGrounded,
                CurrentJumps = CurrentJumps,
                RemainingJumps = RemainingJumps,
                TimeInAir = TimeInAir,
                CoyoteTimer = CoyoteTimer,
                CanJump = CanJump,
            };
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        void OnDestroy()
        {
            // Unsubscribe from events
            if (inputHandler != null)
            {
                inputHandler.OnJumpPressed -= OnJumpPressed;
                inputHandler.OnJumpReleased -= OnJumpReleased;
            }
        }

        void OnValidate()
        {
            // Clamp values in editor
            maxJumps = Mathf.Clamp(maxJumps, 1, 5);
            jumpHeight = Mathf.Clamp(jumpHeight, 1f, 20f);
            jumpModifier = Mathf.Clamp(jumpModifier, 0.5f, 2f);
            lowJumpMultiplier = Mathf.Clamp(lowJumpMultiplier, 0.5f, 2f);
            coyoteTime = Mathf.Clamp(coyoteTime, 0.1f, 1f);
            jumpBufferTime = Mathf.Clamp(jumpBufferTime, 0.1f, 1f);
            airControl = Mathf.Clamp01(airControl);
            fallMultiplier = Mathf.Clamp(fallMultiplier, 0.5f, 3f);
        }
    }

    [System.Serializable]
    public struct JumpInfo
    {
        public bool IsJumping;
        public bool IsFalling;
        public bool IsGrounded;
        public int CurrentJumps;
        public int RemainingJumps;
        public float TimeInAir;
        public float CoyoteTimer;
        public bool CanJump;
    }
}
