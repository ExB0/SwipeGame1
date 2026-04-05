using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _music;

    public void SetMusicEnabled(bool value)
    {
        if (_music == null) return;

        _music.enabled = value;
    }

    public void SetVolume(float volume)
    {
        if (_music == null) return;

        AudioListener.volume = volume;
    }
}
