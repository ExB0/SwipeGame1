using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Music Sources")]
    [SerializeField] private List<AudioSource> _musicSources;

    private AudioSource _currentMusic;

    public event Action<float> OnVolumeChanged;
    public event Action<bool> OnMuteChanged;

    private const string VolumeKey = "MusicVolume";
    private const string EnabledKey = "MusicEnabled";

    private void Awake()
    {
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
            Debug.LogError($"SoundManager: invalid music index {index}");
            return;
        }

        var target = _musicSources[index];

        if (target == null) return;

        if (_currentMusic == target)
            return;

        foreach (var music in _musicSources)
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
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    public void SetMusicEnabled(bool value)
    {
        ApplyMute(value);
        PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
    }

    private void ApplyVolume(float value)
    {
        foreach (var music in _musicSources)
        {
            if (music != null)
                music.volume = value;
        }

        OnVolumeChanged?.Invoke(value);
    }

    private void ApplyMute(bool value)
    {
        foreach (var music in _musicSources)
        {
            if (music != null)
                music.mute = !value;
        }

        OnMuteChanged?.Invoke(value);
    }
}