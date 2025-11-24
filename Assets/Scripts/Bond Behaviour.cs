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
        // [Header("Displays")]

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
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Color bondColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float stiffness = 0.5f;
        [SerializeField] private float maxBondLengthMultiplier = 2f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private float fixedBondDistance;

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
            }
        }
        
        void FixedUpdate()
        {
            if (particleA != null && particleB != null && rbA != null && rbB != null)
            {
                ApplyDistanceConstraint();
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
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
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
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
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
            
            // Determine constraint iterations based on stiffness
            // Low stiffness: 1 iteration, High stiffness: up to 50 iterations for very rigid constraints
            int iterations = Mathf.Max(1, Mathf.RoundToInt(stiffness * correctionIterations));
            
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
                
                if (stiffness < 0.01f)
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
                    correctionStrength = Mathf.Lerp(0.3f, 1.0f, stiffness);
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
            lineWidth = width;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}