using UnityEngine;

namespace LilLycanLord_Official
{
    public class MainMenuAndLevelSelect : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Components")]
        [SerializeField] private UnityEngine.UI.CanvasScaler mainMenu;
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
        
        private float mainMenuStartScaleFactor;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() 
        {
            // Store initial positions and scale factor
            if (mainMenu != null)
            {
                mainMenuStartScaleFactor = mainMenu.scaleFactor;
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
            // Animate mainMenu growing from zero scale factor
            if (mainMenu != null)
            {
                mainMenu.scaleFactor = 0f;
                LeanTween.value(mainMenu.gameObject, 0f, mainMenuStartScaleFactor, growDuration)
                    .setEase(easeType)
                    .setOnUpdate((float val) => {
                        if (mainMenu != null) mainMenu.scaleFactor = val;
                    });
            }
            
            // Position level select off-screen at bottom initially
            if (levelSelect != null)
            {
                RectTransform rt = levelSelect.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector3 offScreenPos = levelSelectStartPos;
                    offScreenPos.y = -Screen.height;
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
            // Shrink mainMenu by animating scaleFactor to zero
            if (mainMenu != null)
            {
                LeanTween.value(mainMenu.gameObject, mainMenu.scaleFactor, 0f, shrinkDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnUpdate((float val) => {
                        if (mainMenu != null) mainMenu.scaleFactor = val;
                    });
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
                offScreenBottom.y = -Screen.height;
                LeanTween.moveLocal(levelSelect, offScreenBottom, slideDuration)
                    .setEase(LeanTweenType.easeInBack);
            }
            
            // Slide portal back to original position
            if (portal != null)
            {
                LeanTween.moveLocal(portal, portalStartPos, slideDuration)
                    .setEase(easeType);
            }
            
            // Grow mainMenu back by animating scaleFactor
            if (mainMenu != null)
            {
                LeanTween.value(mainMenu.gameObject, 0f, mainMenuStartScaleFactor, growDuration)
                    .setEase(easeType)
                    .setDelay(slideDuration)
                    .setOnUpdate((float val) => {
                        if (mainMenu != null) mainMenu.scaleFactor = val;
                    });
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}