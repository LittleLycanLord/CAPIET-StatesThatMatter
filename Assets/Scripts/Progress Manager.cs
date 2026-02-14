using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class ProgressManager : MonoBehaviour
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
        // [Space(10)]
        // [Header("Fields")]
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        public static ProgressManager Instance;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Singleton pattern
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[ProgressManager] Initialized and set to DontDestroyOnLoad");
        }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Marks a level as completed and saves to PlayerPrefs
        /// </summary>
        /// <param name="levelIndex">The index of the level to mark as completed (1-based)</param>
        public void CompleteLevel(int levelIndex)
        {
            PlayerPrefs.SetInt("Level_" + levelIndex, 1);
            PlayerPrefs.Save();
            Debug.Log($"[ProgressManager] Level {levelIndex} marked as completed");
        }
        
        /// <summary>
        /// Checks if a level has been completed
        /// </summary>
        /// <param name="levelIndex">The index of the level to check (1-based)</param>
        /// <returns>True if the level has been completed, false otherwise</returns>
        public bool IsLevelCompleted(int levelIndex)
        {
            bool isCompleted = PlayerPrefs.GetInt("Level_" + levelIndex, 0) == 1;
            return isCompleted;
        }
        
        /// <summary>
        /// Checks if a level is unlocked (level 1 is always unlocked, others require previous level completion)
        /// </summary>
        /// <param name="levelIndex">The index of the level to check (1-based)</param>
        /// <returns>True if the level is unlocked, false otherwise</returns>
        public bool IsLevelUnlocked(int levelIndex)
        {
            // Level 1 is always unlocked
            if (levelIndex == 1) return true;
            
            // Other levels require the previous level to be completed
            return IsLevelCompleted(levelIndex - 1);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}