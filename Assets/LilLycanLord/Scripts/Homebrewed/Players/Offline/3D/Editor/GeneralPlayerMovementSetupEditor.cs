using LilLycanLord_Official;
using UnityEditor;
using UnityEngine;

namespace LilLycanLord_Official.Editor
{
    /// <summary>
    /// Custom editor for GeneralPlayerMovementSetup with automatic component addition
    /// </summary>
    [CustomEditor(typeof(GeneralPlayerMovementSetup))]
    public class GeneralPlayerMovementSetupEditor : UnityEditor.Editor
    {
        private GeneralPlayerMovementSetup setup;

        void OnEnable()
        {
            setup = (GeneralPlayerMovementSetup)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Setup Actions", EditorStyles.boldLabel);

            // Add Components Button
            if (GUILayout.Button("Add All Movement Components", GUILayout.Height(30)))
            {
                AddAllMovementComponents();
            }

            EditorGUILayout.Space(5);

            // Validate Components Button
            if (GUILayout.Button("Validate Components"))
            {
                ValidateComponents();
            }

            EditorGUILayout.Space(5);

            // Remove Components Button
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove All Movement Components"))
            {
                if (
                    EditorUtility.DisplayDialog(
                        "Remove Components",
                        "Are you sure you want to remove all movement components?",
                        "Yes",
                        "Cancel"
                    )
                )
                {
                    RemoveAllMovementComponents();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Add all required movement components to the GameObject
        /// </summary>
        private void AddAllMovementComponents()
        {
            GameObject targetObject = setup.gameObject;
            bool componentsAdded = false;

            // Record the object for undo
            Undo.RecordObject(targetObject, "Add Movement Components");

            // Add CharacterController if missing
            if (targetObject.GetComponent<CharacterController>() == null)
            {
                Undo.AddComponent<CharacterController>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added CharacterController");
            }

            // Add General3DMovementCore if missing
            if (targetObject.GetComponent<General3DMovementCore>() == null)
            {
                Undo.AddComponent<General3DMovementCore>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added General3DMovementCore");
            }

            // Add General3DJumpSystem if missing
            if (targetObject.GetComponent<General3DJumpSystem>() == null)
            {
                Undo.AddComponent<General3DJumpSystem>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added General3DJumpSystem");
            }

            // Add GeneralPlayerInputHandler if missing
            if (targetObject.GetComponent<GeneralPlayerInputHandler>() == null)
            {
                Undo.AddComponent<GeneralPlayerInputHandler>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added GeneralPlayerInputHandler");
            }

            // Add GeneralPlayerController if missing
            if (targetObject.GetComponent<GeneralPlayerController>() == null)
            {
                Undo.AddComponent<GeneralPlayerController>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added GeneralPlayerController");
            }

            // Add General3DMovementAnimator if missing (optional)
            if (
                targetObject.GetComponent<Animator>() != null
                && targetObject.GetComponent<General3DMovementAnimator>() == null
            )
            {
                Undo.AddComponent<General3DMovementAnimator>(targetObject);
                componentsAdded = true;
                Debug.Log("✅ Added General3DMovementAnimator (Animator detected)");
            }

            if (componentsAdded)
            {
                EditorUtility.SetDirty(targetObject);
                Debug.Log("🎮 All movement components added successfully!");

                // Refresh the setup component's cached references
                setup.RefreshComponentCache();
            }
            else
            {
                Debug.Log("ℹ️ All required components are already present.");
            }
        }

        /// <summary>
        /// Validate that all required components are present
        /// </summary>
        private void ValidateComponents()
        {
            GameObject targetObject = setup.gameObject;
            bool allValid = true;

            Debug.Log("🔍 Validating Movement Components...");

            // Check CharacterController
            if (targetObject.GetComponent<CharacterController>() == null)
            {
                Debug.LogError("❌ CharacterController missing!");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ CharacterController found");
            }

            // Check General3DMovementCore
            if (targetObject.GetComponent<General3DMovementCore>() == null)
            {
                Debug.LogError("❌ General3DMovementCore missing!");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ General3DMovementCore found");
            }

            // Check General3DJumpSystem
            if (targetObject.GetComponent<General3DJumpSystem>() == null)
            {
                Debug.LogError("❌ General3DJumpSystem missing!");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ General3DJumpSystem found");
            }

            // Check GeneralPlayerInputHandler
            if (targetObject.GetComponent<GeneralPlayerInputHandler>() == null)
            {
                Debug.LogError("❌ GeneralPlayerInputHandler missing!");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ GeneralPlayerInputHandler found");
            }

            // Check GeneralPlayerController
            if (targetObject.GetComponent<GeneralPlayerController>() == null)
            {
                Debug.LogError("❌ GeneralPlayerController missing!");
                allValid = false;
            }
            else
            {
                Debug.Log("✅ GeneralPlayerController found");
            }

            // Check optional components
            if (targetObject.GetComponent<Animator>() != null)
            {
                if (targetObject.GetComponent<General3DMovementAnimator>() == null)
                {
                    Debug.LogWarning(
                        "⚠️ Animator found but General3DMovementAnimator missing (optional)"
                    );
                }
                else
                {
                    Debug.Log("✅ General3DMovementAnimator found");
                }
            }

            if (allValid)
            {
                Debug.Log("🎉 All required movement components are present!");
            }
            else
            {
                Debug.LogWarning(
                    "⚠️ Some components are missing. Use 'Add All Movement Components' to fix."
                );
            }
        }

        /// <summary>
        /// Remove all movement components from the GameObject
        /// </summary>
        private void RemoveAllMovementComponents()
        {
            GameObject targetObject = setup.gameObject;
            bool componentsRemoved = false;

            // Record the object for undo
            Undo.RecordObject(targetObject, "Remove Movement Components");

            // Remove components in reverse dependency order
            var animatorComp = targetObject.GetComponent<General3DMovementAnimator>();
            if (animatorComp != null)
            {
                Undo.DestroyObjectImmediate(animatorComp);
                componentsRemoved = true;
                Debug.Log("🗑️ Removed General3DMovementAnimator");
            }

            var controllerComp = targetObject.GetComponent<GeneralPlayerController>();
            if (controllerComp != null)
            {
                Undo.DestroyObjectImmediate(controllerComp);
                componentsRemoved = true;
                Debug.Log("🗑️ Removed GeneralPlayerController");
            }

            var inputComp = targetObject.GetComponent<GeneralPlayerInputHandler>();
            if (inputComp != null)
            {
                Undo.DestroyObjectImmediate(inputComp);
                componentsRemoved = true;
                Debug.Log("🗑️ Removed GeneralPlayerInputHandler");
            }

            var jumpComp = targetObject.GetComponent<General3DJumpSystem>();
            if (jumpComp != null)
            {
                Undo.DestroyObjectImmediate(jumpComp);
                componentsRemoved = true;
                Debug.Log("🗑️ Removed General3DJumpSystem");
            }

            var coreComp = targetObject.GetComponent<General3DMovementCore>();
            if (coreComp != null)
            {
                Undo.DestroyObjectImmediate(coreComp);
                componentsRemoved = true;
                Debug.Log("🗑️ Removed General3DMovementCore");
            }

            // Note: We don't remove CharacterController as it might be used by other systems

            if (componentsRemoved)
            {
                EditorUtility.SetDirty(targetObject);
                Debug.Log("🗑️ Movement components removed successfully!");
            }
            else
            {
                Debug.Log("ℹ️ No movement components found to remove.");
            }
        }
    }

    /// <summary>
    /// Context menu items for adding movement setup to GameObjects
    /// </summary>
    public static class MovementSetupContextMenu
    {
        [MenuItem("GameObject/LilLycanLord/Add Movement Setup", false, 10)]
        static void AddMovementSetup(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            if (go == null)
            {
                // Create new GameObject if none selected
                go = new GameObject("Player");
                GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);
                Undo.RegisterCreatedObjectUndo(go, "Create Player GameObject");
                Selection.activeObject = go;
            }

            // Add the setup component
            if (go.GetComponent<GeneralPlayerMovementSetup>() == null)
            {
                Undo.AddComponent<GeneralPlayerMovementSetup>(go);
                Debug.Log("✅ Added GeneralPlayerMovementSetup to " + go.name);
            }
            else
            {
                Debug.Log("ℹ️ GeneralPlayerMovementSetup already exists on " + go.name);
            }
        }

        [MenuItem("GameObject/LilLycanLord/Complete Movement Player", false, 11)]
        static void CreateCompleteMovementPlayer(MenuCommand menuCommand)
        {
            // Create new GameObject
            GameObject go = new GameObject("Complete Movement Player");
            GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Complete Movement Player");

            // Add all components in the correct order
            Undo.AddComponent<CharacterController>(go);
            Undo.AddComponent<General3DMovementCore>(go);
            Undo.AddComponent<General3DJumpSystem>(go);
            Undo.AddComponent<GeneralPlayerInputHandler>(go);
            Undo.AddComponent<GeneralPlayerController>(go);
            Undo.AddComponent<GeneralPlayerMovementSetup>(go);

            // Configure CharacterController defaults
            var characterController = go.GetComponent<CharacterController>();
            characterController.center = new Vector3(0, 1, 0);
            characterController.height = 2f;
            characterController.radius = 0.5f;

            // Set the setup to auto-apply 3D Platformer recipe
            var setup = go.GetComponent<GeneralPlayerMovementSetup>();
            var setupSO = new SerializedObject(setup);
            setupSO.FindProperty("selectedRecipe").enumValueIndex = 2; // Platformer3D
            setupSO.FindProperty("autoApplyOnStart").boolValue = true;
            setupSO.ApplyModifiedProperties();

            Selection.activeObject = go;

            Debug.Log("🎮 Created complete movement player with all components!");
            Debug.Log("📝 Set to auto-apply 3D Platformer recipe on Start");
        }

        // Validation methods for context menu items
        [MenuItem("GameObject/LilLycanLord/Add Movement Setup", true)]
        static bool ValidateAddMovementSetup()
        {
            return Selection.activeGameObject != null;
        }
    }
}
