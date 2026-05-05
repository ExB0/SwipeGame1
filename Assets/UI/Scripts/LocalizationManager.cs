using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YG;

public class LocalizationManager : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TMP_Text _startText;
    [SerializeField] private TMP_Text _settingsText;
    [SerializeField] private TMP_Text _levelChoiceText;
    [SerializeField] private TMP_Text _menuText;

    private void OnEnable()
    {
        YG2.onSwitchLang += UpdateLanguage;
    }

    private void OnDisable()
    {
        YG2.onSwitchLang -= UpdateLanguage;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(YG2.lang))
            UpdateLanguage(YG2.lang);
    }

    private void UpdateLanguage(string lang)
    {
        if (_startText == null || _settingsText == null || _levelChoiceText == null)
        {
            Debug.LogError("LocalizationManager: не все TMP_Text назначены!");
            return;
        }

        switch (lang)
        {
            case "ru":
                SetTexts("Нажмите на машину чтобы она двигалась",
                 "Настройки",
                  "Выберите уровень",
                  "Меню");
                break;

            case "tr":
                SetTexts("Arabayı hareket ettirmek için dokunun",
                 "Ayarlar",
                  "Bir seviye seçin",
                  "Menü");
                break;

            default:
                SetTexts("Tab a car to move",
                 "Settings",
                  "Select a level",
                  "Menu");
                break;
        }
    }

    private void SetTexts(string start, string settings, string exit, string menu)
    {
        _startText.text = start;
        _settingsText.text = settings;
        _levelChoiceText.text = exit;
        _menuText.text = menu;
    }

    public void SetRU()
    {
        if (YG2.lang == "ru") return;
        YG2.SwitchLanguage("ru");
    }

    public void SetEN()
    {
        if (YG2.lang == "en") return;
        YG2.SwitchLanguage("en");
    }

    public void SetTR()
    {
        if (YG2.lang == "tr") return;
        YG2.SwitchLanguage("tr");
    }
}
