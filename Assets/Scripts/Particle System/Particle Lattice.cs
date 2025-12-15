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
        
        [Space(10)]
        [Header("Grid Constraint")]
        [SerializeField] private bool enforceGridStructure = false;
        [SerializeField] private float gridSpacing = 2f;
        [SerializeField] private int gridConstraintIterations = 5;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private GameObject[,] particleGrid;
        private List<GameObject> allParticles = new List<GameObject>();
        private List<GameObject> allBonds = new List<GameObject>();
        private Dictionary<GameObject, Vector2Int> particleGridPositions = new Dictionary<GameObject, Vector2Int>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() { }

        void Update() { }
        
        void FixedUpdate()
        {
            if (enforceGridStructure && allParticles.Count > 0)
            {
                EnforceGridConstraints();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        public void AddParticle(GameObject particle)
        {
            if (!allParticles.Contains(particle))
            {
                allParticles.Add(particle);
            }
        }
        
        public void AddBond(GameObject bond)
        {
            if (!allBonds.Contains(bond))
            {
                allBonds.Add(bond);
            }
        }
        
        public void RemoveParticle(GameObject particle)
        {
            allParticles.Remove(particle);
        }
        
        public void RemoveBond(GameObject bond)
        {
            allBonds.Remove(bond);
        }
        
        public List<GameObject> GetParticles() => allParticles;
        public List<GameObject> GetBonds() => allBonds;
        public int ParticleCount => allParticles.Count;
        public int BondCount => allBonds.Count;
        
        /// <summary>
        /// Assign a grid position to a particle for constraint enforcement
        /// </summary>
        public void AssignGridPosition(GameObject particle, Vector2Int gridPos)
        {
            if (allParticles.Contains(particle))
            {
                particleGridPositions[particle] = gridPos;
            }
        }
        
        /// <summary>
        /// Rebuild grid positions based on bond connectivity
        /// </summary>
        public void RebuildGridStructure()
        {
            if (allParticles.Count == 0) return;
            
            particleGridPositions.Clear();
            
            // Start from first particle as origin
            GameObject originParticle = allParticles[0];
            particleGridPositions[originParticle] = Vector2Int.zero;
            
            // Use BFS to assign grid positions based on bonds
            Queue<GameObject> toProcess = new Queue<GameObject>();
            toProcess.Enqueue(originParticle);
            HashSet<GameObject> processed = new HashSet<GameObject>();
            
            while (toProcess.Count > 0)
            {
                GameObject current = toProcess.Dequeue();
                if (processed.Contains(current)) continue;
                processed.Add(current);
                
                Vector2Int currentGridPos = particleGridPositions[current];
                
                // Find all bonded neighbors
                foreach (GameObject bond in allBonds)
                {
                    if (bond == null) continue;
                    
                    BondBehaviour bondBehaviour = bond.GetComponent<BondBehaviour>();
                    if (bondBehaviour == null) continue;
                    
                    var particleAField = typeof(BondBehaviour).GetField("particleA", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var particleBField = typeof(BondBehaviour).GetField("particleB", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    GameObject particleA = particleAField?.GetValue(bondBehaviour) as GameObject;
                    GameObject particleB = particleBField?.GetValue(bondBehaviour) as GameObject;
                    
                    GameObject neighbor = null;
                    
                    if (particleA == current && particleB != null)
                    {
                        neighbor = particleB;
                    }
                    else if (particleB == current && particleA != null)
                    {
                        neighbor = particleA;
                    }
                    
                    if (neighbor != null && !particleGridPositions.ContainsKey(neighbor))
                    {
                        // Determine grid direction based on relative position
                        Vector3 direction = neighbor.transform.position - current.transform.position;
                        Vector2Int gridOffset = DetermineGridOffset(direction);
                        
                        particleGridPositions[neighbor] = currentGridPos + gridOffset;
                        toProcess.Enqueue(neighbor);
                    }
                }
            }
        }
        
        /// <summary>
        /// Determine grid offset based on physical direction
        /// </summary>
        private Vector2Int DetermineGridOffset(Vector3 direction)
        {
            direction.Normalize();
            
            // Determine primary axis
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal
                return direction.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                // Vertical
                return direction.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }
        
        /// <summary>
        /// Enforce particles to maintain square grid formation
        /// </summary>
        private void EnforceGridConstraints()
        {
            // Rebuild grid if positions not assigned
            if (particleGridPositions.Count != allParticles.Count)
            {
                RebuildGridStructure();
            }
            
            // Apply constraint iterations
            for (int iteration = 0; iteration < gridConstraintIterations; iteration++)
            {
                foreach (GameObject particle in allParticles)
                {
                    if (particle == null) continue;
                    if (!particleGridPositions.ContainsKey(particle)) continue;
                    
                    Rigidbody rb = particle.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    
                    Vector2Int gridPos = particleGridPositions[particle];
                    Vector3 targetPosition = new Vector3(gridPos.x * gridSpacing, gridPos.y * gridSpacing, particle.transform.position.z);
                    
                    // Apply constraint force
                    Vector3 correction = targetPosition - rb.position;
                    rb.MovePosition(rb.position + correction * 0.5f);
                }
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}