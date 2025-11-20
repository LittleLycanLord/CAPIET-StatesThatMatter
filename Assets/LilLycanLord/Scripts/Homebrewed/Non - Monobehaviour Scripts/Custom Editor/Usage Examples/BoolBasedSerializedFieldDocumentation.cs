using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    public class BoolBasedSerializedFieldDocumentation : MonoBehaviour
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

        [Header("=== BOOL BASED SERIALIZED FIELD DOCUMENTATION ===")]
        [Space(20)]

        [Header("1. Basic Usage")]
        [Tooltip("Control boolean that determines field visibility")]
        [SerializeField] private bool showBasicFields = false;

        // Basic usage - field shows when boolean is true
        [BoolBasedSerializedField("showBasicFields")]
        [SerializeField] private string basicField = "This field appears when showBasicFields is true";

        // Explicit true condition (same as above)
        [BoolBasedSerializedField("showBasicFields", true)]
        [SerializeField] private int anotherBasicField = 42;

        // Inverted condition - field shows when boolean is false
        [BoolBasedSerializedField("showBasicFields", false)]
        [SerializeField] private string hiddenWhenTrue = "This field appears when showBasicFields is false";

        [Header("2. Advanced Usage")]
        [SerializeField] private bool enableAdvancedMode = false;

        // Advanced usage with custom label and tooltip
        [BoolBasedSerializedField(
            "enableAdvancedMode", 
            true, 
            false, // Don't hide, just disable
            "🔧 Advanced Setting", 
            "This is a custom tooltip for the advanced setting")]
        [SerializeField] private float advancedValue = 3.14f;

        // Hide completely when condition not met
        [BoolBasedSerializedField(
            "enableAdvancedMode", 
            true, 
            true, // Hide completely
            "⚡ Power Setting")]
        [SerializeField] private AnimationCurve powerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("3. Multiple Boolean Controls")]
        [SerializeField] private bool enableFeatureA = false;
        [SerializeField] private bool enableFeatureB = false;
        [SerializeField] private bool enableFeatureC = false;

        [BoolBasedSerializedField("enableFeatureA")]
        [SerializeField] private Color featureAColor = Color.red;

        [BoolBasedSerializedField("enableFeatureB")]
        [SerializeField] private Material featureBMaterial;

        [BoolBasedSerializedField("enableFeatureC")]
        [SerializeField] private GameObject featureCPrefab;

        // Note: For complex conditions (AND/OR), create compound booleans
        [Tooltip("Computed property: enableFeatureA && enableFeatureB")]
        public bool CombinedFeatureAB => enableFeatureA && enableFeatureB;

        [BoolBasedSerializedField("enableFeatureA", true, true, "Combined A+B Feature")]
        [SerializeField] private string combinedFeature = "Only visible when Feature A is enabled";

        [Header("4. Different Field Types")]
        [SerializeField] private bool showAllFieldTypes = false;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private int integerField = 100;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private float floatField = 2.5f;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private string stringField = "Text field";

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private Vector3 vectorField = Vector3.one;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private Color colorField = Color.blue;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private LayerMask layerMaskField = -1;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private Transform transformField;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private Texture2D textureField;

        [BoolBasedSerializedField("showAllFieldTypes")]
        [SerializeField] private AudioClip audioField;

        [Header("5. Complex Scenarios")]
        [SerializeField] private bool enableDebugSystem = false;
        [SerializeField] private bool enableVerboseLogging = false;
        [SerializeField] private bool enablePerformanceMetrics = false;

        // Nested conditional logic
        [BoolBasedSerializedField("enableDebugSystem")]
        [SerializeField] private bool debugSystemActive = false;

        [BoolBasedSerializedField("enableVerboseLogging", true, false, "📝 Verbose Log Level")]
        [SerializeField] private LogLevel verboseLevel = LogLevel.Info;

        [BoolBasedSerializedField("enablePerformanceMetrics")]
        [SerializeField] private bool showFPS = true;

        [BoolBasedSerializedField("enablePerformanceMetrics")]
        [SerializeField] private bool showMemoryUsage = false;

        // Custom enum for demonstration
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        [Header("6. Property-Based Conditionals")]
        [SerializeField] private bool _useProperties = false;

        // This demonstrates that the attribute can work with properties too
        public bool UseProperties
        {
            get => _useProperties;
            set => _useProperties = value;
        }

        [BoolBasedSerializedField("UseProperties")] // Note: Using property name
        [SerializeField] private string propertyBasedField = "Controlled by property";

        // Runtime controls section
        [Header("7. Runtime Controls")]
        [Space(10)]
        [SerializeField] private bool runtimeControlsActive = true;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Start()
        {
            ClearDebugLog.ClearWithMessage("BoolBasedSerializedField Documentation Started");
            LogUsageInstructions();
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        [ContextMenu("Log Usage Instructions")]
        public void LogUsageInstructions()
        {
            Debug.Log("<color=cyan>=== BOOL BASED SERIALIZED FIELD USAGE ===</color>\n\n" +

                     "<color=yellow>BASIC SYNTAX:</color>\n" +
                     "[BoolBasedSerializedField(\"booleanFieldName\")]\n" +
                     "[SerializeField] private float myField;\n\n" +

                     "<color=yellow>ADVANCED SYNTAX:</color>\n" +
                     "[BoolBasedSerializedField(\n" +
                     "    \"booleanFieldName\",    // Boolean field to check\n" +
                     "    true,                   // Expected value (true/false)\n" +
                     "    false,                  // Hide vs Disable (true=hide, false=disable)\n" +
                     "    \"Custom Label\",        // Custom label text\n" +
                     "    \"Custom tooltip\")]     // Custom tooltip\n\n" +

                     "<color=yellow>FEATURES:</color>\n" +
                     "• Show/hide fields based on boolean values\n" +
                     "• Works with both fields and properties\n" +
                     "• Custom labels and tooltips\n" +
                     "• Choose between hiding or disabling\n" +
                     "• Visual feedback for conditional fields\n" +
                     "• Supports all Unity serializable types\n\n" +

                     "<color=yellow>EXAMPLES:</color>\n" +
                     "• [BoolBasedSerializedField(\"enableFeature\")]           // Show when true\n" +
                     "• [BoolBasedSerializedField(\"enableFeature\", false)]   // Show when false\n" +
                     "• [BoolBasedSerializedField(\"debug\", true, false)]     // Disable when false\n" +
                     "• [BoolBasedSerializedField(\"advanced\", true, true, \"⚠️ Advanced\")]  // Hide with custom label");
        }

        [ContextMenu("Toggle All Features")]
        public void ToggleAllFeatures()
        {
            showBasicFields = !showBasicFields;
            enableAdvancedMode = !enableAdvancedMode;
            enableFeatureA = !enableFeatureA;
            enableFeatureB = !enableFeatureB;
            enableFeatureC = !enableFeatureC;
            showAllFieldTypes = !showAllFieldTypes;
            enableDebugSystem = !enableDebugSystem;
            enableVerboseLogging = !enableVerboseLogging;
            enablePerformanceMetrics = !enablePerformanceMetrics;
            UseProperties = !UseProperties;

            Debug.Log("<color=magenta>[Documentation]</color> All feature flags toggled!");
        }

        [ContextMenu("Reset All Features")]
        public void ResetAllFeatures()
        {
            showBasicFields = false;
            enableAdvancedMode = false;
            enableFeatureA = false;
            enableFeatureB = false;
            enableFeatureC = false;
            showAllFieldTypes = false;
            enableDebugSystem = false;
            enableVerboseLogging = false;
            enablePerformanceMetrics = false;
            UseProperties = false;

            Debug.Log("<color=green>[Documentation]</color> All feature flags reset to false!");
        }

        [Header("8. Integration with Other Systems")]
        [SerializeField] private bool integrateWithInputManager = false;
        [SerializeField] private bool integrateWithMovement = false;

        [BoolBasedSerializedField("integrateWithInputManager", true, true, "🎮 Input Integration")]
        [SerializeField] private string inputManagerSettings = "Input manager integration active";

        [BoolBasedSerializedField("integrateWithMovement", true, true, "🏃 Movement Integration")]
        [SerializeField] private string movementSettings = "Movement system integration active";

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
