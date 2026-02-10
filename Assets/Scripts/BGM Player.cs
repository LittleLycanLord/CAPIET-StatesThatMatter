using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    public class BGMPlayer : MonoBehaviour
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
        [Header("BGM Settings")]
        [Tooltip("Name of the BGM sound to play from AudioManager")]
        [SerializeField] private string bgmSoundName = "BGM";
        
        [Tooltip("Duration of the fade in effect")]
        [SerializeField] private float fadeInDuration = 2f;
        
        [Tooltip("Play BGM automatically on Start")]
        [SerializeField] private bool playOnStart = true;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private AudioSource bgmAudioSource;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start()
        {
            if (playOnStart)
            {
                PlayBGM();
            }
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Plays the BGM with fade in effect
        /// </summary>
        public void PlayBGM()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[BGMPlayer] AudioManager instance not found!");
                return;
            }
            
            if (string.IsNullOrEmpty(bgmSoundName))
            {
                Debug.LogWarning("[BGMPlayer] BGM sound name is empty!");
                return;
            }
            
            bgmAudioSource = AudioManager.Instance.PlayWithFadeIn(
                bgmSoundName,
                fadeInDuration,
                gameObject
            );
            
            if (bgmAudioSource != null)
            {
                Debug.Log($"[BGMPlayer] Playing '{bgmSoundName}' with {fadeInDuration}s fade in");
            }
        }
        
        /// <summary>
        /// Stops the BGM with optional fade out
        /// </summary>
        public void StopBGM(float fadeOutDuration = 1f)
        {
            if (AudioManager.Instance == null || bgmAudioSource == null)
                return;
                
            AudioManager.Instance.Stop(bgmAudioSource, fadeOutDuration);
            Debug.Log($"[BGMPlayer] Stopping BGM with {fadeOutDuration}s fade out");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}