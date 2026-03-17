using UnityEngine;
using UnityEngine.SceneManagement;

namespace LilLycanLord_Official
{
    public class MinigameManager : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        public Camera platformerCamera;
        public GameObject platformerEventSystem;
        public GameObject platformerUI;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private MatterBehaviour currentMatterBlock;
        [SerializeField] private PhaseChangeIdentifier currentPhaseIdentifier;
        public MatterPhase currentPhase;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        [SerializeField] private string minigameSceneName = "IceLatticeMinigame";

        [Space(10)]
        [Header("Particle Lattice Material Presets")]
        [SerializeField] private ParticleLatticeMaterial currentMaterial;
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        public static MinigameManager Instance { get; private set; }

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        void OnDestroy()
        {
            // Unsubscribe from scene loaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if the loaded scene is the minigame scene
            if (scene.name == minigameSceneName)
            {
                // Find the Particle Lattice Manager in the newly loaded scene
                ParticleLatticeManager latticeManager = FindAnyObjectByType<ParticleLatticeManager>();
                
                // Find and store reference to Phase Change Identifier
                currentPhaseIdentifier = FindAnyObjectByType<PhaseChangeIdentifier>();
                
                if (latticeManager != null)
                {
                    // Get the appropriate preset based on the current phase
                    ParticleLatticePreset preset = GetPresetForPhase(currentPhase);
                    
                    if (preset != null)
                    {
                        // Load the preset and generate the lattice
                        latticeManager.LoadFromPreset(preset);
                        latticeManager.GenerateLattice();
                        
                        Debug.Log($"Generated {currentPhase} lattice in minigame scene");
                    }
                    else
                    {
                        Debug.LogWarning($"No preset assigned for {currentPhase} phase. Generating with default settings.");
                        latticeManager.GenerateLattice();
                    }
                }
                else
                {
                    Debug.LogError("Particle Lattice Manager not found in minigame scene!");
                }
            }
        }
        
        ParticleLatticePreset GetPresetForPhase(MatterPhase phase)
        {
            switch (phase)
            {
                case MatterPhase.Solid:
                    return currentMaterial.solidPreset;
                case MatterPhase.Liquid:
                    return currentMaterial.liquidPreset;
                case MatterPhase.Gas:
                    return currentMaterial.gasPreset;
                default:
                    Debug.LogWarning($"Unknown phase: {phase}");
                    return null;
            }
        }
        
        public ParticleLatticeMaterial GetCurrentMaterial() => currentMaterial;

        public void StartMinigame(MatterBehaviour matterBlock)
        {
            currentMatterBlock = matterBlock;
            currentPhase = matterBlock.phase;

            // Pause platformer physics
            platformerCamera.enabled = false;            platformerEventSystem.SetActive(false);
            platformerUI.SetActive(false);
            GameState.InMinigame = true;

            // Load minigame scene additively
            SceneManager.LoadSceneAsync(minigameSceneName, LoadSceneMode.Additive);
        }

        public void CompleteMinigame(bool playerSucceeded = true)
        {
            // Extract phase change data before unloading scene
            if (playerSucceeded && currentPhaseIdentifier != null)
            {
                DetectedPhase initialPhase = currentPhaseIdentifier.GetInitialPhase();
                DetectedPhase finalPhase = currentPhaseIdentifier.GetCurrentPhase();
                string phaseChangeName = currentPhaseIdentifier.GetCurrentPhaseChangeName();
                
                Debug.Log($"Minigame completed! Phase change: {initialPhase} → {finalPhase} ({phaseChangeName})");
                
                // Update the matter block with the phase change result
                if (currentMatterBlock != null)
                {
                    currentMatterBlock.OnPhaseChangeApplied(initialPhase, finalPhase, phaseChangeName);
                }
            }
            else if (!playerSucceeded)
            {
                Debug.Log("Minigame failed or cancelled.");
            }
            
            // Clean up references
            currentPhaseIdentifier = null;
            currentMatterBlock = null;

            // Unload minigame scene
            SceneManager.UnloadSceneAsync(minigameSceneName);
            GameState.InMinigame = false;
            platformerCamera.enabled = true;
            platformerEventSystem.SetActive(true);
            platformerUI.SetActive(true);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}