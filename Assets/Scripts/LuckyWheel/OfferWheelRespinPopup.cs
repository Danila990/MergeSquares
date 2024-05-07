using System;
using System.Collections;
using System.Collections.Generic;
using Advertising;
using Core.Windows;using Mono.CSharp;
using UnityEngine;
using Zenject;

public class OfferWheelRespinPopup : MonoBehaviour
{
    [SerializeField] private PopupBase popupBase;
    [SerializeField] private RewardAdButton rewardAdButton;
    
    public Action<bool, OfferWheelRespinPopup> OnClose;

    private bool _isAdsWathed;

    [Inject]
    private void Construct()
    {
        popupBase.Disposed += Dispose;
    }

    private void OnDestroy()
    {
        popupBase.Disposed -= Dispose;
    }
    
    private void Dispose(PopupBaseCloseType closeType)
    {
        OnClose(_isAdsWathed, this);
    }

    public void OnTakeWithAds()
    {
        rewardAdButton.Rewarded += OnRewarded;
        rewardAdButton.Failed += OnFailed;
        rewardAdButton.ShowAd();
    }

    private void OnRewarded()
    {
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
        _isAdsWathed = true;
        popupBase.CloseWindow();
    }

    private void OnFailed()
    {
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
    }
}
