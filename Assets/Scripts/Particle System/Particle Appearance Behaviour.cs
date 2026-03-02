using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class ParticleAppearanceBehaviour : MonoBehaviour
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
        [SerializeField] private bool enableColorChange = true;
        [SerializeField] private Color coldColor = Color.cyan;
        [SerializeField] private Color midColor = Color.white;
        [SerializeField] private Color hotColor = new Color(1f, 0.5f, 0f); // Orange
        
        [Space(10)]
        [Header("Temperature-Based Shaking")]
        [SerializeField] private bool enableShaking = true;
        [SerializeField] [Tooltip("Shake intensity at minimum temperature (usually 0)")]
        private float shakeAtMinTemp = 0f;
        [SerializeField] [Tooltip("Shake intensity at maximum temperature")]
        private float shakeAtMaxTemp = 0.15f;
        [SerializeField] private float shakeSpeed = 10f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Vector3 originalLocalPosition;
        private float shakeOffsetX;
        private float shakeOffsetY;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            parentParticle = GetComponentInParent<ParticleBehaviour>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError("ParticleAppearanceBehaviour: No SpriteRenderer found on this GameObject!");
            }
            
            if (parentParticle == null)
            {
                Debug.LogError("ParticleAppearanceBehaviour: No ParticleBehaviour found in parent!");
            }
            
            // Store original local position
            originalLocalPosition = transform.localPosition;
            
            // Initialize random shake offsets
            shakeOffsetX = Random.Range(0f, 1000f);
            shakeOffsetY = Random.Range(0f, 1000f);
        }

        void Start() { }

        void Update() 
        {
            if (enableColorChange)
            {
                UpdateColor();
            }
            
            if (enableShaking)
            {
                UpdateShake();
            }
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
        
        /// <summary>
        /// Update sprite shake/vibration based on particle temperature
        /// Higher temperature = more intense shaking (simulating particle vibration)
        /// </summary>
        private void UpdateShake()
        {
            if (parentParticle == null) return;
            
            // Get temperature and range from parent particle
            float currentTemp = parentParticle.GetTemperature();
            float minTemp = parentParticle.GetMinTemperature();
            float maxTemp = parentParticle.GetMaxTemperature();
            
            // Calculate normalized temperature (0 = min, 1 = max)
            float normalizedTemp = Mathf.InverseLerp(minTemp, maxTemp, currentTemp);
            
            // Calculate shake intensity based on temperature
            float shakeIntensity = Mathf.Lerp(shakeAtMinTemp, shakeAtMaxTemp, normalizedTemp);
            
            if (shakeIntensity <= 0.001f)
            {
                // No shake, reset to original position
                transform.localPosition = originalLocalPosition;
                return;
            }
            
            // Use Perlin noise for smooth, continuous shake
            float time = Time.time * shakeSpeed;
            float offsetX = (Mathf.PerlinNoise(time + shakeOffsetX, 0f) - 0.5f) * 2f * shakeIntensity;
            float offsetY = (Mathf.PerlinNoise(0f, time + shakeOffsetY) - 0.5f) * 2f * shakeIntensity;
            
            // Apply shake offset to original position
            transform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}