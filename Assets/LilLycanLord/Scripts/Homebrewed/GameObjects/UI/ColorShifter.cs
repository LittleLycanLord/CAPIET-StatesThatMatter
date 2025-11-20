using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced color shifting component that changes the color of a target Image based on a value.
    /// Supports multiple color ranges and can use either a Slider component or a manual float value.
    /// </summary>
    public class ColorShifter : MonoBehaviour
    {
        [System.Serializable]
        public class ColorRange
        {
            [Tooltip("The threshold value for this color range (0-1)")]
            [Range(0f, 1f)]
            public float threshold = 0.5f;

            [Tooltip("The color for this range")]
            public Color color = Color.white;

            public ColorRange(float threshold, Color color)
            {
                this.threshold = threshold;
                this.color = color;
            }
        }

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [SerializeField]
        [Tooltip("Slider component (auto-assigned if using slider mode)")]
        private UnityEngine.UI.Slider slider;

        [SerializeField]
        [Tooltip("Target Image component to change colors")]
        private UnityEngine.UI.Image targetImage;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Displays")]
        [SerializeField]
        [Tooltip("Current normalized value (0-1)")]
        [Range(0f, 1f)]
        private float currentValue = 1.0f;

        [SerializeField]
        [Tooltip("Current color being displayed")]
        private Color currentColor = Color.white;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Value Source")]
        [SerializeField]
        [Tooltip("Use slider component for value, otherwise use manual float value")]
        private bool useSlider = true;

        [SerializeField]
        [Tooltip("Manual value when not using slider (0-1)")]
        [Range(0f, 1f)]
        public float manualValue = 1.0f;

        [Header("Color Ranges")]
        [SerializeField]
        [Tooltip("Color ranges for value mapping (sorted by threshold)")]
        private ColorRange[] colorRanges = new ColorRange[]
        {
            new ColorRange(0.33f, Color.red),
            new ColorRange(0.66f, Color.yellow),
            new ColorRange(1.0f, Color.green),
        };

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Smooth color transitions")]
        private bool smoothColorTransition = true;

        [SerializeField]
        [Tooltip("Speed of color transitions")]
        [Range(0.1f, 10f)]
        private float colorTransitionSpeed = 5f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private float previousValue;
        private Color targetColor;

        /// <summary>Current normalized value (0-1)</summary>
        public float Value => currentValue;

        /// <summary>Current color being displayed</summary>
        public Color CurrentColor => currentColor;

        /// <summary>Whether using slider or manual value</summary>
        public bool UseSlider
        {
            get => useSlider;
            set => useSlider = value;
        }

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            // Get slider component if using slider mode
            if (useSlider)
                slider = GetComponent<UnityEngine.UI.Slider>();

            // Validate color ranges
            ValidateColorRanges();

            // Initialize values
            previousValue = useSlider ? (slider?.value ?? 0f) : manualValue;
            currentValue = previousValue;
            targetColor = CalculateColorForValue(currentValue);
            currentColor = targetColor;
        }

        void Update()
        {
            // Get current value based on mode
            float newValue = useSlider && slider != null ? slider.value : manualValue;
            currentValue = newValue;

            // Update colors
            UpdateColors();

            // Store previous value
            previousValue = currentValue;
        }

        void OnValidate()
        {
            // Validate and sort color ranges
            ValidateColorRanges();

            // Clamp manual value
            manualValue = Mathf.Clamp01(manualValue);
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Set value manually (bypasses slider)
        /// </summary>
        public void SetValue(float value)
        {
            manualValue = Mathf.Clamp01(value);
            if (!useSlider)
                currentValue = manualValue;
        }

        /// <summary>
        /// Add a new color range to the array
        /// </summary>
        public void AddColorRange(float threshold, Color color)
        {
            var newRange = new ColorRange(threshold, color);
            var rangeList = new System.Collections.Generic.List<ColorRange>(colorRanges);
            rangeList.Add(newRange);
            colorRanges = rangeList.ToArray();
            ValidateColorRanges();
        }

        /// <summary>
        /// Remove color range at index
        /// </summary>
        public void RemoveColorRange(int index)
        {
            if (index < 0 || index >= colorRanges.Length || colorRanges.Length <= 1)
                return;

            var rangeList = new System.Collections.Generic.List<ColorRange>(colorRanges);
            rangeList.RemoveAt(index);
            colorRanges = rangeList.ToArray();
        }

        /// <summary>
        /// Get the color that should be displayed for a given value
        /// </summary>
        public Color GetColorForValue(float value)
        {
            return CalculateColorForValue(Mathf.Clamp01(value));
        }

        /// <summary>
        /// Reset color ranges to default (Red, Yellow, Green)
        /// </summary>
        [ContextMenu("Reset Color Ranges")]
        public void ResetColorRanges()
        {
            colorRanges = new ColorRange[]
            {
                new ColorRange(0.33f, Color.red),
                new ColorRange(0.66f, Color.yellow),
                new ColorRange(1.0f, Color.green),
            };
        }

        //* ╔═══════════════════════════╗
        //* ║ Virtual/Overridden Functions ║
        //* ╚═══════════════════════════╝

        private void UpdateColors()
        {
            if (targetImage == null)
                return;

            // Calculate target color
            targetColor = CalculateColorForValue(currentValue);

            // Apply color (smooth or immediate)
            if (smoothColorTransition)
            {
                currentColor = Color.Lerp(
                    currentColor,
                    targetColor,
                    Time.deltaTime * colorTransitionSpeed
                );
                targetImage.color = currentColor;
            }
            else
            {
                currentColor = targetColor;
                targetImage.color = currentColor;
            }
        }

        private Color CalculateColorForValue(float value)
        {
            value = Mathf.Clamp01(value);

            // If no ranges, return white
            if (colorRanges == null || colorRanges.Length == 0)
                return Color.white;

            // Find the appropriate color range
            for (int i = 0; i < colorRanges.Length; i++)
            {
                if (value <= colorRanges[i].threshold)
                {
                    // If this is the first range, just return its color
                    if (i == 0)
                        return colorRanges[i].color;

                    // Interpolate between this range and the previous one
                    float prevThreshold = i > 0 ? colorRanges[i - 1].threshold : 0f;
                    float currentThreshold = colorRanges[i].threshold;

                    // Calculate interpolation factor
                    float t = (value - prevThreshold) / (currentThreshold - prevThreshold);

                    Color prevColor = i > 0 ? colorRanges[i - 1].color : colorRanges[i].color;
                    return Color.Lerp(prevColor, colorRanges[i].color, t);
                }
            }

            // If value exceeds all thresholds, return the last color
            return colorRanges[colorRanges.Length - 1].color;
        }

        private void ValidateColorRanges()
        {
            if (colorRanges == null || colorRanges.Length == 0)
            {
                ResetColorRanges();
                return;
            }

            // Clamp all thresholds to 0-1 range
            for (int i = 0; i < colorRanges.Length; i++)
            {
                colorRanges[i].threshold = Mathf.Clamp01(colorRanges[i].threshold);
            }

            // Sort ranges by threshold
            System.Array.Sort(colorRanges, (a, b) => a.threshold.CompareTo(b.threshold));

            // Ensure last threshold is 1.0
            if (colorRanges.Length > 0 && colorRanges[colorRanges.Length - 1].threshold < 1.0f)
            {
                colorRanges[colorRanges.Length - 1].threshold = 1.0f;
            }
        }
    }
}
