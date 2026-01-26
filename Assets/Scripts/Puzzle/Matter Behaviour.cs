using UnityEngine;

namespace LilLycanLord_Official
{
    public enum MatterPhase
    {
        Solid,
        Liquid,
        Gas
    }

    public class MatterBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        public GameObject interactionButton;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        public MatterPhase phase;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Start()
        {
            if (interactionButton != null)
                interactionButton.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && interactionButton != null)
            {
                interactionButton.SetActive(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && interactionButton != null)
            {
                interactionButton.SetActive(false);
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        public void OnInteractionButtonPressed()
        {
            MinigameManager.Instance?.StartMinigame(this);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}