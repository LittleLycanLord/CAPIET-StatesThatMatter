using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace LilLycanLord_Official
{
    public class LevelSelect : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private Image levelPreview;
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private TMP_Text levelDescription;
        [SerializeField] private GameObject buttonList;
        [SerializeField] private GameObject levelButtonPrefab;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Level Data")]
        [SerializeField] private List<LevelDetails> levelDetails;
        
        [Header("Settings")]
        [SerializeField] private string transitionType = "Crossfade";
        
        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Color for locked level buttons")] 
        private Color lockedButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        [SerializeField, Tooltip("Color for unlocked level buttons")] 
        private Color unlockedButtonColor = Color.white;
        
        [SerializeField, Tooltip("Color for completed level buttons")] 
        private Color completedButtonColor = new Color(0.5f, 1f, 0.5f, 1f);
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private LevelDetails lastPreviewedLevel;
        private Dictionary<LevelDetails, GameObject> levelButtonMap = new Dictionary<LevelDetails, GameObject>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start()
        {
            // Create a button for each level
            if (levelDetails != null && levelButtonPrefab != null && buttonList != null)
            {
                for (int i = 0; i < levelDetails.Count; i++)
                {
                    LevelDetails level = levelDetails[i];
                    if (level != null)
                    {
                        GameObject buttonObj = Instantiate(levelButtonPrefab, buttonList.transform);
                        levelButtonMap[level] = buttonObj;
                        
                        UIVirtualButton virtualButton = buttonObj.GetComponent<UIVirtualButton>();
                        
                        if (virtualButton != null)
                        {
                            // Add click listener for this specific level
                            virtualButton.buttonClickOutputEvent.AddListener(() => OnLevelButtonClicked(level));
                        }
                        
                        // Set the button text to "01", "02", "03", etc.
                        Transform visualsTransform = buttonObj.transform.Find("Visuals");
                        if (visualsTransform != null)
                        {
                            Transform textTransform = visualsTransform.Find("Text (TMP)");
                            if (textTransform != null)
                            {
                                TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
                                if (textComponent != null)
                                {
                                    textComponent.text = (i + 1).ToString("00");
                                }
                            }
                        }
                        
                        // Update button state based on progress
                        UpdateButtonState(level, buttonObj);
                    }
                }
                
                // Preview the first level by default
                LevelDetails defaultPreview = levelDetails.Count > 0 ? levelDetails[0] : null;
                
                if (defaultPreview != null)
                {
                    PreviewLevel(defaultPreview);
                    lastPreviewedLevel = defaultPreview;
                }
            }
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Loads the currently previewed level — equivalent to pressing the level button a second time
        /// </summary>
        public void LoadSelectedLevel()
        {
            if (lastPreviewedLevel == null || !IsLevelUnlocked(lastPreviewedLevel)) return;
            if (!string.IsNullOrEmpty(lastPreviewedLevel.sceneName))
            {
                SceneTransitionManager.Instance.LoadSceneWithTransition(lastPreviewedLevel.sceneName, transitionType);
            }
        }

        /// <summary>
        /// Handles level button clicks - first click previews, second click loads the level
        /// </summary>
        private void OnLevelButtonClicked(LevelDetails level)
        {
            if (level == null) return;
            
            // Check if level is unlocked
            if (!IsLevelUnlocked(level))
            {
                Debug.Log($"[LevelSelect] Level {level.levelIndex} ({level.levelName}) is locked");
                return;
            }
            
            // Check if this is the same level that was just previewed
            if (lastPreviewedLevel == level)
            {
                // Second tap - load the scene
                if (!string.IsNullOrEmpty(level.sceneName))
                {
                    SceneTransitionManager.Instance.LoadSceneWithTransition(level.sceneName, transitionType);
                }
            }
            else
            {
                // First tap - preview the level
                PreviewLevel(level);
                lastPreviewedLevel = level;
            }
        }
        
        /// <summary>
        /// Checks if a level is unlocked
        /// </summary>
        private bool IsLevelUnlocked(LevelDetails level)
        {
            if (level == null || ProgressManager.Instance == null) return false;
            return ProgressManager.Instance.IsLevelUnlocked(level.levelIndex);
        }
        
        /// <summary>
        /// Checks if a level is completed
        /// </summary>
        private bool IsLevelCompleted(LevelDetails level)
        {
            if (level == null || ProgressManager.Instance == null) return false;
            return ProgressManager.Instance.IsLevelCompleted(level.levelIndex);
        }
        
        /// <summary>
        /// Updates the visual state of a level button based on unlock/completion status
        /// </summary>
        private void UpdateButtonState(LevelDetails level, GameObject buttonObj)
        {
            if (level == null || buttonObj == null) return;
            
            bool isUnlocked = IsLevelUnlocked(level);
            bool isCompleted = IsLevelCompleted(level);
            
            // Get the button component
            UIVirtualButton virtualButton = buttonObj.GetComponent<UIVirtualButton>();
            Button button = buttonObj.GetComponent<Button>();
            if (virtualButton != null)
            {
                // Enable/disable based on unlock status
                virtualButton.enabled = isUnlocked;
                button.interactable = isUnlocked;
            }
            
            // Get the visuals to update color
            Transform visualsTransform = buttonObj.transform.Find("Visuals");
            if (visualsTransform != null)
            {
                Image visualsImage = visualsTransform.GetComponent<Image>();
                if (visualsImage != null)
                {
                    // Set color based on state
                    if (!isUnlocked)
                    {
                        visualsImage.color = lockedButtonColor;
                    }
                    else if (isCompleted)
                    {
                        visualsImage.color = completedButtonColor;
                    }
                    else
                    {
                        visualsImage.color = unlockedButtonColor;
                    }
                }
                
                // Optionally dim the text for locked levels
                Transform textTransform = visualsTransform.Find("Text (TMP)");
                if (textTransform != null)
                {
                    TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
                    if (textComponent != null)
                    {
                        textComponent.alpha = isUnlocked ? 1f : 0.5f;
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the UI to preview a level
        /// </summary>
        private void PreviewLevel(LevelDetails level)
        {
            if (level == null) return;
            
            // Update level preview image
            if (levelPreview != null && level.levelPreview != null)
            {
                Sprite sprite = Sprite.Create(
                    level.levelPreview,
                    new Rect(0, 0, level.levelPreview.width, level.levelPreview.height),
                    new Vector2(0.5f, 0.5f)
                );
                levelPreview.sprite = sprite;
            }
            
            // Update level name with double quotes
            if (levelName != null)
            {
                levelName.text = $"\"{level.levelName}\"";
            }
            
            // Update level description with double quotes
            if (levelDescription != null)
            {
                levelDescription.text = $"\"{level.description}\"";
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}