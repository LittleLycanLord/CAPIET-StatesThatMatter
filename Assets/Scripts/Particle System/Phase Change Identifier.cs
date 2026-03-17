using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

namespace LilLycanLord_Official
{
    public enum DetectedPhase
    {
        Solid,
        Liquid,
        Gas,
        Unknown
    }
    public class PhaseChangeIdentifier : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private ParticleLatticeManager latticeManager;
        [SerializeField] private TMP_Text phaseTransitionText;
        [SerializeField] private TMP_Text phaseChangeNameText;
        
        [Space(10)]
        [Header("Debug Displays")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private TMP_Text bondCountText;
        [SerializeField] private TMP_Text avgStiffnessText;
        [SerializeField] private TMP_Text avgSpeedText;
        [SerializeField] private TMP_Text bondPercentageText;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private DetectedPhase initialPhase = DetectedPhase.Unknown;
        [SerializeField] private DetectedPhase currentPhase = DetectedPhase.Unknown;
        [SerializeField] private DetectedPhase previousPhase = DetectedPhase.Unknown;
        [SerializeField] private string currentPhaseChangeName = "No Change";

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Detection Settings")]
        [SerializeField] private float detectionInterval = 0.25f;

        [Space(10)]
        [Header("Thresholds (0–2 combined normalized distance)")]
        [SerializeField][Range(0f, 2f)] private float meltingThreshold = 0.3f;       // Solid → Liquid
        [SerializeField][Range(0f, 2f)] private float freezingThreshold = 0.3f;      // Liquid → Solid
        [SerializeField][Range(0f, 1f)] private float evaporationThreshold = 0.3f;   // Liquid → Gas (bond % only)
        [SerializeField][Range(0f, 2f)] private float condensationThreshold = 0.3f;  // Gas → Liquid
        [SerializeField][Range(0f, 1f)] private float sublimationThreshold = 0.3f;   // Solid → Gas (bond % only)
        [SerializeField][Range(0f, 2f)] private float depositionThreshold = 0.3f;    // Gas → Solid

        [Space(10)]
        [Header("Gas Detection")]
        [Tooltip("Fraction (0–1) of the target preset's bond count that must form before Condensation/Deposition stiffness check is considered. e.g. 0.5 = 50% of target preset's bonds must exist first.")]
        [SerializeField][Range(0f, 1f)] private float minimumFromGasThreshold = 0.3f;

        [Space(10)]
        [Header("Events")]
        public PhaseChangeEvent onPhaseChanged;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private float detectionTimer;
        private int totalPossibleBonds;
        private bool isInitialized = false;
        private ParticleLatticeMaterial currentMaterial;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            if (latticeManager == null)
                latticeManager = FindObjectOfType<ParticleLatticeManager>();
        }

        void Start()
        {
            // Delay initialization to let lattice generate
            Invoke(nameof(Initialize), 0.5f);
        }

