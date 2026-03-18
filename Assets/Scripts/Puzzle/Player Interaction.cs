using UnityEngine;

namespace LilLycanLord_Official
{
    public class PlayerInteraction : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private GameObject interactionButton;
        [SerializeField] private PlatformerMovement platformerMovement;
        [SerializeField] private GameObject visuals;
        

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private MatterBehaviour currentMatterBehaviour;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        [SerializeField] private string matterTag = "Matter";

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private int matterBlocksInRange = 0;
        private float originalXPosition;
        private float originalYPosition;
        private float flippedXPosition;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Start()
        {
            if (interactionButton != null)
                interactionButton.SetActive(false);

            matterBlocksInRange = 0;
            originalXPosition = transform.localPosition.x;
            originalYPosition = transform.localPosition.y;
            flippedXPosition = -originalXPosition;
            
        }
        
        void Update()
        {
            if(platformerMovement != null)
                transform.localPosition = new Vector3(visuals.transform.localScale.x > 0 ? originalXPosition : flippedXPosition, originalYPosition, 0);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(matterTag))
            {
                MatterBehaviour matter = other.GetComponent<MatterBehaviour>();
                if (matter != null)
                {
                    matterBlocksInRange++;
                    currentMatterBehaviour = matter;
                    
                    // Show button when first matter block is entered
                    if (matterBlocksInRange == 1 && interactionButton != null)
                    {
                        interactionButton.SetActive(true);
                    }
                    
                    // Show highlight on the matter block
                    matter.ShowHighlight();
                }

                // Alternative: Using an interface for extensibility
                // IInteractable interactable = other.GetComponent<IInteractable>();
                // if (interactable != null)
                // {
                //     currentInteractable = interactable;
                // }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(matterTag))
            {
                MatterBehaviour matter = other.GetComponent<MatterBehaviour>();
                if (matter != null)
                {
                    matterBlocksInRange--;
                    
                    // Hide highlight on the matter block
                    matter.HideHighlight();
                    
                    // Clear current matter if we're exiting it
                    if (matter == currentMatterBehaviour)
                    {
                        currentMatterBehaviour = null;
                    }
                    
                    // Hide button only when no matter blocks are in range
                    if (matterBlocksInRange <= 0 && interactionButton != null)
                    {
                        matterBlocksInRange = 0; // Clamp to 0
                        interactionButton.SetActive(false);
                    }
                }

                // Alternative: Using an interface
                // IInteractable interactable = other.GetComponent<IInteractable>();
                // if (interactable != null && interactable == currentInteractable)
                // {
                //     currentInteractable = null;
                // }
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        public void Interact()
        {
            if(platformerMovement == null)
                platformerMovement = GetComponent<PlatformerMovement>();
            platformerMovement.Stop();
            if (currentMatterBehaviour != null)
            {
                currentMatterBehaviour.OnInteractionButtonPressed();
            }

            // Alternative: Using an interface
            // if (currentInteractable != null)
            // {
            //     currentInteractable.Interact();
            // }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}

// Alternative Design: Using an Interface for multiple interactable types
// This would allow any object to be interactable, not just MatterBehaviour
/*
namespace LilLycanLord_Official
{
    public interface IInteractable
    {
        void Interact();
    }

    // Then MatterBehaviour would implement IInteractable:
    // public class MatterBehaviour : MonoBehaviour, IInteractable
    // {
    //     public void Interact()
    //     {
    //         OnInteractionButtonPressed();
    //     }
    // }

    // Other interactable objects could implement the same interface:
    // public class Door : MonoBehaviour, IInteractable
    // {
    //     public void Interact()
    //     {
    //         OpenDoor();
    //     }
    // }
}
*/