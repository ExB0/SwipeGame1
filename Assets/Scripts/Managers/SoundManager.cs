using System;
using UnityEngine;
using PlatformPuzzle.Audio;

namespace PlatformPuzzle.Managers
{
    public class SoundManager : MonoBehaviour
    {
        private const int DefaultMusicIndex = 0;

        public static SoundManager Instance;

        [Header("Services")]
        [SerializeField] private AudioSettingsManager _audioSettings;
        [SerializeField] private MusicPlayer _musicPlayer;
        [SerializeField] private SoundEffectsPlayer _soundEffects;
        
        public float CurrentVolume => _audioSettings != null ? _audioSettings.CurrentVolume : 1f;
        public bool IsSoundEnabled => _audioSettings != null ? _audioSettings.IsSoundEnabled : true;

        public event Action<float> VolumeChanged
        {
            add
            {
                if (_audioSettings != null)
                    _audioSettings.VolumeChanged += value;
            }
            remove
            {
                if (_audioSettings != null)
                    _audioSettings.VolumeChanged -= value;
            }
        }

        public event Action<bool> SoundEnabledChanged
        {
            add
            {
                if (_audioSettings != null)
                    _audioSettings.SoundEnabledChanged += value;
            }
            remove
            {
                if (_audioSettings != null)
                    _audioSettings.SoundEnabledChanged -= value;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("SoundManager: дубликат уничтожен");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ValidateServices();
        }

        private void Start()
        {
            if (_audioSettings != null)
                _audioSettings.LoadSettings();

            if (_musicPlayer != null)
                _musicPlayer.PlayMusic(DefaultMusicIndex);
        }

        public void PlayMusic(int index)
        {
            if (_musicPlayer == null)
            {
                Debug.LogError("SoundManager: MusicPlayer не назначен!");
                return;
            }
            _musicPlayer.PlayMusic(index);
        }

        public void PauseMusic()
        {
            if (_musicPlayer == null)
            {
                Debug.LogError("SoundManager: MusicPlayer не назначен!");
                return;
            }
            _musicPlayer.PauseMusic();
        }

        public void ResumeMusic()
        {
            if (_musicPlayer == null)
            {
                Debug.LogError("SoundManager: MusicPlayer не назначен!");
                return;
            }
            _musicPlayer.ResumeMusic();
        }

        public void SetVolume(float value)
        {
            if (_audioSettings == null)
            {
                Debug.LogError("SoundManager: AudioSettingsManager не назначен!");
                return;
            }
            _audioSettings.SetVolume(value);
        }

        public void SetSoundEnabled(bool value)
        {
            if (_audioSettings == null)
            {
                Debug.LogError("SoundManager: AudioSettingsManager не назначен!");
                return;
            }
            _audioSettings.SetSoundEnabled(value);
        }

        public void PlayWinSound()
        {
            if (_soundEffects == null)
            {
                Debug.LogError("SoundManager: SoundEffectsPlayer не назначен!");
                return;
            }
            _soundEffects.PlayWinSound();
        }

        public void PlayLoseSound()
        {
            if (_soundEffects == null)
            {
                Debug.LogError("SoundManager: SoundEffectsPlayer не назначен!");
                return;
            }
            _soundEffects.PlayLoseSound();
        }

        private void ValidateServices()
        {
            if (_audioSettings == null)
                Debug.LogError("SoundManager: AudioSettingsManager не назначен в инспекторе!");

            if (_musicPlayer == null)
                Debug.LogError("SoundManager: MusicPlayer не назначен в инспекторе!");

            if (_soundEffects == null)
                Debug.LogError("SoundManager: SoundEffectsPlayer не назначен в инспекторе!");
        }
    }
}