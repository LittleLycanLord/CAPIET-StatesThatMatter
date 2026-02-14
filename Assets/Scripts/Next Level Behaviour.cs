using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class NextLevelBehaviour : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        [SerializeField] LevelDetails nextLevel;
        [Header("Settings")]
         [Header("Scene Settings")]
        [SerializeField] private string levelSelectSceneName = "MAIN_MENU";
        [SerializeField] private string transitionType = "Crossfade";
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        public void GoToNextLevel()
        {
            if (nextLevel == null)
            {                
                if (!string.IsNullOrEmpty(levelSelectSceneName))
                {
                    SceneTransitionManager.Instance.LoadSceneWithTransition(levelSelectSceneName, transitionType);
                }
                return;
            }

            if (string.IsNullOrEmpty(nextLevel.sceneName))
            {
                Debug.LogWarning("[NextLevelBehaviour] Next level scene name is empty!");
                return;
            }

            SceneTransitionManager.Instance.LoadSceneWithTransition(nextLevel.sceneName, transitionType);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}