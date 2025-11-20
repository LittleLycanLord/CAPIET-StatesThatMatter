using System.Collections;
using System.Collections.Generic;
using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    public enum SampleType
    {
        Play,
        PlayWithFadeIn,
        PlayWithFadeOut,
        PlayWithFadeInAndOut,
        PlayOnTarget,
        PlayMultiple,
        StopSpecificSound,
        StopSpecificSoundWithFade,
        StopAllSounds,
        StopOnGameObject,
        StopOnGameObjectWithFade,
        FindGameObjectsPlayingSound,
        TestAudioSourceTracking,
    }

    /// <summary>
    /// Demo script showcasing the new AudioManager functionality.
    /// Attach to a GameObject to test various audio operations.
    /// </summary>
    public class SampleSoundPlayer : MonoBehaviour
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
        [Header("Audio Test Configuration")]
        [SerializeField]
        private string soundName = "SampleSound";

        [SerializeField]
        private SampleType sampleType = SampleType.Play;

        [SerializeField]
        private float fadeInDuration = 1f;

        [SerializeField]
        private float fadeOutDuration = 2f;

        [SerializeField]
        private float fadeOutDelay = 1f;

        [Header("Target GameObject Settings")]
        [SerializeField]
        private GameObject targetGameObject;

        [SerializeField]
        private bool useThisGameObject = true;

        [Header("Multiple Audio Test")]
        [SerializeField]
        private string[] multipleSounds = { "SampleSound", "FootstepSound", "MusicTrack" };

        [SerializeField]
        private int numberOfInstances = 3;

        [SerializeField]
        private bool autoExecuteOnStart = false;

        [SerializeField]
        private float autoExecuteDelay = 1f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        private List<AudioSource> trackedAudioSources = new List<AudioSource>();

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        private void Start()
        {
            StartCoroutine(InitializeAudioManager());
        }

        private IEnumerator InitializeAudioManager()
        {
            DebugLog("🔄 Initializing AudioManager connection...");

            // Wait a frame to ensure AudioManager is initialized
            yield return null;

            if (AudioManager.Instance == null)
            {
                DebugLog("❌ Failed to get AudioManager instance!", LogType.Error);
                yield break;
            }

            DebugLog("✅ AudioManager connected successfully!");

            // Set default target if needed
            if (useThisGameObject || targetGameObject == null)
            {
                targetGameObject = gameObject;
            }

            // Print available sounds for reference
            LogAvailableSounds();

            // Auto-execute if enabled
            if (autoExecuteOnStart)
            {
                yield return new WaitForSeconds(autoExecuteDelay);
                ExecuteSampleType();
            }
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        /// <summary>
        /// Safely gets AudioManager instance and logs if there's an issue
        /// </summary>
        private AudioManager GetAudioManagerSafely()
        {
            var audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                DebugLog(
                    "⚠️ AudioManager instance is null! Cannot perform audio operation.",
                    LogType.Warning
                );
            }
            return audioManager;
        }

        [ContextMenu("Execute Sample Type")]
        public void ExecuteSampleType()
        {
            if (AudioManager.Instance == null)
            {
                DebugLog("⚠️ AudioManager not initialized yet!", LogType.Warning);
                return;
            }

            DebugLog($"🎵 Executing: {sampleType}");

            switch (sampleType)
            {
                case SampleType.Play:
                    ExecutePlay();
                    break;

                case SampleType.PlayWithFadeIn:
                    ExecutePlayWithFadeIn();
                    break;

                case SampleType.PlayWithFadeOut:
                    ExecutePlayWithFadeOut();
                    break;

                case SampleType.PlayWithFadeInAndOut:
                    ExecutePlayWithFadeInAndOut();
                    break;

                case SampleType.PlayOnTarget:
                    ExecutePlayOnTarget();
                    break;

                case SampleType.PlayMultiple:
                    ExecutePlayMultiple();
                    break;

                case SampleType.StopSpecificSound:
                    ExecuteStopSpecificSound();
                    break;

                case SampleType.StopSpecificSoundWithFade:
                    ExecuteStopSpecificSoundWithFade();
                    break;

                case SampleType.StopAllSounds:
                    ExecuteStopAllSounds();
                    break;

                case SampleType.StopOnGameObject:
                    ExecuteStopOnGameObject();
                    break;

                case SampleType.StopOnGameObjectWithFade:
                    ExecuteStopOnGameObjectWithFade();
                    break;

                case SampleType.FindGameObjectsPlayingSound:
                    ExecuteFindGameObjectsPlayingSound();
                    break;

                case SampleType.TestAudioSourceTracking:
                    ExecuteTestAudioSourceTracking();
                    break;

                default:
                    DebugLog($"⚠️ Unknown sample type: {sampleType}", LogType.Warning);
                    break;
            }
        }

        private void ExecutePlay()
        {
            DebugLog($"▶️ Playing '{soundName}' normally on {targetGameObject.name}");
            AudioSource audioSource = AudioManager.Instance.Play(soundName, targetGameObject);

            if (audioSource != null)
            {
                DebugLog($"✅ Sound playing on AudioSource: {audioSource.name}");
                trackedAudioSources.Add(audioSource);
            }
        }

        private void ExecutePlayWithFadeIn()
        {
            DebugLog(
                $"🔊 Playing '{soundName}' with fade in ({fadeInDuration}s) on {targetGameObject.name}"
            );
            AudioSource audioSource = AudioManager.Instance.PlayWithFadeIn(
                soundName,
                fadeInDuration,
                targetGameObject
            );

            if (audioSource != null)
            {
                DebugLog($"✅ Sound playing with fade in");
                trackedAudioSources.Add(audioSource);
            }
        }

        private void ExecutePlayWithFadeOut()
        {
            DebugLog(
                $"🔉 Playing '{soundName}' with fade out (duration: {fadeOutDuration}s at end of clip)"
            );
            AudioSource audioSource = AudioManager.Instance.PlayWithFadeOut(
                soundName,
                fadeOutDuration,
                targetGameObject
            );

            if (audioSource != null)
            {
                DebugLog($"✅ Sound playing with fade out");
                trackedAudioSources.Add(audioSource);
            }
        }

        private void ExecutePlayWithFadeInAndOut()
        {
            DebugLog(
                $"🔊➡️🔉 Playing '{soundName}' with fade in & out (in: {fadeInDuration}s, out: {fadeOutDuration}s at end)"
            );
            AudioSource audioSource = AudioManager.Instance.PlayWithFadeInAndOut(
                soundName,
                fadeInDuration,
                fadeOutDuration,
                targetGameObject
            );

            if (audioSource != null)
            {
                DebugLog($"✅ Sound playing with both fade effects");
                trackedAudioSources.Add(audioSource);
            }
        }

        private void ExecutePlayOnTarget()
        {
            DebugLog($"🎯 Playing '{soundName}' on specific target: {targetGameObject.name}");
            AudioSource audioSource = AudioManager.Instance.Play(soundName, targetGameObject);

            if (audioSource != null)
            {
                DebugLog($"✅ AudioSource attached to {audioSource.gameObject.name}");

                // Log all AudioSources on this GameObject
                AudioSource[] allSources = targetGameObject.GetComponents<AudioSource>();
                DebugLog($"📊 Total AudioSources on {targetGameObject.name}: {allSources.Length}");
            }
        }

        private void ExecutePlayMultiple()
        {
            DebugLog($"🎵x{numberOfInstances} Playing multiple instances...");

            for (int i = 0; i < numberOfInstances; i++)
            {
                string currentSound = multipleSounds[i % multipleSounds.Length];
                AudioSource audioSource = AudioManager.Instance.Play(
                    currentSound,
                    targetGameObject
                );

                if (audioSource != null)
                {
                    trackedAudioSources.Add(audioSource);
                    DebugLog(
                        $"  [{i + 1}] Playing '{currentSound}' on {audioSource.gameObject.name}"
                    );
                }
            }

            DebugLog($"✅ Created {trackedAudioSources.Count} audio instances");
        }

        private void ExecuteStopSpecificSound()
        {
            DebugLog($"⏹️ Stopping all instances of '{soundName}' immediately");

            int playingBefore = AudioManager.Instance.GetAudioSourcesPlayingSound(soundName).Count;
            AudioManager.Instance.StopSound(soundName);
            int playingAfter = AudioManager.Instance.GetAudioSourcesPlayingSound(soundName).Count;

            DebugLog($"📊 Stopped {playingBefore - playingAfter} instances of '{soundName}'");
        }

        private void ExecuteStopSpecificSoundWithFade()
        {
            DebugLog($"⏹️🔉 Stopping all instances of '{soundName}' with {fadeOutDuration}s fade");

            int playingBefore = AudioManager.Instance.GetAudioSourcesPlayingSound(soundName).Count;
            AudioManager.Instance.StopSound(soundName, fadeOutDuration);

            DebugLog($"📊 Started fade out for {playingBefore} instances of '{soundName}'");
        }

        private void ExecuteStopAllSounds()
        {
            DebugLog("⏹️ Stopping ALL sounds in the scene");

            int totalBefore = AudioManager.Instance.GetActiveAudioCount();
            AudioManager.Instance.StopAll();
            int totalAfter = AudioManager.Instance.GetActiveAudioCount();

            DebugLog($"📊 Stopped {totalBefore - totalAfter} total audio instances");
        }

        private void ExecuteStopOnGameObject()
        {
            DebugLog($"⏹️ Stopping all sounds on GameObject: {targetGameObject.name} immediately");

            AudioSource[] sourcesBefore = targetGameObject.GetComponents<AudioSource>();
            AudioManager.Instance.StopAllOnGameObject(targetGameObject);
            AudioSource[] sourcesAfter = targetGameObject.GetComponents<AudioSource>();

            DebugLog(
                $"📊 AudioSources on {targetGameObject.name}: {sourcesBefore.Length} → {sourcesAfter.Length}"
            );
        }

        private void ExecuteStopOnGameObjectWithFade()
        {
            DebugLog(
                $"⏹️🔉 Stopping all sounds on GameObject: {targetGameObject.name} with {fadeOutDuration}s fade"
            );

            AudioSource[] sourcesBefore = targetGameObject.GetComponents<AudioSource>();
            AudioManager.Instance.StopAllOnGameObject(targetGameObject, fadeOutDuration);

            DebugLog(
                $"📊 Started fade out for {sourcesBefore.Length} AudioSources on {targetGameObject.name}"
            );
        }

        private void ExecuteFindGameObjectsPlayingSound()
        {
            DebugLog($"🔍 Finding GameObjects playing '{soundName}'");

            List<GameObject> playingObjects = AudioManager.Instance.FindGameObjectsPlayingSound(
                soundName
            );

            if (playingObjects.Count == 0)
            {
                DebugLog($"📭 No GameObjects currently playing '{soundName}'");
            }
            else
            {
                DebugLog($"📍 Found {playingObjects.Count} GameObjects playing '{soundName}':");
                for (int i = 0; i < playingObjects.Count; i++)
                {
                    DebugLog($"  [{i + 1}] {playingObjects[i].name}");
                }
            }
        }

        private void ExecuteTestAudioSourceTracking()
        {
            DebugLog("📊 Testing AudioSource tracking system");

            // Play multiple sounds
            DebugLog("  🎵 Playing 3 different sounds...");
            AudioSource audio1 = AudioManager.Instance.Play(soundName, targetGameObject);
            AudioSource audio2 = AudioManager.Instance.Play(soundName, gameObject);
            AudioSource audio3 = AudioManager.Instance.Play(soundName, targetGameObject);

            // Report tracking info
            DebugLog(
                $"  📈 Total active audio instances: {AudioManager.Instance.GetActiveAudioCount()}"
            );
            DebugLog(
                $"  🎯 AudioSources playing '{soundName}': {AudioManager.Instance.GetAudioSourcesPlayingSound(soundName).Count}"
            );
            DebugLog(
                $"  🎮 GameObjects playing '{soundName}': {AudioManager.Instance.FindGameObjectsPlayingSound(soundName).Count}"
            );

            // Test stopping one instance
            if (audio2 != null)
            {
                DebugLog("  ⏹️ Stopping one specific AudioSource...");
                AudioManager.Instance.Stop(audio2);
                DebugLog(
                    $"  📉 Remaining active instances: {AudioManager.Instance.GetActiveAudioCount()}"
                );
            }
        }

        private void LogAvailableSounds()
        {
            List<string> soundNames = AudioManager.Instance.GetAllSoundNames();

            DebugLog($"🎵 Available sounds in AudioManager ({soundNames.Count} total):");

            if (soundNames.Count == 0)
            {
                DebugLog("  ⚠️ No sounds configured in AudioManager!", LogType.Warning);
            }
            else
            {
                for (int i = 0; i < soundNames.Count; i++)
                {
                    DebugLog($"  [{i + 1}] {soundNames[i]}");
                }
            }
        }

        private void DebugLog(string message, LogType logType = LogType.Log)
        {
            if (!enableDebugLogging)
                return;

            string prefix = "<color=cyan>[SampleSoundPlayer]</color>";
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

        [ContextMenu("Play Sound")]
        public void PlaySound()
        {
            sampleType = SampleType.Play;
            ExecuteSampleType();
        }

        [ContextMenu("Play With Fade In")]
        public void PlayWithFadeIn()
        {
            sampleType = SampleType.PlayWithFadeIn;
            ExecuteSampleType();
        }

        [ContextMenu("Play With Fade Out")]
        public void PlayWithFadeOut()
        {
            sampleType = SampleType.PlayWithFadeOut;
            ExecuteSampleType();
        }

        [ContextMenu("Play With Both Fades")]
        public void PlayWithBothFades()
        {
            sampleType = SampleType.PlayWithFadeInAndOut;
            ExecuteSampleType();
        }

        [ContextMenu("Stop All Sounds")]
        public void StopAllSounds()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopAll();
        }

        [ContextMenu("Stop Sound With Fade")]
        public void StopSoundWithFade()
        {
            sampleType = SampleType.StopSpecificSoundWithFade;
            ExecuteSampleType();
        }

        [ContextMenu("Stop On GameObject With Fade")]
        public void StopOnGameObjectWithFade()
        {
            sampleType = SampleType.StopOnGameObjectWithFade;
            ExecuteSampleType();
        }

        [ContextMenu("Print AudioManager Status")]
        public void PrintAudioManagerStatus()
        {
            if (AudioManager.Instance == null)
            {
                DebugLog("❌ AudioManager is null!", LogType.Error);
                return;
            }

            DebugLog("📊 AudioManager Status:");
            DebugLog($"  🎵 Total active instances: {AudioManager.Instance.GetActiveAudioCount()}");
            DebugLog($"  📋 Available sounds: {AudioManager.Instance.GetAllSoundNames().Count}");
            DebugLog(
                $"  🎯 Is '{soundName}' playing: {AudioManager.Instance.IsPlaying(soundName)}"
            );
        }

        [ContextMenu("Test All Sample Types")]
        public void TestAllSampleTypes()
        {
            StartCoroutine(TestAllSampleTypesCoroutine());
        }

        private IEnumerator TestAllSampleTypesCoroutine()
        {
            DebugLog("🧪 Testing all sample types sequentially...");

            SampleType[] allTypes = (SampleType[])System.Enum.GetValues(typeof(SampleType));

            foreach (SampleType type in allTypes)
            {
                DebugLog($"  Testing: {type}");
                sampleType = type;
                ExecuteSampleType();
                yield return new WaitForSeconds(2f);
            }

            DebugLog("✅ All sample types tested!");
        }
    }
}
