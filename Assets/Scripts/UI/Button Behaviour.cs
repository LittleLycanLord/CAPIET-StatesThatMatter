using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;

namespace LilLycanLord_Official
{
    public class ButtonBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private RectTransform rectTransform;
        
        [Header("Animation Targets")]
        [SerializeField] private RectTransform bobTarget; // Child RectTransform to animate for bobbing (use for Layout Groups)
        [SerializeField] private RectTransform blinkTarget; // RectTransform to enable/disable when blinking

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Bob Settings")]
        [SerializeField] private float bobHeight = 10f; // Distance above/below original position
        [SerializeField] private float bobRate = 1f; // Bobs per second (full cycle)
        
        [Space(10)]
        [Header("Blink Settings")]
        [SerializeField] private float blinkDuration = 2f; // How long to blink for
        [SerializeField] private float blinkRate = 2f; // Blinks per second
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Coroutine bobbingCoroutine;
        private Coroutine blinkingCoroutine;
        private Vector2 originalPosition;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
        }

        void Start() 
        {
            // If no bob target is set, use self (backwards compatibility)
            if (bobTarget == null)
            {
                bobTarget = rectTransform;
            }
            
            // Store original position (will be 0,0 for child, or actual position for self)
            originalPosition = bobTarget.anchoredPosition;
            
            // If there's a UIVirtualButton attached, set its clickDelay to match blinkDuration
            UIVirtualButton virtualButton = GetComponent<UIVirtualButton>();
            if (virtualButton != null)
            {
                virtualButton.clickDelay = blinkDuration;
            }
            
            // Start bobbing animation
            StartBobbing();
        }

        void Update() { }
        
        void OnDestroy()
        {
            // Clean up animations when object is destroyed
            if (bobbingCoroutine != null)
            {
                StopCoroutine(bobbingCoroutine);
            }
            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Start the bobbing animation
        /// </summary>
        private void StartBobbing()
        {
            if (bobbingCoroutine != null)
            {
                StopCoroutine(bobbingCoroutine);
            }
            
            bobbingCoroutine = StartCoroutine(BobbingRoutine());
        }
        
        /// <summary>
        /// Stop the bobbing animation
        /// </summary>
        private void StopBobbing()
        {
            if (bobbingCoroutine != null)
            {
                StopCoroutine(bobbingCoroutine);
                bobbingCoroutine = null;
            }
            
            // Reset to original position
            if (bobTarget != null)
            {
                bobTarget.anchoredPosition = originalPosition;
            }
        }
        
        /// <summary>
        /// Coroutine for bobbing animation - bobs above and below original position
        /// </summary>
        private IEnumerator BobbingRoutine()
        {
            if (bobTarget == null)
            {
                yield break;
            }
            
            float cycleTime = 1f / bobRate; // Time for one full bob cycle
            float halfCycle = cycleTime / 2f; // Time to go from top to bottom or bottom to top
            
            while (true)
            {
                // Move from original to top
                float elapsed = 0f;
                Vector2 startPos = bobTarget.anchoredPosition;
                float targetY = originalPosition.y + bobHeight;
                
                while (elapsed < halfCycle)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / halfCycle);
                    
                    // Smooth ease
                    float easedT = Mathf.SmoothStep(0f, 1f, t);
                    
                    Vector2 newPos = bobTarget.anchoredPosition;
                    newPos.y = Mathf.Lerp(startPos.y, targetY, easedT);
                    bobTarget.anchoredPosition = newPos;
                    
                    yield return null;
                }
                
                // Move from top to bottom
                elapsed = 0f;
                startPos = bobTarget.anchoredPosition;
                targetY = originalPosition.y - bobHeight;
                
                while (elapsed < halfCycle)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / halfCycle);
                    
                    // Smooth ease
                    float easedT = Mathf.SmoothStep(0f, 1f, t);
                    
                    Vector2 newPos = bobTarget.anchoredPosition;
                    newPos.y = Mathf.Lerp(startPos.y, targetY, easedT);
                    bobTarget.anchoredPosition = newPos;
                    
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// Start the blinking animation for the specified duration
        /// </summary>
        private void StartBlinking()
        {
            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
            }
            
            blinkingCoroutine = StartCoroutine(BlinkingRoutine());
        }
        
        /// <summary>
        /// Coroutine for blinking animation - blinks for duration then returns to bobbing
        /// </summary>
        private IEnumerator BlinkingRoutine()
        {
            if (blinkTarget == null)
            {
                Debug.LogWarning("ButtonBehaviour: No blink target assigned!");
                StartBobbing();
                yield break;
            }
            
            float timePerBlink = 1f / blinkRate; // Time for one complete blink (on->off->on)
            float halfBlinkTime = timePerBlink / 2f; // Time for on->off or off->on
            float elapsed = 0f;
            
            while (elapsed < blinkDuration)
            {
                // Blink off
                blinkTarget.gameObject.SetActive(false);
                yield return new WaitForSeconds(halfBlinkTime);
                elapsed += halfBlinkTime;
                
                if (elapsed >= blinkDuration) break;
                
                // Blink on
                blinkTarget.gameObject.SetActive(true);
                yield return new WaitForSeconds(halfBlinkTime);
                elapsed += halfBlinkTime;
            }
            
            // Ensure visible at the end
            blinkTarget.gameObject.SetActive(true);
            
            // Return to bobbing
            StartBobbing();
        }
        
        /// <summary>
        /// Call this when button is pressed (attach to Unity Button's onClick event)
        /// </summary>
        public void OnButtonPressed()
        {
            StopBobbing();
            StartBlinking();
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}