using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class ParticleColorPrototype : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private SpriteRenderer spriteRenderer;
        private ParticleBehaviour parentParticle;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Temperature Color Gradient")]
        [SerializeField] private Color coldColor = Color.cyan;
        [SerializeField] private Color midColor = Color.white;
        [SerializeField] private Color hotColor = new Color(1f, 0.5f, 0f); // Orange
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            parentParticle = GetComponentInParent<ParticleBehaviour>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError("ParticleColorPrototype: No SpriteRenderer found on this GameObject!");
            }
            
            if (parentParticle == null)
            {
                Debug.LogError("ParticleColorPrototype: No ParticleBehaviour found in parent!");
            }
        }

        void Start() { }

        void Update() 
        {
            UpdateColor();
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Update sprite color based on particle temperature
        /// Cyan (cold) -> White (mid) -> Orange (hot)
        /// </summary>
        private void UpdateColor()
        {
            if (spriteRenderer == null || parentParticle == null) return;
            
            // Get temperature and range from parent particle
            float currentTemp = parentParticle.GetTemperature();
            float minTemp = parentParticle.GetMinTemperature();
            float maxTemp = parentParticle.GetMaxTemperature();
            
            // Calculate normalized temperature (0 = min, 1 = max)
            float normalizedTemp = Mathf.InverseLerp(minTemp, maxTemp, currentTemp);
            
            // Interpolate color based on temperature
            Color targetColor;
            
            if (normalizedTemp < 0.5f)
            {
                // Cold to mid: cyan -> white
                targetColor = Color.Lerp(coldColor, midColor, normalizedTemp * 2f);
            }
            else
            {
                // Mid to hot: white -> orange
                targetColor = Color.Lerp(midColor, hotColor, (normalizedTemp - 0.5f) * 2f);
            }
            
            spriteRenderer.color = targetColor;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}