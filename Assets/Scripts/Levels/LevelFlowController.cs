using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

using Buildings;
using PlatformPuzzle.Managers;

namespace PlatformPuzzle.Levels
{
    public class LevelFlowController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private LevelConstructor _levelConstructor;
        [SerializeField] private GameStateController _stateController;
        [SerializeField] private LevelUIController _uiController;
        [SerializeField] private LevelProgressService _progressService;

        [Header("Managers")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private RoadManager _roadManager;
        [SerializeField] private Spawner[] _spawners;

        [Header("Services")]
        [SerializeField] private LevelTimer _levelTimer;
        [SerializeField] private AdsManager _adsManager;

        private int _lastLevelReward;

        private void Start()
        {
            if (_adsManager == null)
            {
                _adsManager = AdsManager.Instance;
            }

            if (_levelTimer != null)
            {
                _levelTimer.TimeExpired += OnTimeExpired;
            }
        }

        public void ShowMenuWindow()
        {
            if (_stateController == null || !_stateController.CanPause())
            {
                return;
            }

            bool shouldPause = _stateController.IsPlaying;

            if (shouldPause)
            {
                _stateController.SetPaused();
            }
            else
            {
                _stateController.SetPlaying();
            }

            _uiController?.HideStartText();

            SetPause(shouldPause);

            _uiController?.ShowPauseMenu(shouldPause);

            if (_levelTimer != null)
            {
                if (shouldPause)
                {
                    _levelTimer.HideTimer();
                }
                else
                {
                    _levelTimer.ShowTimer();
                }
            }
        }

        public void LoadNextLevel()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError("LevelFlowController: LevelConstructor is null");
                return;
            }

            int nextLevelIndex = _levelConstructor.CurrentLevelIndex + 1;

            if (nextLevelIndex >= _levelConstructor.LevelsCount)
            {
                Debug.Log("No more levels!");
                return;
            }

            if (_adsManager != null)
            {
                _adsManager.RegisterAction(2);

                if (_adsManager.TryShowAd(
                        () => _levelConstructor.LoadLevelWithTimerReset(nextLevelIndex)))
                {
                    return;
                }
            }

            _levelConstructor.LoadLevelWithTimerReset(nextLevelIndex);
        }

        public void LoadCurrentLevel()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError("LevelFlowController: LevelConstructor is null");
                return;
            }

            if (_adsManager != null)
            {
                _adsManager.RegisterAction(1);

                if (_adsManager.TryShowAd(
                        _levelConstructor.RestartCurrentLevelKeepTimer))
                {
                    return;
                }
            }

            _levelConstructor.RestartCurrentLevelKeepTimer();
        }

        public void RestartAfterLose()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError("LevelFlowController: LevelConstructor is null");
                return;
            }

            if (_adsManager != null)
            {
                _adsManager.RegisterAction(1);

                if (_adsManager.TryShowAd(
                        () => _levelConstructor.LoadLevelWithTimerReset(
                            _levelConstructor.CurrentLevelIndex)))
                {
                    return;
                }
            }

            _levelConstructor.LoadLevelWithTimerReset(
                _levelConstructor.CurrentLevelIndex
            );
        }

        public void BackToMainMenu()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError("LevelFlowController: LevelConstructor is null");
                return;
            }

            _stateController?.SetMainMenu();

            Time.timeScale = 1f;

            _uiController?.ShowMainMenu();

            _levelConstructor.CancelCurrentLevel();
            _levelConstructor.ClearLevel();
        }

        public void CheckWinCondition()
        {
            if (_stateController == null || !_stateController.CanCheckWin())
            {
                return;
            }

            if (_spawners != null)
            {
                foreach (Spawner spawner in _spawners)
                {
                    if (spawner == null)
                    {
                        continue;
                    }

                    if (!spawner.IsFinished())
                    {
                        return;
                    }
                }
            }

            if (_gridManager != null && _gridManager.HasActiveCars())
            {
                return;
            }

            if (_roadManager != null && _roadManager.HasCars())
            {
                return;
            }

            HandleWin();
        }

        public void ContinueAfterRewardAd()
        {
            if (_stateController == null || !_stateController.IsLose)
            {
                return;
            }

            YG2.RewardedAdvShow(
                "SecondChance",
                ApplySecondChance
            );

            SoundManager.Instance?.ResumeMusic();
        }

        public void ShowWinWindow()
        {
            _stateController?.SetWin();

            _levelTimer?.HideTimer();

            _uiController?.ShowWinWindow(_lastLevelReward);

            SoundManager.Instance?.PauseMusic();
            SoundManager.Instance?.PlayWinSound();
        }

        public void OnLevelStarted(int levelIndex)
        {
            _stateController?.SetPlaying();

            _uiController?.ShowGameplay();
            _uiController?.ShowStartTextIfFirstLevel(levelIndex);

            _levelTimer?.ShowTimer();

            SoundManager.Instance?.ResumeMusic();
        }

        private void HandleWin()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError("LevelFlowController: LevelConstructor is null");
                return;
            }

            _stateController?.SetWin();

            _levelTimer?.StopAndHide();

            LevelData currentLevel = _levelConstructor.CurrentLevelData;

            if (_progressService != null)
            {
                _lastLevelReward = _progressService.CompleteLevel(
                    currentLevel,
                    _levelConstructor.CurrentLevelIndex
                );
            }

            ShowWinWindow();
        }

        private void OnTimeExpired()
        {
            if (_stateController == null || !_stateController.IsPlaying)
            {
                return;
            }

            _stateController.SetLose();

            _levelTimer?.StopAndHide();

            Time.timeScale = 0f;

            _uiController?.ShowLoseWindow();

            SoundManager.Instance?.PauseMusic();
            SoundManager.Instance?.PlayLoseSound();
        }

        private async void ApplySecondChance()
        {
            _stateController?.SetPlaying();

            _uiController?.HideLoseWindow();
            _uiController?.ShowGameplayButtons();

            await UniTask.Yield();

            Time.timeScale = 1f;

            if (_levelTimer != null)
            {
                _levelTimer.DisableLimit();
                _levelTimer.ShowTimer();
            }
        }

        private void SetPause(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }

        private void OnDestroy()
        {
            if (_levelTimer != null)
            {
                _levelTimer.TimeExpired -= OnTimeExpired;
            }
        }
    }
}