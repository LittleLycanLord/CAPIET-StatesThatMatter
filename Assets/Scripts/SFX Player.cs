using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    public class SFXPlayer : MonoBehaviour
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
        [Header("SFX Settings")]
        [Tooltip("Name of the sound effect to play from AudioManager")]
        [SerializeField] private string sfxName = "";
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

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
        /// Plays the configured sound effect - can be called from UnityEvents
        /// </summary>
        public void PlaySFX()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[SFXPlayer] AudioManager instance not found!");
                return;
            }
            
            if (string.IsNullOrEmpty(sfxName))
            {
                Debug.LogWarning("[SFXPlayer] SFX name is empty!");
                return;
            }
            
            AudioManager.Instance.Play(sfxName, gameObject);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}