        void Update()
        {
            if (!isInitialized) return;

            detectionTimer += Time.deltaTime;

            if (detectionTimer >= detectionInterval)
            {
                detectionTimer = 0f;
                UpdatePhaseDetection();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        void Initialize()
        {
            if (latticeManager == null)
            {
                Debug.LogError("PhaseChangeIdentifier: No ParticleLatticeManager found!");
                return;
            }

            // Calculate initial total possible bonds based on lattice
            CalculateInitialBondCapacity();

            // Get initial phase and material from MinigameManager
            if (MinigameManager.Instance != null)
            {
                currentMaterial = MinigameManager.Instance.GetCurrentMaterial();
                initialPhase = ConvertMatterPhaseToDetectedPhase(MinigameManager.Instance.currentPhase);
                Debug.Log($"Initial phase set from MinigameManager: {initialPhase}");
            }
            else
            {
                Debug.LogWarning("MinigameManager not found. Phase detection disabled.");
                initialPhase = DetectedPhase.Unknown;
            }
            
            currentPhase = initialPhase;
            previousPhase = initialPhase;

            isInitialized = true;

            // Update UI
            UpdateUI();

            Debug.Log($"Phase Change Identifier initialized. Initial phase: {initialPhase}");
        }
        
        DetectedPhase ConvertMatterPhaseToDetectedPhase(MatterPhase matterPhase)
        {
            switch (matterPhase)
            {
                case MatterPhase.Solid:
                    return DetectedPhase.Solid;
                case MatterPhase.Liquid:
                    return DetectedPhase.Liquid;
                case MatterPhase.Gas:
                    return DetectedPhase.Gas;
                default:
                    return DetectedPhase.Unknown;
            }
        }

        void CalculateInitialBondCapacity()
        {
            // Count bonds in all lattices
            List<ParticleLattice> lattices = latticeManager.GetAllLattices();
            totalPossibleBonds = 0;
            int particleCount = 0;

            foreach (var lattice in lattices)
            {
                if (lattice != null)
                {
                    totalPossibleBonds += lattice.GetBondCount();
                    particleCount += lattice.GetParticleCount();
                }
            }

            // If starting as gas (no bonds), estimate potential bonds from particle count
            if (totalPossibleBonds == 0 && particleCount > 0)
            {
                // Rough estimate: each particle could bond to ~4 neighbors in a grid
                // But we divide by 2 since each bond connects 2 particles
                totalPossibleBonds = particleCount * 2;
                
                if (debugMode)
                    Debug.Log($"Starting as gas. Estimated bonds: {totalPossibleBonds} from {particleCount} particles");
            }
            else if (totalPossibleBonds == 0 && particleCount == 0)
            {
                // No lattice yet, try again later
                Debug.LogWarning("No particles or bonds found. Lattice may not be generated yet.");
                Invoke(nameof(CalculateInitialBondCapacity), 0.5f);
                return;
            }
            
            if (debugMode)
                Debug.Log($"Initial bond capacity: {totalPossibleBonds} bonds, {particleCount} particles");
        }

        void UpdatePhaseDetection()
        {
            previousPhase = currentPhase;
            currentPhase = DetectPhase();

            if (currentPhase != previousPhase)
            {
                OnPhaseChanged(initialPhase, currentPhase);
            }
            else if (debugMode)
            {
                // Update debug displays even when no phase change occurs
                UpdateUI();
            }
        }

        DetectedPhase DetectPhase()
        {
            if (currentMaterial == null) return initialPhase;

            // Gather current lattice metrics
            int currentBondCount = GetCurrentBondCount();
            float currentBondPct = totalPossibleBonds > 0
                ? Mathf.Clamp01((float)currentBondCount / totalPossibleBonds)
                : 0f;
            float currentStiffness = GetAverageBondStiffness();

            // Gas transitions: purely bond % based — if enough bonds have broken, call it immediately
            if (initialPhase == DetectedPhase.Solid  && currentBondPct < sublimationThreshold)  return DetectedPhase.Gas;
            if (initialPhase == DetectedPhase.Liquid && currentBondPct < evaporationThreshold)  return DetectedPhase.Gas;

            // Compare against each non-initial, non-gas preset to find the closest
            MatterPhase[] phases = { MatterPhase.Solid, MatterPhase.Liquid, MatterPhase.Gas };
            float closestDistance = float.MaxValue;
            DetectedPhase closestPhase = initialPhase;

            foreach (MatterPhase phase in phases)
            {
                DetectedPhase detectedPhase = ConvertMatterPhaseToDetectedPhase(phase);
                if (detectedPhase == initialPhase) continue;

                ParticleLatticePreset preset = currentMaterial.GetPresetForPhase(phase);
                if (preset == null) continue;

                // Gas preset is handled above — skip it in the distance loop
                if (preset.createAsGas) continue;

                // From-gas transitions require minimum bond % before stiffness is considered
                if (initialPhase == DetectedPhase.Gas)
                {
                    int expectedBondCount = preset.latticeRows * (preset.latticeColumns - 1)
                                         + (preset.latticeRows - 1) * preset.latticeColumns;
                    if (currentBondCount < minimumFromGasThreshold * expectedBondCount) continue;
                }

                float expectedBondPct = preset.createAsGas ? 0f : 1f;
                float expectedStiffness = preset.bondStiffness;
                float distance = Mathf.Abs(currentBondPct - expectedBondPct)
                               + Mathf.Abs(currentStiffness - expectedStiffness);

                if (debugMode)
                    Debug.Log($"[PhaseDetection] Distance to {phase}: {distance:F3} " +
                              $"(bondPct {currentBondPct:F2} vs {expectedBondPct:F2}, " +
                              $"stiffness {currentStiffness:F2} vs {expectedStiffness:F2})");

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPhase = detectedPhase;
                }
            }

            if (closestDistance < GetThresholdForChange(initialPhase, closestPhase))
                return closestPhase;

            return initialPhase;
        }

        float GetThresholdForChange(DetectedPhase from, DetectedPhase to)
        {
            if (from == DetectedPhase.Solid  && to == DetectedPhase.Liquid) return meltingThreshold;
            if (from == DetectedPhase.Liquid && to == DetectedPhase.Solid)  return freezingThreshold;
            if (from == DetectedPhase.Liquid && to == DetectedPhase.Gas)    return evaporationThreshold;
            if (from == DetectedPhase.Gas    && to == DetectedPhase.Liquid) return condensationThreshold;
            if (from == DetectedPhase.Solid  && to == DetectedPhase.Gas)    return sublimationThreshold;
            if (from == DetectedPhase.Gas    && to == DetectedPhase.Solid)  return depositionThreshold;
            return 0f;
        }

        int GetCurrentBondCount()
        {
            List<ParticleLattice> lattices = latticeManager.GetAllLattices();
            int count = 0;

            foreach (var lattice in lattices)
            {
                if (lattice != null)
                {
                    int latticeBonds = lattice.GetBondCount();
                    count += latticeBonds;
                }
            }

            return count;
        }

        float GetAverageBondStiffness()
        {
            List<ParticleLattice> lattices = latticeManager.GetAllLattices();
            float totalStiffness = 0f;
            int bondCount = 0;

            foreach (var lattice in lattices)
            {
                if (lattice != null)
                {
                    List<GameObject> bonds = lattice.GetBonds();
                    foreach (var bondObj in bonds)
                    {
                        if (bondObj != null)
                        {
                            BondBehaviour bond = bondObj.GetComponent<BondBehaviour>();
                            if (bond != null)
                            {
                                totalStiffness += bond.GetCurrentStiffness();
                                bondCount++;
                            }
                        }
                    }
                }
            }

            return bondCount > 0 ? totalStiffness / bondCount : 0f;
        }

        float GetAverageParticleSpeed()
        {
            List<ParticleLattice> lattices = latticeManager.GetAllLattices();
            float totalSpeed = 0f;
            int particleCount = 0;

            foreach (var lattice in lattices)
            {
                if (lattice != null)
                {
                    List<GameObject> particles = lattice.GetParticles();
                    foreach (var particleObj in particles)
                    {
                        if (particleObj != null)
                        {
                            Rigidbody rb = particleObj.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                totalSpeed += rb.linearVelocity.magnitude;
                                particleCount++;
                            }
                        }
                    }
                }
            }

            return particleCount > 0 ? totalSpeed / particleCount : 0f;
        }

