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
        LevelDetails nextLevel;
        [Header("Settings")]
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
                Debug.LogWarning("[NextLevelBehaviour] Next level details not set!");
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