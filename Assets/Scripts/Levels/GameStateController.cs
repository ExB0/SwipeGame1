using UnityEngine;

namespace PlatformPuzzle.Levels
{
    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        public GameState CurrentState => _currentState;

        public bool IsMainMenu => _currentState == GameState.MainMenu;
        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool IsWin => _currentState == GameState.Win;
        public bool IsLose => _currentState == GameState.Lose;

        public void SetMainMenu()
        {
            _currentState = GameState.MainMenu;
        }

        public void SetPlaying()
        {
            _currentState = GameState.Playing;
        }

        public void SetPaused()
        {
            _currentState = GameState.Paused;
        }

        public void SetWin()
        {
            _currentState = GameState.Win;
        }

        public void SetLose()
        {
            _currentState = GameState.Lose;
        }

        public bool CanPause()
        {
            return _currentState == GameState.Playing ||
                   _currentState == GameState.Paused;
        }

        public bool CanCheckWin()
        {
            return _currentState == GameState.Playing;
        }
    }
}