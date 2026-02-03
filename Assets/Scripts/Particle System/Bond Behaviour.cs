using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(LineRenderer))]
    public class BondBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private LineRenderer lineRenderer;
        private Rigidbody rbA;
        private Rigidbody rbB;
        private SphereCollider colliderA;
        private SphereCollider colliderB;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private float currentTemperature = 30.0f;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Bonded Particles")]
        [SerializeField] private GameObject particleA;
        [SerializeField] private GameObject particleB;
        
        [Space(10)]
        [Header("Bond Settings")]
        [SerializeField] private float bondLength = 1f;
        [SerializeField] private int correctionIterations = 30;
        [SerializeField] [Tooltip("Line width when stiffness is at maximum (1.0)")]
        private float maxStiffnessWidth = 0.2f;
        [SerializeField] private Color bondColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float stiffness = 0.5f;
        [SerializeField] private float maxBondLengthMultiplier = 2f;
        
        [Space(10)]
        [Header("Temperature Settings")]
        [SerializeField] [Tooltip("Minimum temperature in Celsius")] private float minTemperature = -10.0f;
        [SerializeField] [Tooltip("Maximum temperature in Celsius")] private float maxTemperature = 110.0f;
        
        [Space(10)]
        [Header("Phase Transition Settings")]
        [SerializeField] [Tooltip("Temperature at which bond behaves as liquid (in Celsius)")] private float liquidTemperature = 50f;
        [SerializeField] [Tooltip("Stiffness when at liquid temperature")] [Range(0f, 1f)] private float liquidStiffness = 0.5f;
        [SerializeField] [Tooltip("Temperature at which bond behaves as solid (in Celsius)")] private float solidTemperature = 0f;
        [SerializeField] [Tooltip("Stiffness when at solid temperature")] [Range(0f, 1f)] private float solidStiffness = 0.98f;
        
        [Space(10)]
        [Header("Breaking Settings")]
        [SerializeField] private bool canBreak = true;
        [SerializeField] private float breakingTempWidth = 0.00f;
        [SerializeField] [Tooltip("Temperature at which bond breaks (in Celsius)")] private float breakingTemperature = 110f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private float fixedBondDistance;
        private ParticleBehaviour particleABehaviour;
        private ParticleBehaviour particleBBehaviour;
        private float baseStiffness; // Store the original stiffness value
        private float currentStiffness; // Current stiffness based on temperature
        
        /// <summary>
        /// Get the current stiffness value of this bond
        /// </summary>
        public float GetCurrentStiffness()
        {
            return currentStiffness;
        }

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            lineRenderer = GetComponent<LineRenderer>();
            InitializeLineRenderer();
        }

        void Start() 
        {
            if (particleA != null && particleB != null)
            {
                // Get Rigidbody and SphereCollider components
                rbA = particleA.GetComponent<Rigidbody>();
                rbB = particleB.GetComponent<Rigidbody>();
                colliderA = particleA.GetComponent<SphereCollider>();
                colliderB = particleB.GetComponent<SphereCollider>();
                
                // Get ParticleBehaviour components
                particleABehaviour = particleA.GetComponent<ParticleBehaviour>();
                particleBBehaviour = particleB.GetComponent<ParticleBehaviour>();
                
                // Get sphere radii (accounting for scale)
                float radiusA = colliderA != null ? colliderA.radius * particleA.transform.localScale.x : 0f;
                float radiusB = colliderB != null ? colliderB.radius * particleB.transform.localScale.x : 0f;
                
                // Calculate initial distance between particle centers
                float currentDistance = Vector3.Distance(particleA.transform.position, particleB.transform.position);
                
                // If bondLength is not manually set, calculate it from current distance minus radii
                if (bondLength <= 0)
                {
                    bondLength = Mathf.Max(0.1f, currentDistance - radiusA - radiusB);
                }
                
                // Fixed bond distance = radiusA + bondLength + radiusB (center to center distance)
                fixedBondDistance = radiusA + bondLength + radiusB;
                
                // Store base stiffness value
                baseStiffness = stiffness;
                currentStiffness = stiffness;
            }
        }
        
        void OnDestroy()
        {
            // Remove bond from lattice when destroyed
            ParticleLattice lattice = GetComponentInParent<ParticleLattice>();
            if (lattice != null)
            {
                lattice.RemoveBond(gameObject);
            }
        }
        
        void FixedUpdate()
        {
            if (particleA != null && particleB != null && rbA != null && rbB != null)
            {
                ApplyDistanceConstraint();
            }
        }
        
        void Update()
        {
            // Update temperature based on average of connected particles
            UpdateTemperature();
            
            // Calculate dynamic stiffness based on temperature
            CalculateDynamicStiffness();
            
            // Apply drag based on stiffness
            ApplyStiffnessDrag();
            
            // Check if bond should break
            if (canBreak && currentTemperature >= breakingTemperature)
            {
                BreakBond();
            }
        }
        
        void LateUpdate()
        {
            if (particleA != null && particleB != null)
            {
                // Update line renderer positions
                UpdateLineRenderer();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        private void InitializeLineRenderer()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = breakingTempWidth;
            lineRenderer.endWidth = breakingTempWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = bondColor;
            lineRenderer.endColor = bondColor;
        }

        private void UpdateLineRenderer()
        {
            lineRenderer.SetPosition(0, particleA.transform.position);
            lineRenderer.SetPosition(1, particleB.transform.position);
            lineRenderer.startColor = bondColor;
            lineRenderer.endColor = bondColor;
            
            // Calculate width based on temperature directly for foolproof behavior
            // coldest (minTemperature) → thickest (maxStiffnessWidth)
            // hottest (breakingTemperature) → thinnest (breakingTempWidth)
            float tempRange = breakingTemperature - minTemperature;
            float normalizedTemp = 0f;
            
            if (tempRange > 0f)
            {
                normalizedTemp = Mathf.Clamp01((currentTemperature - minTemperature) / tempRange);
            }
            
            // Invert: 0 (cold) = thick, 1 (hot) = thin
            float dynamicWidth = Mathf.Lerp(maxStiffnessWidth, breakingTempWidth, normalizedTemp);
            lineRenderer.startWidth = dynamicWidth;
            lineRenderer.endWidth = dynamicWidth;
        }
        
        private void ApplyDistanceConstraint()
        {
            Vector3 posA = rbA.position;
            Vector3 posB = rbB.position;
            
            Vector3 direction = posB - posA;
            float currentDistance = direction.magnitude;
            
            if (currentDistance < 0.0001f) return;
            
            // Calculate maximum allowed distance based on stiffness
            float radiusA = colliderA != null ? colliderA.radius * particleA.transform.localScale.x : 0f;
            float radiusB = colliderB != null ? colliderB.radius * particleB.transform.localScale.x : 0f;
            float maxAllowedDistance = radiusA + (bondLength * maxBondLengthMultiplier) + radiusB;
            
            // Determine constraint iterations based on current dynamic stiffness
            // Low stiffness: 1 iteration, High stiffness: up to 50 iterations for very rigid constraints
            int iterations = Mathf.Max(1, Mathf.RoundToInt(currentStiffness * correctionIterations));
            
            for (int i = 0; i < iterations; i++)
            {
                // Recalculate for each iteration
                posA = rbA.position;
                posB = rbB.position;
                direction = posB - posA;
                currentDistance = direction.magnitude;
                
                if (currentDistance < 0.0001f) return;
                
                float targetDistance;
                float correctionStrength;
                
                if (currentStiffness < 0.01f)
                {
                    // Very low stiffness: only enforce max stretch limit
                    if (currentDistance > maxAllowedDistance)
                    {
                        targetDistance = maxAllowedDistance;
                        correctionStrength = 0.5f; // Gentle correction
                    }
                    else
                    {
                        continue; // No correction needed if within max stretch
                    }
                }
                else
                {
                    // Normal stiffness: blend between stretchy and rigid
                    targetDistance = fixedBondDistance;
                    // Higher stiffness = stronger correction per iteration
                    correctionStrength = Mathf.Lerp(0.3f, 1.0f, currentStiffness);
                }
                
                // Calculate correction needed
                float error = (currentDistance - targetDistance) / currentDistance;
                Vector3 correction = direction * error * correctionStrength;
                
                // Use Rigidbody.MovePosition to respect collisions
                rbA.MovePosition(posA + correction);
                rbB.MovePosition(posB - correction);
            }
        }
        
        /// <summary>
        /// Update bond temperature based on average of connected particles
        /// </summary>
        private void UpdateTemperature()
        {
            if (particleABehaviour == null || particleBBehaviour == null) return;
            
            // Calculate average temperature from both particles
            float tempA = particleABehaviour.GetTemperature();
            float tempB = particleBBehaviour.GetTemperature();
            
            currentTemperature = (tempA + tempB) / 2f;
            
            // Clamp to min/max range
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
        }
        
        /// <summary>
        /// Calculate dynamic stiffness based on current temperature
        /// Interpolates between solid and liquid states
        /// </summary>
        private void CalculateDynamicStiffness()
        {
            // If temperatures are the same, no transition occurs
            if (Mathf.Approximately(solidTemperature, liquidTemperature))
            {
                currentStiffness = baseStiffness;
                return;
            }
            
            // Determine the temperature range for interpolation
            float tempRange = liquidTemperature - solidTemperature;
            
            // Calculate normalized position in temperature range (0 = solid, 1 = liquid)
            float normalizedTemp = Mathf.Clamp01((currentTemperature - solidTemperature) / tempRange);
            
            // Interpolate stiffness based on temperature
            // At solidTemperature: use solidStiffness
            // At liquidTemperature: use liquidStiffness
            currentStiffness = Mathf.Lerp(solidStiffness, liquidStiffness, normalizedTemp);
            
            // Update the serialized stiffness field for visualization in inspector
            stiffness = currentStiffness;
        }
        
        /// <summary>
        /// Break the bond - remove from particles and destroy
        /// </summary>
        private void BreakBond()
        {
            Debug.Log($"Bond breaking at temperature {currentTemperature}°C (breaking point: {breakingTemperature}°C)");
            
            // Remove bond from particles
            if (particleABehaviour != null)
            {
                particleABehaviour.RemoveBond(gameObject);
            }
            
            if (particleBBehaviour != null)
            {
                particleBBehaviour.RemoveBond(gameObject);
            }
            
            // Destroy this bond
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Apply drag to connected particles based on bond stiffness
        /// Higher stiffness = more drag, stiffness of 1.0 = complete freeze
        /// </summary>
        private void ApplyStiffnessDrag()
        {
            if (rbA == null || rbB == null) return;
            
            // Calculate drag multiplier based on stiffness
            // At 0 stiffness: no drag (1.0 multiplier)
            // At 1 stiffness: complete freeze (0.0 multiplier)
            float dragEffect = Mathf.Lerp(0f, 20f, currentStiffness);
            
            // Apply drag to both rigidbodies
            // Higher stiffness = higher drag = less movement
            rbA.linearDamping = dragEffect;
            rbB.linearDamping = dragEffect;
            
            // At maximum stiffness, also apply angular drag to prevent rotation
            if (currentStiffness >= 0.95f)
            {
                float angularDragEffect = Mathf.Lerp(0f, 10f, (currentStiffness - 0.95f) / 0.05f);
                rbA.angularDamping = angularDragEffect;
                rbB.angularDamping = angularDragEffect;
            }
            
            // Freeze particles when bond reaches minimum temperature
            if (currentTemperature <= minTemperature)
            {
                // Lock X and Y position (allow Z for 2D depth)
                rbA.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                rbB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                // Unlock position constraints when temperature rises above minimum
                rbA.constraints = RigidbodyConstraints.None;
                rbB.constraints = RigidbodyConstraints.None;
            }
        }
                
        /// <summary>
        /// Update the bond color at runtime
        /// </summary>
        public void SetBondColor(Color color)
        {
            bondColor = color;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
        
        /// <summary>
        /// Update the bond width at runtime
        /// </summary>
        public void SetBondWidth(float width)
        {
            breakingTempWidth = width;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}