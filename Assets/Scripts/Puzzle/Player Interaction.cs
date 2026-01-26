using UnityEngine;

namespace LilLycanLord_Official
{
    public class PlayerInteraction : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

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

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(matterTag))
            {
                MatterBehaviour matter = other.GetComponent<MatterBehaviour>();
                if (matter != null)
                {
                    currentMatterBehaviour = matter;
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
                if (matter != null && matter == currentMatterBehaviour)
                {
                    currentMatterBehaviour = null;
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