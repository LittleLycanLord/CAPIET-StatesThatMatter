using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace LilLycanLord_Official
{
    public class GoalBehaviour : MonoBehaviour
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
        [SerializeField] private BoxCollider2D triggerArea;
        [SerializeField] private Transform doorPosition;
        [SerializeField] private PauseMenu levelComplete;
        
        [Space(10)]
        [Header("Camera Settings")]
        [SerializeField] private string goalViewName = "Goal View";
        [SerializeField] private float cameraTransitionSpeed = 2f;
        
        [Space(10)]
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Space(10)]
        [Header("Progress Settings")]
        [SerializeField, Tooltip("Level index for progress tracking (1-based)")] 
        private int levelIndex = 1;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool isActivated = false;

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
        }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[GoalBehaviour] Trigger entered by: {other.gameObject.name}");
            
            // Check if player entered the trigger
            if (other.gameObject == player && !isActivated)
            {
                Debug.Log("[GoalBehaviour] Player detected! Starting goal sequence...");
                isActivated = true;
                StartCoroutine(EnterGoalSequence());
            }
            else if (isActivated)
            {
                Debug.Log("[GoalBehaviour] Goal already activated, ignoring trigger.");
            }
        }
        
        /// <summary>
        /// Sequence for player entering the goal door
        /// </summary>
        private IEnumerator EnterGoalSequence()
        {
            Debug.Log("[GoalBehaviour] EnterGoalSequence started");
            
            // Transition camera to Goal View
            if (CameraViewManager.Instance != null)
            {
                Debug.Log($"[GoalBehaviour] Switching camera to {goalViewName}");
                CameraViewManager.Instance.SetView(goalViewName, cameraTransitionSpeed);
            }
            else
            {
                Debug.LogWarning("[GoalBehaviour] CameraViewManager instance not found!");
            }
            
            if (player == null || doorPosition == null)
            {
                Debug.LogWarning("[GoalBehaviour] player or doorPosition not assigned!");
                yield break;
            }
            
            // Get player components
            PlayerDoorAnimation doorAnimation = player.GetComponent<PlayerDoorAnimation>();
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            
            Debug.Log($"[GoalBehaviour] Player components - DoorAnimation: {doorAnimation != null}, Rigidbody2D: {playerRb != null}");
            
            // Disable player input (but keep script running for animator updates)
            PlatformerMovement playerMovement = player.GetComponent<PlatformerMovement>();
            if (playerMovement != null)
            {
                Debug.Log("[GoalBehaviour] Disabling player input");
                playerMovement.SetInputEnabled(false);
            }
            
            // Stop horizontal movement but keep physics active
            if (playerRb != null)
            {
                Debug.Log("[GoalBehaviour] Stopping horizontal velocity");
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
                playerRb.angularVelocity = 0f;
            }
            
            // Step 1: Fall to door's Y position (using gravity/velocity)
            Debug.Log($"[GoalBehaviour] Step 1: Falling to Y position {doorPosition.position.y}");
            // while (player.transform.position.y > doorPosition.position.y + 0.1f)
            // {
            //     // Let gravity do its thing, or apply downward velocity
            //     if (playerRb != null)
            //     {
            //         playerRb.linearVelocity = new Vector2(0f, Mathf.Max(playerRb.linearVelocity.y, -moveSpeed));
            //     }
            //     yield return null;
            // }
            
            // Stop vertical movement when reached
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
            Debug.Log("[GoalBehaviour] Step 1 complete: Reached Y position");
            
            // Step 2: Walk to door's X position (using horizontal velocity)
            Debug.Log($"[GoalBehaviour] Step 2: Walking to X position {doorPosition.position.x}");
            float targetX = doorPosition.position.x;
            while (Mathf.Abs(player.transform.position.x - targetX) > 0.1f)
            {
                if (playerRb != null)
                {
                    float direction = Mathf.Sign(targetX - player.transform.position.x);
                    playerRb.linearVelocity = new Vector2(direction * moveSpeed, playerRb.linearVelocity.y);
                }
                yield return null;
            }
            
            // Stop all movement when reached
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
            Debug.Log("[GoalBehaviour] Step 2 complete: Reached X position");
            
            // Step 3: Trigger player door animation
            Debug.Log("[GoalBehaviour] Step 3: Triggering player door animation");
            if (doorAnimation != null)
            {
                doorAnimation.EnterDoor();
            }
            else
            {
                Debug.LogWarning("[GoalBehaviour] PlayerDoorAnimation component not found on player!");
            }
            
            // Step 4: Trigger door animator to open
            Debug.Log("[GoalBehaviour] Step 4: Opening door");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
            }
            else
            {
                Debug.LogWarning("[GoalBehaviour] Door animator not assigned!");
            }
            
            // Step 5: Wait for enter duration, then close door
            float enterDuration = doorAnimation != null ? doorAnimation.enterDuration : 1f;
            Debug.Log($"[GoalBehaviour] Step 5: Waiting {enterDuration} seconds before closing door");
            yield return new WaitForSeconds(enterDuration);
            
            Debug.Log("[GoalBehaviour] Closing door");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Close");
            }
            
            Debug.Log("[GoalBehaviour] Goal sequence complete!");
            
            // Mark level as completed
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.CompleteLevel(levelIndex);
                Debug.Log($"[GoalBehaviour] Level {levelIndex} marked as completed");
            }
            else
            {
                Debug.LogWarning("[GoalBehaviour] ProgressManager instance not found!");
            }
            
            // Show level complete menu
            if (levelComplete != null)
            {
                Debug.Log("[GoalBehaviour] Showing level complete menu");
                levelComplete.Pause();
            }
            else
            {
                Debug.LogWarning("[GoalBehaviour] levelComplete PauseMenu not assigned!");
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}