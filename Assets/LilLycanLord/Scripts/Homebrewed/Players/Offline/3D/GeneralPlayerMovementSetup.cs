using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Automatic setup utility for the Modular 3D Movement System
    /// Provides one-click configuration for various game types and movement styles
    /// </summary>
    public class GeneralPlayerMovementSetup : MonoBehaviour
    {
        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                        COMPONENTS                             ║
        // ╚═══════════════════════════════════════════════════════════════╝

        // Movement system components
        private GeneralPlayerController playerController;
        private General3DMovementCore coreMovement;
        private General3DMovementCore enhancedMovement;
        private General3DJumpSystem jumpSystem;
        private GeneralPlayerInputHandler inputHandler;
        private General3DMovementAnimator movementAnimator;

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                         DISPLAYS                             ║
        // ╚═══════════════════════════════════════════════════════════════╝


        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                          FIELDS                              ║
        // ╚═══════════════════════════════════════════════════════════════╝

        [Space(10)]
        [Header("Movement Recipe Configuration")]
        [SerializeField]
        [Tooltip("Select the movement style preset to apply")]
        private MovementRecipe selectedRecipe = MovementRecipe.Custom;

        [SerializeField]
        [Tooltip("Apply the selected recipe configuration automatically on Start")]
        private bool autoApplyOnStart = false;

        [SerializeField]
        [Tooltip("Show detailed configuration info in console")]
        private bool showDebugInfo = true;

        [Space(10)]
        [Header("Manual Configuration")]
        [SerializeField]
        [Tooltip("Override specific settings after applying recipe")]
        private bool useCustomOverrides = false;

        [SerializeField]
        private CustomMovementSettings customSettings = new CustomMovementSettings();

        [Space(10)]
        [Header("Quick Actions")]
        [SerializeField]
        [Tooltip("Test different recipes at runtime (Editor only)")]
        private MovementRecipe runtimeTestRecipe = MovementRecipe.Custom;

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                        ATTRIBUTES                            ║
        // ╚═══════════════════════════════════════════════════════════════╝


        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                      MONOBEHAVIOUR                           ║
        // ╚═══════════════════════════════════════════════════════════════╝

        void Awake()
        {
            CacheComponents();
        }

        void Start()
        {
            if (autoApplyOnStart && selectedRecipe != MovementRecipe.Custom)
            {
                ApplyMovementRecipe(selectedRecipe);
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            // Runtime testing in editor
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.F1))
            {
                if (runtimeTestRecipe != MovementRecipe.Custom)
                {
                    ApplyMovementRecipe(runtimeTestRecipe);
                    Debug.Log($"Applied runtime test recipe: {runtimeTestRecipe}");
                }
            }
#endif
        }

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                    NON-MONOBEHAVIOUR                         ║
        // ╚═══════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Cache all movement system components
        /// </summary>
        private void CacheComponents()
        {
            playerController = GetComponent<GeneralPlayerController>();
            coreMovement = GetComponent<General3DMovementCore>();
            enhancedMovement = GetComponent<General3DMovementCore>();
            jumpSystem = GetComponent<General3DJumpSystem>();
            inputHandler = GetComponent<GeneralPlayerInputHandler>();
            movementAnimator = GetComponent<General3DMovementAnimator>();

            if (playerController == null)
            {
                Debug.LogError(
                    "PlayerMovementSetup: Streamlined3DPlayerController not found! Please add it first."
                );
            }
        }

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║               VIRTUAL/OVERRIDDEN FUNCTIONS                   ║
        // ╚═══════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Apply a specific movement recipe configuration
        /// </summary>
        /// <param name="recipe">The recipe to apply</param>
        public void ApplyMovementRecipe(MovementRecipe recipe)
        {
            if (!ValidateComponents())
            {
                Debug.LogError(
                    "PlayerMovementSetup: Required components missing! Cannot apply recipe."
                );
                return;
            }

            if (showDebugInfo)
            {
                Debug.Log($"🎮 Applying Movement Recipe: {recipe}");
            }

            switch (recipe)
            {
                case MovementRecipe.Platformer2D:
                    Apply2DPlatformerRecipe();
                    break;
                case MovementRecipe.Platformer3D:
                    Apply3DPlatformerRecipe();
                    break;
                case MovementRecipe.ActionFPS:
                    ApplyActionFPSRecipe();
                    break;
                case MovementRecipe.ExplorationFPS:
                    ApplyExplorationFPSRecipe();
                    break;
                case MovementRecipe.IsometricShooter:
                    ApplyIsometricShooterRecipe();
                    break;
                case MovementRecipe.RacingVehicle:
                    ApplyRacingVehicleRecipe();
                    break;
                case MovementRecipe.Custom:
                    if (useCustomOverrides)
                    {
                        ApplyCustomSettings();
                    }
                    break;
            }

            if (useCustomOverrides && recipe != MovementRecipe.Custom)
            {
                ApplyCustomOverrides();
            }

            if (showDebugInfo)
            {
                LogCurrentConfiguration();
            }
        }

        /// <summary>
        /// Apply 2D Platformer/Metroidvania recipe
        /// </summary>
        private void Apply2DPlatformerRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 7f,
                runSpeed: 11f,
                gravity: 27.5f,
                groundCheckDistance: 0.2f
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: false, // Maintain facing direction
                rotationSpeed: 0.1f, // Minimal rotation
                sprintMultiplier: 1.4f,
                sprintBuildup: 0.1f, // Instant sprint
                requireMovementForSprint: true,
                preventAirSprinting: false, // Allow air control
                enableMovementSmoothing: false, // Precise input
                accelerationTime: 0.05f,
                decelerationTime: 0.05f
            );

            // General3DJumpSystem Settings
            SetJumpSystemSettings(
                enableJumping: true,
                maxJumps: 2, // Double jump
                jumpHeight: 3.5f,
                jumpModifier: 1.0f,
                enableVariableJumpHeight: true,
                lowJumpMultiplier: 2.0f, // Responsive short hops
                enableCoyoteTime: true,
                coyoteTime: 0.15f,
                enableJumpBuffer: true,
                jumpBufferTime: 0.1f,
                fallMultiplier: 2.5f // Quick falls
            );

            if (showDebugInfo)
            {
                Debug.Log(
                    "✅ Applied 2D Platformer Recipe - Precise, responsive side-scrolling movement"
                );
            }
        }

        /// <summary>
        /// Apply 3D Platformer recipe (Mario/Crash Bandicoot style)
        /// </summary>
        private void Apply3DPlatformerRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 6f,
                runSpeed: 9.5f,
                gravity: 22.5f,
                groundCheckDistance: 0.3f
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: true, // Smooth character turning
                rotationSpeed: 10f,
                sprintMultiplier: 1.5f,
                sprintBuildup: 0.3f, // Slight buildup for feel
                requireMovementForSprint: true,
                preventAirSprinting: true, // Realistic
                enableMovementSmoothing: true,
                accelerationTime: 0.15f,
                decelerationTime: 0.1f
            );

            // General3DJumpSystem Settings
            SetJumpSystemSettings(
                enableJumping: true,
                maxJumps: 2,
                jumpHeight: 4f,
                jumpModifier: 1.0f,
                enableVariableJumpHeight: true,
                lowJumpMultiplier: 1.8f,
                enableCoyoteTime: true,
                coyoteTime: 0.2f,
                enableJumpBuffer: true,
                jumpBufferTime: 0.15f,
                fallMultiplier: 2.0f
            );

            if (showDebugInfo)
            {
                Debug.Log(
                    "✅ Applied 3D Platformer Recipe - Smooth 3D platforming with precise controls"
                );
            }
        }

        /// <summary>
        /// Apply Action FPS recipe (Call of Duty/Valorant style)
        /// </summary>
        private void ApplyActionFPSRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 5f,
                runSpeed: 8f,
                gravity: 17.5f,
                groundCheckDistance: 0.2f
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: false, // Camera controls facing
                rotationSpeed: 0.1f, // Minimal body rotation
                sprintMultiplier: 1.65f,
                sprintBuildup: 0.2f, // Quick but not instant
                requireMovementForSprint: true,
                preventAirSprinting: false, // Allow strafe jumping
                enableMovementSmoothing: true,
                accelerationTime: 0.05f, // Very responsive
                decelerationTime: 0.05f
            );

            // General3DJumpSystem Settings
            SetJumpSystemSettings(
                enableJumping: true,
                maxJumps: 1, // Realistic
                jumpHeight: 2.25f, // Realistic height
                jumpModifier: 1.0f,
                enableVariableJumpHeight: true,
                lowJumpMultiplier: 1.5f,
                enableCoyoteTime: true,
                coyoteTime: 0.1f, // Minimal
                enableJumpBuffer: true,
                jumpBufferTime: 0.1f,
                fallMultiplier: 1.5f // Realistic
            );

            if (showDebugInfo)
            {
                Debug.Log(
                    "✅ Applied Action FPS Recipe - Responsive shooter movement with realistic physics"
                );
            }
        }

        /// <summary>
        /// Apply Exploration FPS recipe (Firewatch/Edith Finch style)
        /// </summary>
        private void ApplyExplorationFPSRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 3.5f,
                runSpeed: 5.5f,
                gravity: 16.5f,
                groundCheckDistance: 0.4f // Forgiving
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: false,
                rotationSpeed: 0.1f,
                sprintMultiplier: 1.4f, // Gentle sprint
                sprintBuildup: 0.5f, // Gradual acceleration
                requireMovementForSprint: false, // Can sprint in place
                preventAirSprinting: true,
                enableMovementSmoothing: true,
                accelerationTime: 0.3f, // Smooth starts
                decelerationTime: 0.4f // Smooth stops
            );

            // General3DJumpSystem Settings
            SetJumpSystemSettings(
                enableJumping: true,
                maxJumps: 1,
                jumpHeight: 1.75f, // Realistic/small hops
                jumpModifier: 1.0f,
                enableVariableJumpHeight: true,
                lowJumpMultiplier: 1.3f,
                enableCoyoteTime: true,
                coyoteTime: 0.25f, // Forgiving
                enableJumpBuffer: true,
                jumpBufferTime: 0.2f,
                fallMultiplier: 1.2f // Gentle
            );

            if (showDebugInfo)
            {
                Debug.Log(
                    "✅ Applied Exploration FPS Recipe - Relaxed, immersive exploration movement"
                );
            }
        }

        /// <summary>
        /// Apply Isometric Shooter recipe (Diablo/Path of Exile style)
        /// </summary>
        private void ApplyIsometricShooterRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 6f,
                runSpeed: 9f,
                gravity: 20f,
                groundCheckDistance: 0.3f
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: true, // Toward movement
                rotationSpeed: 17.5f, // Quick turns
                sprintMultiplier: 1.55f,
                sprintBuildup: 0.1f, // Quick response
                requireMovementForSprint: true,
                preventAirSprinting: false,
                enableMovementSmoothing: true,
                accelerationTime: 0.1f,
                decelerationTime: 0.1f
            );

            // General3DJumpSystem Settings (Usually no jumping in isometric)
            SetJumpSystemSettings(
                enableJumping: false,
                maxJumps: 1,
                jumpHeight: 1.5f,
                jumpModifier: 1.0f,
                enableVariableJumpHeight: false,
                lowJumpMultiplier: 1.0f,
                enableCoyoteTime: true,
                coyoteTime: 0.1f,
                enableJumpBuffer: false,
                jumpBufferTime: 0.1f,
                fallMultiplier: 2.0f
            );

            if (showDebugInfo)
            {
                Debug.Log(
                    "✅ Applied Isometric Shooter Recipe - Quick, responsive overhead action movement"
                );
            }
        }

        /// <summary>
        /// Apply Racing/Vehicle Feel recipe
        /// </summary>
        private void ApplyRacingVehicleRecipe()
        {
            // Core3DMovement Settings
            SetCoreMovementSettings(
                moveSpeed: 7f,
                runSpeed: 13.5f,
                gravity: 18f,
                groundCheckDistance: 0.3f
            );

            // General3DMovement Settings
            SetGeneralMovementSettings(
                enableMovement: true,
                enableSprinting: true,
                faceMovementDirection: true,
                rotationSpeed: 6.5f, // Slower turning
                sprintMultiplier: 2.0f, // Significant speed boost
                sprintBuildup: 1.0f, // Momentum buildup
                requireMovementForSprint: false,
                preventAirSprinting: false,
                enableMovementSmoothing: true,
                accelerationTime: 0.65f, // Momentum building
                decelerationTime: 0.8f // Sliding stops
            );

            // General3DJumpSystem Settings
            SetJumpSystemSettings(
                enableJumping: true,
                maxJumps: 1,
                jumpHeight: 2f,
                jumpModifier: 1.0f,
                enableVariableJumpHeight: false,
                lowJumpMultiplier: 1.0f,
                enableCoyoteTime: true,
                coyoteTime: 0.15f,
                enableJumpBuffer: true,
                jumpBufferTime: 0.1f,
                fallMultiplier: 1.8f
            );

            if (showDebugInfo)
            {
                Debug.Log("✅ Applied Racing/Vehicle Recipe - Momentum-based movement with buildup");
            }
        }

        /// <summary>
        /// Apply custom settings from the inspector
        /// </summary>
        private void ApplyCustomSettings()
        {
            SetCoreMovementSettings(
                customSettings.moveSpeed,
                customSettings.runSpeed,
                customSettings.gravity,
                customSettings.groundCheckDistance
            );

            SetGeneralMovementSettings(
                customSettings.enableMovement,
                customSettings.enableSprinting,
                customSettings.faceMovementDirection,
                customSettings.rotationSpeed,
                customSettings.sprintMultiplier,
                customSettings.sprintBuildup,
                customSettings.requireMovementForSprint,
                customSettings.preventAirSprinting,
                customSettings.enableMovementSmoothing,
                customSettings.accelerationTime,
                customSettings.decelerationTime
            );

            SetJumpSystemSettings(
                customSettings.enableJumping,
                customSettings.maxJumps,
                customSettings.jumpHeight,
                customSettings.jumpModifier,
                customSettings.enableVariableJumpHeight,
                customSettings.lowJumpMultiplier,
                customSettings.enableCoyoteTime,
                customSettings.coyoteTime,
                customSettings.enableJumpBuffer,
                customSettings.jumpBufferTime,
                customSettings.fallMultiplier
            );

            if (showDebugInfo)
            {
                Debug.Log("✅ Applied Custom Settings");
            }
        }

        /// <summary>
        /// Apply custom overrides on top of a recipe
        /// </summary>
        private void ApplyCustomOverrides()
        {
            // Apply only non-default custom settings as overrides
            if (customSettings.moveSpeed != 5f)
                coreMovement.SetMoveSpeed(customSettings.moveSpeed);

            if (customSettings.runSpeed != 8f)
                coreMovement.SetRunSpeed(customSettings.runSpeed);

            if (customSettings.sprintMultiplier != 1.5f)
                enhancedMovement.SetSprintMultiplier(customSettings.sprintMultiplier);

            if (customSettings.jumpHeight != 2f)
                jumpSystem.SetJumpHeight(customSettings.jumpHeight);

            if (showDebugInfo)
            {
                Debug.Log("🔧 Applied Custom Overrides");
            }
        }

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║               VIRTUAL/OVERRIDDEN FUNCTIONS                   ║
        // ╚═══════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Validate that all required components are present
        /// </summary>
        private bool ValidateComponents()
        {
            if (coreMovement == null)
            {
                Debug.LogError("Core3DMovement component missing!");
                return false;
            }
            if (enhancedMovement == null)
            {
                Debug.LogError("General3DMovement component missing!");
                return false;
            }
            if (jumpSystem == null)
            {
                Debug.LogError("General3DJumpSystem component missing!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Core3DMovement component settings
        /// </summary>
        private void SetCoreMovementSettings(
            float moveSpeed,
            float runSpeed,
            float gravity,
            float groundCheckDistance
        )
        {
            if (coreMovement == null)
                return;

            coreMovement.SetMoveSpeed(moveSpeed);
            coreMovement.SetRunSpeed(runSpeed);
            coreMovement.SetGravity(gravity);
            coreMovement.SetGroundCheckDistance(groundCheckDistance);
        }

        /// <summary>
        /// Set General3DMovement component settings
        /// </summary>
        private void SetGeneralMovementSettings(
            bool enableMovement,
            bool enableSprinting,
            bool faceMovementDirection,
            float rotationSpeed,
            float sprintMultiplier,
            float sprintBuildup,
            bool requireMovementForSprint,
            bool preventAirSprinting,
            bool enableMovementSmoothing,
            float accelerationTime,
            float decelerationTime
        )
        {
            if (enhancedMovement == null)
                return;

            if (enableMovement)
                enhancedMovement.EnableMovement();
            else
                enhancedMovement.DisableMovement();

            enhancedMovement.SetSprintMultiplier(sprintMultiplier);
            enhancedMovement.SetRotationSpeed(rotationSpeed);
            enhancedMovement.SetAccelerationTime(accelerationTime);
            enhancedMovement.SetDecelerationTime(decelerationTime);

            // Note: Some settings require reflection or exposing more public methods
            // For now, we set what's available through the public API
        }

        /// <summary>
        /// Set General3DJumpSystem component settings
        /// </summary>
        private void SetJumpSystemSettings(
            bool enableJumping,
            int maxJumps,
            float jumpHeight,
            float jumpModifier,
            bool enableVariableJumpHeight,
            float lowJumpMultiplier,
            bool enableCoyoteTime,
            float coyoteTime,
            bool enableJumpBuffer,
            float jumpBufferTime,
            float fallMultiplier
        )
        {
            if (jumpSystem == null)
                return;

            if (enableJumping)
                jumpSystem.EnableJumping();
            else
                jumpSystem.DisableJumping();

            jumpSystem.SetJumpHeight(jumpHeight);
            jumpSystem.SetMaxJumps(maxJumps);

            // Note: Some settings require reflection or exposing more public methods
            // For now, we set what's available through the public API
        }

        /// <summary>
        /// Log current movement configuration for debugging
        /// </summary>
        private void LogCurrentConfiguration()
        {
            if (!showDebugInfo)
                return;

            Debug.Log(
                "📊 Current Movement Configuration:\n"
                    + $"Core Movement - Speed: {coreMovement.MoveSpeed:F1}, Run: {coreMovement.RunSpeed:F1}, Gravity: {coreMovement.Gravity:F1}\n"
                    + $"General Movement - Sprint Multiplier: {enhancedMovement.CurrentSprintMultiplier:F2}\n"
                    + $"Jump System - Height: {jumpSystem.GetJumpInfo().CanJump}, Can Jump: {jumpSystem.CanJump}"
            );
        }

        /// <summary>
        /// Get a description of the selected recipe
        /// </summary>
        public string GetRecipeDescription(MovementRecipe recipe)
        {
            switch (recipe)
            {
                case MovementRecipe.Platformer2D:
                    return "Precise, responsive side-scrolling movement with quick falls and air control";
                case MovementRecipe.Platformer3D:
                    return "Smooth 3D platforming with character rotation and momentum";
                case MovementRecipe.ActionFPS:
                    return "Responsive shooter movement with realistic physics and strafe jumping";
                case MovementRecipe.ExplorationFPS:
                    return "Relaxed, immersive exploration with smooth acceleration";
                case MovementRecipe.IsometricShooter:
                    return "Quick, responsive overhead action movement";
                case MovementRecipe.RacingVehicle:
                    return "Momentum-based movement with acceleration buildup";
                case MovementRecipe.Custom:
                    return "Custom configuration defined in inspector";
                default:
                    return "Unknown recipe";
            }
        }

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║                   PUBLIC UTILITY METHODS                     ║
        // ╚═══════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Quick setup method for external scripts
        /// </summary>
        public void QuickSetup(MovementRecipe recipe)
        {
            selectedRecipe = recipe;
            ApplyMovementRecipe(recipe);
        }

        /// <summary>
        /// Reset to default settings
        /// </summary>
        public void ResetToDefaults()
        {
            Apply3DPlatformerRecipe(); // Use 3D Platformer as default
            if (showDebugInfo)
            {
                Debug.Log("🔄 Reset to default 3D Platformer settings");
            }
        }

        /// <summary>
        /// Apply recipe via string name (useful for UI buttons)
        /// </summary>
        public void ApplyRecipeByName(string recipeName)
        {
            if (System.Enum.TryParse<MovementRecipe>(recipeName, out MovementRecipe recipe))
            {
                ApplyMovementRecipe(recipe);
            }
            else
            {
                Debug.LogWarning($"Unknown recipe name: {recipeName}");
            }
        }

        /// <summary>
        /// Refresh cached component references (called by editor)
        /// </summary>
        public void RefreshComponentCache()
        {
            CacheComponents();
            if (showDebugInfo)
            {
                Debug.Log("🔄 Component cache refreshed");
            }
        }
    }

    // ╔═══════════════════════════════════════════════════════════════╗
    // ║                     SUPPORTING ENUMS                         ║
    // ╚═══════════════════════════════════════════════════════════════╝

    [System.Serializable]
    public enum MovementRecipe
    {
        [Tooltip("Use custom settings from inspector")]
        Custom = 0,

        [Tooltip("2D Platformer/Metroidvania - Precise side-scrolling")]
        Platformer2D = 1,

        [Tooltip("3D Platformer - Mario/Crash Bandicoot style")]
        Platformer3D = 2,

        [Tooltip("Action FPS - Call of Duty/Valorant style")]
        ActionFPS = 3,

        [Tooltip("Exploration FPS - Firewatch/Edith Finch style")]
        ExplorationFPS = 4,

        [Tooltip("Isometric Shooter - Diablo/Path of Exile style")]
        IsometricShooter = 5,

        [Tooltip("Racing/Vehicle Feel - Momentum-based")]
        RacingVehicle = 6,
    }

    [System.Serializable]
    public struct CustomMovementSettings
    {
        [Header("Core Movement")]
        public float moveSpeed;
        public float runSpeed;
        public float gravity;
        public float groundCheckDistance;

        [Header("General Movement")]
        public bool enableMovement;
        public bool enableSprinting;
        public bool faceMovementDirection;
        public float rotationSpeed;
        public float sprintMultiplier;
        public float sprintBuildup;
        public bool requireMovementForSprint;
        public bool preventAirSprinting;
        public bool enableMovementSmoothing;
        public float accelerationTime;
        public float decelerationTime;

        [Header("Jump System")]
        public bool enableJumping;
        public int maxJumps;
        public float jumpHeight;
        public float jumpModifier;
        public bool enableVariableJumpHeight;
        public float lowJumpMultiplier;
        public bool enableCoyoteTime;
        public float coyoteTime;
        public bool enableJumpBuffer;
        public float jumpBufferTime;
        public float fallMultiplier;

        public static CustomMovementSettings Default =>
            new CustomMovementSettings
            {
                moveSpeed = 5f,
                runSpeed = 8f,
                gravity = 20f,
                groundCheckDistance = 0.3f,
                enableMovement = true,
                enableSprinting = true,
                faceMovementDirection = true,
                rotationSpeed = 10f,
                sprintMultiplier = 1.5f,
                sprintBuildup = 0.5f,
                requireMovementForSprint = true,
                preventAirSprinting = true,
                enableMovementSmoothing = true,
                accelerationTime = 0.2f,
                decelerationTime = 0.1f,
                enableJumping = true,
                maxJumps = 1,
                jumpHeight = 2f,
                jumpModifier = 1f,
                enableVariableJumpHeight = true,
                lowJumpMultiplier = 1.5f,
                enableCoyoteTime = true,
                coyoteTime = 0.2f,
                enableJumpBuffer = true,
                jumpBufferTime = 0.2f,
                fallMultiplier = 2f,
            };
    }
}
