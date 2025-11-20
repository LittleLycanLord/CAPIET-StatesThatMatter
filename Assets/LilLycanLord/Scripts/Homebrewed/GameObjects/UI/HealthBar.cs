using System;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Advanced health bar component that provides smooth health transitions and color-coded feedback.
    /// Integrates with ColorShifter for visual feedback and GameEventManager for per-GameObject events.
    /// Supports both immediate health changes and smooth sliding transitions.
    /// </summary>
    [RequireComponent(typeof(ColorShifter))]
    [RequireComponent(typeof(CanvasFader))]
    public class HealthBar : MonoBehaviour, IHasSignals
    {
        [System.Serializable]
        public class HealthChangeEventData
        {
            public float previousHealth;
            public float newHealth;
            public float changeAmount;
            public GameObject source;

            public HealthChangeEventData(float prev, float newVal, float change, GameObject src)
            {
                previousHealth = prev;
                newHealth = newVal;
                changeAmount = change;
                source = src;
            }
        }

        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [SerializeField]
        [Tooltip("ColorShifter component (auto-assigned)")]
        private ColorShifter colorShifter;

        [SerializeField]
        [Tooltip("UICanvasFader component (auto-assigned)")]
        private CanvasFader alphaFader;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Health Status")]
        [SerializeField]
        [Tooltip("Current displayed health value")]
        private float currentHealth = 100f;

        [SerializeField]
        [Tooltip("Actual calculated health (with modifiers)")]
        private float actualHealth = 100f;

        [SerializeField]
        [Tooltip("Target health for sliding animations")]
        private float targetHealth = 100f;

        [SerializeField]
        [Tooltip("Current health as normalized value (0-1)")]
        [Range(0f, 1f)]
        private float healthPercentage = 1f;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Health Configuration")]
        [SerializeField]
        [Tooltip("Maximum health value")]
        private float maxHealth = 100f;

        [SerializeField]
        [Tooltip("Multiplier applied to health calculations")]
        private float healthMultiplier = 1.0f;

        [SerializeField]
        [Tooltip("Flat modifier added to health calculations")]
        private float healthModifier = 0.0f;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Enable smooth health bar sliding animations")]
        private bool enableSliding = true;

        [SerializeField]
        [Tooltip("Rate at which health increases (per second)")]
        private float healRate = 50f;

        [SerializeField]
        [Tooltip("Rate at which health decreases (per second)")]
        private float damageRate = 30f;

        [SerializeField]
        [Tooltip("Clamp health values to valid range")]
        private bool clampHealthValues = true;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Called when health value changes")]
        private UnityEvent<HealthChangeEventData> onHealthChanged;

        [SerializeField]
        [Tooltip("Called when health is added/restored")]
        private UnityEvent<HealthChangeEventData> onHealthAdded;

        [SerializeField]
        [Tooltip("Called when health is reduced")]
        private UnityEvent<HealthChangeEventData> onHealthDeduction;

        [SerializeField]
        [Tooltip("Called when health reaches maximum")]
        private UnityEvent onHealthFull;

        [SerializeField]
        [Tooltip("Called when health reaches zero")]
        private UnityEvent onHealthEmpty;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        // GameEventManager signal keys for this specific GameObject
        private string damageEventKey;
        private string healEventKey;
        private string healthEmptyEventKey;
        private string healthFullEventKey;

        /// <summary>Current health value</summary>
        public float CurrentHealth => currentHealth;

        /// <summary>Maximum health value</summary>
        public float MaxHealth
        {
            get => maxHealth;
            set
            {
                maxHealth = Mathf.Max(1f, value);
                RecalculateHealth();
            }
        }

        /// <summary>Actual calculated health with modifiers</summary>
        public float ActualHealth => actualHealth;

        /// <summary>Health as normalized percentage (0-1)</summary>
        public float HealthPercentage => healthPercentage;

        /// <summary>Is health currently sliding to target</summary>
        public bool IsSliding => enableSliding && Mathf.Abs(currentHealth - targetHealth) > 0.01f;

        /// <summary>Is health at maximum</summary>
        public bool IsHealthFull => actualHealth >= maxHealth;

        /// <summary>Is health at zero</summary>
        public bool IsHealthEmpty => actualHealth <= 0f;

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            // Get components
            colorShifter = GetComponent<ColorShifter>();
            alphaFader = GetComponent<CanvasFader>();

            // Initialize health values
            targetHealth = currentHealth;
            RecalculateHealth();

            // Initialize GameEventManager signal keys
            InitializeSignals();
        }

        void Update()
        {
            // Handle sliding animation
            HandleHealthSliding();

            // Recalculate health with modifiers
            RecalculateHealth();

            // Update ColorShifter value
            UpdateColorShifterValue();

            // Check for health state changes
            CheckHealthStates();
        }

        void OnDestroy()
        {
            CleanupSignals();
        }

        void OnValidate()
        {
            // Clamp values to valid ranges
            maxHealth = Mathf.Max(1f, maxHealth);
            healRate = Mathf.Max(0f, healRate);
            damageRate = Mathf.Max(0f, damageRate);

            if (clampHealthValues)
            {
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                targetHealth = Mathf.Clamp(targetHealth, 0f, maxHealth);
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        /// <summary>
        /// Add health (positive) or damage (negative) to the health bar
        /// </summary>
        public void AddHealth(float amount)
        {
            if (amount == 0f)
                return;

            alphaFader?.FadeIn();

            float previousHealth = actualHealth;
            float newTargetHealth = targetHealth + amount;

            // Clamp if enabled
            if (clampHealthValues)
                newTargetHealth = Mathf.Clamp(newTargetHealth, 0f, maxHealth);

            // Apply change
            if (enableSliding)
            {
                targetHealth = newTargetHealth;
            }
            else
            {
                currentHealth = newTargetHealth;
                targetHealth = currentHealth;
            }

            // Recalculate and trigger events
            RecalculateHealth();
            TriggerHealthChangeEvents(previousHealth, actualHealth, amount);

            // Trigger GameEventManager signals
            if (amount > 0)
                GameEventManager.Instance?.SendSignal(healEventKey);
            else
                GameEventManager.Instance?.SendSignal(damageEventKey);
        }

        public void DealDamage(float amount)
        {
            AddHealth(-amount);
        }

        /// <summary>
        /// Set health to a specific value
        /// </summary>
        public void SetHealth(float newHealth)
        {
            alphaFader?.FadeIn();

            float previousHealth = actualHealth;
            float changeAmount = newHealth - targetHealth;

            // Clamp if enabled
            if (clampHealthValues)
                newHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

            // Apply change
            if (enableSliding)
            {
                targetHealth = newHealth;
            }
            else
            {
                currentHealth = newHealth;
                targetHealth = currentHealth;
            }

            // Recalculate and trigger events
            RecalculateHealth();
            TriggerHealthChangeEvents(previousHealth, actualHealth, changeAmount);
        }

        /// <summary>
        /// Set health immediately without sliding animation
        /// </summary>
        public void SetHealthImmediate(float newHealth)
        {
            alphaFader?.FadeIn();

            float previousHealth = actualHealth;
            float changeAmount = newHealth - currentHealth;

            // Clamp if enabled
            if (clampHealthValues)
                newHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

            // Apply change immediately
            currentHealth = newHealth;
            targetHealth = currentHealth;

            // Recalculate and trigger events
            RecalculateHealth();
            TriggerHealthChangeEvents(previousHealth, actualHealth, changeAmount);
        }

        /// <summary>
        /// Heal to full health
        /// </summary>
        [ContextMenu("Heal to Full")]
        public void HealToFull()
        {
            SetHealth(maxHealth);
        }

        /// <summary>
        /// Set health to zero (kill)
        /// </summary>
        [ContextMenu("Set to Zero")]
        public void SetToZero()
        {
            SetHealth(0f);
        }

        /// <summary>
        /// Test heal functionality
        /// </summary>
        [ContextMenu("Test Heal")]
        public void TestHeal()
        {
            AddHealth(20f);
        }

        /// <summary>
        /// Test damage functionality
        /// </summary>
        [ContextMenu("Test Damage")]
        public void TestDamage()
        {
            DealDamage(20f);
        }

        /// <summary>
        /// Reset health modifiers to default
        /// </summary>
        [ContextMenu("Reset Modifiers")]
        public void ResetModifiers()
        {
            healthModifier = 0f;
            healthMultiplier = 1f;
            RecalculateHealth();
        }

        /// <summary>
        /// Get health change event data for the current state
        /// </summary>
        public HealthChangeEventData GetCurrentHealthData()
        {
            return new HealthChangeEventData(actualHealth, actualHealth, 0f, gameObject);
        }

        //* ╔═══════════════════════════╗
        //* ║ Virtual/Overridden Functions ║
        //* ╚═══════════════════════════╝

        /// <summary>
        /// Initialize GameEventManager signals for this specific GameObject
        /// </summary>
        public void InitializeSignals()
        {
            if (GameEventManager.Instance == null)
                return;

            // Register this object as a signal source
            GameEventManager.Instance.RegisterSignalObject(this);

            // Create unique signal keys for this specific GameObject
            string objectId = gameObject.GetInstanceID().ToString();
            damageEventKey = $"HealthBar_Damage_{objectId}";
            healEventKey = $"HealthBar_Heal_{objectId}";
            healthEmptyEventKey = $"HealthBar_Empty_{objectId}";
            healthFullEventKey = $"HealthBar_Full_{objectId}";
        }

        /// <summary>
        /// Cleanup GameEventManager signals when destroyed
        /// </summary>
        public void CleanupSignals()
        {
            if (GameEventManager.Instance == null)
                return;

            // Unregister this object from GameEventManager
            GameEventManager.Instance.UnregisterSignalObject(this);
        }

        private void HandleHealthSliding()
        {
            if (!enableSliding || Mathf.Approximately(currentHealth, targetHealth))
                return;

            float rate = (targetHealth > currentHealth) ? healRate : damageRate;
            float direction = Mathf.Sign(targetHealth - currentHealth);

            currentHealth += direction * rate * Time.deltaTime;

            // Clamp to target
            if (direction > 0 && currentHealth >= targetHealth)
                currentHealth = targetHealth;
            else if (direction < 0 && currentHealth <= targetHealth)
                currentHealth = targetHealth;
        }

        private void RecalculateHealth()
        {
            float previousActualHealth = actualHealth;
            actualHealth = (currentHealth + healthModifier) * healthMultiplier;

            if (clampHealthValues)
                actualHealth = Mathf.Clamp(
                    actualHealth,
                    0f,
                    maxHealth * healthMultiplier + healthModifier
                );

            healthPercentage = maxHealth > 0 ? Mathf.Clamp01(actualHealth / maxHealth) : 0f;
        }

        private void UpdateColorShifterValue()
        {
            if (colorShifter == null)
                return;

            // Update the ColorShifter's value based on health percentage
            colorShifter.SetValue(healthPercentage);

            // Also update the slider value if ColorShifter is using a slider
            if (colorShifter.UseSlider)
            {
                var slider = colorShifter.GetComponent<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    slider.value = healthPercentage;
                }
            }
        }

        private void CheckHealthStates()
        {
            // Check for health full state
            if (IsHealthFull && actualHealth < maxHealth) // Just became full
            {
                onHealthFull?.Invoke();
                GameEventManager.Instance?.SendSignal(healthFullEventKey);
            }

            // Check for health empty state
            if (IsHealthEmpty && actualHealth > 0f) // Just became empty
            {
                onHealthEmpty?.Invoke();
                GameEventManager.Instance?.SendSignal(healthEmptyEventKey);
            }
        }

        private void TriggerHealthChangeEvents(
            float previousHealth,
            float newHealth,
            float changeAmount
        )
        {
            if (Mathf.Approximately(changeAmount, 0f))
                return;

            var eventData = new HealthChangeEventData(
                previousHealth,
                newHealth,
                changeAmount,
                gameObject
            );

            // Trigger Unity Events
            onHealthChanged?.Invoke(eventData);

            if (changeAmount > 0)
            {
                onHealthAdded?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(healEventKey);
            }
            else
            {
                onHealthDeduction?.Invoke(eventData);
                GameEventManager.Instance?.SendSignal(damageEventKey);
            }
        }
    }
}
