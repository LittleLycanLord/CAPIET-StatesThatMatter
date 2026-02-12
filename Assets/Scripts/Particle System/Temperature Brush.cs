using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class TemperatureBrush : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        private Camera mainCamera;
        private ParticleLatticeManager latticeManager;
        [SerializeField] private GameObject parkingPoint;
        [SerializeField] private GameObject coldParticles;
        [SerializeField] private GameObject hotParticles;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool heatingMode = false;
        [SerializeField] private bool coolingMode = false;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Brush Settings")]
        [SerializeField] private GameObject brushChild;
        [SerializeField] private float temperatureChangeRate = 10f;
        [SerializeField] private float brushDistanceFromCamera = 10f;
        
        [Space(10)]
        [Header("Audio Settings")]
        [SerializeField] private string heatingLoopSFXName = "HeatingLoop";
        [SerializeField] private string coolingLoopSFXName = "CoolingLoop";
        [SerializeField] private float audioFadeInDuration = 0.3f;
        [SerializeField] private float audioFadeOutDuration = 0.2f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private bool isActive = false;
        private SphereCollider brushCollider;
        private HashSet<GameObject> particlesInRange = new HashSet<GameObject>();
        private AudioSource currentLoopingAudio;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            mainCamera = Camera.main;
            latticeManager = FindAnyObjectByType<ParticleLatticeManager>();
            
            // Setup brush child
            if (brushChild != null)
            {
                brushCollider = brushChild.GetComponent<SphereCollider>();
                if (brushCollider == null)
                {
                    brushCollider = brushChild.AddComponent<SphereCollider>();
                }
                
                // Ensure it's a trigger
                brushCollider.isTrigger = true;

                // Add helper component for trigger events
                if (brushChild.GetComponent<TemperatureBrushCollider>() == null)
                {
                    brushChild.AddComponent<TemperatureBrushCollider>();
                }
                
                if (brushChild != null)
                {
                    brushChild.transform.position = parkingPoint != null ? parkingPoint.transform.position : Vector3.zero;
                }
            }
        }

        void OnEnable()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        }

        void Start() { }

        void Update() 
        {
            // Check if either mode is active
            if (heatingMode || coolingMode)
            {
                HandleTouchInput();
            }
            else if (isActive)
            {
                // Deactivate brush if modes are turned off
                DeactivateBrush();
            }
        }
        
        void FixedUpdate()
        {
            // Apply temperature changes to particles in range
            if (isActive && (heatingMode || coolingMode))
            {
                ApplyTemperatureChanges();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        private void HandleTouchInput()
        {
            // New Input System - Touch input
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    ActivateBrush(touch.screenPosition);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && isActive)
                {
                    UpdateBrushPosition(touch.screenPosition);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || 
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    DeactivateBrush();
                }
            }
            // Fallback to mouse input for editor testing
            else if (UnityEngine.InputSystem.Mouse.current != null)
            {
                if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ActivateBrush(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                }
                else if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed && isActive)
                {
                    UpdateBrushPosition(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                }
                else if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    DeactivateBrush();
                }
            }
        }
        
        private void ActivateBrush(Vector3 screenPosition)
        {
            if (brushChild == null || mainCamera == null) return;
            
            isActive = true;
            hotParticles.SetActive(heatingMode);
            coldParticles.SetActive(coolingMode);
            UpdateBrushPosition(screenPosition);
            
            // Start playing the appropriate looping sound
            PlayLoopingSound();
        }
        
        private void UpdateBrushPosition(Vector3 screenPosition)
        {
            if (brushChild == null || mainCamera == null) return;
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, brushDistanceFromCamera));
            brushChild.transform.position = worldPosition;
        }
        
        private void DeactivateBrush()
        {
            isActive = false;
            
            // Stop the looping sound with fade out
            StopLoopingSound();
            
            if (brushChild != null)
            {
                brushChild.transform.position = parkingPoint != null ? parkingPoint.transform.position : Vector3.zero;
            }
            
            particlesInRange.Clear();
        }
        
        private void ApplyTemperatureChanges()
        {
            foreach (GameObject particle in particlesInRange)
            {
                if (particle == null) continue;
                
                ParticleBehaviour particleBehaviour = particle.GetComponent<ParticleBehaviour>();
                if (particleBehaviour != null)
                {
                    if (heatingMode)
                    {
                        particleBehaviour.IncreaseTemperature(temperatureChangeRate * Time.fixedDeltaTime);
                    }
                    else if (coolingMode)
                    {
                        particleBehaviour.DecreaseTemperature(temperatureChangeRate * Time.fixedDeltaTime);
                    }
                }
            }
        }
        
        /// <summary>
        /// Enable heating mode and disable cooling mode and bonding
        /// </summary>
        public void SetHeatingMode(bool enabled)
        {
            heatingMode = enabled;
            
            if (enabled)
            {
                coolingMode = false;
                
                if (latticeManager != null)
                {
                    latticeManager.SetBondingMode(false);
                }
            }
        }

        public void DisableHeatingMode()
        {
            heatingMode = false;
        }
        
        /// <summary>
        /// Enable cooling mode and disable heating mode and bonding
        /// </summary>
        public void SetCoolingMode(bool enabled)
        {
            coolingMode = enabled;
            
            if (enabled)
            {
                heatingMode = false;
                
                if (latticeManager != null)
                {
                    latticeManager.SetBondingMode(false);
                }
            }
        }

        public void DisableCoolingMode()
        {
            coolingMode = false;
        }
        
        /// <summary>
        /// Called when bonding mode is enabled - disables temperature modes
        /// </summary>
        public void OnBondingModeEnabled()
        {
            heatingMode = false;
            coolingMode = false;
            
            if (isActive)
            {
                DeactivateBrush();
            }
        }
        
        /// <summary>
        /// Check if either heating or cooling mode is active
        /// </summary>
        public bool IsAnyModeActive()
        {
            return heatingMode || coolingMode;
        }
        
        /// <summary>
        /// Play the appropriate looping sound based on current mode
        /// </summary>
        private void PlayLoopingSound()
        {
            if (AudioManager.Instance == null) return;
            
            string soundToPlay = heatingMode ? heatingLoopSFXName : coolingMode ? coolingLoopSFXName : null;
            
            if (!string.IsNullOrEmpty(soundToPlay))
            {
                currentLoopingAudio = AudioManager.Instance.PlayWithFadeIn(
                    soundToPlay,
                    audioFadeInDuration,
                    gameObject
                );
            }
        }
        
        /// <summary>
        /// Stop the currently playing looping sound with fade out
        /// </summary>
        private void StopLoopingSound()
        {
            if (AudioManager.Instance != null && currentLoopingAudio != null)
            {
                AudioManager.Instance.Stop(currentLoopingAudio, audioFadeOutDuration);
                currentLoopingAudio = null;
            }
        }
        
        public void OnChildTriggerEnter(Collider other)
        {
            ParticleBehaviour particle = other.GetComponent<ParticleBehaviour>();
            if (particle != null)
            {
                particlesInRange.Add(other.gameObject);
                Debug.Log($"[TemperatureBrush] Particle {other.gameObject.name} entered brush range. Mode: {(heatingMode ? "Heating" : coolingMode ? "Cooling" : "None")}");
            }
        }
        
        public void OnChildTriggerExit(Collider other)
        {
            particlesInRange.Remove(other.gameObject);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}