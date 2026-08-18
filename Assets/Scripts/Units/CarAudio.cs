using UnityEngine;

namespace Units
{
    public class CarAudio : MonoBehaviour
    {
        private const float MinPitchVariation = 0.95f;
        private const float MaxPitchVariation = 1.05f;
        private const float MinVolumeVariation = 0.85f;
        private const float MaxVolumeVariation = 1f;

        [SerializeField] private AudioSource _engineStartAudio;

        public void PlayEngineStart()
        {
            if (_engineStartAudio == null)
            {
                return;
            }

            _engineStartAudio.pitch =
                Random.Range(MinPitchVariation, MaxPitchVariation);

            _engineStartAudio.volume =
                Random.Range(MinVolumeVariation, MaxVolumeVariation);

            _engineStartAudio.Play();
        }
    }
}