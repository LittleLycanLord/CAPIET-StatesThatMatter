using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LilLycanLord_Official.Tools
{
    /// <summary>
    /// Migration tool to help convert from old string-based GameEventManager to new type-safe system.
    /// This tool scans your project for old event usage patterns and suggests modern replacements.
    /// </summary>
    public class GameEventMigrationTool
    {
        public class MigrationSuggestion
        {
            public string filePath;
            public int lineNumber;
            public string oldCode;
            public string suggestedCode;
            public string description;
        }

        /// <summary>
        /// Scan project for old event usage patterns and suggest modern alternatives.
        /// </summary>
        public static List<MigrationSuggestion> ScanForOldEventUsage()
        {
            var suggestions = new List<MigrationSuggestion>();

            var scriptPaths = Directory.GetFiles(
                Application.dataPath,
                "*.cs",
                SearchOption.AllDirectories
            );

            foreach (var scriptPath in scriptPaths)
            {
                // Skip the GameEventManager files themselves
                if (scriptPath.Contains("Game Event Manager"))
                    continue;

                var lines = File.ReadAllLines(scriptPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // Look for old string-based event usage
                    CheckForOldEventPatterns(suggestions, scriptPath, i + 1, line);
                }
            }

            return suggestions;
        }

        private static void CheckForOldEventPatterns(
            List<MigrationSuggestion> suggestions,
            string filePath,
            int lineNumber,
            string line
        )
        {
            // Pattern 1: GameEventManager.Instance.AddAction("string", method)
            var addActionPattern =
                @"GameEventManager\.Instance\.AddAction\s*\(\s*""([^""]+)""\s*,\s*(\w+)\s*\)";
            var addActionMatch = Regex.Match(line, addActionPattern);
            if (addActionMatch.Success)
            {
                string eventName = addActionMatch.Groups[1].Value;
                string methodName = addActionMatch.Groups[2].Value;

                suggestions.Add(
                    new MigrationSuggestion
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        oldCode = line.Trim(),
                        suggestedCode =
                            $"Subscribe(GameEvents.{ConvertToModernEventName(eventName)}, {methodName});",
                        description = $"Convert string-based subscription to type-safe event",
                    }
                );
            }

            // Pattern 2: GameEventManager.Instance.SendSignal("string")
            var sendSignalPattern =
                @"GameEventManager\.Instance\.SendSignal\s*\(\s*""([^""]+)""\s*\)";
            var sendSignalMatch = Regex.Match(line, sendSignalPattern);
            if (sendSignalMatch.Success)
            {
                string eventName = sendSignalMatch.Groups[1].Value;

                suggestions.Add(
                    new MigrationSuggestion
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        oldCode = line.Trim(),
                        suggestedCode =
                            $"RaiseSignal(GameEvents.{ConvertToModernEventName(eventName)});",
                        description = $"Convert string-based signal to type-safe event",
                    }
                );
            }

            // Pattern 3: MonoBehaviour inheritance that should be HasSignalsBase
            var monoBehaviourPattern = @"class\s+(\w+)\s*:\s*MonoBehaviour";
            var monoBehaviourMatch = Regex.Match(line, monoBehaviourPattern);
            if (
                monoBehaviourMatch.Success
                && (line.Contains("AddAction") || line.Contains("SendSignal"))
            )
            {
                string className = monoBehaviourMatch.Groups[1].Value;

                suggestions.Add(
                    new MigrationSuggestion
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        oldCode = line.Trim(),
                        suggestedCode = $"class {className} : HasSignalsBase",
                        description =
                            "Consider inheriting from HasSignalsBase for automatic signal lifecycle management",
                    }
                );
            }
        }

        private static string ConvertToModernEventName(string oldEventName)
        {
            // Convert old string constants to modern event names
            var conversions = new Dictionary<string, string>
            {
                { "Player_Spawned", "PlayerSpawned" },
                { "Player_Died", "PlayerDied" },
                { "Player_Health_Changed", "PlayerHealthChanged" },
                { "Player_Level_Up", "PlayerLevelUp" },
                { "Game_Started", "GameStarted" },
                { "Game_Paused", "GamePaused" },
                { "Game_Resumed", "GameResumed" },
                { "Game_Over", "GameOver" },
                { "UI_Menu_Opened", "UIMenuOpened" },
                { "UI_Menu_Closed", "UIMenuClosed" },
                { "UI_Button_Clicked", "UIButtonClicked" },
                { "Audio_Music_Started", "AudioMusicStarted" },
                { "Audio_Music_Stopped", "AudioMusicStopped" },
                { "Audio_SFX_Played", "AudioSFXPlayed" },
                { "Scene_Loading_Started", "SceneLoadingStarted" },
                { "Scene_Loading_Finished", "SceneLoadingFinished" },
                { "Scene_Transition_Started", "SceneTransitionStarted" },
                { "Custom_Event_Example", "CustomEventExample" },
            };

            if (conversions.TryGetValue(oldEventName, out string modernName))
            {
                return modernName;
            }

            // Convert snake_case to PascalCase as fallback
            string[] parts = oldEventName.Split('_');
            string result = "";
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    result += char.ToUpper(part[0]) + part.Substring(1).ToLower();
                }
            }

            return result;
        }

        /// <summary>
        /// Generate a migration report showing all suggested changes.
        /// </summary>
        public static string GenerateMigrationReport(List<MigrationSuggestion> suggestions)
        {
            if (suggestions.Count == 0)
            {
                return "No migration suggestions found. Your code is already using the modern event system!";
            }

            string report = $"=== GAMEEVENTMANAGER MIGRATION REPORT ===\n";
            report +=
                $"Found {suggestions.Count} suggestions for modernizing your event system:\n\n";

            foreach (var suggestion in suggestions)
            {
                report += $"File: {Path.GetFileName(suggestion.filePath)}\n";
                report += $"Line {suggestion.lineNumber}: {suggestion.description}\n";
                report += $"  Old: {suggestion.oldCode}\n";
                report += $"  New: {suggestion.suggestedCode}\n\n";
            }

            report += "=== MIGRATION STEPS ===\n";
            report += "1. Change MonoBehaviour inheritance to HasSignalsBase where appropriate\n";
            report += "2. Replace AddAction() calls with Subscribe() in OnInitializeSignals()\n";
            report += "3. Replace SendSignal() calls with RaiseSignal() using type-safe events\n";
            report +=
                "4. Add proper cleanup in OnCleanupSignals() (or rely on automatic cleanup)\n";
            report += "5. Test thoroughly to ensure all events are working correctly\n\n";

            report += "=== BENEFITS AFTER MIGRATION ===\n";
            report += "- Compile-time type safety (catch errors early)\n";
            report += "- Better IntelliSense and code completion\n";
            report += "- Automatic memory leak prevention\n";
            report += "- Improved performance with event pooling\n";
            report += "- Better debugging and profiling capabilities\n";

            return report;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Unity Editor window for the GameEvent Migration Tool.
    /// Access via Window -> LilLycanLord -> GameEvent Migration Tool
    /// </summary>
    public class GameEventMigrationWindow : EditorWindow
    {
        private List<GameEventMigrationTool.MigrationSuggestion> suggestions;
        private Vector2 scrollPosition;
        private bool showDetails = true;

        [MenuItem("Window/LilLycanLord/GameEvent Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<GameEventMigrationWindow>("GameEvent Migration Tool");
        }

        void OnGUI()
        {
            GUILayout.Label("GameEvent Migration Tool", EditorStyles.boldLabel);
            GUILayout.Label(
                "Scan your project for old event usage and get modernization suggestions.",
                EditorStyles.helpBox
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Scan Project for Old Event Usage"))
            {
                suggestions = GameEventMigrationTool.ScanForOldEventUsage();
            }

            if (suggestions != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Found {suggestions.Count} suggestions:", EditorStyles.boldLabel);

                showDetails = EditorGUILayout.Toggle("Show Details", showDetails);

                if (GUILayout.Button("Copy Migration Report to Clipboard"))
                {
                    string report = GameEventMigrationTool.GenerateMigrationReport(suggestions);
                    EditorGUIUtility.systemCopyBuffer = report;
                    Debug.Log("Migration report copied to clipboard!");
                }

                GUILayout.Space(10);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var suggestion in suggestions)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(
                        $"File: {Path.GetFileName(suggestion.filePath)}",
                        EditorStyles.boldLabel
                    );
                    EditorGUILayout.LabelField(
                        $"Line {suggestion.lineNumber}: {suggestion.description}"
                    );

                    if (showDetails)
                    {
                        EditorGUILayout.LabelField("Old Code:", EditorStyles.miniBoldLabel);
                        EditorGUILayout.SelectableLabel(suggestion.oldCode, EditorStyles.textArea);

                        EditorGUILayout.LabelField("Suggested Code:", EditorStyles.miniBoldLabel);
                        EditorGUILayout.SelectableLabel(
                            suggestion.suggestedCode,
                            EditorStyles.textArea
                        );
                    }

                    if (GUILayout.Button("Open File"))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                            suggestion.filePath.Replace(Application.dataPath, "Assets")
                        );
                        if (asset != null)
                        {
                            AssetDatabase.OpenAsset(asset, suggestion.lineNumber);
                        }
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
#endif
}
