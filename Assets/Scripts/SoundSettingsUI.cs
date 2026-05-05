using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Toggle _toggle;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        bool enabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        _slider.SetValueWithoutNotify(volume);
        _toggle.SetIsOnWithoutNotify(enabled);

        _slider.onValueChanged.AddListener(OnSliderChanged);
        _toggle.onValueChanged.AddListener(OnToggleChanged);

        SoundManager.Instance.OnVolumeChanged += UpdateSlider;
        SoundManager.Instance.OnMuteChanged += UpdateToggle;
    }

    private void OnDestroy()
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.OnVolumeChanged -= UpdateSlider;
        SoundManager.Instance.OnMuteChanged -= UpdateToggle;
    }

    private void OnSliderChanged(float value)
    {
        SoundManager.Instance.SetVolume(value);
    }

    private void OnToggleChanged(bool value)
    {
        SoundManager.Instance.SetMusicEnabled(value);
    }

    private void UpdateSlider(float value)
    {
        _slider.SetValueWithoutNotify(value);
    }

    private void UpdateToggle(bool value)
    {
        _toggle.SetIsOnWithoutNotify(value);
    }
}