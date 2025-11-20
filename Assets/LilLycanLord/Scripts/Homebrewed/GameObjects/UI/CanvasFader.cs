using System;
using System.Collections;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced UI fading component that provides smooth alpha transitions for CanvasGroup objects.
    /// Features automatic fading, manual control, interactivity management, and customizable animation curves.
    /// Perfect for UI panels, overlays, tooltips, and any Canvas-based elements that need smooth visibility transitions.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasFader : MonoBehaviour
    {
        [System.Serializable]
        public enum FadeState
        {
            Idle,
            FadingIn,
            FadingOut,
            Interrupted,
        }

        [System.Serializable]
        public enum AutoFadeMode
        {
            None,
            AutoFadeOut,
            AutoFadeIn,
            AutoFadeBoth,
        }

        [System.Serializable]
        public class FadeSettings
        {
            [Header("Animation Curves")]
            [Tooltip("Animation curve for fade in transitions (default: linear)")]
            public AnimationCurve fadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            [Tooltip("Animation curve for fade out transitions (default: linear)")]
            public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            [Header("Timing")]
            [Tooltip("Duration of fade in animation in seconds")]
            [Range(0.1f, 10f)]
            public float fadeInDuration = 0.75f;

            [Tooltip("Duration of fade out animation in seconds")]
            [Range(0.1f, 10f)]
            public float fadeOutDuration = 1.0f;

            [Tooltip("Delay before automatic fade triggers")]
            [Range(0f, 10f)]
            public float autoFadeDelay = 2.0f;

            [Header("Thresholds")]
            [Tooltip("Alpha threshold for considering fade complete")]
            [Range(0f, 0.1f)]
            public float fadeCompleteThreshold = 0.01f;
        }

        [System.Serializable]
        public class InteractivitySettings
        {
            [Tooltip("Control Canvas Group interactability based on alpha")]
            public bool controlInteractability = false;

            [Tooltip("Objects are interactable when visible (alpha above threshold)")]
            public bool interactableWhenVisible = true;

            [Tooltip("Objects remain interactable even when faded out (alpha below threshold)")]
            public bool interactableWhenInvisible = false;

            [Tooltip("Alpha threshold for enabling interactability")]
            [Range(0f, 1f)]
            public float interactabilityThreshold = 0.1f;

            [Tooltip("Control Canvas Group raycast blocking based on alpha")]
            public bool controlRaycastBlocking = false;

            [Tooltip("Alpha threshold for raycast blocking")]
            [Range(0f, 1f)]
            public float raycastBlockingThreshold = 0.1f;
        }

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [SerializeField]
        [Tooltip("Canvas Group component (auto-assigned if null)")]
        private CanvasGroup canvasGroup;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField]
        [Tooltip("Current alpha value")]
        private float currentAlpha = 1f;

        [SerializeField]
        [Tooltip("Current fade state")]
        private FadeState currentState = FadeState.Idle;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        [Tooltip("Enable/disable all fading functionality")]
        private bool enableFading = true;

        [SerializeField]
        [Tooltip("Allow interrupting fade animations")]
        private bool allowFadeInterruption = true;

        [SerializeField]
        [Tooltip("Automatic fade behavior")]
        private AutoFadeMode autoFadeMode = AutoFadeMode.None;

        [SerializeField]
        [Tooltip("Start faded in (alpha = upperAlpha) or faded out (alpha = lowerAlpha)")]
        private bool startFadedIn = true;

        [Header("Alpha Range")]
        [SerializeField]
        [Tooltip("Maximum alpha value (fade in target)")]
        [Range(0f, 1f)]
        private float upperAlpha = 1f;

        [SerializeField]
        [Tooltip("Minimum alpha value (fade out target)")]
        [Range(0f, 1f)]
        private float lowerAlpha = 0f;

        [SerializeField]
        private FadeSettings fadeSettings = new FadeSettings();

        [SerializeField]
        private InteractivitySettings interactivitySettings = new InteractivitySettings();

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when fade in starts")]
        private UnityEvent onFadeInStarted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when fade in completes")]
        private UnityEvent onFadeInCompleted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when fade out starts")]
        private UnityEvent onFadeOutStarted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when fade out completes")]
        private UnityEvent onFadeOutCompleted = new UnityEvent();

        [SerializeField]
        [Tooltip("Called when fade is interrupted")]
        private UnityEvent onFadeInterrupted = new UnityEvent();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private Coroutine currentFadeCoroutine;
        private Coroutine autoFadeCoroutine;
        private float targetAlpha = 1f;
        private bool wasInterruptedLastFrame = false;

        /// <summary>Current alpha value of the canvas group</summary>
        public float Alpha
        {
            get => currentAlpha;
            private set => currentAlpha = Mathf.Clamp01(value);
        }

        /// <summary>Current fade state</summary>
        public FadeState State => currentState;

        /// <summary>Is currently fading in any direction</summary>
        public bool IsFading =>
            currentState == FadeState.FadingIn || currentState == FadeState.FadingOut;

        /// <summary>Is completely visible (alpha >= upperAlpha - threshold)</summary>
        public bool IsVisible => currentAlpha >= (upperAlpha - fadeSettings.fadeCompleteThreshold);

        /// <summary>Is completely invisible (alpha <= lowerAlpha + threshold)</summary>
        public bool IsInvisible =>
            currentAlpha <= (lowerAlpha + fadeSettings.fadeCompleteThreshold);

        /// <summary>Enable/disable fading functionality at runtime</summary>
        public bool EnableFading
        {
            get => enableFading;
            set
            {
                enableFading = value;
                if (!value)
                    StopAllFades();
            }
        }

        /// <summary>Allow interrupting current fade operations</summary>
        public bool AllowInterruption
        {
            get => allowFadeInterruption;
            set => allowFadeInterruption = value;
        }

        /// <summary>Maximum alpha value (fade in target)</summary>
        public float UpperAlpha
        {
            get => upperAlpha;
            set => upperAlpha = Mathf.Clamp01(value);
        }

        /// <summary>Minimum alpha value (fade out target)</summary>
        public float LowerAlpha
        {
            get => lowerAlpha;
            set => lowerAlpha = Mathf.Clamp01(value);
        }

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            // Get or assign canvas group
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Initialize alpha using the new range variables
            currentAlpha = startFadedIn ? upperAlpha : lowerAlpha;
            targetAlpha = currentAlpha;
            ApplyAlphaToCanvasGroup();
            UpdateInteractability();
        }

        void Start()
        {
            // Start auto fade if configured
            if (autoFadeMode != AutoFadeMode.None)
            {
                StartAutoFadeTimer();
            }
        }

        void Update()
        {
            // Only update interactability based on alpha
            UpdateInteractability();

            // Handle auto fade logic
            HandleAutoFadeLogic();
        }

        void OnValidate()
        {
            // Validate animation curves
            if (fadeSettings.fadeInCurve == null || fadeSettings.fadeInCurve.length == 0)
                fadeSettings.fadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            if (fadeSettings.fadeOutCurve == null || fadeSettings.fadeOutCurve.length == 0)
                fadeSettings.fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f); // Use consistent linear curve

            // Validate alpha ranges
            upperAlpha = Mathf.Clamp01(upperAlpha);
            lowerAlpha = Mathf.Clamp01(lowerAlpha);

            // Ensure upperAlpha >= lowerAlpha
            if (upperAlpha < lowerAlpha)
            {
                upperAlpha = lowerAlpha;
            }
        }

        //* ╔════════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚════════════════════╝

        /// <summary>
        /// Fade in to full visibility with optional completion callback
        /// </summary>
        [ContextMenu("Fade In")]
        public void FadeIn(Action onComplete = null)
        {
            if (!enableFading)
                return;

            StartFade(
                upperAlpha,
                fadeSettings.fadeInDuration,
                fadeSettings.fadeInCurve,
                onComplete,
                true
            );
        }

        public void FadeInViaUnityEvent()
        {
            FadeIn(null);
        }

        /// <summary>
        /// Fade out to full transparency with optional completion callback
        /// </summary>
        [ContextMenu("Fade Out")]
        public void FadeOut(Action onComplete = null)
        {
            if (!enableFading)
                return;

            StartFade(
                lowerAlpha,
                fadeSettings.fadeOutDuration,
                fadeSettings.fadeOutCurve,
                onComplete,
                false
            );
        }

        public void FadeOutViaUnityEvent()
        {
            FadeOut(null);
        }

        /// <summary>
        /// Fade to specific alpha value with custom duration
        /// </summary>
        public void FadeTo(float targetAlpha, float duration, Action onComplete = null)
        {
            if (!enableFading)
                return;

            bool isFadingIn = targetAlpha > currentAlpha;
            AnimationCurve curve = isFadingIn
                ? fadeSettings.fadeInCurve
                : fadeSettings.fadeOutCurve;

            StartFade(targetAlpha, duration, curve, onComplete, isFadingIn);
        }

        /// <summary>
        /// Fade to specific alpha using custom curve
        /// </summary>
        public void FadeToWithCurve(
            float targetAlpha,
            float duration,
            AnimationCurve customCurve,
            Action onComplete = null
        )
        {
            if (!enableFading)
                return;

            bool isFadingIn = targetAlpha > currentAlpha;
            StartFade(targetAlpha, duration, customCurve, onComplete, isFadingIn);
        }

        /// <summary>
        /// Instantly set alpha without animation
        /// </summary>
        public void SetAlphaImmediate(float alpha)
        {
            StopAllFades();
            currentAlpha = Mathf.Clamp01(alpha);
            targetAlpha = currentAlpha;
            currentState = FadeState.Idle;
            ApplyAlphaToCanvasGroup();
            UpdateInteractability();
        }

        /// <summary>
        /// Stop all fade animations and lock at current alpha
        /// </summary>
        [ContextMenu("Stop All Fades")]
        public void StopAllFades()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            if (autoFadeCoroutine != null)
            {
                StopCoroutine(autoFadeCoroutine);
                autoFadeCoroutine = null;
            }

            if (currentState != FadeState.Idle)
            {
                currentState = FadeState.Interrupted;
                onFadeInterrupted.Invoke();
                currentState = FadeState.Idle;
            }
        }

        /// <summary>
        /// Toggle between fully visible and invisible
        /// </summary>
        [ContextMenu("Toggle Visibility")]
        public void ToggleVisibility(Action onComplete = null)
        {
            if (IsVisible)
                FadeOut(onComplete);
            else
                FadeIn(onComplete);
        }

        /// <summary>
        /// Set alpha range at runtime
        /// </summary>
        public void SetAlphaRange(float newLowerAlpha, float newUpperAlpha)
        {
            lowerAlpha = Mathf.Clamp01(newLowerAlpha);
            upperAlpha = Mathf.Clamp01(newUpperAlpha);

            // Ensure valid range
            if (upperAlpha < lowerAlpha)
            {
                upperAlpha = lowerAlpha;
            }
        }

        /// <summary>
        /// Fade to upper alpha value (equivalent to FadeIn)
        /// </summary>
        public void FadeToUpperAlpha(Action onComplete = null)
        {
            FadeTo(upperAlpha, fadeSettings.fadeInDuration, onComplete);
        }

        /// <summary>
        /// Fade to lower alpha value (equivalent to FadeOut)
        /// </summary>
        public void FadeToLowerAlpha(Action onComplete = null)
        {
            FadeTo(lowerAlpha, fadeSettings.fadeOutDuration, onComplete);
        }

        /// <summary>
        /// Get detailed state information for debugging
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UICanvasFader Debug:\n"
                + $"Alpha: {currentAlpha:F3} | Target: {targetAlpha:F3}\n"
                + $"State: {currentState}\n"
                + $"Visible: {IsVisible} | Invisible: {IsInvisible}\n"
                + $"Interactable: {(canvasGroup?.interactable ?? false)}\n"
                + $"Blocks Raycasts: {(canvasGroup?.blocksRaycasts ?? false)}\n"
                + $"Auto Fade: {autoFadeMode}";
        }

        /// <summary>
        /// Reset to default settings
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            StopAllFades();
            fadeSettings = new FadeSettings();
            interactivitySettings = new InteractivitySettings();
            enableFading = true;
            allowFadeInterruption = true;
            autoFadeMode = AutoFadeMode.None;
            startFadedIn = true;
            upperAlpha = 1f;
            lowerAlpha = 0f;
            SetAlphaImmediate(upperAlpha);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        private void StartFade(
            float target,
            float duration,
            AnimationCurve curve,
            Action onComplete,
            bool isFadeIn
        )
        {
            // Check if interruption is allowed
            if (IsFading && !allowFadeInterruption)
                return;

            // Stop any existing fades
            if (IsFading)
            {
                StopAllFades();
                wasInterruptedLastFrame = true;
            }

            // Set target and state
            targetAlpha = Mathf.Clamp01(target);
            currentState = isFadeIn ? FadeState.FadingIn : FadeState.FadingOut;

            // Invoke start event
            if (isFadeIn)
                onFadeInStarted.Invoke();
            else
                onFadeOutStarted.Invoke();

            // Start fade coroutine
            currentFadeCoroutine = StartCoroutine(
                FadeCoroutine(targetAlpha, duration, curve, onComplete, isFadeIn)
            );
        }

        private IEnumerator FadeCoroutine(
            float target,
            float duration,
            AnimationCurve curve,
            Action onComplete,
            bool isFadeIn
        )
        {
            float startAlpha = currentAlpha;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / duration;

                // Apply curve evaluation
                float curveValue = curve.Evaluate(normalizedTime);
                currentAlpha = Mathf.Lerp(startAlpha, target, curveValue);

                // Apply the alpha to the canvas group immediately
                ApplyAlphaToCanvasGroup();

                yield return null;
            }

            // Ensure final value is set
            currentAlpha = target;
            ApplyAlphaToCanvasGroup(); // Apply final alpha value
            currentState = FadeState.Idle;
            currentFadeCoroutine = null;

            // Invoke completion events
            onComplete?.Invoke();
            if (isFadeIn)
                onFadeInCompleted.Invoke();
            else
                onFadeOutCompleted.Invoke();

            // Restart auto fade timer if needed
            if (autoFadeMode != AutoFadeMode.None)
            {
                StartAutoFadeTimer();
            }
        }

        private void HandleAutoFadeLogic()
        {
            if (!enableFading || autoFadeMode == AutoFadeMode.None || IsFading)
                return;

            // Auto fade logic is handled by timer, but we can add immediate triggers here if needed
        }

        private void StartAutoFadeTimer()
        {
            if (!enableFading || autoFadeMode == AutoFadeMode.None)
                return;

            if (autoFadeCoroutine != null)
            {
                StopCoroutine(autoFadeCoroutine);
            }

            autoFadeCoroutine = StartCoroutine(AutoFadeTimer());
        }

        private IEnumerator AutoFadeTimer()
        {
            yield return new WaitForSeconds(fadeSettings.autoFadeDelay);

            // Determine which auto fade to trigger
            switch (autoFadeMode)
            {
                case AutoFadeMode.AutoFadeOut:
                    if (IsVisible)
                        FadeOut();
                    break;

                case AutoFadeMode.AutoFadeIn:
                    if (IsInvisible)
                        FadeIn();
                    break;

                case AutoFadeMode.AutoFadeBoth:
                    if (IsVisible)
                        FadeOut();
                    else if (IsInvisible)
                        FadeIn();
                    break;
            }

            autoFadeCoroutine = null;
        }

        private void ApplyAlphaToCanvasGroup()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = currentAlpha;
            }
        }

        private void UpdateInteractability()
        {
            if (canvasGroup == null)
                return;

            // Update interactability
            if (interactivitySettings.controlInteractability)
            {
                bool isVisible = currentAlpha >= interactivitySettings.interactabilityThreshold;
                bool isInvisible = currentAlpha < interactivitySettings.interactabilityThreshold;

                bool shouldBeInteractable =
                    (isVisible && interactivitySettings.interactableWhenVisible)
                    || (isInvisible && interactivitySettings.interactableWhenInvisible);

                canvasGroup.interactable = shouldBeInteractable;
            }

            // Update raycast blocking
            if (interactivitySettings.controlRaycastBlocking)
            {
                canvasGroup.blocksRaycasts =
                    currentAlpha >= interactivitySettings.raycastBlockingThreshold;
            }
        }

        [ContextMenu("Print Debug Info")]
        private void PrintDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }

        [ContextMenu("Test Fade In")]
        private void TestFadeIn()
        {
            FadeIn(() => Debug.Log("Fade In Complete!"));
        }

        [ContextMenu("Test Fade Out")]
        private void TestFadeOut()
        {
            FadeOut(() => Debug.Log("Fade Out Complete!"));
        }
    }
}
