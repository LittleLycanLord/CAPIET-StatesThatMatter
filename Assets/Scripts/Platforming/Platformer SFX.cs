using LilLycanLord_Official;
using UnityEngine;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class PlatformerSFX : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private PlatformerMovement platformerMovement;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Jump & Landing SFX")]
        [SerializeField] private string jumpSFXName = "Jump";
        [SerializeField] private string landingSFXName = "Landing";
        
        [Space(10)]
        [Header("Footsteps SFX")]
        [SerializeField] private List<string> footstepsSFXNames = new List<string> { "Footstep1", "Footstep2", "Footstep3" };
        [SerializeField] private float footstepRate = 0.3f; // Time between footstep sounds
        [SerializeField] private float minimumSpeedForFootsteps = 0.1f; // Minimum horizontal speed to play footsteps
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool wasGrounded = false;
        private bool wasMoving = false;
        private float footstepTimer = 0f;
        private List<string> shuffledFootsteps;
        private int currentFootstepIndex = 0;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            if (platformerMovement == null)
            {
                platformerMovement = GetComponent<PlatformerMovement>();
                if (platformerMovement == null)
                {
                    Debug.LogWarning("[PlatformerSFX] PlatformerMovement component not found!");
                }
            }
        }

        void Start()
        {
            ShuffleFootsteps();
        }

        void Update()
        {
            if (platformerMovement == null) return;
            
            // Get current state from PlatformerMovement using reflection to access private fields
            bool isGrounded = GetIsGrounded();
            float xVelocity = GetXVelocity();
            bool isMoving = Mathf.Abs(xVelocity) > minimumSpeedForFootsteps;
            
            // Detect landing (transition from airborne to grounded)
            if (isGrounded && !wasGrounded)
            {
                PlayLandingSFX();
            }
            
            // Play footsteps when moving on ground
            if (isGrounded && isMoving)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstepSFX();
                    footstepTimer = footstepRate;
                }
            }
            else
            {
                // Reset timer when not moving or in air
                footstepTimer = 0f;
            }
            
            // Update previous state
            wasGrounded = isGrounded;
            wasMoving = isMoving;
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Shuffle the footstep sounds list once at start
        /// </summary>
        private void ShuffleFootsteps()
        {
            if (footstepsSFXNames == null || footstepsSFXNames.Count == 0)
                return;
                
            shuffledFootsteps = new List<string>(footstepsSFXNames);
            
            // Fisher-Yates shuffle
            for (int i = shuffledFootsteps.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = shuffledFootsteps[i];
                shuffledFootsteps[i] = shuffledFootsteps[j];
                shuffledFootsteps[j] = temp;
            }
            
            currentFootstepIndex = 0;
        }
        
        /// <summary>
        /// Play jump sound effect - call this from PlatformerMovement.Jump()
        /// </summary>
        public void PlayJumpSFX()
        {
            if (AudioManager.Instance == null || string.IsNullOrEmpty(jumpSFXName))
                return;
                
            AudioManager.Instance.Play(jumpSFXName, gameObject);
        }
        
        /// <summary>
        /// Play landing sound effect
        /// </summary>
        private void PlayLandingSFX()
        {
            if (AudioManager.Instance == null || string.IsNullOrEmpty(landingSFXName))
                return;
                
            AudioManager.Instance.Play(landingSFXName, gameObject);
        }
        
        /// <summary>
        /// Play the next footstep sound in the shuffled sequence
        /// </summary>
        private void PlayFootstepSFX()
        {
            if (AudioManager.Instance == null || shuffledFootsteps == null || shuffledFootsteps.Count == 0)
                return;
                
            string footstepSound = shuffledFootsteps[currentFootstepIndex];
            
            if (!string.IsNullOrEmpty(footstepSound))
            {
                AudioManager.Instance.Play(footstepSound, gameObject);
            }
            
            // Move to next footstep, loop back to start when reaching the end
            currentFootstepIndex = (currentFootstepIndex + 1) % shuffledFootsteps.Count;
        }
        
        /// <summary>
        /// Get isGrounded from PlatformerMovement using reflection
        /// </summary>
        private bool GetIsGrounded()
        {
            if (platformerMovement == null) return false;
            
            var field = typeof(PlatformerMovement).GetField("isGrounded", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (bool)field.GetValue(platformerMovement);
            }
            
            return false;
        }
        
        /// <summary>
        /// Get currentXVelocity from PlatformerMovement using reflection
        /// </summary>
        private float GetXVelocity()
        {
            if (platformerMovement == null) return 0f;
            
            var field = typeof(PlatformerMovement).GetField("currentXVelocity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (float)field.GetValue(platformerMovement);
            }
            
            return 0f;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}