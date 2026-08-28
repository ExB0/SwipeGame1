using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformPuzzle.UI
{
    public class StartTextController : MonoBehaviour
    {
        [SerializeField] private GameObject _startTextObject;

        [SerializeField] private int _showDelay = 5000;

        public async UniTask ShowIfFirstLevel(int levelIndex)
        {
            if (levelIndex != 0)
            {
                _startTextObject.SetActive(false);
                return;
            }

            if (_startTextObject == null)
            {
                Debug.LogError("StartTextController: startTextObject не назначен!");
                return;
            }

            _startTextObject.SetActive(true);

            await UniTask.Delay(_showDelay);

            _startTextObject.SetActive(false);
        }

        public void HideText()
        {
            if (_startTextObject == null)
            {
                Debug.LogError("StartTextController: startTextObject не назначен!");
                return;
            }
            
            _startTextObject.SetActive(false);
        }
    }
}