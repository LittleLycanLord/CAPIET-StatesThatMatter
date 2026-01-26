using UnityEngine;

namespace LilLycanLord_Official
{
    [CreateAssetMenu(fileName = "New Particle Lattice Preset", menuName = "States That Matter/Particle Lattice Preset")]
    public class ParticleLatticePreset : ScriptableObject
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Prefabs")]
        public GameObject particlePrefab;
        public GameObject bondPrefab;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Lattice Configuration")]
        public int latticeRows = 4;
        public int latticeColumns = 4;
        public bool createAsGas = false;
        
        [Space(10)]
        [Header("Particle Attributes")]
        public float particleRadius = 1f;
        public float particleMass = 1f;
        public Color particleColor = Color.white;
        
        [Space(10)]
        [Header("Particle Temperature Settings")]
        [Tooltip("Initial temperature for particles in Celsius")] 
        public float particleTemperature = 30f;
        [Tooltip("Minimum temperature in Celsius")] 
        public float particleMinTemperature = -10f;
        [Tooltip("Maximum temperature in Celsius")] 
        public float particleMaxTemperature = 110f;
        
        [Space(10)]
        [Header("Bond Attributes")]
        public float bondLength = 1f;
        public float bondWidth = 0.5f;
        public Color bondColor = Color.white;
        [Range(0f, 1f)] public float bondStiffness = 0.98f;
        public float maxBondLengthMultiplier = 2f;
        
        [Space(10)]
        [Header("Bond Temperature Settings")]
        [Tooltip("Minimum temperature in Celsius")] 
        public float bondMinTemperature = -10f;
        [Tooltip("Maximum temperature in Celsius")] 
        public float bondMaxTemperature = 110f;
        [Tooltip("Temperature at which bond behaves as liquid")] 
        public float bondLiquidTemperature = 50f;
        [Tooltip("Stiffness when at liquid temperature")] 
        [Range(0f, 1f)] public float bondLiquidStiffness = 0.5f;
        [Tooltip("Temperature at which bond behaves as solid")] 
        public float bondSolidTemperature = 0f;
        [Tooltip("Stiffness when at solid temperature")] 
        [Range(0f, 1f)] public float bondSolidStiffness = 0.98f;
        public bool bondCanBreak = true;
        [Tooltip("Temperature at which bond breaks")] 
        public float bondBreakingTemperature = 110f;
        
        [Space(10)]
        [Header("Dynamic Bond Attributes")]
        public float dynamicBondLength = 1f;
        public float dynamicBondWidth = 0.5f;
        public Color dynamicBondColor = Color.blue;
        [Range(0f, 1f)] public float dynamicBondStiffness = 0.98f;
        public float dynamicMaxBondLengthMultiplier = 2f;
        
        [Space(10)]
        [Header("Dynamic Bonding")]
        public float bondingProximity = 1.5f;
        public float bondCheckInterval = 0.2f;
        public int maxDynamicBondsCreated = 2;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}