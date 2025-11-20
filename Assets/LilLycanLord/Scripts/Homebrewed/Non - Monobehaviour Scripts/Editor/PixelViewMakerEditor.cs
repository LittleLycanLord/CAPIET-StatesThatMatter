using LilLycanLord_Official;
using UnityEditor;
using UnityEngine;

namespace LilLycanLord_Official.Editor
{
    /// <summary>
    /// Simple custom editor for PixelViewMaker that adds control buttons while preserving the default inspector
    /// </summary>
    [CustomEditor(typeof(PixelViewMaker))]
    public class PixelViewMakerEditor : UnityEditor.Editor
    {
        private PixelViewMaker pixelView;

        void OnEnable()
        {
            pixelView = (PixelViewMaker)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector first (shows all fields with their headers and tooltips)
            DrawDefaultInspector();

            // Add spacing before our custom buttons
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Pixel View Controls", EditorStyles.boldLabel);

            // Show current status
            if (pixelView.IsPixelViewActive)
            {
                EditorGUILayout.HelpBox("✓ Pixel View is currently ACTIVE", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("○ Pixel View is currently INACTIVE", MessageType.None);
            }

            // Main action buttons
            // Apply Permanent button
            GUI.backgroundColor = !pixelView.IsPixelViewActive ? Color.green : Color.gray;
            GUI.enabled =
                !Application.isPlaying
                && !pixelView.IsPixelViewActive
                && pixelView.TargetCamera != null;
            if (GUILayout.Button("Apply Pixel View", GUILayout.Height(30)))
            {
                pixelView.ApplyPixelView();
                EditorUtility.SetDirty(pixelView);
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            // Remove button
            GUI.backgroundColor = pixelView.IsPixelViewActive ? Color.red : Color.gray;
            GUI.enabled = !Application.isPlaying; // Allow removal during edit mode regardless of pixel view state
            if (GUILayout.Button("Remove Pixel View", GUILayout.Height(25)))
            {
                pixelView.RemovePixelView();
                EditorUtility.SetDirty(pixelView);
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            // Utility buttons when active
            if (pixelView.IsPixelViewActive)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();

                // Refresh button
                GUI.backgroundColor = Color.yellow;
                GUI.enabled = !Application.isPlaying;
                if (GUILayout.Button("Refresh Display", GUILayout.Height(25)))
                {
                    pixelView.RefreshPixelView();
                }

                // Select render texture button
                if (pixelView.CurrentRenderTexture != null)
                {
                    GUI.backgroundColor = Color.cyan;
                    GUI.enabled = !Application.isPlaying;
                    if (GUILayout.Button("Select RenderTexture", GUILayout.Height(25)))
                    {
                        Selection.activeObject = pixelView.CurrentRenderTexture;
                        EditorGUIUtility.PingObject(pixelView.CurrentRenderTexture);
                    }
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // Test raycast button
                GUI.backgroundColor = Color.cyan;
                GUI.enabled = !Application.isPlaying && pixelView.TargetCamera != null;
                if (GUILayout.Button("Test Raycast from Screen Center", GUILayout.Height(25)))
                {
                    Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                    if (pixelView.RaycastFromScreen(screenCenter, out RaycastHit hit))
                    {
                        Debug.Log(
                            $"✓ Raycast hit: {hit.collider.name} at {hit.point}",
                            hit.collider
                        );
                    }
                    else
                    {
                        Debug.Log("⚪ Raycast hit nothing from screen center");
                    }
                }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }

            // Simple validation
            EditorGUILayout.Space(10);
            if (pixelView.TargetCamera == null)
            {
                EditorGUILayout.HelpBox(
                    "Please assign a Target Camera to use Pixel View features.",
                    MessageType.Warning
                );
            }

            // Mode explanation
            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "Pixel View objects are saved in the scene permanently.",
                    MessageType.Info
                );
            }
        }

        /// <summary>
        /// Right-click context menu for quick setup
        /// </summary>
        [MenuItem("CONTEXT/PixelViewMaker/Quick Setup with Main Camera")]
        static void QuickSetupWithMainCamera(MenuCommand command)
        {
            PixelViewMaker pixelView = (PixelViewMaker)command.context;

            // Auto-assign the camera and apply permanent pixel view
            Camera mainCam = Camera.main;
            if (mainCam != null && pixelView.TargetCamera == null)
            {
                SerializedObject so = new SerializedObject(pixelView);
                so.FindProperty("targetCamera").objectReferenceValue = mainCam;
                so.ApplyModifiedProperties();

                pixelView.ApplyPixelView();
                EditorUtility.SetDirty(pixelView);

                Debug.Log(
                    "PixelViewMaker: Quick setup complete! Applied permanent pixel view with Main Camera.",
                    pixelView
                );
            }
            else if (pixelView.TargetCamera == null)
            {
                Debug.LogWarning(
                    "PixelViewMaker: No Main Camera found in scene. Please assign a camera manually.",
                    pixelView
                );
            }
            else
            {
                Debug.LogWarning(
                    "PixelViewMaker: Camera already assigned. Use the inspector buttons instead.",
                    pixelView
                );
            }
        }
    }
}
