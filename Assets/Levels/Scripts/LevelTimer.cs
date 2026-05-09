using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;

    private CancellationTokenSource _timerCts;
    private float _remainingTime;
    private bool _isRunning;
    private bool _isUnlimited;

    public event Action OnTimeExpired;

    public bool IsUnlimited => _isUnlimited;
    public float RemainingTime => _remainingTime;

    public void StartTimer(float seconds, CancellationToken levelToken)
    {
        StopTimer();

        _remainingTime = Mathf.Clamp(seconds, 1f, 300f);
        _isRunning = true;
        _isUnlimited = false;

        _timerCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy(),
            levelToken
        );

        UpdateView();
        RunTimerAsync(_timerCts.Token).Forget();
    }

    public void StopTimer()
    {
        _isRunning = false;

        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    public void ResetTimer()
    {
        StopTimer();

        _remainingTime = 0f;
        _isUnlimited = false;

        UpdateView();
    }

    public void DisableLimit()
    {
        StopTimer();

        _remainingTime = 0f;
        _isUnlimited = true;

        UpdateView();
    }

    private async UniTaskVoid RunTimerAsync(CancellationToken token)
    {
        try
        {
            while (_isRunning && _remainingTime > 0f)
            {
                token.ThrowIfCancellationRequested();

                if (Time.timeScale > 0f)
                {
                    _remainingTime -= Time.deltaTime;
                    UpdateView();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (!token.IsCancellationRequested && !_isUnlimited)
            {
                _isRunning = false;
                _remainingTime = 0f;
                UpdateView();
                OnTimeExpired?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateView()
    {
        if (_timerText == null) return;

        if (_isUnlimited)
        {
            _timerText.text = "∞";
            return;
        }

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, _remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        StopTimer();
    }
}
