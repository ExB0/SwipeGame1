using UnityEngine;

namespace PlatformPuzzle.Levels
{
    public class PauseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelTimer _levelTimer;
        [SerializeField] private LevelUIController _uiController;

        public bool IsPaused { get; private set; }

        public void SetPause(bool pause)
        {
            IsPaused = pause;
            Time.timeScale = pause ? 0f : 1f;

            if (_levelTimer != null)
            {
                if (pause)
                    _levelTimer.HideTimer();
                else
                    _levelTimer.ShowTimer();
            }

            _uiController?.ShowPauseMenu(pause);
        }

        public void ResumeAfterAd()
        {
            Time.timeScale = 1f;
            IsPaused = false;

            if (_levelTimer != null)
            {
                _levelTimer.DisableLimit();
                _levelTimer.ShowTimer();
            }

            _uiController?.ShowGameplayButtons();
        }
    }
}