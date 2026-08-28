using System.Collections.Generic;

using UnityEngine;

namespace PlatformPuzzle.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private List<AudioSource> _musicSources;
        
        private AudioSource _currentMusic;

        public void PlayMusic(int index)
        {
            if (_musicSources == null || index < 0 || index >= _musicSources.Count)
            {
                Debug.LogError($"MusicPlayer: invalid music index {index}");
                return;
            }

            AudioSource target = _musicSources[index];
            if (target == null)
                return;

            if (_currentMusic == target)
                return;

            foreach (AudioSource music in _musicSources)
            {
                if (music == null)
                    continue;

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
    }
}