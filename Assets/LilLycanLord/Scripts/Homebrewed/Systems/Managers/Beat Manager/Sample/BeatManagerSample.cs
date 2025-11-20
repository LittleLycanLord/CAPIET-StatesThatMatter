using System.Collections;
using System.Collections.Generic;
using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    public enum BeatSampleType
    {
        CreateManualRhythm,
        CreateAutoRhythm,
        StartRhythm,
        StopRhythm,
        RemoveRhythm,
        StopAllRhythms,
        StartAllManualRhythms,
        TestMultipleRhythms,
        TestSyncedToAudio,
        TestDifferentBPMSpeeds,
        PrintAllRhythms,
    }

    /// <summary>
    /// Demo script showcasing the BeatManager functionality with visual pulsing effects.
    /// Attach to a GameObject (like a cube) to test various rhythm operations.
    /// </summary>
    public class BeatManagerSample : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        [Header("Debug & Testing")]
        [SerializeField]
        private bool enableDebugLogging = true;

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Beat Test Configuration")]
        [SerializeField]
        private BeatSampleType sampleType = BeatSampleType.CreateManualRhythm;

        [SerializeField]
        private string rhythmName = "SampleRhythm";

        [SerializeField]
        private float bpm = 120f;

        [SerializeField]
        private float steps = 1f;

        [Header("Auto Rhythm Settings")]
        [SerializeField]
        private string soundName = "120 BPM Sync Track";

        [SerializeField]
        private bool useSoundBPM = true;

        [Header("Visual Pulse Settings")]
        [SerializeField]
        private float pulseScale = 1.2f;

        [SerializeField]
        private float pulseDuration = 0.1f;

        [SerializeField]
        private Color pulseColor = Color.red;

        [SerializeField]
        private bool enableVisualPulse = true;

        [Header("Multiple Rhythms Test")]
        [SerializeField]
        private string[] testRhythms = { "FastBeat", "MediumBeat", "SlowBeat" };

        [SerializeField]
        private float[] testBPMs = { 180f, 120f, 80f };

        [SerializeField]
        private bool autoExecuteOnStart = false;

        [SerializeField]
        private float autoExecuteDelay = 1f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private bool isCurrentlyPulsing = false;
        private Vector3 originalScale;
        private Color originalColor;
        private Renderer objectRenderer;
        private List<Rhythm> createdRhythms = new List<Rhythm>();
        private List<AutoRhythm> createdAutoRhythms = new List<AutoRhythm>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        private void Start()
        {
            StartCoroutine(InitializeBeatManager());
        }

        private IEnumerator InitializeBeatManager()
        {
            DebugLog("🔄 Initializing BeatManager connection...");

            // Wait a frame to ensure BeatManager is initialized
            yield return null;

            if (BeatManager.Instance == null)
            {
                DebugLog("❌ Failed to get BeatManager instance!", LogType.Error);
                yield break;
            }

            DebugLog("✅ BeatManager connected successfully!");

            // Store original visual properties
            SetupVisualProperties();

            // Add a demo AutoRhythm automatically on start
            CreateDemoAutoRhythm();

            // Auto-execute if enabled
            if (autoExecuteOnStart)
            {
                yield return new WaitForSeconds(autoExecuteDelay);
                ExecuteSampleType();
            }
        }

        private void SetupVisualProperties()
        {
            originalScale = transform.localScale;
            objectRenderer = GetComponent<Renderer>();

            if (objectRenderer != null)
            {
                originalColor = objectRenderer.material.color;
                DebugLog(
                    $"📐 Visual properties set - Scale: {originalScale}, Color: {originalColor}"
                );
            }
            else
            {
                DebugLog(
                    "⚠️ No Renderer found - visual pulse effects will be limited to scale only",
                    LogType.Warning
                );
            }
        }

        /// <summary>
        /// Creates a demo AutoRhythm automatically on start.
        /// NOTE: This demonstrates how to add an AutoRhythm via script!
        /// </summary>
        private void CreateDemoAutoRhythm()
        {
            DebugLog("🎵 Creating demo AutoRhythm...");

            //* HOW TO ADD AN AUTORHYTHM VIA SCRIPT:
            //* 1. Call BeatManager.Instance.CreateAutoRhythm() with these parameters:
            //*    - rhythmName: Unique name for the rhythm
            //*    - soundName: Name of the sound to sync to (must exist in AudioManager)
            //*    - beatAction: UnityAction to execute on each beat
            //*    - customBPM: Optional custom BPM (-1 to use sound's BPM)
            //*    - steps: Beat subdivisions (1=quarter, 2=eighth, 4=sixteenth notes)

            AutoRhythm demoAutoRhythm = BeatManager.Instance.CreateAutoRhythm(
                rhythmName,
                soundName,
                OnDemoAutoBeat,
                -1,
                steps
            );

            // Store the created rhythm for tracking
            createdAutoRhythms.Add(demoAutoRhythm);

            DebugLog($"✅ Created demo AutoRhythm '{demoAutoRhythm.name}' synced to '{soundName}'");
            DebugLog(
                "💡 This AutoRhythm will automatically start/stop when the sound plays/stops!"
            );
        }

        /// <summary>
        /// Beat action called by the demo AutoRhythm
        /// </summary>
        private void OnDemoAutoBeat()
        {
            DebugLog("🎵 Demo AutoRhythm beat triggered!");

            if (enableVisualPulse)
            {
                StartCoroutine(PulseEffect());
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        [ContextMenu("Execute Sample Type")]
        public void ExecuteSampleType()
        {
            if (BeatManager.Instance == null)
            {
                DebugLog("⚠️ BeatManager not initialized yet!", LogType.Warning);
                return;
            }

            DebugLog($"🥁 Executing: {sampleType}");

            switch (sampleType)
            {
                case BeatSampleType.CreateManualRhythm:
                    ExecuteCreateManualRhythm();
                    break;

                case BeatSampleType.CreateAutoRhythm:
                    ExecuteCreateAutoRhythm();
                    break;

                case BeatSampleType.StartRhythm:
                    ExecuteStartRhythm();
                    break;

                case BeatSampleType.StopRhythm:
                    ExecuteStopRhythm();
                    break;

                case BeatSampleType.RemoveRhythm:
                    ExecuteRemoveRhythm();
                    break;

                case BeatSampleType.StopAllRhythms:
                    ExecuteStopAllRhythms();
                    break;

                case BeatSampleType.StartAllManualRhythms:
                    ExecuteStartAllManualRhythms();
                    break;

                case BeatSampleType.TestMultipleRhythms:
                    ExecuteTestMultipleRhythms();
                    break;

                case BeatSampleType.TestSyncedToAudio:
                    ExecuteTestSyncedToAudio();
                    break;

                case BeatSampleType.TestDifferentBPMSpeeds:
                    ExecuteTestDifferentBPMSpeeds();
                    break;

                case BeatSampleType.PrintAllRhythms:
                    ExecutePrintAllRhythms();
                    break;

                default:
                    DebugLog($"⚠️ Unknown sample type: {sampleType}", LogType.Warning);
                    break;
            }
        }

        private void ExecuteCreateManualRhythm()
        {
            DebugLog($"🎵 Creating manual rhythm '{rhythmName}' at {bpm} BPM (steps: {steps})");

            Rhythm rhythm = BeatManager.Instance.CreateManualRhythm(
                rhythmName,
                bpm,
                OnBeatTriggered,
                steps
            );

            if (rhythm != null)
            {
                createdRhythms.Add(rhythm);
                DebugLog($"✅ Manual rhythm created and added to tracking");
                rhythm.SetActive(true); // Start it immediately
                DebugLog($"▶️ Rhythm '{rhythmName}' started automatically");
            }
        }

        private void ExecuteCreateAutoRhythm()
        {
            DebugLog($"🎶 Creating auto rhythm '{rhythmName}' synced to sound '{soundName}'");
            DebugLog(
                $"   Using sound BPM: {useSoundBPM}, Custom BPM: {(useSoundBPM ? "N/A" : bpm.ToString())}"
            );

            float customBPM = useSoundBPM ? -1 : bpm;
            AutoRhythm autoRhythm = BeatManager.Instance.CreateAutoRhythm(
                rhythmName,
                soundName,
                OnBeatTriggered,
                customBPM,
                steps
            );

            if (autoRhythm != null)
            {
                createdAutoRhythms.Add(autoRhythm);
                DebugLog(
                    $"✅ Auto rhythm created - it will activate automatically when '{soundName}' plays"
                );
            }
        }

        private void ExecuteStartRhythm()
        {
            DebugLog($"▶️ Starting rhythm '{rhythmName}'");
            BeatManager.Instance.StartRhythm(rhythmName);
        }

        private void ExecuteStopRhythm()
        {
            DebugLog($"⏹️ Stopping rhythm '{rhythmName}'");
            BeatManager.Instance.StopRhythm(rhythmName);
        }

        private void ExecuteRemoveRhythm()
        {
            DebugLog($"🗑️ Removing rhythm '{rhythmName}' completely");
            BeatManager.Instance.RemoveRhythm(rhythmName);

            // Remove from our tracking lists
            createdRhythms.RemoveAll(r => r.name == rhythmName);
            createdAutoRhythms.RemoveAll(r => r.name == rhythmName);
        }

        private void ExecuteStopAllRhythms()
        {
            DebugLog("⏹️ Stopping ALL rhythms");
            BeatManager.Instance.StopAllRhythms();
        }

        private void ExecuteStartAllManualRhythms()
        {
            DebugLog("▶️ Starting ALL manual rhythms");
            BeatManager.Instance.StartAllManualRhythms();
        }

        private void ExecuteTestMultipleRhythms()
        {
            DebugLog($"🎵x{testRhythms.Length} Creating multiple test rhythms...");

            for (int i = 0; i < testRhythms.Length && i < testBPMs.Length; i++)
            {
                string testRhythmName = testRhythms[i];
                float testBPM = testBPMs[i];

                DebugLog($"  [{i + 1}] Creating '{testRhythmName}' at {testBPM} BPM");
                Rhythm rhythm = BeatManager.Instance.CreateManualRhythm(
                    testRhythmName,
                    testBPM,
                    OnBeatTriggered,
                    steps
                );

                if (rhythm != null)
                {
                    createdRhythms.Add(rhythm);
                    rhythm.SetActive(true); // Start immediately
                }
            }

            DebugLog(
                $"✅ Created {testRhythms.Length} simultaneous rhythms - prepare for visual chaos! 🎨"
            );
        }

        private void ExecuteTestSyncedToAudio()
        {
            DebugLog($"🎶🔗 Testing rhythm synced to audio playback");

            // First, create the auto rhythm
            string autoRhythmName = $"AutoSync_{soundName}";
            AutoRhythm autoRhythm = BeatManager.Instance.CreateAutoRhythm(
                autoRhythmName,
                soundName,
                OnBeatTriggered,
                -1,
                steps
            );

            if (autoRhythm != null)
            {
                createdAutoRhythms.Add(autoRhythm);
                DebugLog($"✅ Auto rhythm '{autoRhythmName}' created");

                // Now play the sound to trigger the rhythm
                if (AudioManager.Instance != null)
                {
                    DebugLog($"🎵 Playing sound '{soundName}' to trigger auto rhythm...");
                    AudioManager.Instance.Play(soundName, gameObject);
                    DebugLog($"   The cube should start pulsing when the audio plays!");
                }
                else
                {
                    DebugLog(
                        "⚠️ AudioManager not available - auto rhythm won't sync",
                        LogType.Warning
                    );
                }
            }
        }

        private void ExecuteTestDifferentBPMSpeeds()
        {
            DebugLog("🚀 Testing different BPM speeds in sequence...");
            StartCoroutine(BPMSpeedTestCoroutine());
        }

        private IEnumerator BPMSpeedTestCoroutine()
        {
            float[] testBPMSpeeds = { 60f, 90f, 120f, 150f, 180f, 210f };
            string testRhythmName = "SpeedTest";

            foreach (float testBPM in testBPMSpeeds)
            {
                DebugLog($"⏳ Testing {testBPM} BPM for 3 seconds...");

                // Remove previous test rhythm
                BeatManager.Instance.RemoveRhythm(testRhythmName);

                // Create new rhythm at test BPM
                Rhythm speedTestRhythm = BeatManager.Instance.CreateManualRhythm(
                    testRhythmName,
                    testBPM,
                    OnBeatTriggered,
                    steps
                );
                speedTestRhythm.SetActive(true);

                yield return new WaitForSeconds(3f);
            }

            // Clean up
            BeatManager.Instance.RemoveRhythm(testRhythmName);
            DebugLog("✅ BPM speed test completed!");
        }

        private void ExecutePrintAllRhythms()
        {
            DebugLog("📊 Printing all BeatManager rhythms...");
            BeatManager.Instance.DebugPrintAllRhythms();

            List<string> allRhythmNames = BeatManager.Instance.GetAllRhythmNames();
            DebugLog($"📋 Total rhythms in BeatManager: {allRhythmNames.Count}");

            if (allRhythmNames.Count > 0)
            {
                for (int i = 0; i < allRhythmNames.Count; i++)
                {
                    DebugLog($"  [{i + 1}] {allRhythmNames[i]}");
                }
            }
            else
            {
                DebugLog("  📭 No rhythms currently registered");
            }
        }

        /// <summary>
        /// Called every time a beat is triggered by any rhythm
        /// </summary>
        public void OnBeatTriggered()
        {
            if (enableVisualPulse && !isCurrentlyPulsing)
            {
                StartCoroutine(PulseEffect());
            }
        }

        private IEnumerator PulseEffect()
        {
            isCurrentlyPulsing = true;

            // Pulse scale up
            float elapsedTime = 0f;
            Vector3 targetScale = originalScale * pulseScale;

            while (elapsedTime < pulseDuration / 2)
            {
                transform.localScale = Vector3.Lerp(
                    originalScale,
                    targetScale,
                    elapsedTime / (pulseDuration / 2)
                );

                if (objectRenderer != null)
                {
                    Color currentColor = Color.Lerp(
                        originalColor,
                        pulseColor,
                        elapsedTime / (pulseDuration / 2)
                    );
                    objectRenderer.material.color = currentColor;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Hold at max scale briefly
            transform.localScale = targetScale;
            if (objectRenderer != null)
                objectRenderer.material.color = pulseColor;

            // Pulse scale down
            elapsedTime = 0f;
            while (elapsedTime < pulseDuration / 2)
            {
                transform.localScale = Vector3.Lerp(
                    targetScale,
                    originalScale,
                    elapsedTime / (pulseDuration / 2)
                );

                if (objectRenderer != null)
                {
                    Color currentColor = Color.Lerp(
                        pulseColor,
                        originalColor,
                        elapsedTime / (pulseDuration / 2)
                    );
                    objectRenderer.material.color = currentColor;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Reset to original
            transform.localScale = originalScale;
            if (objectRenderer != null)
                objectRenderer.material.color = originalColor;

            isCurrentlyPulsing = false;
        }

        private void DebugLog(string message, LogType logType = LogType.Log)
        {
            if (!enableDebugLogging)
                return;

            string prefix = "<color=magenta>[BeatManagerSample]</color>";
            string formattedMessage = $"{prefix} {message}";

            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        [ContextMenu("Create Manual Rhythm")]
        public void CreateManualRhythm()
        {
            sampleType = BeatSampleType.CreateManualRhythm;
            ExecuteSampleType();
        }

        [ContextMenu("Create Auto Rhythm")]
        public void CreateAutoRhythm()
        {
            sampleType = BeatSampleType.CreateAutoRhythm;
            ExecuteSampleType();
        }

        [ContextMenu("Start Rhythm")]
        public void StartRhythm()
        {
            sampleType = BeatSampleType.StartRhythm;
            ExecuteSampleType();
        }

        [ContextMenu("Stop Rhythm")]
        public void StopRhythm()
        {
            sampleType = BeatSampleType.StopRhythm;
            ExecuteSampleType();
        }

        [ContextMenu("Stop All Rhythms")]
        public void StopAllRhythms()
        {
            if (BeatManager.Instance != null)
                BeatManager.Instance.StopAllRhythms();
        }

        [ContextMenu("Test Synced to Audio")]
        public void TestSyncedToAudio()
        {
            sampleType = BeatSampleType.TestSyncedToAudio;
            ExecuteSampleType();
        }

        [ContextMenu("Test Different BPM Speeds")]
        public void TestDifferentBPMSpeeds()
        {
            sampleType = BeatSampleType.TestDifferentBPMSpeeds;
            ExecuteSampleType();
        }

        [ContextMenu("Print BeatManager Status")]
        public void PrintBeatManagerStatus()
        {
            if (BeatManager.Instance == null)
            {
                DebugLog("❌ BeatManager is null!", LogType.Error);
                return;
            }

            DebugLog("📊 BeatManager Status:");
            List<string> rhythmNames = BeatManager.Instance.GetAllRhythmNames();
            DebugLog($"  🥁 Total rhythms: {rhythmNames.Count}");
            DebugLog(
                $"  🎵 AudioManager available: {BeatManager.Instance.IsAudioManagerAvailable()}"
            );
            DebugLog($"  📐 Visual pulse enabled: {enableVisualPulse}");
            DebugLog($"  🎨 Pulse scale: {pulseScale}x, duration: {pulseDuration}s");
        }

        [ContextMenu("Test All Sample Types")]
        public void TestAllSampleTypes()
        {
            StartCoroutine(TestAllSampleTypesCoroutine());
        }

        private IEnumerator TestAllSampleTypesCoroutine()
        {
            DebugLog("🧪 Testing all BeatManager sample types sequentially...");

            BeatSampleType[] allTypes = (BeatSampleType[])
                System.Enum.GetValues(typeof(BeatSampleType));

            foreach (BeatSampleType type in allTypes)
            {
                // Skip the time-consuming tests in auto-test mode
                if (type == BeatSampleType.TestDifferentBPMSpeeds)
                {
                    DebugLog($"  Skipping: {type} (too time consuming for auto-test)");
                    continue;
                }

                DebugLog($"  Testing: {type}");
                sampleType = type;
                ExecuteSampleType();
                yield return new WaitForSeconds(2f);
            }

            DebugLog("✅ All sample types tested!");
        }
    }
}
