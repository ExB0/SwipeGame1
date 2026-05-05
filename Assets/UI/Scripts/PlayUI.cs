using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayUI : MonoBehaviour
{
    [SerializeField] private GameObject _levelsWindow;
    [SerializeField] private GameObject[] _hideWindows;

    public void ShowLevelsPanel()
    {
        HideAll();
        _levelsWindow.SetActive(true);
    }

    private void HideAll()
    {
        foreach (var window in _hideWindows)
        {
            if (window != null)
                window.SetActive(false);
        }
    }
}
