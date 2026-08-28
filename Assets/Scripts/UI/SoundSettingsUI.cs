using UnityEngine;
using UnityEngine.UI;
using PlatformPuzzle.Managers;

namespace PlatformPuzzle.UI
{
    public class SoundSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Toggle _toggle;

        private void OnEnable()
        {
            if (_slider == null || _toggle == null)
            {
                Debug.LogError($"{name}: Slider or Toggle is null");
                return;
            }

            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.VolumeChanged += OnVolumeChanged;
                SoundManager.Instance.SoundEnabledChanged += OnSoundEnabledChanged;
            }

            UpdateUI();
        }

        private void OnDisable()
        {
            if (_slider == null || _toggle == null)
            {
                return;
            }

            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.VolumeChanged -= OnVolumeChanged;
                SoundManager.Instance.SoundEnabledChanged -= OnSoundEnabledChanged;
            }
        }

        private void OnSliderValueChanged(float value)
        {
            SoundManager.Instance?.SetVolume(value);
        }

        private void OnToggleValueChanged(bool value)
        {
            SoundManager.Instance?.SetSoundEnabled(value);
        }

        private void OnVolumeChanged(float value)
        {
            if (_slider != null)
            {
                _slider.SetValueWithoutNotify(value);
            }
        }

        private void OnSoundEnabledChanged(bool isEnabled)
        {
            if (_toggle != null)
            {
                _toggle.SetIsOnWithoutNotify(isEnabled);
            }
        }

        private void UpdateUI()
        {
            if (_slider == null || _toggle == null || SoundManager.Instance == null)
            {
                return;
            }
            
            float volume = SoundManager.Instance.CurrentVolume;
            bool enabled = SoundManager.Instance.IsSoundEnabled;

            _slider.SetValueWithoutNotify(volume);
            _toggle.SetIsOnWithoutNotify(enabled);
        }
    }
}