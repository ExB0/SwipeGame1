using System;

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
        [SerializeField] private GridCarSpawner _gridCarSpawner;

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
            ValidateReferences();

            if (_levelTimer != null)
            {
                _levelTimer.TimeExpired += OnTimeExpired;
            }
        }

        private void OnDestroy()
        {
            if (_levelTimer != null)
            {
                _levelTimer.TimeExpired -= OnTimeExpired;
            }
        }

        public void ShowMenuWindow()
        {
            if (_stateController == null ||
                !_stateController.CanPause())
            {
                return;
            }

            bool shouldPause = _stateController.IsPlaying;

            if (shouldPause)
            {
                _stateController.TryChangeState(GameState.Paused);
            }
            else
            {
                _stateController.TryChangeState(GameState.Playing);
            }

            _uiController?.HideStartText();

            SetPause(shouldPause);

            _uiController?.ShowPauseMenu(shouldPause);

            if (_levelTimer == null)
            {
                return;
            }

            if (shouldPause)
            {
                _levelTimer.HideTimer();
            }
            else
            {
                _levelTimer.ShowTimer();
            }
        }

        public void LoadNextLevel()
        {
            if (!HasLevelConstructor())
            {
                return;
            }

            int nextLevelIndex =
                _levelConstructor.CurrentLevelIndex + 1;

            if (nextLevelIndex >= _levelConstructor.LevelsCount)
            {
                Debug.LogError("No more levels!");
                return;
            }

            ExecuteWithAd(
                2,
                () => _levelConstructor.LoadLevelWithTimerReset(nextLevelIndex)
            );
        }

        public void LoadCurrentLevel()
        {
            if (!HasLevelConstructor())
            {
                return;
            }

            ExecuteWithAd(
                1,
                _levelConstructor.RestartCurrentLevelKeepTimer
            );
        }

        public void RestartAfterLose()
        {
            if (!HasLevelConstructor())
            {
                return;
            }

            int currentLevelIndex =
                _levelConstructor.CurrentLevelIndex;

            ExecuteWithAd(
                1,
                () => _levelConstructor.LoadLevelWithTimerReset(currentLevelIndex)
            );
        }

        public void BackToMainMenu()
        {
            if (!HasLevelConstructor())
            {
                return;
            }

            _stateController?.TryChangeState(GameState.MainMenu);

            Time.timeScale = 1f;

            _uiController?.ShowMainMenu();

            _levelConstructor.CancelCurrentLevel();
            _levelConstructor.ClearLevel();
        }

        public void CheckWinCondition()
        {
            if (_stateController == null ||
                !_stateController.CanCheckWin())
            {
                return;
            }

            if (!AreSpawnersFinished())
            {
                return;
            }

            if (_gridCarSpawner != null &&
                _gridCarSpawner.HasActiveCars())
            {
                return;
            }

            if (_roadManager != null &&
                _roadManager.HasCars())
            {
                return;
            }

            HandleWin();
        }

        public void ContinueAfterRewardAd()
        {
            if (_stateController == null ||
                !_stateController.IsLose)
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
            _levelTimer?.HideTimer();

            _uiController?.ShowWinWindow(_lastLevelReward);

            SoundManager.Instance?.PauseMusic();
            SoundManager.Instance?.PlayWinSound();
        }

        public void OnLevelStarted(int levelIndex)
        {
            _stateController?.TryChangeState(GameState.Playing);

            _uiController?.ShowGameplay();
            _uiController?.ShowStartTextIfFirstLevel(levelIndex);

            _levelTimer?.ShowTimer();

            SoundManager.Instance?.ResumeMusic();
        }

        private void ExecuteWithAd(
            int actionWeight,
            Action action)
        {
            if (action == null)
            {
                return;
            }

            if (_adsManager != null)
            {
                _adsManager.RegisterAction(actionWeight);

                if (_adsManager.TryShowAd(action))
                {
                    return;
                }
            }

            action.Invoke();
        }

        private bool HasLevelConstructor()
        {
            if (_levelConstructor != null)
            {
                return true;
            }

            Debug.LogError(
                $"{nameof(LevelFlowController)}: LevelConstructor is null"
            );

            return false;
        }

        private bool AreSpawnersFinished()
        {
            if (_spawners == null)
            {
                return true;
            }

            foreach (Spawner spawner in _spawners)
            {
                if (spawner == null)
                {
                    continue;
                }

                if (!spawner.IsFinished())
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleWin()
        {
            if (!HasLevelConstructor())
            {
                return;
            }

            _stateController?.TryChangeState(GameState.Win);

            _levelTimer?.StopAndHide();

            LevelData currentLevel =
                _levelConstructor.CurrentLevelData;

            if (_progressService != null)
            {
                _lastLevelReward =
                    _progressService.CompleteLevel(
                        currentLevel,
                        _levelConstructor.CurrentLevelIndex
                    );
            }

            ShowWinWindow();
        }

        private async void ApplySecondChance()
        {
            _stateController?.TryChangeState(GameState.Playing);

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

        private void ValidateReferences()
        {
            if (_levelConstructor == null)
            {
                Debug.LogError($"{nameof(LevelFlowController)}: LevelConstructor is missing");
            }

            if (_stateController == null)
            {
                Debug.LogError($"{nameof(LevelFlowController)}: GameStateController is missing");
            }

            if (_adsManager == null)
            {
                Debug.LogWarning($"{nameof(LevelFlowController)}: AdsManager is missing");
            }
        }

        private void OnTimeExpired()
        {
            if (_stateController == null ||
                !_stateController.IsPlaying)
            {
                return;
            }

            _stateController.TryChangeState(GameState.Lose);

            _levelTimer?.StopAndHide();

            Time.timeScale = 0f;

            _uiController?.ShowLoseWindow();

            SoundManager.Instance?.PauseMusic();
            SoundManager.Instance?.PlayLoseSound();
        }
    }
}