using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource _music;

    public event Action<float> OnVolumeChanged;
    public event Action<bool> OnMuteChanged;

    private void Awake()
    {
        Instance = this;

        if (_music == null)
            _music = GetComponent<AudioSource>();
    }

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        bool enabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        ApplyVolume(volume);
        ApplyMute(enabled);
    }

    public void SetVolume(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetMusicEnabled(bool value)
    {
        ApplyMute(value);
        PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
    }

    private void ApplyVolume(float value)
    {
        _music.volume = value;
        OnVolumeChanged?.Invoke(value);
    }

    private void ApplyMute(bool value)
    {
        _music.mute = !value;
        OnMuteChanged?.Invoke(value);
    }
}