using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Simple script to display dialogue text when scene starts
    /// Attach to any GameObject and configure text lines in inspector
    /// </summary>
    public class MiniDialogueStartScript : MonoBehaviour
    {
        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Header("Text Settings")]
        [SerializeField] [TextArea(2, 5)] private string[] textLinesToShow = new string[] 
        {
            "Welcome to the scene!",
            "Configure these lines in the inspector.",
            "They will show when the scene starts."
        };
        [Space(10)]
        [Header("Display Settings")]
        [SerializeField] private float delayBeforeShow = 0.5f;
        [SerializeField] private bool showOnStart = true;
        
        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Start()
        {
            if (showOnStart)
            {
                Invoke(nameof(ShowDialogue), delayBeforeShow);
            }
        }
        
        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Show the configured dialogue text
        /// </summary>
        public void ShowDialogue()
        {
            if (MiniDialogueSystem.Instance == null)
            {
                Debug.LogWarning("MiniDialogueExample: No MiniDialogueSystem found in scene!");
                return;
            }
            
            if (textLinesToShow == null || textLinesToShow.Length == 0)
            {
                Debug.LogWarning("MiniDialogueExample: No text lines configured!");
                return;
            }
            
            MiniDialogueSystem.Instance.ShowText(textLinesToShow);
        }
    }
}
