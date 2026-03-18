using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class MiniDialogueSystem : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private RectTransform dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button progressButton;
        [SerializeField] private GameObject platformerControls;
        [SerializeField] private bool togglePlatformerControls = true;

        // Cached reference to player's platformer movement component
        private PlatformerMovement playerMovement;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Displays")]
        [SerializeField] private bool isShowing = false;
        [SerializeField] private int currentLineIndex = 0;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.4f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private LeanTweenType slideInEase = LeanTweenType.easeOutBack;
        [SerializeField] private LeanTweenType slideOutEase = LeanTweenType.easeInBack;
        
        [Space(10)]
        [Header("Position Settings")]
        [SerializeField] private float hiddenYOffset = 200f; // How far off-screen when hidden
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        public static MiniDialogueSystem Instance { get; private set; }
        
        private List<string> textLines = new List<string>();
        private Vector2 visiblePosition;
        private Vector2 hiddenPosition;
        private int currentTweenId = -1;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple MiniDialogueSystems found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            // Find the player's PlatformerMovement component
            playerMovement = FindObjectOfType<PlatformerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("MiniDialogueSystem: No PlatformerMovement component found in scene!");
            }

            if (dialoguePanel == null)
            {
                Debug.LogError("MiniDialogueSystem: Dialogue Panel not assigned!");
                return;
            }

            // Store positions
            visiblePosition = dialoguePanel.anchoredPosition;
            hiddenPosition = new Vector2(visiblePosition.x, visiblePosition.y + hiddenYOffset);

            // Start hidden
            dialoguePanel.anchoredPosition = hiddenPosition;
            dialoguePanel.gameObject.SetActive(false);

            // Setup button
            if (progressButton != null)
            {
                progressButton.onClick.AddListener(OnProgressButtonClicked);
            }
        }

        void Start() { }

        void Update() { }
        
        void OnDestroy()
        {
            // Clean up singleton reference
            if (Instance == this)
            {
                Instance = null;
            }
            
            // Cancel any ongoing tweens
            if (currentTweenId != -1)
            {
                LeanTween.cancel(currentTweenId);
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Show a single line of text
        /// </summary>
        public void ShowText(string text)
        {
            ShowText(new string[] { text });
        }
        
        /// <summary>
        /// Show multiple lines of text that can be progressed through
        /// </summary>
        public void ShowText(string[] texts)
        {
            // Cancel any ongoing animation
            if (currentTweenId != -1)
            {
                LeanTween.cancel(currentTweenId);
            }

            // Setup text lines
            textLines.Clear();

            // Stop player movement when dialogue starts
            if (togglePlatformerControls)
            {
                if (playerMovement != null)
                {
                    playerMovement.Stop();
                    playerMovement.SetInputEnabled(false);
                }

                // Also disable the platformer controls GameObject as fallback
                if (platformerControls != null)
                {
                    platformerControls.SetActive(false);
                }
            }

            textLines.AddRange(texts);
            currentLineIndex = 0;

            // Display first line
            if (textLines.Count > 0)
            {
                dialogueText.text = textLines[0];
            }

            // Show panel
            if (!isShowing)
            {
                SlideIn();
            }
        }
        
        /// <summary>
        /// Show text from a List<string>
        /// </summary>
        public void ShowText(List<string> texts)
        {
            ShowText(texts.ToArray());
        }
        
        /// <summary>
        /// Progress to next line or hide if at the end
        /// </summary>
        public void ProgressOrClose()
        {
            if (textLines.Count == 0)
            {
                HideText();
                return;
            }
            
            currentLineIndex++;
            
            if (currentLineIndex >= textLines.Count)
            {
                HideText();
            }
            else
            {
                dialogueText.text = textLines[currentLineIndex];
            }
        }
        
        /// <summary>
        /// Hide the dialogue panel
        /// </summary>
        public void HideText()
        {
            if (isShowing)
            {
                SlideOut();
            }

            textLines.Clear();
            currentLineIndex = 0;

            // Re-enable player movement when dialogue ends
            if (togglePlatformerControls)
            {
                if (playerMovement != null)
                {
                    playerMovement.SetInputEnabled(true);
                }

                // Also re-enable the platformer controls GameObject as fallback
                if (platformerControls != null)
                {
                    platformerControls.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// Slide panel in from top
        /// </summary>
        private void SlideIn()
        {
            if (dialoguePanel == null) return;
            
            isShowing = true;
            dialoguePanel.gameObject.SetActive(true);
            dialoguePanel.anchoredPosition = hiddenPosition;
            
            currentTweenId = LeanTween.value(gameObject, UpdatePanelPosition, hiddenPosition, visiblePosition, slideInDuration)
                .setEase(slideInEase)
                .setOnComplete(() => {
                    currentTweenId = -1;
                }).id;
        }
        
        /// <summary>
        /// Slide panel out to top
        /// </summary>
        private void SlideOut()
        {
            if (dialoguePanel == null) return;
            
            isShowing = false;
            
            currentTweenId = LeanTween.value(gameObject, UpdatePanelPosition, dialoguePanel.anchoredPosition, hiddenPosition, slideOutDuration)
                .setEase(slideOutEase)
                .setOnComplete(() => {
                    dialoguePanel.gameObject.SetActive(false);
                    currentTweenId = -1;
                }).id;
        }
        
        /// <summary>
        /// Update panel position during tween
        /// </summary>
        private void UpdatePanelPosition(Vector2 position)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.anchoredPosition = position;
            }
        }
        
        /// <summary>
        /// Button click handler
        /// </summary>
        private void OnProgressButtonClicked()
        {
            ProgressOrClose();
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}