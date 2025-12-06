using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class ParticleLatticeManager : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private TemperatureBrush temperatureBrush;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Prefabs")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private GameObject bondPrefab;
        
        [Space(10)]
        [Header("Lattice Configuration")]
        [SerializeField] private GameObject latticeOrigin;
        [SerializeField] private int latticeRows = 4;
        [SerializeField] private int latticeColumns = 4;
        [SerializeField] private bool createAsGas = false;
        
        [Space(10)]
        [Header("Particle Attributes")]
        [SerializeField] private float particleRadius = 1f;
        [SerializeField] private float particleMass = 1f;
        [SerializeField] private Color particleColor = Color.white;
        
        [Space(10)]
        [Header("Particle Temperature Settings")]
        [SerializeField] [Tooltip("Initial temperature for particles in Celsius")] private float particleTemperature = 30f;
        [SerializeField] [Tooltip("Minimum temperature in Celsius")] private float particleMinTemperature = -10f;
        [SerializeField] [Tooltip("Maximum temperature in Celsius")] private float particleMaxTemperature = 110f;
        
        [Space(10)]
        [Header("Bond Attributes")]
        [SerializeField] private float bondLength = 1f;
        [SerializeField] private float bondWidth = 0.5f;
        [SerializeField] private Color bondColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float bondStiffness = 0.98f;
        [SerializeField] private float maxBondLengthMultiplier = 2f;
        
        [Space(10)]
        [Header("Bond Temperature Settings")]
        [SerializeField] [Tooltip("Minimum temperature in Celsius")] private float bondMinTemperature = -10f;
        [SerializeField] [Tooltip("Maximum temperature in Celsius")] private float bondMaxTemperature = 110f;
        [SerializeField] [Tooltip("Temperature at which bond behaves as liquid")] private float bondLiquidTemperature = 50f;
        [SerializeField] [Tooltip("Stiffness when at liquid temperature")] [Range(0f, 1f)] private float bondLiquidStiffness = 0.5f;
        [SerializeField] [Tooltip("Temperature at which bond behaves as solid")] private float bondSolidTemperature = 0f;
        [SerializeField] [Tooltip("Stiffness when at solid temperature")] [Range(0f, 1f)] private float bondSolidStiffness = 0.98f;
        [SerializeField] private bool bondCanBreak = true;
        [SerializeField] [Tooltip("Temperature at which bond breaks")] private float bondBreakingTemperature = 110f;
        
        [Space(10)]
        [Header("Dynamic Bond Attributes")]
        [SerializeField] private float dynamicBondLength = 1f;
        [SerializeField] private float dynamicBondWidth = 0.5f;
        [SerializeField] private Color dynamicBondColor = Color.blue;
        [SerializeField] [Range(0f, 1f)] private float dynamicBondStiffness = 0.98f;
        [SerializeField] private float dynamicMaxBondLengthMultiplier = 2f;
        
        [Space(10)]
        [Header("Dynamic Bonding")]
        public float bondingProximity = 1.5f;
        [SerializeField] private bool bondingMode = false;
        [SerializeField] private float bondCheckInterval = 0.2f;
        [SerializeField] private int maxDynamicBondsCreated = 2;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private GameObject[,] particleGrid;
        private List<GameObject> allParticles = new List<GameObject>();
        private List<GameObject> allBonds = new List<GameObject>();
        private List<ParticleLattice> allLattices = new List<ParticleLattice>();
        private Dictionary<GameObject, ParticleLattice> particleToLatticeMap = new Dictionary<GameObject, ParticleLattice>();
        private float lastBondCheckTime = 0f;
        private Dictionary<GameObject, List<GameObject>> pendingBonds = new Dictionary<GameObject, List<GameObject>>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            temperatureBrush = FindObjectOfType<TemperatureBrush>();
        }

        void Start() { }

        void Update() 
        {
            // Bonding now happens via drag events, not automatic proximity checks
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        [ContextMenu("Generate Lattice")]
        public void GenerateLattice()
        {
            // Clear any existing lattice
            ClearLattice();
            
            // Validate required references
            if (particlePrefab == null)
            {
                Debug.LogError("ParticleLatticeManager: Particle prefab is not assigned!");
                return;
            }
            
            if (bondPrefab == null)
            {
                Debug.LogError("ParticleLatticeManager: Bond prefab is not assigned!");
                return;
            }
            
            if (latticeOrigin == null)
            {
                Debug.LogError("ParticleLatticeManager: Lattice origin is not assigned!");
                return;
            }
            
            // Initialize grid
            particleGrid = new GameObject[latticeRows, latticeColumns];
            
            // Calculate spacing: 2*radius (for particle diameters) + bondLength
            float particleSpacing = (particleRadius * 2f) + bondLength;
            
            // Generate particles
            Vector3 originPos = latticeOrigin.transform.position;
            
            for (int row = 0; row < latticeRows; row++)
            {
                for (int col = 0; col < latticeColumns; col++)
                {
                    // Calculate position (left to right, top to bottom)
                    Vector3 position = originPos + new Vector3(
                        col * particleSpacing,
                        -row * particleSpacing,
                        0f
                    );
                    
                    // Instantiate particle
                    GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity, transform);
                    particle.name = $"Particle [{row},{col}]";
                    
                    // Configure particle attributes
                    ConfigureParticle(particle);
                    
                    // Store in grid
                    particleGrid[row, col] = particle;
                    allParticles.Add(particle);
                }
            }
            
            // Generate bonds (horizontal and vertical) unless creating as gas
            if (!createAsGas)
            {
                GenerateBonds();
            }
            
            Debug.Log($"Lattice generated: {latticeRows}x{latticeColumns} = {allParticles.Count} particles, {allBonds.Count} bonds");
        }

        [ContextMenu("Clear Lattice")]
        public void ClearLattice()
        {
            // Destroy all particles
            foreach (GameObject particle in allParticles)
            {
                if (particle != null)
                {
                    DestroyImmediate(particle);
                }
            }
            allParticles.Clear();

            // Destroy all bonds
            foreach (GameObject bond in allBonds)
            {
                if (bond != null)
                {
                    DestroyImmediate(bond);
                }
            }
            allBonds.Clear();

            particleGrid = null;

            Debug.Log("Lattice cleared");
        }

        [ContextMenu("Reset Lattice")]
        public void ResetLattice()
        {
            ClearLattice();
            GenerateLattice();
        }
        
        private void ConfigureParticle(GameObject particle)
        {            
            // Configure Rigidbody
            Rigidbody rb = particle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = particleMass;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            
            // Configure visual appearance (assuming sphere mesh renderer)
            MeshRenderer renderer = particle.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = particleColor;
            }
            
            // Scale particle to match radius
            particle.transform.localScale = Vector3.one * particleRadius * 2f;
            
            // Configure temperature settings
            ParticleBehaviour particleBehaviour = particle.GetComponent<ParticleBehaviour>();
            if (particleBehaviour != null)
            {
                // Use reflection to set private serialized temperature fields
                var currentTempField = typeof(ParticleBehaviour).GetField("currentTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var minTempField = typeof(ParticleBehaviour).GetField("minTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxTempField = typeof(ParticleBehaviour).GetField("maxTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (currentTempField != null) currentTempField.SetValue(particleBehaviour, particleTemperature);
                if (minTempField != null) minTempField.SetValue(particleBehaviour, particleMinTemperature);
                if (maxTempField != null) maxTempField.SetValue(particleBehaviour, particleMaxTemperature);
            }
        }
        
        private void GenerateBonds()
        {
            for (int row = 0; row < latticeRows; row++)
            {
                for (int col = 0; col < latticeColumns; col++)
                {
                    GameObject currentParticle = particleGrid[row, col];
                    
                    // Create horizontal bond (to the right)
                    if (col < latticeColumns - 1)
                    {
                        GameObject rightParticle = particleGrid[row, col + 1];
                        CreateBond(currentParticle, rightParticle, $"Bond H[{row},{col}]");
                    }
                    
                    // Create vertical bond (downward)
                    if (row < latticeRows - 1)
                    {
                        GameObject belowParticle = particleGrid[row + 1, col];
                        CreateBond(currentParticle, belowParticle, $"Bond V[{row},{col}]");
                    }
                }
            }
        }
        
        private void CreateBond(GameObject particleA, GameObject particleB, string bondName)
        {
            // Instantiate bond
            GameObject bond = Instantiate(bondPrefab, Vector3.zero, Quaternion.identity, transform);
            bond.name = bondName;
            
            // Configure bond
            BondBehaviour bondBehaviour = bond.GetComponent<BondBehaviour>();
            if (bondBehaviour != null)
            {
                // Use reflection to set private serialized fields
                var particleAField = typeof(BondBehaviour).GetField("particleA", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var particleBField = typeof(BondBehaviour).GetField("particleB", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bondLengthField = typeof(BondBehaviour).GetField("bondLength", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lineWidthField = typeof(BondBehaviour).GetField("lineWidth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bondColorField = typeof(BondBehaviour).GetField("bondColor", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var stiffnessField = typeof(BondBehaviour).GetField("stiffness", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxBondLengthMultiplierField = typeof(BondBehaviour).GetField("maxBondLengthMultiplier", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                // Temperature and phase transition fields
                var minTempField = typeof(BondBehaviour).GetField("minTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxTempField = typeof(BondBehaviour).GetField("maxTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var liquidTempField = typeof(BondBehaviour).GetField("liquidTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var liquidStiffField = typeof(BondBehaviour).GetField("liquidStiffness",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var solidTempField = typeof(BondBehaviour).GetField("solidTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var solidStiffField = typeof(BondBehaviour).GetField("solidStiffness",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var canBreakField = typeof(BondBehaviour).GetField("canBreak",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var breakingTempField = typeof(BondBehaviour).GetField("breakingTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (particleAField != null) particleAField.SetValue(bondBehaviour, particleA);
                if (particleBField != null) particleBField.SetValue(bondBehaviour, particleB);
                if (bondLengthField != null) bondLengthField.SetValue(bondBehaviour, bondLength);
                if (lineWidthField != null) lineWidthField.SetValue(bondBehaviour, bondWidth);
                if (bondColorField != null) bondColorField.SetValue(bondBehaviour, bondColor);
                if (stiffnessField != null) stiffnessField.SetValue(bondBehaviour, bondStiffness);
                if (maxBondLengthMultiplierField != null) maxBondLengthMultiplierField.SetValue(bondBehaviour, maxBondLengthMultiplier);
                
                // Set temperature settings
                if (minTempField != null) minTempField.SetValue(bondBehaviour, bondMinTemperature);
                if (maxTempField != null) maxTempField.SetValue(bondBehaviour, bondMaxTemperature);
                if (liquidTempField != null) liquidTempField.SetValue(bondBehaviour, bondLiquidTemperature);
                if (liquidStiffField != null) liquidStiffField.SetValue(bondBehaviour, bondLiquidStiffness);
                if (solidTempField != null) solidTempField.SetValue(bondBehaviour, bondSolidTemperature);
                if (solidStiffField != null) solidStiffField.SetValue(bondBehaviour, bondSolidStiffness);
                if (canBreakField != null) canBreakField.SetValue(bondBehaviour, bondCanBreak);
                if (breakingTempField != null) breakingTempField.SetValue(bondBehaviour, bondBreakingTemperature);
            }
            
            allBonds.Add(bond);
            
            // Notify particles of the new bond
            ParticleBehaviour particleABehaviour = particleA.GetComponent<ParticleBehaviour>();
            ParticleBehaviour particleBBehaviour = particleB.GetComponent<ParticleBehaviour>();
            
            if (particleABehaviour != null) particleABehaviour.AddBond(bond);
            if (particleBBehaviour != null) particleBBehaviour.AddBond(bond);
            
            // Add bond to lattice management
            RegisterBondToLattice(particleA, particleB, bond);
        }
        
        private void CreateDynamicBond(GameObject particleA, GameObject particleB, string bondName)
        {
            // Instantiate bond
            GameObject bond = Instantiate(bondPrefab, Vector3.zero, Quaternion.identity, transform);
            bond.name = bondName;
            
            // Configure bond with dynamic attributes
            BondBehaviour bondBehaviour = bond.GetComponent<BondBehaviour>();
            if (bondBehaviour != null)
            {
                // Use reflection to set private serialized fields
                var particleAField = typeof(BondBehaviour).GetField("particleA", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var particleBField = typeof(BondBehaviour).GetField("particleB", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bondLengthField = typeof(BondBehaviour).GetField("bondLength", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lineWidthField = typeof(BondBehaviour).GetField("lineWidth", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bondColorField = typeof(BondBehaviour).GetField("bondColor", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var stiffnessField = typeof(BondBehaviour).GetField("stiffness", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxBondLengthMultiplierField = typeof(BondBehaviour).GetField("maxBondLengthMultiplier", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                // Temperature and phase transition fields
                var minTempField = typeof(BondBehaviour).GetField("minTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxTempField = typeof(BondBehaviour).GetField("maxTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var liquidTempField = typeof(BondBehaviour).GetField("liquidTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var liquidStiffField = typeof(BondBehaviour).GetField("liquidStiffness",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var solidTempField = typeof(BondBehaviour).GetField("solidTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var solidStiffField = typeof(BondBehaviour).GetField("solidStiffness",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var canBreakField = typeof(BondBehaviour).GetField("canBreak",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var breakingTempField = typeof(BondBehaviour).GetField("breakingTemperature",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (particleAField != null) particleAField.SetValue(bondBehaviour, particleA);
                if (particleBField != null) particleBField.SetValue(bondBehaviour, particleB);
                if (bondLengthField != null) bondLengthField.SetValue(bondBehaviour, dynamicBondLength);
                if (lineWidthField != null) lineWidthField.SetValue(bondBehaviour, dynamicBondWidth);
                if (bondColorField != null) bondColorField.SetValue(bondBehaviour, dynamicBondColor);

                if (stiffnessField != null) stiffnessField.SetValue(bondBehaviour, dynamicBondStiffness);
                if (maxBondLengthMultiplierField != null) maxBondLengthMultiplierField.SetValue(bondBehaviour, dynamicMaxBondLengthMultiplier);
                
                // Set temperature settings (use same as regular bonds)
                if (minTempField != null) minTempField.SetValue(bondBehaviour, bondMinTemperature);
                if (maxTempField != null) maxTempField.SetValue(bondBehaviour, bondMaxTemperature);
                if (liquidTempField != null) liquidTempField.SetValue(bondBehaviour, bondLiquidTemperature);
                if (liquidStiffField != null) liquidStiffField.SetValue(bondBehaviour, bondLiquidStiffness);
                if (solidTempField != null) solidTempField.SetValue(bondBehaviour, bondSolidTemperature);
                if (solidStiffField != null) solidStiffField.SetValue(bondBehaviour, bondSolidStiffness);
                if (canBreakField != null) canBreakField.SetValue(bondBehaviour, bondCanBreak);
                if (breakingTempField != null) breakingTempField.SetValue(bondBehaviour, bondBreakingTemperature);
            }
            
            allBonds.Add(bond);
            
            // Notify particles of the new bond
            ParticleBehaviour particleABehaviour = particleA.GetComponent<ParticleBehaviour>();
            ParticleBehaviour particleBBehaviour = particleB.GetComponent<ParticleBehaviour>();
            
            if (particleABehaviour != null) particleABehaviour.AddBond(bond);
            if (particleBBehaviour != null) particleBBehaviour.AddBond(bond);
            
            // Add bond to lattice management
            RegisterBondToLattice(particleA, particleB, bond);
        }
        
        /// <summary>
        /// Register a free particle that isn't part of any lattice yet
        /// </summary>
        public void RegisterFreeParticle(GameObject particle)
        {
            if (!allParticles.Contains(particle))
            {
                allParticles.Add(particle);
                
                // Create a single-particle lattice
                GameObject latticeObj = new GameObject($"Lattice_Particle_{particle.name}");
                latticeObj.transform.parent = transform;
                ParticleLattice lattice = latticeObj.AddComponent<ParticleLattice>();
                lattice.AddParticle(particle);
                
                allLattices.Add(lattice);
                particleToLatticeMap[particle] = lattice;
                
                Debug.Log($"Registered free particle {particle.name} as new lattice");
            }
        }
        
        /// <summary>
        /// Check if dragged particle should bond with nearby particles
        /// Called by ParticleBehaviour when a particle is being dragged
        /// </summary>
        public void CheckDragBonding(GameObject draggedParticle)
        {
            if (!bondingMode) return;
            if (draggedParticle == null) return;
            if (!allParticles.Contains(draggedParticle)) return;
            
            // Check if dragged particle can accept more bonds
            ParticleBehaviour draggedBehaviour = draggedParticle.GetComponent<ParticleBehaviour>();
            if (draggedBehaviour == null || !draggedBehaviour.CanAcceptMoreBonds()) return;
            
            // Get dragged particle's sphere collider for radius
            SphereCollider draggedCollider = draggedParticle.GetComponent<SphereCollider>();
            if (draggedCollider == null) return;
            
            // Calculate effective bonding proximity based on particle radius
            float draggedRadius = draggedCollider.radius;
            float effectiveBondingProximity = draggedRadius * bondingProximity;
            
            // Initialize pending bonds list for this particle if needed
            if (!pendingBonds.ContainsKey(draggedParticle))
            {
                pendingBonds[draggedParticle] = new List<GameObject>();
            }
            
            // Clear previous pending bonds
            pendingBonds[draggedParticle].Clear();
            
            // Find candidates sorted by distance
            List<(GameObject particle, float distance)> candidates = new List<(GameObject, float)>();
            
            foreach (GameObject otherParticle in allParticles)
            {
                if (otherParticle == null || otherParticle == draggedParticle) continue;
                
                // Check if other particle can accept bonds
                ParticleBehaviour otherBehaviour = otherParticle.GetComponent<ParticleBehaviour>();
                if (otherBehaviour == null || !otherBehaviour.CanAcceptMoreBonds()) continue;
                
                // Skip if already bonded
                if (AreParticlesBonded(draggedParticle, otherParticle)) continue;
                
                // Check distance
                float distance = Vector3.Distance(draggedParticle.transform.position, otherParticle.transform.position);
                
                // Account for particle radii
                SphereCollider otherCollider = otherParticle.GetComponent<SphereCollider>();
                if (otherCollider == null) continue;
                
                float radiusA = draggedCollider.radius * draggedParticle.transform.localScale.x;
                float radiusB = otherCollider.radius * otherParticle.transform.localScale.x;
                float surfaceDistance = distance - radiusA - radiusB;
                
                if (surfaceDistance <= effectiveBondingProximity)
                {
                    candidates.Add((otherParticle, distance));
                }
            }
            
            // Sort by distance (closest first)
            candidates.Sort((a, b) => a.distance.CompareTo(b.distance));
            
            // Store potential bonds (up to maxDynamicBondsCreated)
            int bondsStored = 0;
            foreach (var candidate in candidates)
            {
                if (bondsStored >= maxDynamicBondsCreated) break;
                
                ParticleBehaviour otherBehaviour = candidate.particle.GetComponent<ParticleBehaviour>();
                if (otherBehaviour != null && otherBehaviour.CanAcceptMoreBonds())
                {
                    pendingBonds[draggedParticle].Add(candidate.particle);
                    bondsStored++;
                }
            }
        }
        
        /// <summary>
        /// Check if two particles are already connected by a bond
        /// </summary>
        private bool AreParticlesBonded(GameObject particleA, GameObject particleB)
        {
            foreach (GameObject bond in allBonds)
            {
                if (bond == null) continue;
                
                BondBehaviour bondBehaviour = bond.GetComponent<BondBehaviour>();
                if (bondBehaviour != null)
                {
                    var particleAField = typeof(BondBehaviour).GetField("particleA", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var particleBField = typeof(BondBehaviour).GetField("particleB", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    GameObject bondParticleA = particleAField?.GetValue(bondBehaviour) as GameObject;
                    GameObject bondParticleB = particleBField?.GetValue(bondBehaviour) as GameObject;
                    
                    if ((bondParticleA == particleA && bondParticleB == particleB) ||
                        (bondParticleA == particleB && bondParticleB == particleA))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Create a bond when a dragged particle comes close to another
        /// </summary>
        private void CreateDragBond(GameObject particleA, GameObject particleB)
        {
            string bondName = $"Bond_Drag_{particleA.name}_{particleB.name}";
            CreateDynamicBond(particleA, particleB, bondName);
            
            Debug.Log($"Created drag bond between {particleA.name} and {particleB.name}");
        }
        
        /// <summary>
        /// Called when particle drag ends - creates all pending bonds
        /// </summary>
        public void OnParticleDragEnd(GameObject draggedParticle)
        {
            if (!pendingBonds.ContainsKey(draggedParticle)) return;
            
            List<GameObject> targets = pendingBonds[draggedParticle];
            foreach (GameObject target in targets)
            {
                if (target == null) continue;
                
                // Double-check if particles can still accept bonds
                ParticleBehaviour draggedBehaviour = draggedParticle.GetComponent<ParticleBehaviour>();
                ParticleBehaviour targetBehaviour = target.GetComponent<ParticleBehaviour>();
                
                if (draggedBehaviour != null && targetBehaviour != null &&
                    draggedBehaviour.CanAcceptMoreBonds() && targetBehaviour.CanAcceptMoreBonds())
                {
                    CreateDragBond(draggedParticle, target);
                }
            }
            
            // Clear pending bonds for this particle
            pendingBonds[draggedParticle].Clear();
        }
        
        /// <summary>
        /// Register a bond and merge lattices if needed
        /// </summary>
        private void RegisterBondToLattice(GameObject particleA, GameObject particleB, GameObject bond)
        {
            ParticleLattice latticeA = particleToLatticeMap.ContainsKey(particleA) ? particleToLatticeMap[particleA] : null;
            ParticleLattice latticeB = particleToLatticeMap.ContainsKey(particleB) ? particleToLatticeMap[particleB] : null;
            
            // Case 1: Both particles are in different lattices - merge them
            if (latticeA != null && latticeB != null && latticeA != latticeB)
            {
                MergeLattices(latticeA, latticeB, bond);
                latticeA.RebuildGridStructure();
            }
            // Case 2: Only particleA has a lattice - add particleB to it
            else if (latticeA != null && latticeB == null)
            {
                latticeA.AddParticle(particleB);
                latticeA.AddBond(bond);
                particleToLatticeMap[particleB] = latticeA;
                latticeA.RebuildGridStructure();
                Debug.Log($"Added {particleB.name} to lattice {latticeA.name}");
            }
            // Case 3: Only particleB has a lattice - add particleA to it
            else if (latticeA == null && latticeB != null)
            {
                latticeB.AddParticle(particleA);
                latticeB.AddBond(bond);
                particleToLatticeMap[particleA] = latticeB;
                latticeB.RebuildGridStructure();
                Debug.Log($"Added {particleA.name} to lattice {latticeB.name}");
            }
            // Case 4: Neither particle has a lattice - create new one
            else if (latticeA == null && latticeB == null)
            {
                GameObject latticeObj = new GameObject($"Lattice_NewBond");
                latticeObj.transform.parent = transform;
                ParticleLattice newLattice = latticeObj.AddComponent<ParticleLattice>();
                
                newLattice.AddParticle(particleA);
                newLattice.AddParticle(particleB);
                newLattice.AddBond(bond);
                
                allLattices.Add(newLattice);
                particleToLatticeMap[particleA] = newLattice;
                particleToLatticeMap[particleB] = newLattice;
                
                newLattice.RebuildGridStructure();
                Debug.Log($"Created new lattice with {particleA.name} and {particleB.name}");
            }
            // Case 5: Both in same lattice - just add the bond
            else if (latticeA != null && latticeA == latticeB)
            {
                latticeA.AddBond(bond);
                latticeA.RebuildGridStructure();
            }
        }
        
        /// <summary>
        /// Merge two lattices into one
        /// </summary>
        private void MergeLattices(ParticleLattice latticeA, ParticleLattice latticeB, GameObject connectingBond)
        {
            Debug.Log($"Merging {latticeB.name} into {latticeA.name}");
            
            // Transfer all particles from latticeB to latticeA
            foreach (GameObject particle in latticeB.GetParticles())
            {
                latticeA.AddParticle(particle);
                particleToLatticeMap[particle] = latticeA;
            }
            
            // Transfer all bonds from latticeB to latticeA
            foreach (GameObject bond in latticeB.GetBonds())
            {
                latticeA.AddBond(bond);
            }
            
            // Add the connecting bond
            latticeA.AddBond(connectingBond);
            
            // Remove latticeB from tracking
            allLattices.Remove(latticeB);
            
            // Destroy latticeB GameObject
            if (latticeB != null && latticeB.gameObject != null)
            {
                Destroy(latticeB.gameObject);
            }
        }
        
        /// <summary>
        /// Get all currently tracked lattices
        /// </summary>
        public List<ParticleLattice> GetAllLattices() => allLattices;
        
        /// <summary>
        /// Get the lattice that contains a specific particle
        /// </summary>
        public ParticleLattice GetLatticeForParticle(GameObject particle)
        {
            return particleToLatticeMap.ContainsKey(particle) ? particleToLatticeMap[particle] : null;
        }
        
        /// <summary>
        /// Check if bonding mode is currently enabled
        /// </summary>
        public bool IsBondingModeEnabled() => bondingMode;

        /// <summary>
        /// Toggle bonding mode on/off
        /// </summary>
        public void SetBondingMode(bool enabled)
        {
            bondingMode = enabled;

            // Notify temperature brush to turn off when bonding is enabled
            if (enabled && temperatureBrush != null)
            {
                temperatureBrush.OnBondingModeEnabled();
            }
        }
        
        public void DisableBondingMode()
        {
            bondingMode = false;
        }
        
        /// <summary>
        /// Get the list of pending bond targets for a particle
        /// </summary>
        public List<GameObject> GetPendingBondTargets(GameObject particle)
        {
            if (pendingBonds.ContainsKey(particle))
            {
                return pendingBonds[particle];
            }
            return null;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}