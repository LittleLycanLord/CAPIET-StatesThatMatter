using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Handles animation for 3D movement system
    /// Integrates with General3DMovementCore, General3DJumpSystem
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class General3DMovementAnimator : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        // Components
        private Animator animator;
        private General3DMovementCore enhanced3DMovement;
        private General3DJumpSystem jumpSystem;
        private General3DMovementCore coreMovement;
        private GeneralPlayerInputHandler inputHandler;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝


        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Animation Settings")]
        [SerializeField]
        private bool enableAnimations = true;

        [SerializeField]
        private bool useRootMotion = false;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float animationSmoothTime = 0.1f;

        [Header("Movement Parameters")]
        [SerializeField]
        private string speedParameter = "Speed";

        [SerializeField]
        private string velocityXParameter = "VelocityX";

        [SerializeField]
        private string velocityZParameter = "VelocityZ";

        [SerializeField]
        private string isMovingParameter = "IsMoving";

        [SerializeField]
        private string isSprintingParameter = "IsSprinting";

        [SerializeField]
        private string sprintProgressParameter = "SprintProgress";

        [Header("Jump Parameters")]
        [SerializeField]
        private string isGroundedParameter = "IsGrounded";

        [SerializeField]
        private string isJumpingParameter = "IsJumping";

        [SerializeField]
        private string isFallingParameter = "IsFalling";

        [SerializeField]
        private string jumpTrigger = "Jump";

        [SerializeField]
        private string landTrigger = "Land";

        [SerializeField]
        private string airTimeParameter = "AirTime";

        [Header("Advanced Parameters")]
        [SerializeField]
        private string inputMagnitudeParameter = "InputMagnitude";

        [SerializeField]
        private string turnAmountParameter = "TurnAmount";

        [SerializeField]
        private string forwardAmountParameter = "ForwardAmount";

        [Header("Animation Smoothing")]
        [SerializeField]
        private bool enableVelocitySmoothing = true;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float velocitySmoothTime = 0.1f;

        [SerializeField]
        private bool enableTurnSmoothing = true;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float turnSmoothTime = 0.2f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // Animation State
        private float currentSpeed;
        private float currentVelocityX;
        private float currentVelocityZ;
        private float currentTurnAmount;
        private float currentForwardAmount;
        private float speedVelocity;
        private float velocityXVelocity;
        private float velocityZVelocity;
        private float turnVelocity;
        private float forwardVelocity;

        // Cached parameter hashes
        private int speedHash;
        private int velocityXHash;
        private int velocityZHash;
        private int isMovingHash;
        private int isSprintingHash;
        private int sprintProgressHash;
        private int isGroundedHash;
        private int isJumpingHash;
        private int isFallingHash;
        private int jumpTriggerHash;
        private int landTriggerHash;
        private int airTimeHash;
        private int inputMagnitudeHash;
        private int turnAmountHash;
        private int forwardAmountHash;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
            animator = GetComponent<Animator>();
            enhanced3DMovement = GetComponent<General3DMovementCore>();
            jumpSystem = GetComponent<General3DJumpSystem>();
            coreMovement = GetComponent<General3DMovementCore>();
            inputHandler = GetComponent<GeneralPlayerInputHandler>();

            CacheParameterHashes();
        }

        void Start()
        {
            // Subscribe to movement events
            if (enhanced3DMovement != null)
            {
                enhanced3DMovement.OnStartedMoving += OnStartedMoving;
                enhanced3DMovement.OnStoppedMoving += OnStoppedMoving;
                enhanced3DMovement.OnStartedSprinting += OnStartedSprinting;
                enhanced3DMovement.OnStoppedSprinting += OnStoppedSprinting;
            }

            // Subscribe to jump events
            if (jumpSystem != null)
            {
                jumpSystem.OnJumpStarted += OnJumpStarted;
                jumpSystem.OnLanded += OnLanded;
            }
        }

        void Update()
        {
            if (!enableAnimations || animator == null)
                return;

            UpdateMovementAnimations();
            UpdateJumpAnimations();
            UpdateAdvancedAnimations();
        }

        private void CacheParameterHashes()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            speedHash = Animator.StringToHash(speedParameter);
            velocityXHash = Animator.StringToHash(velocityXParameter);
            velocityZHash = Animator.StringToHash(velocityZParameter);
            isMovingHash = Animator.StringToHash(isMovingParameter);
            isSprintingHash = Animator.StringToHash(isSprintingParameter);
            sprintProgressHash = Animator.StringToHash(sprintProgressParameter);
            isGroundedHash = Animator.StringToHash(isGroundedParameter);
            isJumpingHash = Animator.StringToHash(isJumpingParameter);
            isFallingHash = Animator.StringToHash(isFallingParameter);
            jumpTriggerHash = Animator.StringToHash(jumpTrigger);
            landTriggerHash = Animator.StringToHash(landTrigger);
            airTimeHash = Animator.StringToHash(airTimeParameter);
            inputMagnitudeHash = Animator.StringToHash(inputMagnitudeParameter);
            turnAmountHash = Animator.StringToHash(turnAmountParameter);
            forwardAmountHash = Animator.StringToHash(forwardAmountParameter);
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        private void UpdateMovementAnimations()
        {
            if (enhanced3DMovement == null || coreMovement == null)
                return;

            // Get movement data
            Vector3 velocity = coreMovement.Velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            // Update speed with smoothing
            float targetSpeed = enhanced3DMovement.CurrentSpeed;
            currentSpeed = Mathf.SmoothDamp(
                currentSpeed,
                targetSpeed,
                ref speedVelocity,
                animationSmoothTime
            );

            // Update velocity components with smoothing if enabled
            if (enableVelocitySmoothing)
            {
                currentVelocityX = Mathf.SmoothDamp(
                    currentVelocityX,
                    localVelocity.x,
                    ref velocityXVelocity,
                    velocitySmoothTime
                );
                currentVelocityZ = Mathf.SmoothDamp(
                    currentVelocityZ,
                    localVelocity.z,
                    ref velocityZVelocity,
                    velocitySmoothTime
                );
            }
            else
            {
                currentVelocityX = localVelocity.x;
                currentVelocityZ = localVelocity.z;
            }

            // Set animation parameters
            SetFloat(speedHash, currentSpeed);
            SetFloat(velocityXHash, currentVelocityX);
            SetFloat(velocityZHash, currentVelocityZ);
            SetBool(isMovingHash, enhanced3DMovement.IsMoving);
            SetBool(isSprintingHash, enhanced3DMovement.IsSprinting);
            SetFloat(sprintProgressHash, enhanced3DMovement.SprintProgress);
        }

        private void UpdateJumpAnimations()
        {
            if (jumpSystem == null)
                return;

            JumpInfo jumpInfo = jumpSystem.GetJumpInfo();

            SetBool(isGroundedHash, jumpInfo.IsGrounded);
            SetBool(isJumpingHash, jumpInfo.IsJumping);
            SetBool(isFallingHash, jumpInfo.IsFalling);
            SetFloat(airTimeHash, jumpInfo.TimeInAir);
        }

        private void UpdateAdvancedAnimations()
        {
            if (inputHandler == null)
                return;

            // Input magnitude
            float inputMagnitude = inputHandler.MovementInput.magnitude;
            SetFloat(inputMagnitudeHash, inputMagnitude);

            // Calculate turn and forward amounts for blend trees
            Vector2 input = inputHandler.MovementInput;
            Vector3 worldInput = transform.TransformDirection(new Vector3(input.x, 0, input.y));
            Vector3 localInput = transform.InverseTransformDirection(worldInput);

            float targetForward = localInput.z;
            float targetTurn = localInput.x;

            // Apply smoothing if enabled
            if (enableTurnSmoothing)
            {
                currentTurnAmount = Mathf.SmoothDamp(
                    currentTurnAmount,
                    targetTurn,
                    ref turnVelocity,
                    turnSmoothTime
                );
                currentForwardAmount = Mathf.SmoothDamp(
                    currentForwardAmount,
                    targetForward,
                    ref forwardVelocity,
                    turnSmoothTime
                );
            }
            else
            {
                currentTurnAmount = targetTurn;
                currentForwardAmount = targetForward;
            }

            SetFloat(turnAmountHash, currentTurnAmount);
            SetFloat(forwardAmountHash, currentForwardAmount);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        private void OnStartedMoving()
        {
            // Handle movement start animation events
        }

        private void OnStoppedMoving()
        {
            // Handle movement stop animation events
        }

        private void OnStartedSprinting()
        {
            // Handle sprint start animation events
        }

        private void OnStoppedSprinting()
        {
            // Handle sprint stop animation events
        }

        private void OnJumpStarted()
        {
            SetTrigger(jumpTriggerHash);
        }

        private void OnLanded()
        {
            SetTrigger(landTriggerHash);
        }

        // Safe parameter setters that check if parameter exists
        private void SetFloat(int hash, float value)
        {
            if (animator != null && HasParameter(hash))
            {
                animator.SetFloat(hash, value);
            }
        }

        private void SetBool(int hash, bool value)
        {
            if (animator != null && HasParameter(hash))
            {
                animator.SetBool(hash, value);
            }
        }

        private void SetTrigger(int hash)
        {
            if (animator != null && HasParameter(hash))
            {
                animator.SetTrigger(hash);
            }
        }

        private bool HasParameter(int hash)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return false;

            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.parameters[i].nameHash == hash)
                    return true;
            }
            return false;
        }

        public void EnableAnimations()
        {
            enableAnimations = true;
        }

        public void DisableAnimations()
        {
            enableAnimations = false;
        }

        public void SetParameterNames(
            string speed,
            string velocityX,
            string velocityZ,
            string isMoving,
            string isSprinting
        )
        {
            speedParameter = speed;
            velocityXParameter = velocityX;
            velocityZParameter = velocityZ;
            isMovingParameter = isMoving;
            isSprintingParameter = isSprinting;
            CacheParameterHashes();
        }

        public void SetJumpParameterNames(
            string isGrounded,
            string isJumping,
            string isFalling,
            string jump,
            string land
        )
        {
            isGroundedParameter = isGrounded;
            isJumpingParameter = isJumping;
            isFallingParameter = isFalling;
            jumpTrigger = jump;
            landTrigger = land;
            CacheParameterHashes();
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (enhanced3DMovement != null)
            {
                enhanced3DMovement.OnStartedMoving -= OnStartedMoving;
                enhanced3DMovement.OnStoppedMoving -= OnStoppedMoving;
                enhanced3DMovement.OnStartedSprinting -= OnStartedSprinting;
                enhanced3DMovement.OnStoppedSprinting -= OnStoppedSprinting;
            }

            if (jumpSystem != null)
            {
                jumpSystem.OnJumpStarted -= OnJumpStarted;
                jumpSystem.OnLanded -= OnLanded;
            }
        }

        void OnValidate()
        {
            // Clamp values in editor
            animationSmoothTime = Mathf.Clamp(animationSmoothTime, 0.1f, 5f);
            velocitySmoothTime = Mathf.Clamp(velocitySmoothTime, 0.01f, 1f);
            turnSmoothTime = Mathf.Clamp(turnSmoothTime, 0.01f, 1f);
        }
    }
}
