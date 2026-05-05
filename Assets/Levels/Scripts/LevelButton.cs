using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelButton : MonoBehaviour
{
     [SerializeField] private int _levelIndex;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _lockIcon;

    public void Init(int unlockedLevel)
    {
        bool isUnlocked = _levelIndex <= unlockedLevel;

        _button.interactable = isUnlocked;

        if (_lockIcon != null)
            _lockIcon.SetActive(!isUnlocked);
    }

    public void OnClick()
    {
        if (!_button.interactable) return;

        LevelConstructor.Instance.LoadLevel(_levelIndex);
    }
}
