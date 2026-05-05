using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class AdsManager : MonoBehaviour
{
public static AdsManager Instance;

    [SerializeField] private int _actionCounter = 0;
    [SerializeField] private float _lastAdTime = -999f;

    private const int ACTION_THRESHOLD = 3;
    private const float COOLDOWN = 30f;

    private System.Action _onAdClosed;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterAction(int weight = 1)
    {
        _actionCounter += weight;
    }

    public bool TryShowAd(System.Action onClosed = null)
    {
        if (_actionCounter < ACTION_THRESHOLD)
            return false;

        if (Time.time - _lastAdTime < COOLDOWN)
            return false;

        _onAdClosed = onClosed;

        ShowAd();
        return true;
    }

    private void ShowAd()
    {
        _actionCounter = 0;
        _lastAdTime = Time.time;

        PauseGameYG.SetState(0, true, true);

        YG2.onCloseInterAdv += OnAdClosed;

        YG2.InterstitialAdvShow();
    }

    private void OnAdClosed()
    {
        PauseGameYG.SetState(1, false, false);

        YG2.onCloseInterAdv -= OnAdClosed;

        _onAdClosed?.Invoke();
    }
}
