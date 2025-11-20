using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Comprehensive 3D movement system that integrates with PlayerInputHandler
    /// Handles movement logic, sprinting, rotation, smoothing, and physics integration
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(GeneralPlayerInputHandler))]
    public class General3DMovementCore : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        private CharacterController characterController;
        private GeneralPlayerInputHandler inputHandler;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // Movement state
        [SerializeField]
        private Vector3 velocity;

        [SerializeField]
        private bool isGrounded;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Space(10)]
        [Header("Movement Settings")]
        [SerializeField]
        private bool enableMovement = true;

        [SerializeField]
        private bool enableSprinting = true;

        [SerializeField]
        private bool faceMovementDirection = true;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float rotationSpeed = 10f;

        [Header("Basic Movement Settings")]
        [SerializeField]
        [Range(1f, 10f)]
        private float moveSpeed = 5f;

        [SerializeField]
        [Range(1f, 15f)]
        private float runSpeed = 8f;

        [SerializeField]
        [Range(5f, 30f)]
        private float gravity = 20f;

        [SerializeField]
        [Range(0.1f, 2f)]
        private float groundCheckDistance = 0.3f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float groundCheckRadius = 0.1f;

        [SerializeField]
        private LayerMask groundMask = 1;

        [SerializeField]
        [Tooltip(
            "Optional transform to use as ground check position. If null, uses character controller center."
        )]
        private Transform groundCheckTransform;

        [SerializeField]
        [Tooltip(
            "GameObjects to ignore during ground checking. The character itself is included by default."
        )]
        private GameObject[] groundCheckIgnoreList = new GameObject[0];

        [Header("Sprint Configuration")]
        [SerializeField]
        [Range(1.1f, 3.0f)]
        private float sprintMultiplier = 1.5f;

        [SerializeField]
        private bool toggleSprint = false;

        [SerializeField]
        private bool preventAirSprinting = true;

        [SerializeField]
        private bool requireMovementForSprint = true;

        [Header("Sprint Timing")]
        [SerializeField]
        [Range(0f, 5.0f)]
        private float sprintGrowthDuration = 0f;

        [SerializeField]
        [Range(0f, 5.0f)]
        private float sprintDecayDuration = 0f;

        [SerializeField]
        private AnimationCurve sprintGrowthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve sprintDecayCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Advanced Settings")]
        [SerializeField]
        private bool enableMovementSmoothing = false;

        [SerializeField]
        [Range(0.0f, 10.0f)]
        private float accelerationTime = 0.2f;

        [SerializeField]
        [Range(0.0f, 10.0f)]
        private float decelerationTime = 0.1f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private Transform cameraTransform;

        private bool wasGroundedLastFrame;
        private bool isRunning;

        // Ground detection
        private Vector3 groundCheckPosition;

        // Public Properties
        public bool IsMoving { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsSprintToggled { get; private set; }
        public bool CanSprint =>
            enableSprinting
            && (!requireMovementForSprint || IsMoving)
            && (!preventAirSprinting || IsGrounded);
        public Vector3 MovementDirection { get; private set; }
        public float CurrentSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
        public float CurrentSprintMultiplier => currentSprintMultiplier;
        public float SprintProgress => sprintProgress;

        // Internal State
        private float currentSprintMultiplier = 1f;
        private float sprintProgress = 0f;
        private bool sprintInputPressed = false;
        private bool wasSprintInputPressed = false;
        private float sprintTimer = 0f;
        private bool isSprintTransitioning = false;

        // Movement Smoothing
        private Vector3 velocitySmoothing;
        private float accelerationProgress = 0f;

        // Events
        public System.Action<bool> OnGroundedChanged;
        public System.Action<Vector3> OnVelocityChanged;
        public System.Action<float> OnSpeedChanged;
        public System.Action OnStartedMoving;
        public System.Action OnStoppedMoving;
        public System.Action OnStartedSprinting;
        public System.Action OnStoppedSprinting;
        public System.Action<float> OnSprintProgressChanged;

        // Properties
        public bool IsGrounded => isGrounded;
        public bool WasGroundedLastFrame => wasGroundedLastFrame;
        public bool IsRunning => isRunning;
        public Vector3 Velocity => velocity;
        public CharacterController Controller => characterController;
        public Transform CameraTransform => cameraTransform;
        public float MoveSpeed => moveSpeed;
        public float WalkSpeed => moveSpeed;
        public float RunSpeed => runSpeed;
        public float Gravity => gravity;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputHandler = GetComponent<GeneralPlayerInputHandler>();

            // Find the main camera or camera tagged as "MainCamera"
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                var camera = GameObject.FindGameObjectWithTag("MainCamera");
                if (camera != null)
                {
                    cameraTransform = camera.transform;
                }
            }

            // Initialize ground check radius if not set
            if (groundCheckRadius <= 0f)
            {
                groundCheckRadius = characterController.radius * 0.9f;
            }
        }

        private void Start()
        {
            // Initialize ground check position
            UpdateGroundCheckPosition();

            // Initialize ground check ignore list with character itself if empty
            if (groundCheckIgnoreList == null || groundCheckIgnoreList.Length == 0)
            {
                groundCheckIgnoreList = new GameObject[] { gameObject };
            }
            else
            {
                // Add the character itself to the ignore list if not already present
                bool characterInList = false;
                foreach (GameObject obj in groundCheckIgnoreList)
                {
                    if (obj == gameObject)
                    {
                        characterInList = true;
                        break;
                    }
                }

                if (!characterInList)
                {
                    // Expand the array to include the character
                    GameObject[] newList = new GameObject[groundCheckIgnoreList.Length + 1];
                    for (int i = 0; i < groundCheckIgnoreList.Length; i++)
                    {
                        newList[i] = groundCheckIgnoreList[i];
                    }
                    newList[groundCheckIgnoreList.Length] = gameObject;
                    groundCheckIgnoreList = newList;
                }
            }

            // Subscribe to input events
            if (inputHandler != null)
            {
                inputHandler.OnMovementInputChanged += OnMovementInputChanged;
                inputHandler.OnSprintStateChanged += OnSprintStateChanged;
            }
        }

        private void Update()
        {
            CheckGrounded();
            ApplyGravity();

            if (enableMovement && inputHandler != null)
            {
                UpdateMovementState();
                UpdateSprintState();
            }

            ProcessMovement();
        }

        private void LateUpdate()
        {
            // Update previous frame state
            wasGroundedLastFrame = isGrounded;
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (inputHandler != null)
            {
                inputHandler.OnMovementInputChanged -= OnMovementInputChanged;
                inputHandler.OnSprintStateChanged -= OnSprintStateChanged;
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        private void UpdateMovementState()
        {
            bool wasMoving = IsMoving;
            IsMoving = inputHandler.HasMovementInput && enableMovement;

            if (wasMoving != IsMoving)
            {
                if (IsMoving)
                    OnStartedMoving?.Invoke();
                else
                    OnStoppedMoving?.Invoke();
            }
        }

        private void UpdateSprintState()
        {
            bool wasSprinting = IsSprinting;
            sprintInputPressed = inputHandler.IsSprintPressed;

            // Handle sprint toggle logic
            if (toggleSprint)
            {
                // Toggle mode: pressing sprint key toggles sprint on/off
                if (sprintInputPressed && !wasSprintInputPressed)
                {
                    IsSprintToggled = !IsSprintToggled;
                }
                IsSprinting = enableSprinting && IsSprintToggled && CanSprint;
            }
            else
            {
                // Hold mode: sprint while key is held
                IsSprinting = enableSprinting && sprintInputPressed && CanSprint;
            }

            // Handle sprint progress with animation curves
            UpdateSprintProgress();

            // Calculate current sprint multiplier based on progress
            currentSprintMultiplier = Mathf.Lerp(1f, sprintMultiplier, sprintProgress);

            // Trigger events
            if (wasSprinting != IsSprinting)
            {
                if (IsSprinting)
                    OnStartedSprinting?.Invoke();
                else
                    OnStoppedSprinting?.Invoke();
            }

            wasSprintInputPressed = sprintInputPressed;
        }

        private void UpdateSprintProgress()
        {
            float previousProgress = sprintProgress;
            bool shouldBeSprinting = IsSprinting;

            if (shouldBeSprinting)
            {
                // Growing sprint
                if (sprintGrowthDuration > 0f)
                {
                    if (!isSprintTransitioning || sprintTimer == 0f)
                    {
                        sprintTimer = 0f;
                        isSprintTransitioning = true;
                    }

                    sprintTimer += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(sprintTimer / sprintGrowthDuration);
                    sprintProgress = sprintGrowthCurve.Evaluate(normalizedTime);

                    if (normalizedTime >= 1f)
                    {
                        isSprintTransitioning = false;
                        sprintProgress = 1f;
                    }
                }
                else
                {
                    // Instant sprint
                    sprintProgress = 1f;
                    isSprintTransitioning = false;
                }
            }
            else
            {
                // Decaying sprint
                if (sprintDecayDuration > 0f && sprintProgress > 0f)
                {
                    if (!isSprintTransitioning || sprintTimer == 0f)
                    {
                        sprintTimer = 0f;
                        isSprintTransitioning = true;
                    }

                    sprintTimer += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(sprintTimer / sprintDecayDuration);
                    sprintProgress = sprintDecayCurve.Evaluate(normalizedTime);

                    if (normalizedTime >= 1f)
                    {
                        isSprintTransitioning = false;
                        sprintProgress = 0f;
                    }
                }
                else
                {
                    // Instant stop
                    sprintProgress = 0f;
                    isSprintTransitioning = false;
                }
            }

            // Trigger progress event if changed
            if (Mathf.Abs(previousProgress - sprintProgress) > 0.001f)
            {
                OnSprintProgressChanged?.Invoke(sprintProgress);
            }
        }

        private void ProcessMovement()
        {
            // Handle case where movement is disabled or no input handler
            if (!enableMovement || inputHandler == null)
            {
                // Stop horizontal movement but keep vertical (gravity)
                velocity.x = 0f;
                velocity.z = 0f;
                MovementDirection = Vector3.zero;
                isRunning = false;

                // Always apply movement (including gravity)
                characterController.Move(velocity * Time.deltaTime);

                // Trigger events
                OnVelocityChanged?.Invoke(velocity);
                OnSpeedChanged?.Invoke(CurrentSpeed);
                return;
            }

            Vector2 inputDirection = inputHandler.MovementInput;

            if (inputDirection.sqrMagnitude < 0.01f)
            {
                // No input - stop horizontal movement but keep vertical (gravity)
                velocity.x = 0f;
                velocity.z = 0f;
                MovementDirection = Vector3.zero;
                isRunning = false;

                // Still need to apply movement for gravity
                characterController.Move(velocity * Time.deltaTime);

                // Trigger events
                OnVelocityChanged?.Invoke(velocity);
                OnSpeedChanged?.Invoke(CurrentSpeed);
                return;
            }

            // Convert input to world space movement direction
            Vector3 worldDirection = CalculateWorldMovementDirection(inputDirection);
            MovementDirection = worldDirection;

            // Handle rotation if enabled
            if (faceMovementDirection && worldDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Apply movement smoothing if enabled
            Vector2 finalDirection = new Vector2(worldDirection.x, worldDirection.z);
            if (enableMovementSmoothing)
            {
                finalDirection = ApplyMovementSmoothing(finalDirection);
            }

            // Calculate movement with sprint integration
            float currentMoveSpeed =
                IsSprinting && CanSprint
                    ? Mathf.Lerp(moveSpeed, runSpeed, sprintProgress) * currentSprintMultiplier
                    : moveSpeed;

            isRunning = IsSprinting && CanSprint;

            if (finalDirection.magnitude > 0.1f)
            {
                Vector3 movement =
                    new Vector3(finalDirection.x, 0, finalDirection.y)
                    * currentMoveSpeed
                    * Time.deltaTime;

                // Apply horizontal movement
                velocity.x = movement.x / Time.deltaTime;
                velocity.z = movement.z / Time.deltaTime;
            }
            else
            {
                // Stop horizontal movement when no input
                velocity.x = 0f;
                velocity.z = 0f;
            }

            // Apply the movement
            characterController.Move(velocity * Time.deltaTime);

            // Trigger events
            OnVelocityChanged?.Invoke(velocity);
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        private Vector3 CalculateWorldMovementDirection(Vector2 inputDirection)
        {
            if (cameraTransform == null)
            {
                // Fallback to world-space movement
                return new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            }

            // Get camera forward and right directions (ignore Y component)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to camera
            Vector3 moveDirection =
                cameraForward * inputDirection.y + cameraRight * inputDirection.x;
            return moveDirection.normalized;
        }

        private Vector2 ApplyMovementSmoothing(Vector2 targetDirection)
        {
            float currentAcceleration =
                targetDirection.magnitude > 0.1f ? accelerationTime : decelerationTime;

            if (targetDirection.magnitude > 0.1f)
            {
                accelerationProgress = Mathf.Min(
                    1f,
                    accelerationProgress + Time.deltaTime / currentAcceleration
                );
            }
            else
            {
                accelerationProgress = Mathf.Max(
                    0f,
                    accelerationProgress - Time.deltaTime / currentAcceleration
                );
            }

            return Vector2.Lerp(Vector2.zero, targetDirection, accelerationProgress);
        }

        private void OnMovementInputChanged(Vector2 newInput)
        {
            // Movement input change handled in Update
        }

        private void OnSprintStateChanged(bool isSprintPressed)
        {
            // Sprint state change handled in Update
        }

        /// <summary>
        /// Move the character with the given input vector (legacy method for compatibility)
        /// </summary>
        /// <param name="inputVector">Input vector (typically from WASD or joystick)</param>
        /// <param name="useRunning">Whether to use running speed (ignored - sprint system handles this)</param>
        public void Move(Vector2 inputVector, bool useRunning)
        {
            // This method is kept for compatibility but the new system handles movement automatically
            // The sprint system and input handler control movement now
        }

        /// <summary>
        /// Move the character with the given input vector (legacy method for compatibility)
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        /// <param name="deltaTime">Time since last frame (ignored - uses Time.deltaTime)</param>
        public void Move(Vector3 inputVector, float deltaTime)
        {
            // This method is kept for compatibility but the new system handles movement automatically
            // The sprint system and input handler control movement now
        }

        /// <summary>
        /// Stop all movement
        /// </summary>
        public void Stop()
        {
            velocity.x = 0f;
            velocity.z = 0f;
            isRunning = false;
            OnVelocityChanged?.Invoke(velocity);
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        /// <summary>
        /// Add an impulse to the character (useful for jumping)
        /// </summary>
        /// <param name="impulse">The impulse vector to add</param>
        public void AddImpulse(Vector3 impulse)
        {
            velocity += impulse;
            OnVelocityChanged?.Invoke(velocity);
        }

        /// <summary>
        /// Set the vertical velocity directly
        /// </summary>
        /// <param name="verticalVelocity">The new vertical velocity</param>
        public void SetVerticalVelocity(float verticalVelocity)
        {
            velocity.y = verticalVelocity;
            OnVelocityChanged?.Invoke(velocity);
        }

        /// <summary>
        /// Set the movement speed
        /// </summary>
        /// <param name="newSpeed">The new movement speed</param>
        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = Mathf.Max(0f, newSpeed);
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        /// <summary>
        /// Get the camera-relative movement direction (legacy method)
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        /// <returns>Camera-relative movement direction</returns>
        private Vector3 CalculateMovementDirection(Vector3 inputVector)
        {
            return CalculateWorldMovementDirection(new Vector2(inputVector.x, inputVector.z));
        }

        /// <summary>
        /// Check if the character is grounded
        /// </summary>
        private void CheckGrounded()
        {
            UpdateGroundCheckPosition();

            bool previousGrounded = isGrounded;

            // Get all overlapping colliders
            Collider[] overlappingColliders = Physics.OverlapSphere(
                groundCheckPosition,
                groundCheckRadius,
                groundMask
            );

            // Filter out ignored GameObjects
            isGrounded = false;
            foreach (Collider collider in overlappingColliders)
            {
                bool shouldIgnore = false;

                // Check if this collider belongs to an ignored GameObject
                foreach (GameObject ignored in groundCheckIgnoreList)
                {
                    if (
                        ignored != null
                        && (
                            collider.gameObject == ignored
                            || collider.transform.IsChildOf(ignored.transform)
                        )
                    )
                    {
                        shouldIgnore = true;
                        break;
                    }
                }

                if (!shouldIgnore)
                {
                    isGrounded = true;
                    break;
                }
            }

            // Trigger event if grounded state changed
            if (previousGrounded != isGrounded)
            {
                OnGroundedChanged?.Invoke(isGrounded);
            }
        }

        /// <summary>
        /// Apply gravity to the character
        /// </summary>
        private void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Update the ground check position
        /// </summary>
        private void UpdateGroundCheckPosition()
        {
            if (groundCheckTransform != null)
            {
                // Use the assigned transform position
                groundCheckPosition = groundCheckTransform.position;
            }
            else
            {
                // Use character controller center with offset
                groundCheckPosition =
                    transform.position
                    + Vector3.down * (characterController.height * 0.5f + groundCheckDistance);
            }
        }

        // Public API Methods
        public void EnableMovement()
        {
            enableMovement = true;
        }

        public void DisableMovement()
        {
            enableMovement = false;
            Stop();
        }

        public void SetSprintMultiplier(float multiplier)
        {
            sprintMultiplier = Mathf.Clamp(multiplier, 1.1f, 3.0f);
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = Mathf.Clamp(speed, 0.1f, 20f);
        }

        public void SetAccelerationTime(float time)
        {
            accelerationTime = Mathf.Clamp(time, 0.1f, 5f);
        }

        public void SetDecelerationTime(float time)
        {
            decelerationTime = Mathf.Clamp(time, 0.1f, 5f);
        }

        public void SetCameraReference(Transform camera)
        {
            cameraTransform = camera;
        }

        public void SetToggleSprint(bool toggle)
        {
            toggleSprint = toggle;
            if (!toggle)
            {
                IsSprintToggled = false; // Reset toggle state when switching to hold mode
            }
        }

        public void SetSprintGrowthDuration(float duration)
        {
            sprintGrowthDuration = Mathf.Max(0f, duration);
        }

        public void SetSprintDecayDuration(float duration)
        {
            sprintDecayDuration = Mathf.Max(0f, duration);
        }

        public void SetSprintGrowthCurve(AnimationCurve curve)
        {
            if (curve != null && curve.keys.Length > 0)
            {
                sprintGrowthCurve = curve;
            }
        }

        public void SetSprintDecayCurve(AnimationCurve curve)
        {
            if (curve != null && curve.keys.Length > 0)
            {
                sprintDecayCurve = curve;
            }
        }

        public void ForceStopSprint()
        {
            IsSprinting = false;
            IsSprintToggled = false;
            sprintProgress = 0f;
            sprintTimer = 0f;
            isSprintTransitioning = false;
        }

        // Debug Information
        public void LogMovementState()
        {
            Debug.Log(
                $"General3DMovementCore State:\n"
                    + $"IsMoving: {IsMoving}\n"
                    + $"IsSprinting: {IsSprinting}\n"
                    + $"IsSprintToggled: {IsSprintToggled}\n"
                    + $"SprintProgress: {sprintProgress:F3}\n"
                    + $"CurrentSpeed: {CurrentSpeed:F2}\n"
                    + $"SprintMultiplier: {currentSprintMultiplier:F2}\n"
                    + $"MovementDirection: {MovementDirection}\n"
                    + $"ToggleMode: {toggleSprint}\n"
                    + $"CanSprint: {CanSprint}"
            );
        }

        // Public methods for external access
        public void SetGravity(float newGravity) => gravity = Mathf.Max(0f, newGravity);

        public void SetGroundCheckDistance(float distance) =>
            groundCheckDistance = Mathf.Max(0.1f, distance);

        public void SetGroundMask(LayerMask mask) => groundMask = mask;

        public void SetRunSpeed(float newRunSpeed) => runSpeed = Mathf.Max(0f, newRunSpeed);

        public void SetGroundCheckRadius(float radius) =>
            groundCheckRadius = Mathf.Clamp(radius, 0.01f, 1f);

        public void SetGroundCheckTransform(Transform transform) =>
            groundCheckTransform = transform;

        public void AddToGroundCheckIgnoreList(GameObject obj)
        {
            if (obj == null)
                return;

            // Check if already in the list
            foreach (GameObject existing in groundCheckIgnoreList)
            {
                if (existing == obj)
                    return;
            }

            // Expand the array to include the new object
            GameObject[] newList = new GameObject[groundCheckIgnoreList.Length + 1];
            for (int i = 0; i < groundCheckIgnoreList.Length; i++)
            {
                newList[i] = groundCheckIgnoreList[i];
            }
            newList[groundCheckIgnoreList.Length] = obj;
            groundCheckIgnoreList = newList;
        }

        public void RemoveFromGroundCheckIgnoreList(GameObject obj)
        {
            if (obj == null)
                return;

            int indexToRemove = -1;
            for (int i = 0; i < groundCheckIgnoreList.Length; i++)
            {
                if (groundCheckIgnoreList[i] == obj)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove >= 0)
            {
                // Create a new array without the removed object
                GameObject[] newList = new GameObject[groundCheckIgnoreList.Length - 1];
                int newIndex = 0;
                for (int i = 0; i < groundCheckIgnoreList.Length; i++)
                {
                    if (i != indexToRemove)
                    {
                        newList[newIndex] = groundCheckIgnoreList[i];
                        newIndex++;
                    }
                }
                groundCheckIgnoreList = newList;
            }
        }

        public void ClearGroundCheckIgnoreList()
        {
            groundCheckIgnoreList = new GameObject[] { gameObject }; // Keep the character itself
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        void OnValidate()
        {
            // Clamp values in editor
            groundCheckRadius = Mathf.Clamp(groundCheckRadius, 0.01f, 1f);
            groundCheckDistance = Mathf.Clamp(groundCheckDistance, 0.1f, 2f);
        }

        /// <summary>
        /// Debug draw the ground check sphere
        /// </summary>
        private void OnDrawGizmos()
        {
            if (characterController == null)
                return;

            UpdateGroundCheckPosition();

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
        }
    }
}
