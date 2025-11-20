using LilLycanLord_Official;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LilLycanLord_Official
{public class EditorUtilitiesDemo : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Clear Debug Log Demo")]
        [SerializeField] private bool clearOnStart = false;
        [SerializeField] private bool enableAutoClear = false;
        
        [Header("Colored Hierarchy Demo")]
        [SerializeField] private string[] availableColors;

        void Start()
        {
            // Clear console demo
            if (clearOnStart)
            {
                ClearDebugLog.ClearWithMessage("EditorUtilitiesDemo started");
            }

            // Show available colors
            availableColors = ColoredHierarchy.GetAvailableColors();
            
            // Auto-clear demo
            if (enableAutoClear)
            {
                ClearDebugLog.ClearOnPlayModeChange();
                ClearDebugLog.ClearIfErrorsExceed(10);
            }

            // Log usage examples
            Debug.Log("<color=cyan>[Editor Utilities Demo]</color> Enhanced editor utilities loaded!");
            Debug.Log("<color=yellow>Colored Hierarchy Usage:</color>");
            Debug.Log("• Predefined colors: /color.Red ObjectName, /color.Blue ObjectName, etc.");
            Debug.Log("• Hex colors: /color.#FF5733 ObjectName, /color.#3498DB ObjectName");
            Debug.Log("• Available predefined colors: " + string.Join(", ", availableColors));
            
            Debug.Log("<color=green>Clear Debug Log Usage:</color>");
            Debug.Log("• ClearDebugLog.Clear() - Simple clear");
            Debug.Log("• ClearDebugLog.ClearWithMessage() - Clear with message");
            Debug.Log("• ClearDebugLog.ClearIfErrorsExceed(threshold) - Conditional clear");

            // Test hex color validation
            TestHexColorValidation();
        }

        private void TestHexColorValidation()
        {
            string[] testColors = { "#FF5733", "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "InvalidColor", "#XYZ" };
            
            Debug.Log("<color=orange>Hex Color Validation Test:</color>");
            foreach (string color in testColors)
            {
                bool isValid = ColoredHierarchy.IsValidColorIdentifier(color);
                Debug.Log($"Color '{color}': {(isValid ? "✓ Valid" : "✗ Invalid")}");
            }
        }

        [ContextMenu("Clear Console")]
        public void ClearConsole()
        {
            ClearDebugLog.ClearWithMessage("Manual clear from context menu");
        }

        [ContextMenu("Generate Test Colored Names")]
        public void GenerateTestColoredNames()
        {
            string[] testNames = { "Player", "Enemy", "Collectible", "Platform" };
            string[] testColors = { "Red", "Blue", "#FF5733", "#2ECC71" };

            Debug.Log("<color=magenta>Test Colored Names:</color>");
            foreach (string name in testNames)
            {
                foreach (string color in testColors)
                {
                    string coloredName = ColoredHierarchy.FormatColoredName(color, name);
                    Debug.Log($"Formatted: '{coloredName}'");
                }
            }
        }
    }
}
