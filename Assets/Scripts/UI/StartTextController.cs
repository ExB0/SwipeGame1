using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformPuzzle.UI
{
    public class StartTextController : MonoBehaviour
    {
        [SerializeField] private GameObject startTextObject;

        public async UniTask ShowIfFirstLevel(int levelIndex)
        {
            if (levelIndex != 0)
            {
                startTextObject.SetActive(false);
                return;
            }

            startTextObject.SetActive(true);

            await UniTask.Delay(5000);

            startTextObject.SetActive(false);
        }

        public void HideText()
        {
            startTextObject.SetActive(false);
        }
    }
}