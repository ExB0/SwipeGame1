using System;

using UnityEngine;

using PlatformPuzzle.Managers;

namespace PlatformPuzzle.Levels
{
    public class AdActionExecutor : MonoBehaviour
    {
        [SerializeField] private AdsManager _adsManager;

        public void ExecuteWithAd(int actionWeight, Action action)
        {
            if (action == null)
                return;

            if (_adsManager != null)
            {
                _adsManager.RegisterAction(actionWeight);
                if (_adsManager.TryShowAd(action))
                    return;
            }

            action.Invoke();
        }

        public bool IsAdAvailable()
        {
            return _adsManager != null;
        }
    }
}