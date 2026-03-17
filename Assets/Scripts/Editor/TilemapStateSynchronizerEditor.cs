using UnityEngine;
using UnityEditor;
using LilLycanLord_Official;

namespace LilLycanLord_Official
{
    [CustomEditor(typeof(TilemapStateSynchronizer))]
    public class TilemapStateSynchronizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            // Add space
            EditorGUILayout.Space(10);
            
            // Get reference to the target script
            TilemapStateSynchronizer synchronizer = (TilemapStateSynchronizer)target;
            
            // Add the sync button
            if (GUILayout.Button("Sync Tilemaps", GUILayout.Height(30)))
            {
                synchronizer.SynchronizeTilemaps();

                // Mark scene as dirty so Unity knows to save changes
                EditorUtility.SetDirty(synchronizer);
            }

            // Re-enable drawing tilemaps for viewing after sync
            if (GUILayout.Button("Re-enable Drawing Tilemaps", GUILayout.Height(30)))
            {
                synchronizer.EnableDrawingTilemaps();
                EditorUtility.SetDirty(synchronizer);
            }

            // Reset all tilemaps
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Reset Tilemaps", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Tilemaps",
                    "This will clear ALL tiles from every drawing and sprite reference tilemap. This cannot be undone. Are you sure?",
                    "Reset", "Cancel"))
                {
                    synchronizer.ResetTilemaps();
                    EditorUtility.SetDirty(synchronizer);
                }
            }
            GUI.backgroundColor = Color.white;
            
            // Add help box
            EditorGUILayout.HelpBox(
                "This will copy tile positions across all three tilemaps. Any position that has a tile in one tilemap will get tiles placed in the other two tilemaps using the assigned tile references.",
                MessageType.Info
            );
        }
    }
}
