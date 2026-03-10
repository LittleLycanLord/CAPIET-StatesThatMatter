using UnityEngine;

namespace LilLycanLord_Official
{
    public enum MatterPhase
    {
        Solid,
        Liquid,
        Gas
    }
    
    public enum HighlightType
    {
        None,
        Glow,
        Outline,
        Both
    }

    public class MatterBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        public GameObject interactionButton;
        [HideInInspector] public GameObject glowVisual;
        [HideInInspector] public GameObject outlineVisual;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        public ParticleLatticeMaterial material;
        public HighlightType highlightType = HighlightType.Glow;
        
        [Space(10)]
        [Header("Glow Settings")]
        public Color glowColor = new Color(1f, 1f, 1f, 0.5f);
        public float glowScale = 1.15f;
        
        [Space(10)]
        [Header("Outline Settings")]
        public Color outlineColor = Color.white;
        public float outlineThickness = 0.1f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        public MatterPhase phase;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Start()
        {
            // Button management now handled by PlayerInteraction
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        /// <summary>
        /// Called when player completes minigame with a phase change
        /// </summary>
        public void OnPhaseChangeApplied(DetectedPhase initialPhase, DetectedPhase finalPhase, string phaseChangeName)
        {
            Debug.Log($"Matter block received phase change: {initialPhase} → {finalPhase} ({phaseChangeName})");
            
            // Delegate to material's phase change events
            if (material != null && material.phaseChangeEvents != null)
            {
                switch (phaseChangeName)
                {
                    case "Melting":
                        material.phaseChangeEvents.OnMelt(gameObject);
                        break;
                    case "Freezing":
                        material.phaseChangeEvents.OnFreeze(gameObject);
                        break;
                    case "Evaporation":
                        material.phaseChangeEvents.OnEvaporate(gameObject);
                        break;
                    case "Condensation":
                        material.phaseChangeEvents.OnCondense(gameObject);
                        break;
                    case "Sublimation":
                        material.phaseChangeEvents.OnSublimate(gameObject);
                        break;
                    case "Deposition":
                        material.phaseChangeEvents.OnDeposition(gameObject);
                        break;
                    case "No Change":
                        Debug.Log("No phase change occurred.");
                        break;
                    default:
                        Debug.LogWarning($"Unknown phase change: {phaseChangeName}");
                        break;
                }
            }
            else
            {
                Debug.LogWarning("No material or phase change events assigned to this matter block!");
            }
        }
        
        /// <summary>
        /// Shows the highlight visual based on highlightType setting
        /// </summary>
        public void ShowHighlight()
        {
            switch (highlightType)
            {
                case HighlightType.Glow:
                    if (glowVisual != null) glowVisual.SetActive(true);
                    if (outlineVisual != null) outlineVisual.SetActive(false);
                    break;
                case HighlightType.Outline:
                    if (glowVisual != null) glowVisual.SetActive(false);
                    if (outlineVisual != null) outlineVisual.SetActive(true);
                    break;
                case HighlightType.Both:
                    if (glowVisual != null) glowVisual.SetActive(true);
                    if (outlineVisual != null) outlineVisual.SetActive(true);
                    break;
                case HighlightType.None:
                    if (glowVisual != null) glowVisual.SetActive(false);
                    if (outlineVisual != null) outlineVisual.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// Hides all highlight visuals
        /// </summary>
        public void HideHighlight()
        {
            if (glowVisual != null) glowVisual.SetActive(false);
            if (outlineVisual != null) outlineVisual.SetActive(false);
        }
        
        public void OnInteractionButtonPressed()
        {
            MinigameManager.Instance?.StartMinigame(this);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}