using System;
using Core.Localization;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using Zenject;

namespace Advertising
{
    public class RewardAdButton : MonoBehaviour
    {
        [SerializeField] private Transform overlay;
        [SerializeField] private string placementName = "Unknown";

        public event Action Rewarded = () => {};
        public event Action Failed = () => {};
        
        private bool _rewardGot;
        private bool _failGot;
        
        private AdvertisingService _advertisingService;
        
        [Inject]
        public void Construct(AdvertisingService advertisingService)
        {
            _advertisingService = advertisingService;
            _advertisingService.RewardAdLoaded += OnRewardAdLoaded;
            _advertisingService.RewardAdFailed += OnRewardAdFailed;
            _advertisingService.RewardAdRewarded += OnRewardAdRewarded;
        }

        private void OnDestroy()
        {
            _advertisingService.RewardAdLoaded -= OnRewardAdLoaded;
            _advertisingService.RewardAdFailed -= OnRewardAdFailed;
            _advertisingService.RewardAdRewarded -= OnRewardAdRewarded;
        }

        private void Start()
        {
            // UpdateState();
        }

        private void LateUpdate()
        {
            if (_rewardGot)
            {
                _rewardGot = false;
                Rewarded.Invoke();
            }
            if (_failGot)
            {
                _failGot = false;
                Failed.Invoke();
            }
        }

        public bool CanShow() => _advertisingService.CanShow(EAdType.Rewarded);

        public void ShowAd() => _advertisingService.ShowRewardAd(placementName);
        
        private void OnRewardAdLoaded()
        {
            // UpdateState();
        }
        
        private void OnRewardAdFailed()
        {
            _failGot = true;
        }
        
        private void OnRewardAdRewarded()
        {
            _rewardGot = true;
        }

        // private void UpdateState()
        // {
        //     if (overlay != null)
        //     {
        //         overlay.gameObject.SetActive(!_advertisingService.IsRewardAdAvailable);
        //     }
        // }
    }
}
