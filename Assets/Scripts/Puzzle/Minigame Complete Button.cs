using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class MinigameCompleteButton : MonoBehaviour
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
        // [Space(10)]
        // [Header("Fields")]
        
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
        public void OnCompleteButtonClicked()
        {
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.CompleteMinigame(true);
            }
            else
            {
                Debug.LogError("MinigameManager instance not found! Make sure platformer scene is loaded.");
            }
        }
        
        public void OnCancelButtonClicked()
        {
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.CompleteMinigame(false);
            }
            else
            {
                Debug.LogError("MinigameManager instance not found! Make sure platformer scene is loaded.");
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}