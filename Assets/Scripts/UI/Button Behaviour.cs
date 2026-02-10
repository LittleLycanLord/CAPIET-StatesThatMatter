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
        [Header("Animation Targets")]
        [SerializeField] private RectTransform blinkTarget; // RectTransform to enable/disable when blinking

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Blink Settings")]
        [SerializeField] private float blinkDuration = 2f; // How long to blink for
        [SerializeField] private float blinkRate = 2f; // Blinks per second
        
        [Space(10)]
        [Header("Audio Settings")]
        [SerializeField] private string sfxName = "ButtonClick"; // Sound effect to play on button press
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Coroutine blinkingCoroutine;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() 
        {
            // If there's a UIVirtualButton attached, set its clickDelay to match blinkDuration
            UIVirtualButton virtualButton = GetComponent<UIVirtualButton>();
            if (virtualButton != null)
            {
                virtualButton.clickDelay = blinkDuration;
            }
        }

        void Update() { }
        
        void OnDestroy()
        {
            // Clean up animations when object is destroyed
            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
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
        /// Plays the button sound effect using AudioManager
        /// </summary>
        private void PlaySFX()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[ButtonBehaviour] AudioManager instance not found!");
                return;
            }
            
            if (string.IsNullOrEmpty(sfxName))
            {
                return;
            }
            
            AudioManager.Instance.Play(sfxName, gameObject);
        }
        
        /// <summary>
        /// Coroutine for blinking animation - blinks for duration then returns to bobbing
        /// </summary>
        private IEnumerator BlinkingRoutine()
        {
            if (blinkTarget == null)
            {
                Debug.LogWarning("ButtonBehaviour: No blink target assigned!");
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
        }
        
        /// <summary>
        /// Call this when button is pressed (attach to Unity Button's onClick event)
        /// </summary>
        public void OnButtonPressed()
        {
            PlaySFX();
            StartBlinking();
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}