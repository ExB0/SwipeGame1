using System.Threading;

using UnityEngine;

namespace PlatformPuzzle.Levels
{
    public class LevelSession : MonoBehaviour
    {
        private CancellationTokenSource _levelCts;

        public CancellationToken Token =>
            _levelCts?.Token ?? CancellationToken.None;

        public void Create()
        {
            Cancel();

            _levelCts = new CancellationTokenSource();
        }

        public void Cancel()
        {
            _levelCts?.Cancel();
            _levelCts?.Dispose();
            _levelCts = null;
        }

        private void OnDestroy()
        {
            Cancel();
        }
    }
}
