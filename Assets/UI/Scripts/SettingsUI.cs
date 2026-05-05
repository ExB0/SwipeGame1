using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject[] _showSettings;
    [SerializeField] private GameObject[] _hideSettings;

    private bool _isButtonPressed = false;

    public void ShowSetings()
    {
        _isButtonPressed = !_isButtonPressed;

        SetActive(_showSettings, _isButtonPressed);
        SetActive(_hideSettings, !_isButtonPressed);
    }

    private void SetActive(GameObject[] objects, bool state)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }
}
