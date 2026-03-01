using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(Collider2D))]
    public class MiniDialogueTrigger : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private MiniDialogueSystem dialogueSystem;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool hasTriggered = false;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Text Settings")]
        [SerializeField] [TextArea(3, 10)] private string[] textLines = new string[] { "Enter your text here" };
        
        [Space(10)]
        [Header("Trigger Settings")]
        [SerializeField] private string triggerTag = "Player";
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private bool triggerOnEnter = true;
        [SerializeField] private bool triggerOnExit = false;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            // Find dialogue system if not assigned
            if (dialogueSystem == null)
            {
                dialogueSystem = FindAnyObjectByType<MiniDialogueSystem>();
            }
            
            // Ensure collider is trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"MiniDialogueTrigger on {gameObject.name}: Collider2D is not set as trigger. Setting it now.");
                col.isTrigger = true;
            }
        }

        void Start() { }

        void Update() { }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!triggerOnEnter) return;
            
            if (other.CompareTag(triggerTag))
            {
                TriggerDialogue();
            }
        }
        
        void OnTriggerExit2D(Collider2D other)
        {
            if (!triggerOnExit) return;
            
            if (other.CompareTag(triggerTag))
            {
                TriggerDialogue();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Trigger the dialogue display
        /// </summary>
        private void TriggerDialogue()
        {
            if (triggerOnce && hasTriggered) return;
            
            if (dialogueSystem == null)
            {
                Debug.LogError($"MiniDialogueTrigger on {gameObject.name}: No dialogue system found!");
                return;
            }
            
            if (textLines.Length == 0)
            {
                Debug.LogWarning($"MiniDialogueTrigger on {gameObject.name}: No text lines defined!");
                return;
            }
            
            dialogueSystem.ShowText(textLines);
            hasTriggered = true;
        }
        
        /// <summary>
        /// Manually trigger the dialogue (can be called from other scripts or UnityEvents)
        /// </summary>
        public void ManualTrigger()
        {
            TriggerDialogue();
        }
        
        /// <summary>
        /// Reset the trigger so it can be activated again
        /// </summary>
        public void ResetTrigger()
        {
            hasTriggered = false;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
