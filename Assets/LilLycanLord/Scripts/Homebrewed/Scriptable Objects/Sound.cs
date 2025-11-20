using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    [Serializable]
    public struct BeatInfo
    {
        public float BPM;
        public float BeatInterval;
        public bool HasBeatInfo;
        public float Duration;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Sound", menuName = "LilLycanLord/Audio/Sound")]
    public class Sound : ScriptableObject
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        [Header("Sound Information")]
        [SerializeField]
        [Tooltip("Display name for this sound in the inspector")]
        private string displayName;

        [SerializeField]
        [Tooltip("Category for organizing sounds (Music, SFX, Voice, etc.)")]
        private string category = "SFX";

        [SerializeField]
        [Tooltip("Tags for searching and filtering sounds")]
        private string[] tags = new string[0];

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Space(10)]
        [Header("Audio Configuration")]
        [SerializeField]
        [Tooltip("The audio clip to play")]
        public AudioClip audioClip;

        [SerializeField]
        [Tooltip("Beats per minute for rhythm-based games (0 = not rhythm-based)")]
        [Range(0f, 300f)]
        public float BPM = 0;

        [SerializeField]
        [Tooltip("Duration override (leave 0 to use clip length)")]
        [Min(0f)]
        public float customDuration = 0f;

        [Space(10)]
        [Header("AudioSource Properties")]
        [SerializeField]
        [Tooltip("Should this sound loop continuously?")]
        public bool loop = false;

        [SerializeField]
        [Tooltip("Playback pitch (affects speed and tone)")]
        [Range(0.1f, 3.0f)]
        public float pitch = 1.0f;

        [SerializeField]
        [Tooltip("Volume level")]
        [Range(0.0f, 1.0f)]
        public float volume = 1.0f;

        [SerializeField]
        [Tooltip("Spatial blend (0 = 2D, 1 = 3D)")]
        [Range(0f, 1f)]
        public float spatialBlend = 0f;

        [Space(10)]
        [Header("Advanced Settings")]
        [SerializeField]
        [Tooltip("Priority for audio source (0 = highest, 256 = lowest)")]
        [Range(0, 256)]
        public int priority = 128;

        [SerializeField]
        [Tooltip("Doppler level for 3D sounds")]
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;

        [SerializeField]
        [Tooltip("How much reverb affects this sound")]
        [Range(0f, 1.1f)]
        public float reverbZoneMix = 1f;

        [Space(10)]
        [Header("Fade Effects")]
        [SerializeField]
        [Tooltip("Enable fade in effect")]
        public bool enableFadeIn = false;

        [SerializeField]
        [Tooltip("Fade in duration in seconds")]
        [Min(0f)]
        public float fadeInDuration = 0.5f;

        [SerializeField]
        [Tooltip("Enable fade out effect")]
        public bool enableFadeOut = false;

        [SerializeField]
        [Tooltip("Fade out duration in seconds")]
        [Min(0f)]
        public float fadeOutDuration = 0.5f;

        [NonSerialized]
        public AudioSource audioSource;

        //* Runtime tracking fields
        [NonSerialized]
        private bool isPlaying = false;

        [NonSerialized]
        private float playStartTime = 0f;

        [NonSerialized]
        private bool isPaused = false;

        [NonSerialized]
        private float pauseTime = 0f;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝

        public string DisplayName => !string.IsNullOrEmpty(displayName) ? displayName : name;
        public string Category => category;
        public string[] Tags => tags;
        public float Duration
        {
            get
            {
                if (customDuration > 0f)
                    return customDuration;
                return audioClip != null ? audioClip.length : 0f;
            }
        }
        public bool IsPlaying
        {
            get
            {
                if (audioSource != null)
                    return audioSource.isPlaying;
                return isPlaying;
            }
        }
        public bool IsPaused => isPaused;
        public float PlaybackTime
        {
            get
            {
                if (audioSource != null)
                    return audioSource.time;
                if (isPlaying && !isPaused)
                    return Time.time - playStartTime;
                return pauseTime;
            }
        }
        public bool HasBeatInfo => BPM > 0f;
        public float BeatInterval => HasBeatInfo ? 60f / BPM : 0f;

        public void ConfigureAudioSource(AudioSource source)
        {
            if (source == null)
            {
                Debug.LogError($"Sound '{DisplayName}': Cannot configure null AudioSource");
                return;
            }

            if (audioClip == null)
            {
                Debug.LogError($"Sound '{DisplayName}': No AudioClip assigned");
                return;
            }

            source.clip = audioClip;
            source.loop = loop;
            source.pitch = pitch;
            source.volume = volume;
            source.spatialBlend = spatialBlend;
            source.priority = priority;
            source.dopplerLevel = dopplerLevel;
            source.reverbZoneMix = reverbZoneMix;

            audioSource = source;
        }

        public void Play()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(this.name);
            }
            else
            {
                Debug.LogWarning(
                    $"Sound '{DisplayName}': AudioManager not available, cannot play sound"
                );
            }
        }

        public void Stop()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopSound(this.name);
            }
            else if (audioSource != null)
            {
                audioSource.Stop();
                isPlaying = false;
                isPaused = false;
            }
        }

        public void Pause()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                isPaused = true;
                pauseTime = audioSource.time;
            }
        }

        public void Resume()
        {
            if (audioSource != null && isPaused)
            {
                audioSource.UnPause();
                isPaused = false;
            }
        }

        public bool HasTag(string tag)
        {
            return Array.Exists(
                tags,
                t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)
            );
        }

        public bool IsInCategory(string categoryName)
        {
            return string.Equals(category, categoryName, StringComparison.OrdinalIgnoreCase);
        }

        public BeatInfo GetBeatInfo()
        {
            return new BeatInfo
            {
                BPM = this.BPM,
                BeatInterval = this.BeatInterval,
                HasBeatInfo = this.HasBeatInfo,
                Duration = this.Duration,
            };
        }

        public bool IsValid()
        {
            List<string> errors = new List<string>();

            if (audioClip == null)
                errors.Add("No AudioClip assigned");

            if (string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(name))
                errors.Add("No display name or asset name");

            if (volume < 0f || volume > 1f)
                errors.Add("Volume out of range (0-1)");

            if (pitch < 0.1f || pitch > 3f)
                errors.Add("Pitch out of range (0.1-3)");

            if (errors.Count > 0)
            {
                Debug.LogWarning(
                    $"Sound '{DisplayName}' validation errors: {string.Join(", ", errors)}"
                );
                return false;
            }

            return true;
        }

        public string GetInfo()
        {
            return $"Sound: {DisplayName} | Category: {Category} | Duration: {Duration:F2}s | "
                + $"Volume: {volume:F2} | Pitch: {pitch:F2} | Loop: {loop} | "
                + $"BPM: {(HasBeatInfo ? BPM.ToString("F1") : "None")} | "
                + $"Tags: [{string.Join(", ", tags)}]";
        }

        public Sound CreateVariant(
            string variantName,
            float? volumeMultiplier = null,
            float? pitchMultiplier = null
        )
        {
            Sound variant = CreateInstance<Sound>();
            variant.name = $"{name}_{variantName}";
            variant.displayName = $"{DisplayName} ({variantName})";
            variant.audioClip = this.audioClip;
            variant.category = this.category;
            variant.tags = (string[])this.tags.Clone();
            variant.BPM = this.BPM;
            variant.customDuration = this.customDuration;
            variant.loop = this.loop;
            variant.pitch = this.pitch * (pitchMultiplier ?? 1f);
            variant.volume = this.volume * (volumeMultiplier ?? 1f);
            variant.spatialBlend = this.spatialBlend;
            variant.priority = this.priority;
            variant.dopplerLevel = this.dopplerLevel;
            variant.reverbZoneMix = this.reverbZoneMix;
            variant.enableFadeIn = this.enableFadeIn;
            variant.fadeInDuration = this.fadeInDuration;
            variant.enableFadeOut = this.enableFadeOut;
            variant.fadeOutDuration = this.fadeOutDuration;

            return variant;
        }

        internal void OnPlayStarted()
        {
            isPlaying = true;
            isPaused = false;
            playStartTime = Time.time;
        }

        internal void OnPlayStopped()
        {
            isPlaying = false;
            isPaused = false;
            playStartTime = 0f;
            pauseTime = 0f;
        }

        //* ╔═══════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚═══════════════════════════════╝
    }
}
