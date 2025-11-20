using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Handles input processing and translation for 3D player movement
    /// Interfaces between GeneralInputManager and movement systems
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class GeneralPlayerInputHandler : MonoBehaviour
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

        [Space(10)]
        [Header("Input Settings")]
        [SerializeField]
        private bool enableInput = true;

        [SerializeField]
        private bool enableInputBuffering = true;

        [Header("Input Response")]
        [SerializeField]
        private bool enableSprintToggle = false;

        [SerializeField]
        private float sprintHoldThreshold = 0.0f;

        [SerializeField]
        private bool enableJumpBuffer = false;

        [SerializeField]
        private float jumpBufferTime = 0.0f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // Input State
        public Vector2 MovementInput { get; private set; }
        public bool SprintInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool InteractInput { get; private set; }
        public Vector2 MouseInput { get; private set; }
        public float ScrollInput { get; private set; }
        public bool PrimaryMouseInput { get; private set; }
        public bool SecondaryMouseInput { get; private set; }
        public bool MiddleMouseInput { get; private set; }

        // Input State Properties
        public bool HasMovementInput => MovementInput.sqrMagnitude > 0.01f;
        public bool IsSprintPressed => SprintInput;
        public bool IsJumpPressed => JumpInput;
        public bool IsJumpHeld => JumpHeld;
        public bool IsInteractPressed => InteractInput;

        // Sprint State
        private bool sprintToggleState = false;
        private float sprintHoldTime = 0f;

        // Jump Buffer
        private float jumpBufferTimer = 0f;
        private bool jumpBuffered = false;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // Events
        public System.Action<Vector2> OnMovementInputChanged;
        public System.Action<bool> OnSprintStateChanged;
        public System.Action OnJumpPressed;
        public System.Action OnJumpReleased;
        public System.Action OnInteractPressed;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake() { }

        void Update()
        {
            if (!enableInput || GeneralInputManager.Instance == null)
                return;

            ProcessInputs();
            UpdateInputState();
            UpdateJumpBuffer();
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        private void ProcessInputs()
        {
            // Process movement input
            Vector2 newMovementInput = GeneralInputManager.Instance.GetPlayerWASDInput();
            if (newMovementInput != MovementInput)
            {
                MovementInput = newMovementInput;
                OnMovementInputChanged?.Invoke(MovementInput);
            }

            // Process sprint input
            bool newSprintInput = ProcessSprintInput();
            if (newSprintInput != SprintInput)
            {
                SprintInput = newSprintInput;
                OnSprintStateChanged?.Invoke(SprintInput);
            }

            // Process jump input
            bool newJumpInput = ProcessJumpInput();
            if (newJumpInput && !JumpInput)
            {
                OnJumpPressed?.Invoke();
            }
            else if (!newJumpInput && JumpInput)
            {
                OnJumpReleased?.Invoke();
            }
            JumpInput = newJumpInput;

            // Process other inputs
            JumpHeld = GeneralInputManager.Instance.GetPlayerJumpInput(true);

            bool newInteractInput = GeneralInputManager.Instance.GetPlayerInteractInput();
            if (newInteractInput && !InteractInput)
            {
                OnInteractPressed?.Invoke();
            }
            InteractInput = newInteractInput;

            // Mouse and scroll inputs
            MouseInput = GeneralInputManager.Instance.GetPlayerMouseMovement();
            ScrollInput = GeneralInputManager.Instance.GetPlayerScroll();
            PrimaryMouseInput = GeneralInputManager.Instance.GetPlayerPrimaryMouse();
            SecondaryMouseInput = GeneralInputManager.Instance.GetPlayerSecondaryMouse();
            MiddleMouseInput = GeneralInputManager.Instance.GetPlayerMMB();
        }

        private bool ProcessSprintInput()
        {
            bool sprintPressed = GeneralInputManager.Instance.GetPlayerSprintInput(true);

            if (enableSprintToggle)
            {
                // Toggle mode: tap to toggle sprint on/off
                bool sprintTapped = GeneralInputManager.Instance.GetPlayerSprintInput();

                if (sprintTapped)
                {
                    sprintToggleState = !sprintToggleState;
                }

                return sprintToggleState && HasMovementInput;
            }
            else
            {
                // Hold mode: hold to sprint
                if (sprintPressed)
                {
                    sprintHoldTime += Time.deltaTime;
                    return sprintHoldTime >= sprintHoldThreshold && HasMovementInput;
                }
                else
                {
                    sprintHoldTime = 0f;
                    return false;
                }
            }
        }

        private bool ProcessJumpInput()
        {
            bool jumpPressed = GeneralInputManager.Instance.GetPlayerJumpInput();

            if (enableJumpBuffer && jumpPressed)
            {
                jumpBuffered = true;
                jumpBufferTimer = jumpBufferTime;
            }

            return jumpPressed || (jumpBuffered && jumpBufferTimer > 0f);
        }

        private void UpdateInputState()
        {
            // Additional input state processing can go here
        }

        private void UpdateJumpBuffer()
        {
            if (jumpBuffered && jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
                if (jumpBufferTimer <= 0f)
                {
                    jumpBuffered = false;
                }
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        public void ConsumeJumpInput()
        {
            jumpBuffered = false;
            jumpBufferTimer = 0f;
        }

        public void EnableInput()
        {
            enableInput = true;
        }

        public void DisableInput()
        {
            enableInput = false;
            ResetInputState();
        }

        public void ResetInputState()
        {
            MovementInput = Vector2.zero;
            SprintInput = false;
            JumpInput = false;
            JumpHeld = false;
            InteractInput = false;
            MouseInput = Vector2.zero;
            ScrollInput = 0f;
            PrimaryMouseInput = false;
            SecondaryMouseInput = false;
            MiddleMouseInput = false;

            sprintToggleState = false;
            sprintHoldTime = 0f;
            jumpBuffered = false;
            jumpBufferTimer = 0f;
        }

        public void SetSprintToggleMode(bool enabled)
        {
            enableSprintToggle = enabled;
            if (!enabled)
            {
                sprintToggleState = false;
            }
        }
    }
}
