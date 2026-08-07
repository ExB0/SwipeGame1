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

        public static SoundManager Instance;

        [Header("Music Sources")]
        [SerializeField] private List<AudioSource> _musicSources;

        [SerializeField] private AudioSource _winSound;
        [SerializeField] private AudioSource _loseSound;
        [SerializeField] private AudioMixer _audioMixer;

        private AudioSource _currentMusic;

        public event Action<float> OnVolumeChanged;
        public event Action<bool> OnMuteChanged;

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
            ApplyMute(enabled);
            PlayMusic(0);
        }

        public void PlayMusic(int index)
        {
            if (index < 0 || index >= _musicSources.Count)
            {
                Debug.LogError(
                    $"SoundManager: invalid music index {index}"
                );

                return;
            }

            AudioSource target = _musicSources[index];

            if (target == null)
            {
                return;
            }

            if (_currentMusic == target)
            {
                return;
            }

            foreach (AudioSource music in _musicSources)
            {
                if (music == null)
                {
                    continue;
                }

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
            {
                _currentMusic.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (_currentMusic != null)
            {
                _currentMusic.UnPause();
            }
        }

        public void SetVolume(float value)
        {
            if (value < 0f || value > 1f)
            {
                Debug.LogWarning(
                    $"SoundManager: громкость вне диапазона {value}, будет зажата"
                );
            }

            ApplyVolume(value);
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
        }

        public void SetMusicEnabled(bool value)
        {
            ApplyMute(value);
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
                Debug.LogError(
                    $"SoundManager: некорректное значение громкости {value}"
                );

                return;
            }

            value = Mathf.Clamp(value, 0.0001f, 1f);

            float db = Mathf.Log10(value) * 20;

            _audioMixer.SetFloat("MusicVolume", db);
            _audioMixer.SetFloat("SFXVolume", db);

            OnVolumeChanged?.Invoke(value);
        }

        private void ApplyMute(bool value)
        {
            if (_audioMixer == null)
            {
                Debug.LogError("SoundManager: AudioMixer не назначен!");
                return;
            }

            float db = value ? 0f : -80f;

            _audioMixer.SetFloat("MusicVolume", db);
            _audioMixer.SetFloat("SFXVolume", db);

            OnMuteChanged?.Invoke(value);
        }

        private void PlayRandomized(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            source.volume = UnityEngine.Random.Range(0.9f, 1f);

            source.Play();
        }
    }
}