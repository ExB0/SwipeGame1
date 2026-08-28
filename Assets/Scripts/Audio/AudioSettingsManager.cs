using System;

using UnityEngine;

using UnityEngine.Audio;

namespace PlatformPuzzle.Audio
{
    public class AudioSettingsManager : MonoBehaviour
    {
        private const string VolumeKey = "MusicVolume";
        private const string EnabledKey = "MusicEnabled";

        private const float MinVolume = 0.0001f;
        private const float MaxVolume = 1f;
        private const float DefaultVolume = 1f;
        private const int DefaultEnabled = 1;
        private const float DecibelMultiplier = 20f;
        private const float MutedDecibels = -80f;

        [SerializeField] private AudioMixer _audioMixer;

        private float _currentVolume = MaxVolume;
        private bool _isSoundEnabled = true;

        public event Action<float> VolumeChanged;
        public event Action<bool> SoundEnabledChanged;

        public float CurrentVolume => _currentVolume;
        public bool IsSoundEnabled => _isSoundEnabled;

        public void LoadSettings()
        {
            float volume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
            bool enabled = PlayerPrefs.GetInt(EnabledKey, DefaultEnabled) == DefaultEnabled;

            ApplyVolume(volume);
            if (!enabled)
                ApplyMute(true);
            else
                ApplyMute(false);
        }

        public void SetVolume(float value)
        {
            value = Mathf.Clamp01(value);
            ApplyVolume(value);
            PlayerPrefs.SetFloat(VolumeKey, value);
        }

        public void SetSoundEnabled(bool value)
        {
            _isSoundEnabled = value;
            ApplyMute(!value);
            PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
        }

        public void ApplyVolume(float value)
        {
            if (_audioMixer == null)
            {
                Debug.LogError("AudioSettingsManager: AudioMixer не назначен!");
                return;
            }

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogError($"AudioSettingsManager: некорректное значение громкости {value}");
                return;
            }

            value = Mathf.Clamp(value, MinVolume, MaxVolume);
            _currentVolume = value;

            if (_isSoundEnabled)
            {
                float decibelValue = Mathf.Log10(value) * DecibelMultiplier;
                _audioMixer.SetFloat("MusicVolume", decibelValue);
                _audioMixer.SetFloat("SFXVolume", decibelValue);
            }

            VolumeChanged?.Invoke(value);
        }

        public void ApplyMute(bool mute)
        {
            if (_audioMixer == null)
            {
                Debug.LogError("AudioSettingsManager: AudioMixer не назначен!");
                return;
            }

            if (mute)
            {
                _audioMixer.SetFloat("MusicVolume", MutedDecibels);
                _audioMixer.SetFloat("SFXVolume", MutedDecibels);
                _isSoundEnabled = false;
            }
            else
            {
                float decibelValue = Mathf.Log10(_currentVolume) * DecibelMultiplier;
                _audioMixer.SetFloat("MusicVolume", decibelValue);
                _audioMixer.SetFloat("SFXVolume", decibelValue);
                _isSoundEnabled = true;
            }

            SoundEnabledChanged?.Invoke(_isSoundEnabled);
        }
    }
}