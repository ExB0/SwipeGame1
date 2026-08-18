using System;
using Cysharp.Threading.Tasks;

using UnityEngine;

using YG;

namespace PlatformPuzzle.Managers
{
    public class AdsManager : MonoBehaviour
    {
        private const int ActionThreshold = 3;
        private const float Cooldown = 60f;

        public static AdsManager Instance;

        [SerializeField] private int _actionCounter;
        [SerializeField] private float _lastAdTime = -999f;

        private bool _isAdShowing;
        private Action _onAdClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            YG2.onCloseInterAdv += OnAdClosed;
        }

        private void OnDisable()
        {
            YG2.onCloseInterAdv -= OnAdClosed;

            if (_isAdShowing)
            {
                _isAdShowing = false;
                PauseGameYG.SetState(1, false, false);
                _onAdClosed = null;
            }
        }

        private void OnDestroy()
        {
            YG2.onCloseInterAdv -= OnAdClosed;
        }

        public void RegisterAction(int weight = 1)
        {
            _actionCounter += weight;
        }

        public bool TryShowAd(Action onClosed = null)
        {
            if (_isAdShowing)
                return true;

            if (_actionCounter < ActionThreshold)
                return false;

            if (Time.time - _lastAdTime < Cooldown)
                return false;

            if (!YG2.isTimerAdvCompleted)
                return false;

            _onAdClosed = onClosed;
            ShowAd();
            return true;
        }

        private void ShowAd()
        {
            _isAdShowing = true;
            _actionCounter = 0;
            _lastAdTime = Time.time;

            PauseGameYG.SetState(0, true, true);

            YG2.InterstitialAdvShow();
        }

        private void OnAdClosed()
        {
            _isAdShowing = false;

            PauseGameYG.SetState(1, false, false);

            UniTask.Void(async () =>
            {
                await UniTask.Yield();
                await UniTask.DelayFrame(2);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                Action callback = _onAdClosed;
                _onAdClosed = null;
                callback?.Invoke();
            });
        }
    }
}