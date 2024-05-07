using Advertising;
using Core.Windows;
using GameStats;
using Shop.AnalyticsSignals;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class CoinsCell : MonoBehaviour
    {
        [SerializeField] private int reward;
        [SerializeField] private RewardAdButton rewardAdButton;
        [SerializeField] private TextMeshProUGUI rewardText;

        private GameStatService _gameStatService;
        private WindowManager _windowManager;
        private SignalBus _signalBus;
        private AdvertisingService _advertisingService;

        [Inject]
        private void Construct(GameStatService gameStatService, WindowManager windowManager, SignalBus signalBus, AdvertisingService advertisingService)
        {
            _gameStatService = gameStatService;
            _windowManager = windowManager;
            _signalBus = signalBus;
            _advertisingService = advertisingService;
            rewardText.text = $"+{reward}";
        }

        public void OnCellClicked()
        {
            if(rewardAdButton.CanShow())
            {
                rewardAdButton.Rewarded += OnRewarded;
                rewardAdButton.Failed += OnFailed;
                rewardAdButton.ShowAd();
            }
        }

        private void OnRewarded()
        {
            rewardAdButton.Rewarded -= OnRewarded;
            rewardAdButton.Failed -= OnFailed;

            if (_windowManager.TryShowAndGetWindow<CoinsAddPopup>(EPopupType.CoinsAdd.ToString(), out var coinsAddPopup))
            {
                coinsAddPopup.SetArgs(reward, () =>{});
                _gameStatService.TryIncWithAnim(EGameStatType.Soft, reward);
            }

            _signalBus.Fire(new ShopPurchaseSignal(0, $"{reward} Soft Currency by ad"));
        }

        private void OnFailed()
        {
            rewardAdButton.Rewarded -= OnRewarded;
            rewardAdButton.Failed -= OnFailed;
        }
    }
}
