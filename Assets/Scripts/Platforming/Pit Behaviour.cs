using LilLycanLord_Official;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Assertions;
using System.Collections;

namespace LilLycanLord_Official
{
    public class PitBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private SFXPlayer sfxPlayer;
        [SerializeField] private BoxCollider2D triggerArea;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("References")]
        [SerializeField, Tooltip("The player GameObject")]
        private GameObject player;
        
        [SerializeField, Tooltip("The entrance transform to teleport the player back to")]
        private Transform entranceTransform;
        
        [Space(10)]
        [Header("Settings")]
        [SerializeField, Tooltip("Delay before teleporting the player back (in seconds)")]
        private float teleportDelay = 0.5f;
        
        [SerializeField, Tooltip("Name of the crossfade transition")]
        private string transitionName = "Crossfade";
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool isTriggered = false;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            // Ensure trigger area is set as trigger
            if (triggerArea != null)
            {
                triggerArea.isTrigger = true;
            }
            
            // Get SFXPlayer if not assigned
            if (sfxPlayer == null)
            {
                sfxPlayer = GetComponent<SFXPlayer>();
            }
        }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[PitBehaviour] Trigger entered by: {other.gameObject.name}");
            
            // Check if player entered the trigger
            if (other.gameObject == player && !isTriggered)
            {
                Debug.Log("[PitBehaviour] Player fell into pit!");
                isTriggered = true;
                StartCoroutine(PitFallSequence());
            }
        }
        
        /// <summary>
        /// Sequence for when player falls into pit
        /// </summary>
        private IEnumerator PitFallSequence()
        {
            Debug.Log("[PitBehaviour] Pit fall sequence started");
            Camera.main.GetComponent<CinemachineBrain>().enabled = false;
            // Play fall sound effect
            if (sfxPlayer != null)
            {
                Debug.Log("[PitBehaviour] Playing fall SFX");
                sfxPlayer.PlaySFX();
            }
            else
            {
                Debug.LogWarning("[PitBehaviour] SFXPlayer component not assigned!");
            }
            
            // Trigger transition effect with teleport at midpoint
            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log($"[PitBehaviour] Playing transition effect '{transitionName}'");
                 yield return new WaitForSeconds(teleportDelay);
                SceneTransitionManager.Instance.PlayTransitionEffect(transitionName, () =>
                {
                   
                    TeleportPlayerToEntrance();
                });
            }
            else
            {
                Debug.LogWarning("[PitBehaviour] SceneTransitionManager instance not found! Teleporting without transition.");
                // Wait for delay then teleport
                yield return new WaitForSeconds(teleportDelay);
                TeleportPlayerToEntrance();
            }
            
            // Reset triggered state
            isTriggered = false;
            Debug.Log("[PitBehaviour] Pit fall sequence complete");
        }
        
        /// <summary>
        /// Teleports the player back to the entrance position
        /// </summary>
        private void TeleportPlayerToEntrance()
        {
            
            if (player != null && entranceTransform != null)
            {
                Debug.Log($"[PitBehaviour] Teleporting player to {entranceTransform.position}");
                player.transform.position = entranceTransform.position;
                
                // Reset player velocity if they have a Rigidbody2D
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Camera.main.GetComponent<CinemachineBrain>().enabled = true;
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.angularVelocity = 0f;
                }
            }
            else
            {
                Debug.LogWarning("[PitBehaviour] Player or entrance transform not assigned!");
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}