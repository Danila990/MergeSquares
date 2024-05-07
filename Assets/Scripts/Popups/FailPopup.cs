using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advertising;
using Advertising.AnalyticsSignals;
using CloudServices;
using Core.Audio;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameScripts.PointPanel;
using GameStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FailPopupParams
{
    public Action OnSkipped;
    public Action OnRestarted;
}

public class FailPopup : MonoBehaviour
{
    [SerializeField] private SoundSource fail;
    [SerializeField] private RewardAdButton rewardAdButton;
    [SerializeField] private PopupBase popupBase;
    [SerializeField] private SkipLevelWithCoinsButton skipLevelWithCoinsButton;
    [SerializeField] private int skipCost = 100;

    public void PlayFailSound() => fail.Play();
    public bool FullScreenAdsEnables { get; set; } = true;

    private FailPopupParams popupParams;
    private AdvertisingService _advertisingService;
    private GameStatService _gameStatService;
    private WindowManager _windowManager;
    private CloudService _cloudService;

    [Inject]
    private void Construct(CloudService cloudService, AdvertisingService advertisingService, GameStatService gameStatService, WindowManager windowManager)
    {
        _cloudService = cloudService;
        _advertisingService = advertisingService;
        _gameStatService = gameStatService;
        _windowManager = windowManager;
        popupBase.ShowArgsGot += OnShowArgsGot;
        popupBase.Inited += OnInited;
        PlayFailSound();
        
        if(skipLevelWithCoinsButton != null)
        {
            skipLevelWithCoinsButton.SetButtonState(skipCost, true);
        }
    }

    private void OnDestroy()
    {
        if(_cloudService.CloudProvider.IsStickySideAvailable)
        {
            _advertisingService.StopStickyAd("FailPopup");
        }
        popupBase.ShowArgsGot -= OnShowArgsGot;
        popupBase.Inited -= OnInited;
    }

    private void OnInited()
    {
        if (FullScreenAdsEnables && _advertisingService.CanShowForLevel(EAdLevelType.Before))
        {
            _advertisingService.ShowFullscreenAd("FailPopup");
        }
        if(_cloudService.CloudProvider.IsStickySideAvailable)
        {
            _advertisingService.ShowStickyAd("FailPopup");
        }
    }

    public void OnClick()
    {
        if (FullScreenAdsEnables && _advertisingService.CanShowForLevel(EAdLevelType.After))
        {
            _advertisingService.ShowFullscreenAd("FailPopup");
        }
        popupParams.OnRestarted?.Invoke();
    }

    public void SkipLevelWithAd()
    {
#if !UNITY_EDITOR
        if(rewardAdButton.CanShow())
        {
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
#else
        OnRewarded();
#endif
    }

    public void SkipLevelWithCoins()
    {
        if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, skipCost))
        {
            popupParams.OnSkipped?.Invoke();
            popupBase.CloseWindow();
        }
        else
        {
            SquaresShop.OpenSection(_windowManager, EShopMarkers.Coins);
        }
    }
    
    private void OnRewarded()
    {
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
        popupParams.OnSkipped?.Invoke();
        popupBase.CloseWindow();
    }

    private void OnFailed()
    {
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
    }

    private void OnShowArgsGot(object[] args)
    {
        if(args.Length > 0)
        {
            popupParams = args.First() as FailPopupParams;
        }
    }
}
