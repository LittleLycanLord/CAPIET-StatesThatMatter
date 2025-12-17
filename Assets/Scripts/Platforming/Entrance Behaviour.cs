using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace LilLycanLord_Official
{
    public class EntranceBehaviour : MonoBehaviour
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
        [SerializeField] private GameObject player;
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private Transform doorPosition;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() 
        {
            StartCoroutine(ExitDoorSequence());
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Sequence for player exiting from entrance door when scene starts
        /// </summary>
        private IEnumerator ExitDoorSequence()
        {
            Debug.Log("[EntranceBehaviour] ExitDoorSequence started");
            
            if (player == null || doorPosition == null)
            {
                Debug.LogWarning("[EntranceBehaviour] player or doorPosition not assigned!");
                yield break;
            }
            
            // Get player components
            PlayerDoorAnimation doorAnimation = player.GetComponent<PlayerDoorAnimation>();
            PlatformerMovement playerMovement = player.GetComponent<PlatformerMovement>();
            
            Debug.Log($"[EntranceBehaviour] Player components - DoorAnimation: {doorAnimation != null}, Movement: {playerMovement != null}");
            
            // Disable player input during entrance sequence
            if (playerMovement != null)
            {
                Debug.Log("[EntranceBehaviour] Disabling player input");
                playerMovement.SetInputEnabled(false);
            }
            
            // Teleport player to door position
            Debug.Log($"[EntranceBehaviour] Teleporting player to door position: {doorPosition.position}");
            player.transform.position = doorPosition.position;
            
            // Trigger door animator to open
            Debug.Log("[EntranceBehaviour] Opening door");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
            }
            else
            {
                Debug.LogWarning("[EntranceBehaviour] Door animator not assigned!");
            }
            
            // Trigger player exit door animation
            Debug.Log("[EntranceBehaviour] Triggering player exit door animation");
            if (doorAnimation != null)
            {
                doorAnimation.ExitDoor();
            }
            else
            {
                Debug.LogWarning("[EntranceBehaviour] PlayerDoorAnimation component not found on player!");
            }
            
            // Wait for exit duration
            float exitDuration = doorAnimation != null ? doorAnimation.exitDuration : 1f;
            Debug.Log($"[EntranceBehaviour] Waiting {exitDuration} seconds for exit animation");
            yield return new WaitForSeconds(exitDuration);
            
            // Close door
            Debug.Log("[EntranceBehaviour] Closing door");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Close");
            }
            
            // Re-enable player input
            if (playerMovement != null)
            {
                Debug.Log("[EntranceBehaviour] Re-enabling player input");
                playerMovement.SetInputEnabled(true);
            }
            
            Debug.Log("[EntranceBehaviour] Entrance sequence complete!");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}