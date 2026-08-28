using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;
using PlatformPuzzle.Managers;

namespace PlatformPuzzle.Levels
{
    public class LevelFlowController : MonoBehaviour
    {
        private const int NextLevelActionWeight = 2;
        private const int RestartActionWeight = 1;

        [Header("Core")]
        [SerializeField] private LevelConstructor _levelConstructor;
        [SerializeField] private GameStateController _stateController;
        [SerializeField] private LevelUIController _uiController;
        [SerializeField] private LevelProgressService _progressService;

        [Header("Managers")]
        [SerializeField] private LevelTimer _levelTimer;
        [SerializeField] private AdsManager _adsManager;

        [Header("Services")]
        [SerializeField] private WinConditionChecker _winConditionChecker;
        [SerializeField] private PauseManager _pauseManager;
        [SerializeField] private AdActionExecutor _adActionExecutor;

        private int _lastLevelReward;

        private void Start()
        {
            ValidateReferences();

            if (_levelTimer != null)
                _levelTimer.TimeExpired += OnTimeExpired;
        }

        private void OnDestroy()
        {
            if (_levelTimer != null)
                _levelTimer.TimeExpired -= OnTimeExpired;
        }

        public void ShowMenuWindow()
        {
            if (_stateController == null || !_stateController.CanPause())
                return;

            bool shouldPause = _stateController.IsPlaying;
            if (shouldPause)
                _stateController.TryChangeState(GameState.Paused);
            else
                _stateController.TryChangeState(GameState.Playing);

            _uiController?.HideStartText();
            _pauseManager.SetPause(shouldPause);
        }

        public void LoadNextLevel()
        {
            if (!HasLevelConstructor())
                return;

            int nextLevelIndex = _levelConstructor.CurrentLevelIndex + 1;
            if (nextLevelIndex >= _levelConstructor.LevelsCount)
            {
                Debug.LogError("No more levels!");
                return;
            }

            _adActionExecutor.ExecuteWithAd(
                NextLevelActionWeight,
                () => _levelConstructor.LoadLevelWithTimerReset(nextLevelIndex)
            );
        }

        public void LoadCurrentLevel()
        {
            if (!HasLevelConstructor())
                return;

            _adActionExecutor.ExecuteWithAd(
                RestartActionWeight,
                _levelConstructor.RestartCurrentLevelKeepTimer
            );
        }

        public void RestartAfterLose()
        {
            if (!HasLevelConstructor())
                return;

            int currentLevelIndex = _levelConstructor.CurrentLevelIndex;
            _adActionExecutor.ExecuteWithAd(
                RestartActionWeight,
                () => _levelConstructor.LoadLevelWithTimerReset(currentLevelIndex)
            );
        }

        public void BackToMainMenu()
        {
            if (!HasLevelConstructor())
                return;

            _stateController?.TryChangeState(GameState.MainMenu);
            Time.timeScale = 1f;
            _uiController?.ShowMainMenu();
            _levelConstructor.CancelCurrentLevel();
            _levelConstructor.ClearLevel();
        }

        public void CheckWinCondition()
        {
            if (_stateController == null || !_stateController.CanCheckWin())
                return;

            if (_winConditionChecker != null && _winConditionChecker.IsWinConditionMet())
                HandleWin();
        }

        public void ContinueAfterRewardAd()
        {
            if (_stateController == null || !_stateController.IsLose)
                return;

            YG2.RewardedAdvShow("SecondChance", ApplySecondChance);
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

        private bool HasLevelConstructor()
        {
            if (_levelConstructor != null)
                return true;

            Debug.LogError($"{nameof(LevelFlowController)}: LevelConstructor is null");
            return false;
        }

        private void HandleWin()
        {
            if (!HasLevelConstructor())
                return;

            _stateController?.TryChangeState(GameState.Win);
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

        private async void ApplySecondChance()
        {
            _stateController?.TryChangeState(GameState.Playing);
            _uiController?.HideLoseWindow();
            _uiController?.ShowGameplayButtons();

            await UniTask.Yield();
            _pauseManager.ResumeAfterAd();
        }

        private void ValidateReferences()
        {
            if (_levelConstructor == null)
                Debug.LogError($"{nameof(LevelFlowController)}: LevelConstructor is missing");

            if (_stateController == null)
                Debug.LogError($"{nameof(LevelFlowController)}: GameStateController is missing");

            if (_winConditionChecker == null)
                Debug.LogError($"{nameof(LevelFlowController)}: WinConditionChecker is missing");

            if (_pauseManager == null)
                Debug.LogError($"{nameof(LevelFlowController)}: PauseManager is missing");

            if (_adActionExecutor == null)
                Debug.LogError($"{nameof(LevelFlowController)}: AdActionExecutor is missing");
        }

        private void OnTimeExpired()
        {
            if (_stateController == null || !_stateController.IsPlaying)
                return;

            _stateController.TryChangeState(GameState.Lose);
            _levelTimer?.StopAndHide();
            Time.timeScale = 0f;
            _uiController?.ShowLoseWindow();
            SoundManager.Instance?.PauseMusic();
            SoundManager.Instance?.PlayLoseSound();
        }
    }
}