using System;
using UnityEngine;

namespace PlatformPuzzle.Levels
{
    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool IsWin => _currentState == GameState.Win;
        public bool IsLose => _currentState == GameState.Lose;

        public bool TryChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                return true;
            }

            if (!CanChangeState(_currentState, newState))
            {
                Debug.LogWarning($"Invalid state transition: {_currentState} -> {newState}");
                return false;
            }

            _currentState = newState;
            return true;
        }

        public bool CanPause()
        {
            return _currentState == GameState.Playing || _currentState == GameState.Paused;
        }

        public bool CanCheckWin()
        {
            return _currentState == GameState.Playing;
        }

        private bool CanChangeState(GameState currentState, GameState newState)
        {
            switch (currentState)
            {
                case GameState.MainMenu:
                    return newState == GameState.Playing;
                case GameState.Playing:
                    return newState == GameState.Paused ||
                           newState == GameState.Win ||
                           newState == GameState.Lose ||
                           newState == GameState.MainMenu;
                case GameState.Paused:
                    return newState == GameState.Playing ||
                           newState == GameState.MainMenu;
                case GameState.Win:
                case GameState.Lose:
                    return newState == GameState.Playing ||
                           newState == GameState.MainMenu;
                default:
                    return false;
            }
        }
    }
}