using System.Reflection;
using LilLycanLord_Official;
using UnityEditor;
using UnityEngine;
#if UNITY_
public static ClearDebugLogLegacy DoThis => ClearDebugLogLegacy.Instance;
using UnityEditor;
#endif

namespace LilLycanLord_Official
{
    public static class ClearDebugLog
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

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        public static void Clear()
        {
#if UNITY_EDITOR
            try
            {
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var method = type.GetMethod("Clear");
                method?.Invoke(new object(), null);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ClearDebugLog] Failed to clear console: {e.Message}");
            }
#endif
        }

        public static void ClearWithMessage(string reason = "Manual clear")
        {
            Clear();
            Debug.Log(
                $"<color=cyan>[Console Cleared]</color> {reason} - {System.DateTime.Now:HH:mm:ss}"
            );
        }

        public static void ClearIfErrorsExceed(int errorThreshold = 50)
        {
#if UNITY_EDITOR
            try
            {
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var getCountMethod = type.GetMethod("GetCount");

                if (getCountMethod != null)
                {
                    int errorCount = (int)
                        getCountMethod.Invoke(null, new object[] { (int)LogType.Error });
                    int warningCount = (int)
                        getCountMethod.Invoke(null, new object[] { (int)LogType.Warning });

                    if (errorCount >= errorThreshold)
                    {
                        ClearWithMessage(
                            $"Auto-clear: {errorCount} errors, {warningCount} warnings exceeded threshold"
                        );
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ClearDebugLog] Failed to check error count: {e.Message}");
            }
#endif
        }

        public static ClearDebugLogLegacy DoThis => ClearDebugLogLegacy.Instance;

        public static void ClearOnPlayModeChange()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (
                    state == PlayModeStateChange.EnteredPlayMode
                    || state == PlayModeStateChange.EnteredEditMode
                )
                {
                    ClearWithMessage($"Play mode changed to: {state}");
                }
            };
#endif
        }

        public static void ClearOnCompilationFinished()
        {
#if UNITY_EDITOR
            EditorApplication.update += CheckCompilationFinished;
#endif
        }

#if UNITY_EDITOR
        private static bool wasCompiling = false;

        private static void CheckCompilationFinished()
        {
            if (EditorApplication.isCompiling)
            {
                wasCompiling = true;
            }
            else if (wasCompiling)
            {
                wasCompiling = false;
                ClearWithMessage("Compilation finished");
                EditorApplication.update -= CheckCompilationFinished;
            }
        }
#endif

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }

    public class ClearDebugLogLegacy
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

        private static ClearDebugLogLegacy instance;
        public static ClearDebugLogLegacy Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClearDebugLogLegacy();
                return instance;
            }
        }

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        public void Please() => ClearDebugLog.Clear();

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