        void OnPhaseChanged(DetectedPhase from, DetectedPhase to)
        {
            currentPhaseChangeName = GetPhaseChangeName(from, to);

            Debug.Log($"Phase change detected: {from} → {to} ({currentPhaseChangeName})");

            // Update UI
            UpdateUI();

            onPhaseChanged?.Invoke(from, to, currentPhaseChangeName);
        }

        void UpdateUI()
        {
            if (phaseTransitionText != null)
            {
                if (currentPhase == initialPhase)
                {
                    phaseTransitionText.text = $"{initialPhase} → ???";
                }
                else
                {
                    phaseTransitionText.text = $"{initialPhase} → {currentPhase}";
                }
            }

            if (phaseChangeNameText != null)
            {
                phaseChangeNameText.text = currentPhaseChangeName;
            }
            
            // Update debug displays if enabled
            if (debugMode)
            {
                int currentBondCount = GetCurrentBondCount();
                float avgStiffness = GetAverageBondStiffness();
                float avgSpeed = GetAverageParticleSpeed();
                float bondPercentage = totalPossibleBonds > 0 
                    ? (float)currentBondCount / totalPossibleBonds 
                    : 0f;
                
                if (bondCountText != null)
                    bondCountText.text = $"Bonds: {currentBondCount}/{totalPossibleBonds}";
                    
                if (avgStiffnessText != null)
                    avgStiffnessText.text = $"Stiffness: {avgStiffness:F2}";
                    
                if (avgSpeedText != null)
                    avgSpeedText.text = $"Speed: {avgSpeed:F2}";
                    
                if (bondPercentageText != null)
                    bondPercentageText.text = $"Bond %: {bondPercentage:P0}";
            }
        }

        string GetPhaseChangeName(DetectedPhase from, DetectedPhase to)
        {
            if(from == to)
                return "No Change";

            if (from == DetectedPhase.Solid && to == DetectedPhase.Liquid)
                return "Melting";

            if (from == DetectedPhase.Liquid && to == DetectedPhase.Solid)
                return "Freezing";

            if (from == DetectedPhase.Liquid && to == DetectedPhase.Gas)
                return "Evaporation";

            if (from == DetectedPhase.Gas && to == DetectedPhase.Liquid)
                return "Condensation";

            if (from == DetectedPhase.Solid && to == DetectedPhase.Gas)
                return "Sublimation";

            if (from == DetectedPhase.Gas && to == DetectedPhase.Solid)
                return "Deposition";

            return "UNKNOWN CHANGE";
        }

        /// <summary>
        /// Get UI display string for current phase state
        /// </summary>
        public string GetPhaseTransitionText()
        {
            if (currentPhase == initialPhase)
            {
                return $"{initialPhase} → ???";
            }
            else
            {
                return $"{initialPhase} → {currentPhase}";
            }
        }

        /// <summary>
        /// Get the current phase change name
        /// </summary>
        public string GetCurrentPhaseChangeName()
        {
            return currentPhaseChangeName;
        }

        /// <summary>
        /// Public accessors for UI
        /// </summary>
        public DetectedPhase GetInitialPhase() => initialPhase;
        public DetectedPhase GetCurrentPhase() => currentPhase;

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }    

    [System.Serializable]
    public class PhaseChangeEvent : UnityEvent<DetectedPhase, DetectedPhase, string> { }

}