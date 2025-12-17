using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace LilLycanLord_Official
{
    public class PlayerDoorAnimation : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("References")]
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private GameObject playerVisuals;
        
        [Space(10)]
        [Header("Scale Settings")]
        [SerializeField] [Range(0f, 1f)] private float insideScale = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float outsideScale = 1f;
        
        [Space(10)]
        [Header("Alpha Settings")]
        [SerializeField] [Range(0f, 1f)] private float insideAlpha = 0f;
        [SerializeField] [Range(0f, 1f)] private float outsideAlpha = 1f;
        
        [Space(10)]
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Space(10)]
        [Header("Timing")]
        public float enterDuration = 1f;
        public float exitDuration = 1f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Coroutine currentAnimation;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Animate player entering door (outside -> inside)
        /// </summary>
        public void EnterDoor()
        {
            // Stop any running animation
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(AnimateDoorTransition(outsideScale, insideScale, outsideAlpha, insideAlpha, enterDuration));
        }
        
        /// <summary>
        /// Animate player exiting door (inside -> outside)
        /// </summary>
        public void ExitDoor()
        {
            // Stop any running animation
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(AnimateDoorTransition(insideScale, outsideScale, insideAlpha, outsideAlpha, exitDuration));
        }
        
        /// <summary>
        /// Coroutine to animate scale and alpha over time
        /// </summary>
        private IEnumerator AnimateDoorTransition(float startScale, float endScale, float startAlpha, float endAlpha, float duration)
        {
            if (playerVisuals == null || playerSprite == null)
            {
                Debug.LogWarning("PlayerDoorAnimation: playerVisuals or playerSprite not assigned!");
                yield break;
            }
            
            float elapsedTime = 0f;
            Vector3 originalScale = playerVisuals.transform.localScale;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                
                // Evaluate curves
                float scaleProgress = scaleCurve.Evaluate(normalizedTime);
                float alphaProgress = alphaCurve.Evaluate(normalizedTime);
                
                // Interpolate scale
                float currentScale = Mathf.Lerp(startScale, endScale, scaleProgress);
                playerVisuals.transform.localScale = originalScale * currentScale;
                
                // Interpolate alpha
                float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, alphaProgress);
                Color spriteColor = playerSprite.color;
                spriteColor.a = currentAlpha;
                playerSprite.color = spriteColor;
                
                yield return null;
            }
            
            // Ensure final values are set
            playerVisuals.transform.localScale = originalScale * endScale;
            Color finalColor = playerSprite.color;
            finalColor.a = endAlpha;
            playerSprite.color = finalColor;
            
            currentAnimation = null;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}