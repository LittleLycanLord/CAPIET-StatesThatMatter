using UnityEngine;

namespace LilLycanLord_Official
{
    public class MainMenuAndLevelSelect : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject levelSelect;
        [SerializeField] private GameObject portal;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Animation Settings")]
        [SerializeField] private float growDuration = 0.5f;
        [SerializeField] private float shrinkDuration = 0.3f;
        [SerializeField] private float slideDuration = 0.4f;
        [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private Vector3 levelSelectStartPos;
        private Vector3 portalStartPos;

        private Vector3 mainMenuStartScale;
        private float levelSelectOffScreenY;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            // Store initial positions and scale
            if (mainMenu != null)
            {
                mainMenuStartScale = mainMenu.transform.localScale;
            }
            
            if (levelSelect != null)
            {
                levelSelectStartPos = levelSelect.transform.localPosition;
            }
            
            if (portal != null)
            {
                portalStartPos = portal.transform.localPosition;
            }
        }

        void Start() 
        {
            // Animate mainMenu growing from zero scale
            if (mainMenu != null)
            {
                mainMenu.transform.localScale = Vector3.zero;
                LeanTween.scale(mainMenu, mainMenuStartScale, growDuration)
                    .setEase(easeType);
            }
            
            // Position level select off-screen at bottom initially
            if (levelSelect != null)
            {
                Canvas.ForceUpdateCanvases();
                RectTransform rt = levelSelect.GetComponent<RectTransform>();
                if (rt != null)
                {
                    levelSelectOffScreenY = -rt.rect.height;
                    Vector3 offScreenPos = levelSelectStartPos;
                    offScreenPos.y = levelSelectOffScreenY;
                    levelSelect.transform.localPosition = offScreenPos;
                }
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Transition from main menu to level select screen
        /// </summary>
        public void GoToLevelSelect()
        {
            // Shrink mainMenu to zero scale
            if (mainMenu != null)
            {
                LeanTween.scale(mainMenu, Vector3.zero, shrinkDuration)
                    .setEase(LeanTweenType.easeInBack);
            }
            
            // Slide portal to the right (off screen)
            if (portal != null)
            {
                Vector3 offScreenRight = portalStartPos;
                offScreenRight.x = Screen.width;
                LeanTween.moveLocal(portal, offScreenRight, slideDuration)
                    .setEase(easeType)
                    .setDelay(shrinkDuration);
            }
            
            // Slide level select up from bottom
            if (levelSelect != null)
            {
                LeanTween.moveLocal(levelSelect, levelSelectStartPos, slideDuration)
                    .setEase(easeType)
                    .setDelay(shrinkDuration);
            }
        }
        
        /// <summary>
        /// Transition from level select back to main menu
        /// </summary>
        public void GoToMainMenu()
        {
            // Slide level select down off screen
            if (levelSelect != null)
            {
                Vector3 offScreenBottom = levelSelectStartPos;
                offScreenBottom.y = levelSelectOffScreenY;
                LeanTween.moveLocal(levelSelect, offScreenBottom, slideDuration)
                    .setEase(LeanTweenType.easeInBack);
            }
            
            // Slide portal back to original position
            if (portal != null)
            {
                LeanTween.moveLocal(portal, portalStartPos, slideDuration)
                    .setEase(easeType);
            }
            
            // Grow mainMenu back to original scale
            if (mainMenu != null)
            {
                LeanTween.scale(mainMenu, mainMenuStartScale, growDuration)
                    .setEase(easeType)
                    .setDelay(slideDuration);
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}