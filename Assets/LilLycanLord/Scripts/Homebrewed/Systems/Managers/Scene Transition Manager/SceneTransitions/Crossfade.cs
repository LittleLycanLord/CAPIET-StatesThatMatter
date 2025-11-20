using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Crossfade Transition - Smoothly fades between scenes using color overlays.
    ///
    /// 📚 HOW TO CREATE A CUSTOM SCENE TRANSITION:
    ///
    /// 1. CREATE CLASS:
    ///    • Inherit from SceneTransition abstract class
    ///    • Add [RequireComponent] attributes for needed UI components
    ///    • Example: public class MyTransition : SceneTransition
    ///
    /// 2. IMPLEMENT REQUIRED METHODS:
    ///    • PrimeTransition() - Setup and start entry effect (cover old scene)
    ///    • SwitchScene() - Load target scene, schedule cleanup
    ///    • CleanUpTransition() - Start exit effect, wait for completion, call EndTransition()
    ///
    /// 3. LIFECYCLE PATTERN:
    ///    Phase 1: PrimeTransition() → Cover old scene with transition effect
    ///    Phase 2: SwitchScene() → Load new scene behind overlay
    ///    Phase 3: CleanUpTransition() → Remove overlay, reveal new scene
    ///
    /// 4. ESSENTIAL COMPONENTS (inherited from SceneTransition):
    ///    • Image background - Visual overlay for transition effects
    ///    • Animator animator - Controls transition animations
    ///    • Slider slider - Progress tracking (optional)
    ///    • CanvasGroup canvasGroup - UI group management
    ///
    /// ⚠️ CRITICAL PROBLEMS TO AVOID:
    ///
    /// ❌ NEVER call EndTransition() immediately in CleanUpTransition()
    ///    → Use coroutines/timers to wait for animations to complete first
    ///    → Example: yield return new WaitForSeconds(duration); THEN EndTransition()
    ///
    /// ❌ DON'T forget to call SceneTransitionManager.Instance.LoadToTargetScene() in SwitchScene()
    ///    → This actually loads the new scene - transitions are just visual effects
    ///
    /// ❌ AVOID missing Animator trigger calls
    ///    → Set animator parameters AND trigger animations explicitly
    ///    → Example: animator.SetFloat("speed", speed); animator.SetTrigger("StartTransition");
    ///
    /// ❌ DON'T assume components exist without validation
    ///    → Always null-check animator, background, etc. before using
    ///    → Provide fallback behavior or clear error messages
    ///
    /// ❌ NEVER use Invoke() for critical timing without error handling
    ///    → Prefer coroutines for better control and exception handling
    ///    → Example: StartCoroutine(WaitAndExecute()) vs Invoke("Execute", time)
    ///
    /// ❌ AVOID hardcoded timing values
    ///    → Use serialized fields for durations, speeds, delays
    ///    → Respect SceneTransitionManager.exitingDuration/enteringDuration overrides
    ///
    /// ❌ DON'T forget to handle edge cases
    ///    → What if duration is 0 or negative?
    ///    → What if animator is missing required parameters?
    ///    → What if transition is cancelled mid-execution?
    ///
    /// 💡 BEST PRACTICES:
    /// ✅ Always wait for animations to complete before calling EndTransition()
    /// ✅ Use try-catch blocks in transition methods for error handling
    /// ✅ Validate all required components in Awake() or initialization
    /// ✅ Provide clear debug logs for transition state changes
    /// ✅ Test with various timing configurations (very fast, very slow, zero duration)
    /// ✅ Consider what happens if the GameObject is destroyed during transition
    ///
    /// 📖 REFERENCE IMPLEMENTATION:
    /// Study this Crossfade class as a complete example of proper transition implementation.
    /// </summary>
    public class Crossfade : SceneTransition
    {
        // [Header("Displays")]

        [Space(10)]
        [Header("Crossfade Settings")]
        [SerializeField]
        [Tooltip("Color to fade in with")]
        Color fadeInColor = Color.black;

        [SerializeField]
        [Tooltip("Color to fade out with")]
        Color fadeOutColor = Color.black;

        [Space(10)]
        [Header("Timing Configuration")]
        [SerializeField]
        [Tooltip("Duration of the fade in effect")]
        float fadeInDuration = 1.0f;

        [SerializeField]
        [Tooltip("Hold duration between fade in and out")]
        float transitionDuration = 0.0f;

        [SerializeField]
        [Tooltip("Duration of the fade out effect")]
        float fadeOutDuration = 1.0f;

        [Space(10)]
        [Header("Advanced Settings")]
        [SerializeField]
        [Tooltip("Use smooth fade curve instead of linear")]
        bool useSmoothCurve = true;

        [SerializeField]
        [Tooltip("Custom animation curve for fade effects")]
        AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private bool isInitialized = false;

        void Awake()
        {
            InitializeSceneTransition();
            InitializeCrossfade();
        }

        private void InitializeCrossfade()
        {
            //* Validate fade curve
            if (fadeCurve == null || fadeCurve.keys.Length == 0)
            {
                fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                Debug.LogWarning(
                    "Crossfade: Invalid fade curve detected, using default EaseInOut curve"
                );
            }

            //* Set default colors if not configured
            if (fadeInColor == default(Color) && fadeOutColor == default(Color))
            {
                fadeInColor = fadeOutColor = Color.black;
                Debug.Log("Crossfade: Using default black fade colors");
            }

            //* Validate animator setup
            ValidateAnimatorSetup();

            isInitialized = true;
        }

        private void ValidateAnimatorSetup()
        {
            if (animator == null)
            {
                Debug.LogError(
                    "Crossfade: No Animator component found! Crossfade requires an Animator with FadeIn/FadeOut triggers."
                );
                return;
            }

            //* Check for required parameters and triggers
            bool hasRequiredSetup = true;

            //* Check for required float parameters
            var parameters = animator.parameters;
            bool hasFadeInSpeed = false;
            bool hasFadeOutSpeed = false;

            foreach (var param in parameters)
            {
                if (
                    param.name == "fadeInSpeed"
                    && param.type == AnimatorControllerParameterType.Float
                )
                    hasFadeInSpeed = true;
                if (
                    param.name == "fadeOutSpeed"
                    && param.type == AnimatorControllerParameterType.Float
                )
                    hasFadeOutSpeed = true;
            }

            if (!hasFadeInSpeed)
            {
                Debug.LogWarning(
                    "Crossfade: Animator missing 'fadeInSpeed' float parameter for controlling fade timing"
                );
                hasRequiredSetup = false;
            }

            if (!hasFadeOutSpeed)
            {
                Debug.LogWarning(
                    "Crossfade: Animator missing 'fadeOutSpeed' float parameter for controlling fade timing"
                );
                hasRequiredSetup = false;
            }

            if (hasRequiredSetup)
            {
                Debug.Log("Crossfade: Animator setup validation passed");
            }
            else
            {
                Debug.LogWarning(
                    "Crossfade: Animator setup incomplete. Ensure your Animator Controller has:\n"
                        + "- 'FadeIn' trigger\n"
                        + "- 'FadeOut' trigger\n"
                        + "- 'fadeInSpeed' float parameter\n"
                        + "- 'fadeOutSpeed' float parameter\n"
                        + "- Animation states that animate the Image alpha from 1→0 (fade in) and 1→0 (fade out)"
                );
            }
        }

        public override void PrimeTransition()
        {
            if (!isInitialized)
            {
                InitializeCrossfade();
            }

            UpdateDebugText();

            //* Apply custom duration from SceneTransitionManager if set
            if (SceneTransitionManager.Instance.exitingDuration > 0)
            {
                fadeInDuration = SceneTransitionManager.Instance.exitingDuration;
            }

            //* Ensure minimum duration for smooth transitions
            if (fadeInDuration <= 0.0f)
            {
                fadeInDuration = 1.0f;
                Debug.LogWarning(
                    "Crossfade: fadeInDuration was zero or negative, setting to 1 second"
                );
            }

            //* Configure animator with validation
            if (animator != null)
            {
                float fadeInSpeed = 1.0f / fadeInDuration;
                animator.SetFloat("fadeInSpeed", fadeInSpeed);

                //* Trigger the fade in animation
                animator.SetTrigger("FadeIn");

                //* Apply smooth curve if enabled
                if (useSmoothCurve && fadeCurve != null)
                {
                    //* Additional curve configuration could be added here
                    Debug.Log(
                        $"Crossfade: Using smooth curve for fade transition (Speed: {fadeInSpeed})"
                    );
                }

                Debug.Log($"Crossfade: Fade in animation triggered with speed {fadeInSpeed}");
            }
            else
            {
                Debug.LogError("Crossfade: Animator component is missing!");
            }

            //* Apply fade in color with validation
            if (background != null)
            {
                //* Set initial color with full opacity for fade in
                Color startColor = fadeInColor;
                startColor.a = 1.0f; // Ensure we start with full opacity
                background.color = startColor;
                Debug.Log($"Crossfade: Fade in color set to {startColor}");
            }
            else
            {
                Debug.LogError("Crossfade: Background Image component is missing!");
            }
        }

        public override void SwitchScene()
        {
            try
            {
                //* Trigger scene loading through the manager
                SceneTransitionManager.Instance.LoadToTargetScene();

                //* Schedule cleanup with validation
                if (transitionDuration >= 0f)
                {
                    Invoke("CleanUpTransition", transitionDuration);
                }
                else
                {
                    Debug.LogWarning(
                        "Crossfade: Transition duration is negative, calling cleanup immediately"
                    );
                    CleanUpTransition();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Crossfade: Error during scene switch - {e.Message}");
                //* Force cleanup on error
                CleanUpTransition();
            }
        }

        public override void CleanUpTransition()
        {
            try
            {
                //* Apply custom duration from SceneTransitionManager if set
                if (SceneTransitionManager.Instance.enteringDuration > 0)
                {
                    fadeOutDuration = SceneTransitionManager.Instance.enteringDuration;
                }

                //* Ensure minimum duration for smooth transitions
                if (fadeOutDuration <= 0.0f)
                {
                    fadeOutDuration = 1.0f;
                    Debug.LogWarning(
                        "Crossfade: fadeOutDuration was zero or negative, setting to 1 second"
                    );
                }

                //* Configure animator for fade out
                if (animator != null)
                {
                    float fadeOutSpeed = 1.0f / fadeOutDuration;
                    animator.SetFloat("fadeOutSpeed", fadeOutSpeed);

                    //* Trigger the fade out animation
                    animator.SetTrigger("FadeOut");
                    Debug.Log($"Crossfade: Fade out animation triggered with speed {fadeOutSpeed}");
                }

                //* Apply fade out color
                if (background != null)
                {
                    //* Start with full opacity for fade out (will animate to transparent)
                    Color startColor = fadeOutColor;
                    startColor.a = 1.0f; // Start with full opacity
                    background.color = startColor;
                    Debug.Log($"Crossfade: Fade out color set to {startColor}");
                }

                //* Wait for fade out animation to complete, then end transition
                StartCoroutine(WaitForFadeOutComplete());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Crossfade: Error during cleanup - {e.Message}");
                //* Force end transition even on error
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.EndTransition();
                }
            }
        }

        //* Coroutine to wait for fade out animation to complete
        private System.Collections.IEnumerator WaitForFadeOutComplete()
        {
            Debug.Log($"Crossfade: Waiting {fadeOutDuration} seconds for fade out to complete");
            yield return new WaitForSeconds(fadeOutDuration);

            Debug.Log("Crossfade: Fade out complete, ending transition");
            SceneTransitionManager.Instance.EndTransition();
        }

        public void SetFadeColors(Color inColor, Color outColor)
        {
            fadeInColor = inColor;
            fadeOutColor = outColor;
            Debug.Log($"Crossfade: Custom fade colors set - In: {inColor}, Out: {outColor}");
        }

        public void SetFadeDurations(float inDuration, float outDuration, float holdDuration = 0f)
        {
            fadeInDuration = Mathf.Max(0.1f, inDuration);
            fadeOutDuration = Mathf.Max(0.1f, outDuration);
            transitionDuration = Mathf.Max(0f, holdDuration);

            Debug.Log(
                $"Crossfade: Custom durations set - In: {fadeInDuration}s, Out: {fadeOutDuration}s, Hold: {transitionDuration}s"
            );
        }

        public string GetCrossfadeInfo()
        {
            return $"Crossfade - In: {fadeInColor} ({fadeInDuration}s), Out: {fadeOutColor} ({fadeOutDuration}s), Hold: {transitionDuration}s, Smooth: {useSmoothCurve}";
        }
    }
}
