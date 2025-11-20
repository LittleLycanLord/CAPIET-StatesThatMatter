using System;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Sample script demonstrating GameProgressionManager usage with context menu functions
    /// </summary>
    public class GameProgressionSaverSample : MonoBehaviour
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
        [Header("Sample Data")]
        [SerializeField, Tooltip("Player level to save/load")]
        private int playerLevel = 1;

        [SerializeField, Tooltip("Player name to save/load")]
        private string playerName = "DefaultPlayer";

        [SerializeField, Tooltip("Player health to save/load")]
        private float playerHealth = 100f;

        [SerializeField, Tooltip("Whether player has completed tutorial")]
        private bool hasCompletedTutorial = false;

        [Header("Object References")]
        [SerializeField, Tooltip("Transform to save/load position data")]
        private Transform targetTransform;

        [SerializeField, Tooltip("GameObject to save/load state")]
        private GameObject targetGameObject;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Set default target transform to this transform if not assigned
            if (targetTransform == null)
                targetTransform = transform;

            // Set default target GameObject to this GameObject if not assigned
            if (targetGameObject == null)
                targetGameObject = gameObject;
        }

        void Start()
        {
            // Automatically load progression on start
            LoadProgressionData();
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        #region Context Menu Functions

        [ContextMenu("💾 Save All Sample Progression")]
        private void SaveAllSampleProgression()
        {
            Debug.Log("=== Saving Sample Progression Data ===");

            // Save individual primitive values
            GameProgressionManager.Instance.SaveProgression("sample_player_level", playerLevel);
            GameProgressionManager.Instance.SaveProgression("sample_player_name", playerName);
            GameProgressionManager.Instance.SaveProgression("sample_player_health", playerHealth);
            GameProgressionManager.Instance.SaveProgression(
                "sample_has_completed_tutorial",
                hasCompletedTutorial
            );

            // Save transform data using primitives
            if (targetTransform != null)
            {
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_position",
                    targetTransform.position
                );
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_rotation",
                    targetTransform.rotation
                );
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_scale",
                    targetTransform.localScale
                );
                Debug.Log($"Saved transform position: {targetTransform.position}");
            }

            // Save GameObject data using primitives
            if (targetGameObject != null)
            {
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_active",
                    targetGameObject.activeInHierarchy
                );
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_go_position",
                    targetGameObject.transform.position
                );
                GameProgressionManager.Instance.SaveProgression(
                    "sample_target_go_rotation",
                    targetGameObject.transform.rotation
                );
                Debug.Log($"Saved GameObject: {targetGameObject.name}");
            }

            // Save timestamp
            GameProgressionManager.Instance.SaveProgression(
                "sample_last_save_time",
                DateTime.Now.ToBinary()
            );

            Debug.Log("✅ Sample progression data saved successfully!");

            // Log all progression for debugging
            GameProgressionManager.Instance.LogAllProgression();
        }

        [ContextMenu("📁 Load All Sample Progression")]
        private void LoadAllSampleProgression()
        {
            Debug.Log("=== Loading Sample Progression Data ===");

            // Load individual primitive values
            playerLevel = GameProgressionManager.Instance.GetProgression<int>(
                "sample_player_level"
            );
            playerName = GameProgressionManager.Instance.GetProgression<string>(
                "sample_player_name"
            );
            playerHealth = GameProgressionManager.Instance.GetProgression<float>(
                "sample_player_health"
            );
            hasCompletedTutorial = GameProgressionManager.Instance.GetProgression<bool>(
                "sample_has_completed_tutorial"
            );

            // Load transform data using primitives
            if (GameProgressionManager.Instance.HasProgression("sample_target_position"))
            {
                Vector3 savedPosition = GameProgressionManager.Instance.GetProgression<Vector3>(
                    "sample_target_position"
                );
                Quaternion savedRotation =
                    GameProgressionManager.Instance.GetProgression<Quaternion>(
                        "sample_target_rotation"
                    );
                Vector3 savedScale = GameProgressionManager.Instance.GetProgression<Vector3>(
                    "sample_target_scale"
                );

                if (targetTransform != null)
                {
                    targetTransform.position = savedPosition;
                    targetTransform.rotation = savedRotation;
                    targetTransform.localScale = savedScale;
                    Debug.Log($"Restored transform position: {savedPosition}");
                }
            }

            // Load GameObject data using primitives
            if (GameProgressionManager.Instance.HasProgression("sample_target_active"))
            {
                bool savedActive = GameProgressionManager.Instance.GetProgression<bool>(
                    "sample_target_active"
                );
                Vector3 savedGoPosition = GameProgressionManager.Instance.GetProgression<Vector3>(
                    "sample_target_go_position"
                );
                Quaternion savedGoRotation =
                    GameProgressionManager.Instance.GetProgression<Quaternion>(
                        "sample_target_go_rotation"
                    );

                if (targetGameObject != null)
                {
                    targetGameObject.SetActive(savedActive);
                    targetGameObject.transform.position = savedGoPosition;
                    targetGameObject.transform.rotation = savedGoRotation;
                    Debug.Log($"Restored GameObject active state: {savedActive}");
                }
            }

            // Load timestamp
            if (GameProgressionManager.Instance.HasProgression("sample_last_save_time"))
            {
                long savedTimeBinary = GameProgressionManager.Instance.GetProgression<long>(
                    "sample_last_save_time"
                );
                DateTime savedTime = DateTime.FromBinary(savedTimeBinary);
                Debug.Log($"Last save time: {savedTime}");
            }

            Debug.Log("✅ Sample progression data loaded successfully!");
        }

        [ContextMenu("💾 Save to Custom File")]
        private void SaveToCustomFile()
        {
            SaveAllSampleProgression();

            string customFileName = $"SampleProgression_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            bool success = GameProgressionManager.Instance.SaveAllProgression(customFileName);

            if (success)
            {
                Debug.Log($"✅ Saved progression to custom file: {customFileName}");
            }
            else
            {
                Debug.LogError($"❌ Failed to save progression to custom file: {customFileName}");
            }
        }

        [ContextMenu("� Save with Timestamp")]
        private void SaveWithTimestamp()
        {
            SaveAllSampleProgression();

            // Temporarily enable timestamp to demonstrate the feature
            Debug.Log("💡 Saving with timestamp enabled - creates unique file each time!");
            Debug.Log("💡 Enable 'Append Timestamp To File Name' in Inspector for automatic timestamping!");
            
            bool success = GameProgressionManager.Instance.SaveAllProgression();

            if (success)
            {
                Debug.Log("✅ Saved with timestamp! Check your save directory for the timestamped file.");
                GameProgressionManager.Instance.ShowSaveFileInfo();
            }
            else
            {
                Debug.LogError("❌ Failed to save with timestamp");
            }
        }

        [ContextMenu("�📁 Load from Default File")]
        private void LoadFromDefaultFile()
        {
            bool success = GameProgressionManager.Instance.LoadAllProgression();

            if (success)
            {
                LoadAllSampleProgression();
                Debug.Log("✅ Loaded progression from default file and applied to sample data!");
            }
            else
            {
                Debug.LogWarning("⚠️ No default progression file found or failed to load");
            }
        }

        [ContextMenu("🗑️ Clear All Sample Progression")]
        private void ClearAllSampleProgression()
        {
            // Remove specific sample progression entries
            GameProgressionManager.Instance.RemoveProgression("sample_player_level");
            GameProgressionManager.Instance.RemoveProgression("sample_player_name");
            GameProgressionManager.Instance.RemoveProgression("sample_player_health");
            GameProgressionManager.Instance.RemoveProgression("sample_has_completed_tutorial");

            // Remove transform primitive data
            GameProgressionManager.Instance.RemoveProgression("sample_target_position");
            GameProgressionManager.Instance.RemoveProgression("sample_target_rotation");
            GameProgressionManager.Instance.RemoveProgression("sample_target_scale");

            // Remove GameObject primitive data
            GameProgressionManager.Instance.RemoveProgression("sample_target_active");
            GameProgressionManager.Instance.RemoveProgression("sample_target_go_position");
            GameProgressionManager.Instance.RemoveProgression("sample_target_go_rotation");

            // Remove additional primitive data
            GameProgressionManager.Instance.RemoveProgression("sample_last_save_time");

            // Reset local values to defaults
            playerLevel = 1;
            playerName = "DefaultPlayer";
            playerHealth = 100f;
            hasCompletedTutorial = false;

            Debug.Log("🗑️ Cleared all sample progression data!");
        }

        [ContextMenu("📊 Show Progression Info")]
        private void ShowProgressionInfo()
        {
            Debug.Log("=== Sample Progression Info ===");
            Debug.Log(
                $"Total progression flags: {GameProgressionManager.Instance.GetProgressionCount()}"
            );

            // Check for specific sample progressions
            bool hasLevel = GameProgressionManager.Instance.HasProgression("sample_player_level");
            bool hasName = GameProgressionManager.Instance.HasProgression("sample_player_name");
            bool hasPosition = GameProgressionManager.Instance.HasProgression(
                "sample_target_position"
            );
            bool hasTimestamp = GameProgressionManager.Instance.HasProgression(
                "sample_last_save_time"
            );

            Debug.Log($"Has Level: {hasLevel}");
            Debug.Log($"Has Name: {hasName}");
            Debug.Log($"Has Position: {hasPosition}");
            Debug.Log($"Has Timestamp: {hasTimestamp}");

            // Show all progression keys
            var allKeys = GameProgressionManager.Instance.GetAllProgressionKeys();
            Debug.Log($"All progression keys: {string.Join(", ", allKeys)}");
        }

        [ContextMenu("🎲 Randomize Sample Data")]
        private void RandomizeSampleData()
        {
            // Randomize values for testing
            playerLevel = UnityEngine.Random.Range(1, 100);
            playerName = $"Player_{UnityEngine.Random.Range(1000, 9999)}";
            playerHealth = UnityEngine.Random.Range(1f, 100f);
            hasCompletedTutorial = UnityEngine.Random.value > 0.5f;

            // Randomize transform position
            if (targetTransform != null)
            {
                targetTransform.position = new Vector3(
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(0f, 5f),
                    UnityEngine.Random.Range(-10f, 10f)
                );
            }

            Debug.Log(
                $"🎲 Randomized sample data - Level: {playerLevel}, Name: {playerName}, Health: {playerHealth:F1}, Tutorial: {hasCompletedTutorial}"
            );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Load progression data and apply to inspector fields
        /// </summary>
        private void LoadProgressionData()
        {
            if (!GameProgressionManager.HasInstance)
            {
                Debug.LogWarning("GameProgressionManager instance not available yet");
                return;
            }

            // Only load if data exists
            if (GameProgressionManager.Instance.HasProgression("sample_player_level"))
            {
                LoadAllSampleProgression();
            }
        }

        /// <summary>
        /// Validate that required references are assigned
        /// </summary>
        private void ValidateReferences()
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("Target Transform is not assigned, using this transform");
                targetTransform = transform;
            }

            if (targetGameObject == null)
            {
                Debug.LogWarning("Target GameObject is not assigned, using this GameObject");
                targetGameObject = gameObject;
            }
        }

        #endregion

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝

        private void OnValidate()
        {
            // Clamp values in editor
            playerLevel = Mathf.Clamp(playerLevel, 1, 999);
            playerHealth = Mathf.Clamp(playerHealth, 0f, 1000f);

            // Validate references
            ValidateReferences();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Auto-save when application is paused/minimized
            if (pauseStatus)
            {
                SaveAllSampleProgression();
                GameProgressionManager.Instance.SaveAllProgression();
                Debug.Log("📱 Auto-saved progression due to application pause");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Auto-save when application loses focus
            if (!hasFocus)
            {
                SaveAllSampleProgression();
                GameProgressionManager.Instance.SaveAllProgression();
                Debug.Log("🔍 Auto-saved progression due to focus loss");
            }
        }

        private void OnDestroy()
        {
            // Final save when object is destroyed
            if (GameProgressionManager.HasInstance)
            {
                SaveAllSampleProgression();
                Debug.Log("💀 Final save on destroy");
            }
        }
    }
}
