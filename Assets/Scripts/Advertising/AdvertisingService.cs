using System;
using Advertising.AnalyticsSignals;
using Core.SaveLoad;
using Settings;
using UnityEngine;
using Zenject;

namespace Advertising
{
    [Serializable]
    public class AdvertisingServiceData
    {
        public bool isAdsDisable;
    }
    
    public class AdvertisingService : MonoBehaviour
    {
        [SerializeField] private bool testMode;
        [SerializeField] private AdProviderBase adProvider;
        [Space]
        [SerializeField] private Saver saver;

        public event Action RewardAdLoaded = () => {};
        public event Action RewardAdFailed = () => {};
        public event Action RewardAdRewarded = () => {};
        public event Action FullScreenAdRewarded = () => {};
        public bool TestMode => testMode;
        public bool IsAdsDisable => _data.isAdsDisable;

        private AdvertisingServiceData _data;
        
        private SignalBus _signalBus;
        private SettingsService _settingsService;

        [Inject]
        public void Construct(SignalBus signalBus, SettingsService settingsService)
        {
            _signalBus = signalBus;
            _settingsService = settingsService;
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }
        
        private void Start()
        {
            adProvider.Loaded += OnRewardAdLoaded;
            adProvider.Clicked += OnRewardAdClicked;
            adProvider.Failed += OnRewardAdFailed;
            adProvider.Rewarded += OnRewardAdRewarded;
        }

        private void OnDestroy()
        {
            adProvider.Loaded -= OnRewardAdLoaded;
            adProvider.Clicked -= OnRewardAdClicked;
            adProvider.Failed -= OnRewardAdFailed;
            adProvider.Rewarded -= OnRewardAdRewarded;
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
        
        private void OnRewardAdLoaded(EAdType type) => RewardAdLoaded.Invoke();
        private void OnRewardAdClicked(EAdType type) => RewardAdRewarded.Invoke();

        private void OnRewardAdFailed(EAdType type, string placementName)
        {
            _settingsService.TurnOnGlobalVolume();
            switch (type)
            {
                case EAdType.Rewarded:
                    RewardAdFailed.Invoke();
                    _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Failed, placementName));
                    break;
                case EAdType.FullScreen:
                    _signalBus.Fire(new AdSignal(AdType.Interstitial, AdStatus.Failed, placementName));
                    break;
            }
        }

        private void OnRewardAdRewarded(EAdType type, string placementName)
        {
            _settingsService.TurnOnGlobalVolume();
            switch (type)
            {
                case EAdType.Rewarded:
                    RewardAdRewarded.Invoke();
                    _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Completed, placementName));
                    break;
                case EAdType.FullScreen:
                    FullScreenAdRewarded.Invoke();
                    _signalBus.Fire(new AdSignal(AdType.Interstitial, AdStatus.Completed, placementName));
                    break;
            }
        }

        public void SetAdsDisable()
        {
            _data.isAdsDisable = true;
            saver.SaveNeeded.Invoke(true);
        }
        
        public bool CanShow(EAdType type)
        {
            return adProvider.CanShow(type);
        }
        
        public bool CanShowForLevel(EAdLevelType type)
        {
            return adProvider.CanShowForLevel(type);
        }

        public void ShowStickyAd(string placementName)
        {
            if (!CanShow(EAdType.Sticky))
            {
                return;
            }
            
            adProvider.Show(EAdType.Sticky, placementName);
        }
        
        public void StopStickyAd(string placementName)
        {
            adProvider.StopShow(EAdType.Sticky, placementName);
        }
        
        public void ShowRewardAd(string placementName)
        {
            if (!CanShow(EAdType.Rewarded))
            {
                return;
            }
            
            _settingsService.TurnOffGlobalVolume();
            adProvider.Show(EAdType.Rewarded, placementName);
        }

        public void ShowFullscreenAd(string placementName)
        {
            if (IsAdsDisable || !CanShow(EAdType.FullScreen))
            {
                return;
            }
            
            _settingsService.TurnOffGlobalVolume();
            adProvider.Show(EAdType.FullScreen, placementName);
        }

        private void Init(AdvertisingServiceData data, LoadContext context)
        {
            _data = data;
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new AdvertisingServiceData()), context);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}
