using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class ParticleLattice : MonoBehaviour
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
        [Space(10)]
        [Header("Prefabs")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private GameObject bondPrefab;
        
        [Space(10)]
        [Header("Lattice Configuration")]
        [SerializeField] private GameObject latticeOrigin;
        [SerializeField] private int latticeRows = 3;
        [SerializeField] private int latticeColumns = 3;
        
        [Space(10)]
        [Header("Particle Attributes")]
        [SerializeField] private float particleRadius = 0.5f;
        [SerializeField] private float particleMass = 1f;
        [SerializeField] private Color particleColor = Color.white;
        
        [Space(10)]
        [Header("Bond Attributes")]
        [SerializeField] private float bondLength = 1f;
        [SerializeField] private float bondWidth = 0.05f;
        [SerializeField] private Color bondColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float bondStiffness = 0.5f;
        [SerializeField] private float maxBondLengthMultiplier = 2f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private GameObject[,] particleGrid;
        private List<GameObject> allParticles = new List<GameObject>();
        private List<GameObject> allBonds = new List<GameObject>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() { }

        void Update() { }

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
                Debug.LogError("ParticleLattice: Particle prefab is not assigned!");
                return;
            }
            
            if (bondPrefab == null)
            {
                Debug.LogError("ParticleLattice: Bond prefab is not assigned!");
                return;
            }
            
            if (latticeOrigin == null)
            {
                Debug.LogError("ParticleLattice: Lattice origin is not assigned!");
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
            
            // Generate bonds (horizontal and vertical)
            GenerateBonds();
            
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
                
                if (particleAField != null) particleAField.SetValue(bondBehaviour, particleA);
                if (particleBField != null) particleBField.SetValue(bondBehaviour, particleB);
                if (bondLengthField != null) bondLengthField.SetValue(bondBehaviour, bondLength);
                if (lineWidthField != null) lineWidthField.SetValue(bondBehaviour, bondWidth);
                if (bondColorField != null) bondColorField.SetValue(bondBehaviour, bondColor);
                if (stiffnessField != null) stiffnessField.SetValue(bondBehaviour, bondStiffness);
                if (maxBondLengthMultiplierField != null) maxBondLengthMultiplierField.SetValue(bondBehaviour, maxBondLengthMultiplier);
            }
            
            allBonds.Add(bond);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}