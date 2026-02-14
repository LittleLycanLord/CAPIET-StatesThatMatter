using UnityEngine;

namespace LilLycanLord_Official
{
    [CreateAssetMenu(fileName = "New Level Details", menuName = "States That Matter/Level Details")]
    public class LevelDetails : ScriptableObject
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
        [Header("Level Information")]
        [Tooltip("Level index/number (1-based, used for progress tracking)")]
        public int levelIndex = 1;
        
        [Tooltip("Visual preview/thumbnail of the level")]
        public Texture2D levelPreview;
        
        [Tooltip("Display name of the level")]
        public string levelName;
        
        [TextArea(3, 5)]
        [Tooltip("Description of the level")]
        public string description;
        
        [Tooltip("Scene name as it appears in Build Settings")]
        public string sceneName;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}