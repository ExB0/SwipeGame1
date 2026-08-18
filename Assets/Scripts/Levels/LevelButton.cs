using UnityEngine;
using UnityEngine.UI;

namespace PlatformPuzzle.Levels
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private int _levelIndex;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private LevelConstructor _levelConstructor;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        public void Initialize(int unlockedLevel)
        {
            bool isUnlocked = _levelIndex <= unlockedLevel;

            _button.interactable = isUnlocked;

            if (_lockIcon != null)
            {
                _lockIcon.SetActive(!isUnlocked);
            }
        }

        public void OnClick()
        {
            if (_button == null ||
                !_button.interactable)
            {
                return;
            }

            if (_levelConstructor == null)
            {
                Debug.LogError(
                    $"{nameof(LevelButton)}: LevelConstructor is missing"
                );

                return;
            }

            _levelConstructor.LoadLevelWithTimerReset(_levelIndex);
        }
    }
}