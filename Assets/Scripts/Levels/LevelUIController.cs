using Cysharp.Threading.Tasks;
using UnityEngine;

using PlatformPuzzle.UI;

namespace PlatformPuzzle.Levels
{
    public class LevelUIController : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] private GameObject _winWindow;
        [SerializeField] private GameObject _menuWindow;
        [SerializeField] private GameObject _levelsWindow;
        [SerializeField] private GameObject _reloadButtonWindow;
        [SerializeField] private GameObject _menuButtonWindow;
        [SerializeField] private GameObject _mainMenuWindow;
        [SerializeField] private GameObject _loseWindow;

        [Header("Additional UI")]
        [SerializeField] private StartTextController _startTextController;
        [SerializeField] private LocalizationManager _localizationManager;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetActive(_levelsWindow, false);
            SetActive(_menuButtonWindow, false);
            SetActive(_reloadButtonWindow, false);
            SetActive(_loseWindow, false);
        }

        public void ShowGameplay()
        {
            SetActive(_mainMenuWindow, false);
            SetActive(_levelsWindow, false);
            SetActive(_menuWindow, false);
            SetActive(_winWindow, false);
            SetActive(_loseWindow, false);

            SetActive(_menuButtonWindow, true);
            SetActive(_reloadButtonWindow, true);
        }

        public void ShowPauseMenu(bool show)
        {
            SetActive(_menuWindow, show);
        }

        public void ShowWinWindow(int reward)
        {
            HideGameplayButtons();

            if (_localizationManager != null)
            {
                _localizationManager.SetScoreReward(reward);
            }

            SetActive(_winWindow, true);
        }

        public void ShowLoseWindow()
        {
            HideGameplayButtons();
            SetActive(_loseWindow, true);
        }

        public void HideLoseWindow()
        {
            SetActive(_loseWindow, false);
        }

        public void ShowMainMenu()
        {
            SetActive(_mainMenuWindow, true);
            SetActive(_levelsWindow, false);
            SetActive(_winWindow, false);
            SetActive(_menuWindow, false);
            SetActive(_loseWindow, false);

            HideGameplayButtons();
            HideStartText();
        }

        public void HideGameplayButtons()
        {
            SetActive(_menuButtonWindow, false);
            SetActive(_reloadButtonWindow, false);
        }

        public void ShowGameplayButtons()
        {
            SetActive(_menuButtonWindow, true);
            SetActive(_reloadButtonWindow, true);
        }

        public void HideStartText()
        {
            if (_startTextController != null)
            {
                _startTextController.HideText();
            }
        }

        public void ShowStartTextIfFirstLevel(int levelIndex)
        {
            if (_startTextController != null)
            {
                _startTextController.ShowIfFirstLevel(levelIndex).Forget();
            }
        }

        private void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}