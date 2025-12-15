using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

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
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Animator animator;
        private bool movingLeft = false;
        private bool movingRight = false;
        private float moveDirection = 0f;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            
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
            // Check if grounded
            CheckGrounded();
            
            // Calculate move direction based on button states
            moveDirection = 0f;
            if (movingLeft) moveDirection -= 1f;
            if (movingRight) moveDirection += 1f;
            
            // Update animator parameters
            UpdateAnimator();
        }
        
        void FixedUpdate()
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
        /// Called by UIVirtualButton's buttonStateOutputEvent for left movement
        /// Parameter is true when button is held, false when released
        /// </summary>
        public void SetMovingLeft(bool isPressed)
        {
            movingLeft = isPressed;
        }
        
        /// <summary>
        /// Called by UIVirtualButton's buttonStateOutputEvent for right movement
        /// Parameter is true when button is held, false when released
        /// </summary>
        public void SetMovingRight(bool isPressed)
        {
            movingRight = isPressed;
        }
        
        /// <summary>
        /// Called when jump button is pressed
        /// </summary>
        public void Jump()
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}