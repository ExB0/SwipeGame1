using UnityEngine;

using Effects;

namespace Units
{
    public class CarEffects : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _smoke;
        [SerializeField] private ScaleShakeEffect _scaleShakeEffect;

        public bool IsShaking =>
            _scaleShakeEffect != null &&
            _scaleShakeEffect.IsShaking;

        private void Awake()
        {
            if (_scaleShakeEffect == null)
            {
                _scaleShakeEffect = GetComponent<ScaleShakeEffect>();
            }

            StopSmoke();
        }

        public void Shake()
        {
            _scaleShakeEffect?.Shake();
        }

        public void PlaySmoke()
        {
            if (_smoke == null)
            {
                return;
            }

            _smoke.Clear();
            _smoke.Play();
        }

        public void StopSmoke()
        {
            _smoke?.Stop();
        }
    }
}