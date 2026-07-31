using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                button.Init(data.UnlockedLevel);
            }
        }
    }
}
