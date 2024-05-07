using System;
using System.Linq;
using Advertising;
using Advertising.AnalyticsSignals;
using CloudServices;
using Core.Audio;
using Core.Localization;
using Core.Windows;
using DG.Tweening;
using GameScripts.PointPanel;
using GameStats;
using LuckyWheel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

public class WinPopupParams
{
    public int showedLevel;
    public int winBonus;
    public int giftBonus;
    public float startGiftProgress = 0;
    public float giftProgress = 100;
    public string wheelId;
    public Action ClosePopup;
}
public class WinPopup : MonoBehaviour
{
    [SerializeField] private SoundSource bonus;
    [SerializeField] private RewardAdButton rewardAdButton;
    [SerializeField] private Button adButton;
    [SerializeField] private Button giftButton;
    [SerializeField] private Image coinAddCoin;
    [SerializeField] private Image giftIcon;
    [SerializeField] private Animator giftAnimator;
    [SerializeField] private TextMeshProUGUI coinAddText;
    [SerializeField] private TextMeshProUGUI levelNum;
    [SerializeField] private TextMeshProUGUI giftBonus;
    [SerializeField] private PopupBase popupBase;
    [SerializeField] private float addCoinsDuration;
    [SortingLayer]
    [SerializeField] private string sortingLayerName;
    [SerializeField] private LocalizationRepository _localizationRepository;

    public void PlayBonusSound() => bonus.Play();
    public bool FullScreenAdsEnables { get; set; } = true;

    private Tweener _tweenerShowAddedCoins;
    private WinPopupParams winPopupParams;

    private GameStatService _gameStatService;
    private WindowManager _windowManager;
    private SignalBus _signalBus;
    private AdvertisingService _advertisingService;
    private RateService _rateService;
    private CloudService _cloudService;
    private WheelService _wheelService;

    [Inject]
    private void Construct(
        GameStatService gameStatService,
        WindowManager windowManager,
        SignalBus signalBus,
        AdvertisingService advertisingService,
        RateService rateService,
        CloudService cloudService,
        DiContainer diContainer
    )
    {
        popupBase.Disposed += Dispose;
        popupBase.ShowArgsGot += OnShowArgsGot;
        popupBase.Inited += OnInited;

        _gameStatService = gameStatService;
        _windowManager = windowManager;
        _signalBus = signalBus;
        _advertisingService = advertisingService;
        _rateService = rateService;
        _cloudService = cloudService;
        _wheelService = diContainer.TryResolve<WheelService>();
        // _gridManager = gridManager;
        PlayBonusSound();
    }

    private void OnDestroy()
    {
        if(_cloudService.CloudProvider.IsStickySideAvailable)
        {
            _advertisingService.StopStickyAd("WinPopup");
        }
        popupBase.Disposed -= Dispose;
        popupBase.ShowArgsGot -= OnShowArgsGot;
        popupBase.Inited -= OnInited;
    }
    
    public void Ready()
    {
        if (FullScreenAdsEnables && _advertisingService.CanShowForLevel(EAdLevelType.Before))
        {
            _advertisingService.ShowFullscreenAd("WinPopup");
        }
        if(_cloudService.CloudProvider.IsStickySideAvailable)
        {
            _advertisingService.ShowStickyAd("WinPopup");
        }
    }

    public void OnClick()
    {
        popupBase.CloseWindow();
        if (_rateService.CanShowOnLevel(winPopupParams.showedLevel))
        {
            _rateService.StartRating();
        }
        else
        {
            if (FullScreenAdsEnables && _advertisingService.CanShowForLevel(EAdLevelType.After))
            {
                _advertisingService.ShowFullscreenAd("WinPopup");
            }
        }
        winPopupParams.ClosePopup();
    }

    public void GetGiftWithAds()
    {
        if(rewardAdButton.CanShow())
        {
            adButton.interactable = false;
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
    }

    public void GetFreeGift()
    {
        _wheelService.SetGiftProgress(0f);
        // _gridManager.GiftProgress = 0f;
        DOTween.To(() => giftIcon.fillAmount, newAmount =>
        {
            giftIcon.fillAmount = newAmount;
        }, winPopupParams.giftProgress, 1f);
        
        _wheelService.ShowWheel(winPopupParams.wheelId);
        // WinPopup winPopup;
        
        
        // _windowManager.ShowWindow(EPopupType.LuckyWheelPopup.ToString());
    }

    private void OnShowArgsGot(object[] args)
    {
        if (args.Length > 0)
        {
            winPopupParams = args.First() as WinPopupParams;
            levelNum.text = winPopupParams.showedLevel.ToString();
            ShowAddedCoins();
            if (giftBonus != null)
            {
                giftBonus.text = "+" + winPopupParams.giftBonus;
            }

            if (giftIcon == null || giftAnimator == null)
            {
                return;
            }

            DOTween.To(() => winPopupParams.startGiftProgress, newAmount => { giftIcon.fillAmount = newAmount; },
                winPopupParams.giftProgress, 1f);

            if (winPopupParams.giftProgress >= 1f)
            {
                giftAnimator.SetTrigger("start");
                giftButton.gameObject.SetActive(true);
                if (giftIcon != null)
                {
                    DOTween.To(() => giftIcon.fillAmount, newAmount => { giftIcon.fillAmount = newAmount; },
                        winPopupParams.giftProgress, 1f);
                }
            }
        }
    }

    private void Dispose(PopupBaseCloseType closeType)
    {
        _tweenerShowAddedCoins.Kill();
    }

    private void ShowAddedCoins()
    {
        _gameStatService.TryIncWithAnim(EGameStatType.Soft, winPopupParams.winBonus, null, addCoinsDuration);
        coinAddText.text = "+" + winPopupParams.winBonus;
        var colorCoin = coinAddCoin.color;
        var colorText = coinAddText.color;

        _tweenerShowAddedCoins = DOTween.To(() => coinAddCoin.color.a, newColorA =>
        {
            colorCoin.a = newColorA;
            colorText.a = newColorA;
            coinAddCoin.color = colorCoin;
            coinAddText.color = colorText;
        }, 0f, addCoinsDuration).SetEase(Ease.InQuint);
    }

    private void OnRewarded()
    {
        rewardAdButton.GetComponent<Button>().interactable = true;
        _gameStatService.TryIncWithAnim(EGameStatType.Soft, winPopupParams.giftBonus);
        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;

        _windowManager.TryShowAndGetWindow<CoinsAddPopup>(EPopupType.CoinsAdd.ToString(), out var coinsAddPopup);
        coinsAddPopup.SetArgs(winPopupParams.giftBonus, () =>
        {
            winPopupParams.ClosePopup();
            popupBase.CloseWindow();
        });
    }

    private void OnFailed()
    {
        adButton.interactable = true;

        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Failed, "Gift after win"));

        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
    }

    private void OnInited()
    {
        popupBase.Canvas.sortingLayerName = sortingLayerName;
    }
}
