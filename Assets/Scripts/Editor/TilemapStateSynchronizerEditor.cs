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
            
            // Add help box
            EditorGUILayout.HelpBox(
                "This will copy tile positions across all three tilemaps. Any position that has a tile in one tilemap will get tiles placed in the other two tilemaps using the assigned tile references.",
                MessageType.Info
            );
        }
    }
}
