using UnityEngine;
using SavingSystem;

namespace PlatformPuzzle.Levels
{
    public class LevelSelectInitializer : MonoBehaviour
    {
        [SerializeField] private LevelButton[] _buttons;

        private void OnEnable()
        {
            var data = SaveSystem.Load();

            foreach (var button in _buttons)
            {
                button.Initialize(data.UnlockedLevel);
            }
        }
    }
}