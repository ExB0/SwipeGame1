using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformPuzzle.UI
{
    public class StartTextController : MonoBehaviour
    {
        [SerializeField] private GameObject _startTextObject;

        public async UniTask ShowIfFirstLevel(int levelIndex)
        {
            if (levelIndex != 0)
            {
                _startTextObject.SetActive(false);
                return;
            }

            _startTextObject.SetActive(true);

            await UniTask.Delay(5000);

            _startTextObject.SetActive(false);
        }

        public void HideText()
        {
            _startTextObject.SetActive(false);
        }
    }
}