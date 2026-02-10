using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlatformerMovement : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private PlatformerSFX platformerSFX;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private float currentXVelocity = 0f;
        [SerializeField] private float currentYVelocity = 0f;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("References")]
        [SerializeField] private GameObject animatorObject;
        [SerializeField] private GameObject visuals;
        
        [Space(10)]
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 10f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;
        
        [Space(10)]
        [Header("Dev Settings")]
        [SerializeField] private bool devmode = true;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Animator animator;
        private bool movingLeft = false;
        private bool movingRight = false;
        private float moveDirection = 0f;
        private bool inputEnabled = true; // Control input without disabling the script

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            platformerSFX = GetComponent<PlatformerSFX>();
            
            if (animatorObject != null)
            {
                animator = animatorObject.GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("PlatformerMovement: Animator component not found on animatorObject!");
                }
            }
            else
            {
                Debug.LogWarning("PlatformerMovement: animatorObject not assigned!");
            }
        }

        void Start() { }

        void Update()
        {
            if (GameState.InMinigame) return;

            // Dev mode keyboard controls
            if (inputEnabled && devmode && Keyboard.current != null)
            {
                // A key for left movement
                if (Keyboard.current[Key.A].wasPressedThisFrame)
                {
                    movingLeft = true;
                }
                if (Keyboard.current[Key.A].wasReleasedThisFrame)
                {
                    movingLeft = false;
                }
                
                // D key for right movement
                if (Keyboard.current[Key.D].wasPressedThisFrame)
                {
                    movingRight = true;
                }
                if (Keyboard.current[Key.D].wasReleasedThisFrame)
                {
                    movingRight = false;
                }
                
                // Space for jump
                if (Keyboard.current[Key.Space].wasPressedThisFrame)
                {
                    Jump();
                }
            }
            
            // Check if grounded
            CheckGrounded();
            
            // Calculate move direction based on button states (only if input enabled)
            if (inputEnabled)
            {
                moveDirection = 0f;
                if (movingLeft) moveDirection -= 1f;
                if (movingRight) moveDirection += 1f;
            }
            else
            {
                // When input disabled, reset movement flags
                moveDirection = 0f;
            }
            
            // Update animator parameters
            UpdateAnimator();
        }
        
        void FixedUpdate()
        {
            if (GameState.InMinigame) return;

            // Only apply player-controlled movement if input is enabled
            if (inputEnabled)
            {
                // Apply horizontal movement
                if (moveDirection != 0f)
                {
                    rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
                }
                else
                {
                    // Decay horizontal velocity when no input
                    float newXVelocity = Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
                    rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
                }
            }
            // When input disabled, external scripts (like GoalBehaviour) control velocity
            
            // Update velocity display values
            currentXVelocity = rb.linearVelocity.x;
            currentYVelocity = rb.linearVelocity.y;
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Check if the player is grounded using a raycast
        /// </summary>
        private void CheckGrounded()
        {
            // Cast a ray downward from the bottom of the capsule collider
            Vector2 origin = new Vector2(transform.position.x, transform.position.y);
            float rayDistance = (capsuleCollider.size.y / 2f) + groundCheckDistance;
            
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);
            isGrounded = hit.collider != null;
            
            // Debug visualization
            Debug.DrawRay(origin, Vector2.down * rayDistance, isGrounded ? Color.green : Color.red);
        }
        
        /// <summary>
        /// Update animator parameters with current movement state
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null) return;
            
            // Update animator parameters
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("xVelocity", rb.linearVelocity.x);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
            animator.SetFloat("absoluteXVelocity", Mathf.Abs(rb.linearVelocity.x));
            
            // Handle visual flipping based on input direction (sprite faces right by default)
            if (visuals != null && moveDirection != 0f)
            {
                Vector3 scale = visuals.transform.localScale;
                scale.x = moveDirection < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                visuals.transform.localScale = scale;
            }
        }
        
        //* ╔══════════════════════╗
        //* ║ Public Button Methods ║
        //* ╚══════════════════════╝
        
        /// <summary>
        /// Enable or disable player input
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
            {
                // Clear input flags when disabling
                movingLeft = false;
                movingRight = false;
                moveDirection = 0f;
            }
        }
        
        /// <summary>
        /// Called by UIVirtualButton's buttonStateOutputEvent for left movement
        /// Parameter is true when button is held, false when released
        /// </summary>
        public void SetMovingLeft(bool isPressed)
        {
            if (inputEnabled)
            {
                movingLeft = isPressed;
            }
        }
        
        /// <summary>
        /// Called by UIVirtualButton's buttonStateOutputEvent for right movement
        /// Parameter is true when button is held, false when released
        /// </summary>
        public void SetMovingRight(bool isPressed)
        {
            if (inputEnabled)
            {
                movingRight = isPressed;
            }
        }
        
        /// <summary>
        /// Called when jump button is pressed
        /// </summary>
        public void Jump()
        {
            if (inputEnabled && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
                
                // Play jump sound effect
                if (platformerSFX != null)
                {
                    platformerSFX.PlayJumpSFX();
                }
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}