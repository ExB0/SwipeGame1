using PlatformPuzzle.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformPuzzle.UI
{
    public class SoundSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Toggle _toggle;

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.VolumeChanged += OnVolumeChanged;
                SoundManager.Instance.MuteChanged += OnMuteChanged;
            }

            UpdateUI();
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.VolumeChanged -= OnVolumeChanged;
                SoundManager.Instance.MuteChanged -= OnMuteChanged;
            }
        }

        private void OnSliderValueChanged(float value)
        {
            SoundManager.Instance?.SetVolume(value);
        }

        private void OnToggleValueChanged(bool value)
        {
            SoundManager.Instance?.SetMusicEnabled(value);
        }

        private void OnVolumeChanged(float value)
        {
            _slider.SetValueWithoutNotify(value);
        }

        private void OnMuteChanged(bool isMuted)
        {
            _toggle.SetIsOnWithoutNotify(isMuted);
        }

        private void UpdateUI()
        {
            if (SoundManager.Instance == null)
            {
                return;
            }

            float volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            bool enabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

            _slider.SetValueWithoutNotify(volume);
            _toggle.SetIsOnWithoutNotify(enabled);
        }
    }
}