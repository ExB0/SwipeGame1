using UnityEngine;

namespace PlatformPuzzle.Audio
{
    public class SoundEffectsPlayer : MonoBehaviour
    {
        private const float MinPitchVariation = 0.95f;
        private const float MaxPitchVariation = 1.05f;
        private const float MinVolumeVariation = 0.9f;
        private const float MaxVolumeVariation = 1f;

        [SerializeField] private AudioSource _winSound;
        [SerializeField] private AudioSource _loseSound;

        public void PlayWinSound()
        {
            PlayRandomized(_winSound);
        }

        public void PlayLoseSound()
        {
            PlayRandomized(_loseSound);
        }

        private void PlayRandomized(AudioSource source)
        {
            if (source == null)
                return;

            source.pitch = Random.Range(MinPitchVariation, MaxPitchVariation);
            source.volume = Random.Range(MinVolumeVariation, MaxVolumeVariation);
            source.Play();
        }
    }
}