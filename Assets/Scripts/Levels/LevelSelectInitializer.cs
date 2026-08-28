using UnityEngine;
using SavingSystem;

namespace PlatformPuzzle.Levels
{
    public class LevelSelectInitializer : MonoBehaviour
    {
        [SerializeField] private LevelButton[] _buttons;

        private void OnEnable()
        {
            if (_buttons == null || _buttons.Length == 0)
            {
                Debug.LogWarning($"{name}: No buttons assigned");
                return;
            }

            var data = SaveSystem.Load();
            int unlockedLevel = data != null ? data.UnlockedLevel : 0;

            for (int i = 0; i < _buttons.Length; i++)
            {
                var button = _buttons[i];
                if (button == null)
                {
                    Debug.LogWarning($"{name}: Button at index {i} is null");
                    continue;
                }
                button.Initialize(unlockedLevel);
            }
        }
    }
}