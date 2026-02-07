using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private LevelDetails lastPreviewedLevel;

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
                    }
                }
                
                // Preview the first level by default
                if (levelDetails.Count > 0 && levelDetails[0] != null)
                {
                    PreviewLevel(levelDetails[0]);
                    lastPreviewedLevel = levelDetails[0];
                }
            }
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Handles level button clicks - first click previews, second click loads the level
        /// </summary>
        private void OnLevelButtonClicked(LevelDetails level)
        {
            if (level == null) return;
            
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