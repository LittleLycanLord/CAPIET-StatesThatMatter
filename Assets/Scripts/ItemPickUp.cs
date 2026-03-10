using LilLycanLord_Official;
using UnityEngine;
using System.Collections;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickUp : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool hasBeenPickedUp = false;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 90f; // Degrees per second
        [SerializeField] private Vector3 rotationAxis = Vector3.up; // Z-axis for 2D
        
        [Space(10)]
        [Header("Camera Settings")]
        [SerializeField] private string itemViewName = "ItemView";
        [SerializeField] private float cameraChangeSpeed = 2f;
        
        [Space(10)]
        [Header("Rise Animation")]
        [SerializeField] private float rotationAlignDuration = 0.5f; // Time to rotate to Y=0
        [SerializeField] private LeanTweenType rotationAlignEase = LeanTweenType.easeInOutQuad;
        [SerializeField] private float riseHeight = 2f;
        [SerializeField] private float riseDuration = 1.5f;
        [SerializeField] private LeanTweenType riseEase = LeanTweenType.easeOutQuad;
        
        [Space(10)]
        [Header("Effects")]
        [SerializeField] private GameObject particleSystemPrefab;
        [SerializeField] private string pickupSFXName = "ItemPickup";
        
        [Space(10)]
        [Header("Trigger Settings")]
        [SerializeField] private string triggerTag = "Player";
        [SerializeField] private PlatformerMovement player;
        [SerializeField] private bool requireButtonPress = false;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private string previousCameraView;
        private bool playerInRange = false;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            // Ensure collider is set as trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"ItemPickUp on {gameObject.name}: Collider2D is not set as trigger. Setting it now.");
                col.isTrigger = true;
            }
        }

        void Start() { }

        void Update() 
        {
            // Continuous rotation
            if (!hasBeenPickedUp)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
            }
            
            // Check for interaction button if required
            if (requireButtonPress && playerInRange && !hasBeenPickedUp)
            {
                if (Input.GetKeyDown(interactionKey))
                {
                    StartPickupSequence();
                }
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenPickedUp) return;
            
            if (other.CompareTag(triggerTag))
            {
                playerInRange = true;
                
                if (!requireButtonPress)
                {
                    StartPickupSequence();
                }
            }
        }
        
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(triggerTag))
            {
                playerInRange = false;
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Initiates the pickup sequence
        /// </summary>
        private void StartPickupSequence()
        {
            if (hasBeenPickedUp) return;
            
            hasBeenPickedUp = true;
            StartCoroutine(PickupSequence());
        }
        
        /// <summary>
        /// The full pickup sequence: camera change, rise, effects, destroy
        /// </summary>
        private IEnumerator PickupSequence()
        {
            // 1. Store previous camera view
            player.SetMovingLeft(false);
            player.SetMovingRight(false);

            if (CameraViewManager.Instance != null)
            {
                previousCameraView = CameraViewManager.Instance.GetCurrentViewName();
                
                // Switch to item view
                if (!string.IsNullOrEmpty(itemViewName))
                {
                    CameraViewManager.Instance.SetView(itemViewName, cameraChangeSpeed);
                    
                    // Wait for camera blend to complete
                    yield return new WaitForSeconds(cameraChangeSpeed);
                }
            }
            else
            {
                Debug.LogWarning("ItemPickUp: CameraViewManager not found!");
            }
            
            // 2. Rotate to Y = 0 before rising
            float currentYRotation = transform.eulerAngles.y;
            if (Mathf.Abs(currentYRotation) > 0.01f || Mathf.Abs(currentYRotation - 360f) > 0.01f)
            {
                LeanTween.rotateY(gameObject, 0f, rotationAlignDuration)
                    .setEase(rotationAlignEase);
                
                // Wait for rotation alignment to complete
                yield return new WaitForSeconds(rotationAlignDuration);
            }
            
            // 3. Calculate rise target position
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + (Vector3.up * riseHeight);
            
            // 4. Animate the rise using LeanTween
            int tweenId = LeanTween.move(gameObject, targetPosition, riseDuration)
                .setEase(riseEase)
                .id;
            
            // Wait for rise animation to complete
            yield return new WaitForSeconds(riseDuration);
            
            // 5. Spawn particle system at the top
            if (particleSystemPrefab != null)
            {
                GameObject particles = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);
                
                // Auto-destroy particles after their duration
                ParticleSystem ps = particles.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(particles, 3f); // Fallback destruction time
                }
            }
            
            // 6. Play pickup SFX
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSFXName))
            {
                // Play on AudioManager to prevent cutoff when this object is destroyed
                AudioManager.Instance.Play(pickupSFXName, AudioManager.Instance.gameObject);
            }
            
            // Small delay to let effects play
            yield return new WaitForSeconds(0.5f);
            
            // 7. Switch back to previous camera view
            if (CameraViewManager.Instance != null && !string.IsNullOrEmpty(previousCameraView))
            {
                CameraViewManager.Instance.SetView(previousCameraView, cameraChangeSpeed);
                
                // Wait for camera to start blending back
                yield return new WaitForSeconds(cameraChangeSpeed * 0.5f);
            }
            
            // 8. Destroy the item
            Destroy(gameObject);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}