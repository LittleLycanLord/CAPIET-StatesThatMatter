using System;
using LilLycanLord_Official;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LilLycanLord_Official
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BoolBasedSerializedFieldAttribute : PropertyAttribute
    {
        public string BoolFieldName { get; private set; }
        public bool ExpectedValue { get; private set; }
        public bool HideInsteadOfDisable { get; private set; }
        public string CustomLabel { get; private set; }
        public string CustomTooltip { get; private set; }

        public BoolBasedSerializedFieldAttribute(string boolFieldName, bool expectedValue = true)
        {
            BoolFieldName = boolFieldName;
            ExpectedValue = expectedValue;
            HideInsteadOfDisable = true;
            CustomLabel = null;
            CustomTooltip = null;
        }

        public BoolBasedSerializedFieldAttribute(
            string boolFieldName,
            bool expectedValue,
            bool hideInsteadOfDisable,
            string customLabel = null,
            string customTooltip = null
        )
        {
            BoolFieldName = boolFieldName;
            ExpectedValue = expectedValue;
            HideInsteadOfDisable = hideInsteadOfDisable;
            CustomLabel = customLabel;
            CustomTooltip = customTooltip;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoolBasedSerializedFieldAttribute))]
    public class BoolBasedSerializedFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (BoolBasedSerializedFieldAttribute)attribute;
            bool shouldShow = ShouldShowProperty(property, conditionalAttribute);

            if (!shouldShow && conditionalAttribute.HideInsteadOfDisable)
            {
                return;
            }

            var displayLabel = GetDisplayLabel(label, conditionalAttribute);

            bool originalEnabled = GUI.enabled;
            Color originalColor = GUI.color;

            if (!shouldShow)
            {
                GUI.enabled = false;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
            }
            else if (!string.IsNullOrEmpty(conditionalAttribute.CustomLabel))
            {
                GUI.color = new Color(0.8f, 1.0f, 0.8f, 1.0f);
            }

            EditorGUI.PropertyField(position, property, displayLabel, true);

            GUI.enabled = originalEnabled;
            GUI.color = originalColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (BoolBasedSerializedFieldAttribute)attribute;
            bool shouldShow = ShouldShowProperty(property, conditionalAttribute);

            if (!shouldShow && conditionalAttribute.HideInsteadOfDisable)
            {
                return 0f;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool ShouldShowProperty(
            SerializedProperty property,
            BoolBasedSerializedFieldAttribute conditionalAttribute
        )
        {
            var targetObject = property.serializedObject.targetObject;
            var targetType = targetObject.GetType();

            var boolField = targetType.GetField(
                conditionalAttribute.BoolFieldName,
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance
            );

            if (boolField == null)
            {
                var boolProperty = targetType.GetProperty(
                    conditionalAttribute.BoolFieldName,
                    System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

                if (boolProperty != null && boolProperty.PropertyType == typeof(bool))
                {
                    bool propertyValue = (bool)boolProperty.GetValue(targetObject);
                    return propertyValue == conditionalAttribute.ExpectedValue;
                }

                Debug.LogWarning(
                    $"[BoolBasedSerializedField] Boolean field/property '{conditionalAttribute.BoolFieldName}' not found on {targetType.Name}"
                );
                return true;
            }

            if (boolField.FieldType != typeof(bool))
            {
                Debug.LogWarning(
                    $"[BoolBasedSerializedField] Field '{conditionalAttribute.BoolFieldName}' on {targetType.Name} is not a boolean"
                );
                return true;
            }

            bool fieldValue = (bool)boolField.GetValue(targetObject);
            return fieldValue == conditionalAttribute.ExpectedValue;
        }

        private GUIContent GetDisplayLabel(
            GUIContent originalLabel,
            BoolBasedSerializedFieldAttribute conditionalAttribute
        )
        {
            string labelText = !string.IsNullOrEmpty(conditionalAttribute.CustomLabel)
                ? conditionalAttribute.CustomLabel
                : originalLabel.text;

            string tooltipText = !string.IsNullOrEmpty(conditionalAttribute.CustomTooltip)
                ? conditionalAttribute.CustomTooltip
                : originalLabel.tooltip;

            return new GUIContent(labelText, tooltipText);
        }
    }
#endif

    public class BoolBasedInspectorField : MonoBehaviour
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

        [Header("Boolean Controls")]
        [SerializeField]
        private bool enableAdvancedSettings = false;

        [SerializeField]
        private bool showDebugInfo = false;

        [SerializeField]
        private bool useCustomSettings = false;

        [SerializeField]
        private bool enableExperimentalFeatures = false;

        [Header("Basic Conditional Fields")]
        [BoolBasedSerializedField("enableAdvancedSettings")]
        [SerializeField]
        private float advancedParameter = 1.0f;

        [BoolBasedSerializedField("enableAdvancedSettings")]
        [SerializeField]
        private AnimationCurve advancedCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [BoolBasedSerializedField("showDebugInfo", true)]
        [SerializeField]
        private string debugMessage = "Debug information here";

        [BoolBasedSerializedField("showDebugInfo", true)]
        [SerializeField]
        private Color debugColor = Color.red;

        [Header("Advanced Conditional Fields")]
        [BoolBasedSerializedField(
            "useCustomSettings",
            true,
            false,
            "Custom Value",
            "This field is disabled when useCustomSettings is false"
        )]
        [SerializeField]
        private int customValue = 100;

        [BoolBasedSerializedField(
            "enableExperimentalFeatures",
            true,
            true,
            "⚠️ Experimental Setting",
            "Use with caution - experimental feature"
        )]
        [SerializeField]
        private GameObject experimentalPrefab;

        [BoolBasedSerializedField("enableAdvancedSettings", false)]
        [SerializeField]
        private string basicModeMessage = "Running in basic mode";

        void Start()
        {
            ClearDebugLog.ClearWithMessage("BoolBasedInspectorField Demo Started");
            LogCurrentConfiguration();
        }

        void Update()
        {
            if (enableAdvancedSettings && showDebugInfo)
            {
                Debug.DrawRay(transform.position, Vector3.up * advancedParameter, debugColor);
            }
        }

        [ContextMenu("Log Current Configuration")]
        public void LogCurrentConfiguration()
        {
            Debug.Log(
                $"<color=cyan>[BoolBasedInspectorField Configuration]</color>\n"
                    + $"Advanced Settings: {enableAdvancedSettings}\n"
                    + $"Debug Info: {showDebugInfo}\n"
                    + $"Custom Settings: {useCustomSettings}\n"
                    + $"Experimental Features: {enableExperimentalFeatures}"
            );

            if (enableAdvancedSettings)
            {
                Debug.Log(
                    $"<color=yellow>Advanced Parameters:</color>\n"
                        + $"Parameter: {advancedParameter}\n"
                        + $"Curve Keys: {advancedCurve.keys.Length}"
                );
            }

            if (showDebugInfo)
            {
                Debug.Log(
                    $"<color=green>Debug Info:</color>\n"
                        + $"Message: {debugMessage}\n"
                        + $"Color: {debugColor}"
                );
            }
        }

        [ContextMenu("Toggle All Settings")]
        public void ToggleAllSettings()
        {
            enableAdvancedSettings = !enableAdvancedSettings;
            showDebugInfo = !showDebugInfo;
            useCustomSettings = !useCustomSettings;
            enableExperimentalFeatures = !enableExperimentalFeatures;

            Debug.Log("<color=magenta>[BoolBasedInspectorField]</color> All settings toggled!");
        }

    }
}
