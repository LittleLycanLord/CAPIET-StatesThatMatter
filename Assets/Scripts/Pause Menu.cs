using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LilLycanLord_Official
{
    public class PauseMenu : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject controls;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Scene Settings")]
        [SerializeField] private string levelSelectSceneName = "Level Select";
        [SerializeField] private string transitionType = "Crossfade";
        
        [Header("Animation Settings")]
        [SerializeField] private float growDuration = 0.5f;
        [SerializeField] private float shrinkDuration = 0.3f;
        [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Vector3 pauseMenuStartScale;
        private bool isPaused = false;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Store initial scale
            if (pauseMenu != null)
            {
                pauseMenuStartScale = pauseMenu.transform.localScale;
                pauseMenu.transform.localScale = Vector3.zero; // Start hidden
            }
        }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Pauses the game and shows the pause menu with animation
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            // Animate pause menu growing from center
            if (pauseMenu != null)
            {
                pauseMenu.transform.localScale = Vector3.zero;
                LeanTween.scale(pauseMenu, pauseMenuStartScale, growDuration)
                    .setEase(easeType)
                    .setIgnoreTimeScale(true); // Critical: allows animation during pause
                controls.SetActive(false);
            }
        }
        
        /// <summary>
        /// Unpauses the game and hides the pause menu with animation
        /// </summary>
        public void Unpause()
        {
            if (!isPaused) return;
            
            // Shrink pause menu
            if (pauseMenu != null)
            {
                LeanTween.scale(pauseMenu, Vector3.zero, shrinkDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setIgnoreTimeScale(true) // Critical: allows animation during pause
                    .setOnComplete(() =>
                    {
                        Time.timeScale = 1f;
                        isPaused = false;
                    });
                controls.SetActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
        
        /// <summary>
        /// Returns to the level select screen
        /// </summary>
        public void GoToLevelSelect()
        {
            Time.timeScale = 1f; // Ensure time scale is reset before loading
            isPaused = false;
            
            if (!string.IsNullOrEmpty(levelSelectSceneName))
            {
                SceneTransitionManager.Instance.LoadSceneWithTransition(levelSelectSceneName, transitionType);
            }
        }
        
        /// <summary>
        /// Restarts the current level
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f; // Ensure time scale is reset before loading
            isPaused = false;
            
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneTransitionManager.Instance.LoadSceneWithTransition(currentSceneName, transitionType);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}