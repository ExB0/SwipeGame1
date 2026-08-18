using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace PlatformPuzzle.Managers
{
    public class SoundManager : MonoBehaviour
    {
        private const string VolumeKey = "MusicVolume";
        private const string EnabledKey = "MusicEnabled";
        private const float MinVolume = 0.0001f;
        private const float DecibelMultiplier = 20f;
        private const float MutedDecibels = -80f;
        private const float UnmutedDecibels = 0f;
        private const int DefaultMusicIndex = 0;

        public static SoundManager Instance;

        [Header("Music Sources")]
        [SerializeField] private List<AudioSource> _musicSources;
        [SerializeField] private AudioSource _winSound;
        [SerializeField] private AudioSource _loseSound;
        [SerializeField] private AudioMixer _audioMixer;

        private AudioSource _currentMusic;
        private float _currentVolume = 1f;
        private bool _isSoundEnabled = true;

        public event Action<float> VolumeChanged;
        public event Action<bool> SoundEnabledChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("SoundManager: дубликат уничтожен");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            bool enabled = PlayerPrefs.GetInt(EnabledKey, 1) == 1;

            ApplyVolume(volume);

            if (!enabled)
            {
                ApplyMute(true);
            }
            else
            {
                ApplyMute(false);
            }

            PlayMusic(DefaultMusicIndex);
        }

        public void PlayMusic(int index)
        {
            if (_musicSources == null || index < 0 || index >= _musicSources.Count)
            {
                Debug.LogError($"SoundManager: invalid music index {index}");
                return;
            }

            AudioSource target = _musicSources[index];
            if (target == null) return;

            if (_currentMusic == target) return;

            foreach (AudioSource music in _musicSources)
            {
                if (music == null) continue;
                if (music == target)
                {
                    music.Play();
                    _currentMusic = music;
                }
                else
                {
                    music.Stop();
                }
            }
        }

        public void PauseMusic()
        {
            if (_currentMusic != null && _currentMusic.isPlaying)
                _currentMusic.Pause();
        }

        public void ResumeMusic()
        {
            if (_currentMusic != null)
                _currentMusic.UnPause();
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

        public void PlayWinSound()
        {
            PlayRandomized(_winSound);
        }

        public void PlayLoseSound()
        {
            PlayRandomized(_loseSound);
        }

        private void ApplyVolume(float value)
        {
            if (_audioMixer == null)
            {
                Debug.LogError("SoundManager: AudioMixer не назначен!");
                return;
            }

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogError($"SoundManager: некорректное значение громкости {value}");
                return;
            }

            value = Mathf.Clamp(value, MinVolume, 1f);
            _currentVolume = value;

            if (_isSoundEnabled)
            {
                float db = Mathf.Log10(value) * DecibelMultiplier;
                _audioMixer.SetFloat("MusicVolume", db);
                _audioMixer.SetFloat("SFXVolume", db);
            }

            VolumeChanged?.Invoke(value);
        }

        private void ApplyMute(bool mute)
        {
            if (_audioMixer == null)
            {
                Debug.LogError("SoundManager: AudioMixer не назначен!");
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
                float db = Mathf.Log10(_currentVolume) * DecibelMultiplier;
                _audioMixer.SetFloat("MusicVolume", db);
                _audioMixer.SetFloat("SFXVolume", db);
                _isSoundEnabled = true;
            }

            SoundEnabledChanged?.Invoke(_isSoundEnabled);
        }

        private void PlayRandomized(AudioSource source)
        {
            if (source == null) return;

            source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            source.volume = UnityEngine.Random.Range(0.9f, 1f);
            source.Play();
        }
    }
}