using System;
using System.Collections.Generic;
using Advertising;
using UnityEngine;
using Zenject;

namespace CloudServices
{
    [Serializable]
    public class CloudAdConfig
    {
        public EAdType type;
        public EPlatformType platformType;
        public float pauseTime;
    }
    
    [Serializable]
    public class CloudSingleAdData
    {
        public string id;
        public string placementName;
        public bool started;
        public bool rewarded;
    }
    
    [Serializable]
    public class CloudAdData
    {
        public EAdType type;
        public List<CloudSingleAdData> ads = new();
        public float nextShowTime;

        public CloudSingleAdData GetById(string id)
        {
            foreach (var ad in ads)
            {
                if (ad.id == id)
                {
                    return ad;
                }
            }

            return null;
        }
    }
    
    public class CloudAdProvider : AdProviderBase
    {
        [SerializeField] private List<CloudAdConfig> configs = new();
        
        private List<CloudAdData> _data = new();
        
        private CloudService _cloudService;
        
        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
            _cloudService.CloudProvider.RewardAdFinished += OnRewardAdFinished;
            _cloudService.CloudProvider.FullscreenAdFinished += OnFullscreenAdFinished;
        }

        private void Start()
        {
            _available = _cloudService.CloudProvider.IsSdkInit();
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.RewardAdFinished -= OnRewardAdFinished;
            _cloudService.CloudProvider.FullscreenAdFinished -= OnFullscreenAdFinished;
        }

        public void DebugShowFullscreen()
        {
            if (TryShow(EAdType.FullScreen, "DebugShow"))
            {
                Debug.Log("[CloudAdProvider][DebugShowFullscreen] OK!");
            }
            else
            {
                Debug.Log("[CloudAdProvider][DebugShowFullscreen] Error!");
            }
        }
        
        public override void Show(EAdType type, string placementName)
        {
            TryShow(type, placementName);
        }

        public override bool TryShow(EAdType type, string placementName)
        {
            if (!_available || !CanShow(type))
            {
                return false;
            }
            
            var data = GetData(type);
            var config = GetConfig(type);
            
            data.ads.Clear();
            var ad = new CloudSingleAdData
            {
                id = Guid.NewGuid().ToString(),
                placementName = placementName,
                started = true
            };
            data.ads.Add(ad);
            data.nextShowTime = Time.time + config.pauseTime;
            
            switch (type)
            {
                case EAdType.Rewarded:
                    _cloudService.CloudProvider.StartShowRewardAd(ad.id);
                    break;
                case EAdType.FullScreen:
                    _cloudService.CloudProvider.StartShowFullScreenAd();
                    break;
                case EAdType.Sticky:
                    _cloudService.CloudProvider.RefreshStickySideAd();
                    _cloudService.CloudProvider.StartShowStickySideAd();
                    break;
            }

            return true;
        }

        public override void StopShow(EAdType type, string placementName)
        {
            var data = GetData(type);
            if(data.ads.Count > 0)
            {
                switch (type)
                {
                    case EAdType.Sticky:
                        _cloudService.CloudProvider.StopShowStickySideAd();
                        data.ads.Clear();
                        break;
                }
            }
            
        }

        public override bool CanShowForLevel(EAdLevelType type)
        {
            switch (type)
            {
                case EAdLevelType.Before:
                    return _cloudService.CloudProvider.IsFullscreenBeforeLevelAvailable;
                default:
                    return _cloudService.CloudProvider.IsFullscreenAfterLevelAvailable;
            }
        }

        public override bool CanShow(EAdType type)
        {
            var config = GetConfig(type);
            if (config == null)
            {
                return false;
            }
            
            var data = GetData(type);
            var started = false;
            foreach (var ad in data.ads)
            {
                if (ad.started)
                {
                    started = true;
                    break;
                }
            }
            var timeOk = Time.time > data.nextShowTime;
            return _available && timeOk && !started;
        }

        private CloudAdConfig GetConfig(EAdType type)
        {
            CloudAdConfig typeConfig = null;
            foreach (var config in configs)
            {
                if (config.type == type)
                {
                    typeConfig = config;
                }
                if (config.type == type && _cloudService.CloudProvider.GetPlatformType() == config.platformType)
                {
                    return config;
                }
            }

            return typeConfig;
        }
        
        private CloudAdData GetData(EAdType type)
        {
            foreach (var data in _data)
            {
                if (data.type == type)
                {
                    return data;
                }
            }

            var newData = new CloudAdData {type = type, nextShowTime = 0};
            _data.Add(newData);
            return newData;
        }
        
        private void OnRewardAdFinished(string id, string finishType, bool success)
        {
            var data = GetData(EAdType.Rewarded);
            var ad = data.GetById(id);
            if(ad != null)
            {
                var type = _cloudService.CloudProvider.GetPlatformType();
                switch (type)
                {
                    // Editor
                    case EPlatformType.None:
                        switch (finishType)
                        {
                            case CloudProviderBase.RewardAdRewarded:
                                if (success)
                                {
                                    Rewarded.Invoke(EAdType.Rewarded, ad.placementName);
                                }
                                else
                                {
                                    Failed.Invoke(EAdType.Rewarded, ad.placementName);                            
                                }
                                ad.started = false;
                                break;
                        }
                        break;
                    case EPlatformType.YANDEX_NAO:
                        YandexRewardFinished(ad, finishType);
                        break;
                    case EPlatformType.CRAZY_GAMES_NAO:
                        CrazyRewardFinished(ad, success);
                        break;
                    default:
                        RewardFinished(ad, finishType, success);
                        break;
                }
            }
        }

        private void YandexRewardFinished(CloudSingleAdData ad, string finishType)
        {
            switch (finishType)
            {
                case CloudProviderBase.RewardAdClosed:
                    if (ad.rewarded)
                    {
                        Rewarded.Invoke(EAdType.Rewarded, ad.placementName);
                    }
                    else
                    {
                        Failed.Invoke(EAdType.Rewarded, ad.placementName);                            
                    }
                    ad.started = false;
                    break;
                case CloudProviderBase.RewardAdError:
                    Failed.Invoke(EAdType.Rewarded, ad.placementName);
                    ad.started = false;
                    break;
                case CloudProviderBase.RewardAdRewarded:
                    ad.rewarded = true;
                    break;
            }
        }
        
        private void CrazyRewardFinished(CloudSingleAdData ad, bool success = false)
        {
            if (success)
            {
                Rewarded.Invoke(EAdType.Rewarded, ad.placementName);
            }
            else
            {
                Failed.Invoke(EAdType.Rewarded, ad.placementName);
            }
            ad.started = false;
        }
        
        private void RewardFinished(CloudSingleAdData ad, string finishType, bool success = false)
        {
            switch (finishType)
            {
                case CloudProviderBase.RewardAdRewarded:
                    if (ad.rewarded)
                    {
                        Rewarded.Invoke(EAdType.Rewarded, ad.placementName);
                    }
                    else
                    {
                        Failed.Invoke(EAdType.Rewarded, ad.placementName);                            
                    }
                    ad.started = false;
                    break;
                case CloudProviderBase.RewardAdError:
                    Failed.Invoke(EAdType.Rewarded, ad.placementName);
                    ad.started = false;
                    break;
                case CloudProviderBase.RewardAdClosed:
                    ad.rewarded = success;
                    break;
            }
        }
        
        private void OnFullscreenAdFinished(bool finished)
        {
            var data = GetData(EAdType.FullScreen);
            if(data.ads.Count > 0)
            {
                var ad = data.ads[0];
                if (finished)
                {
                    Rewarded.Invoke(EAdType.FullScreen, ad.placementName);
                }
                else
                {
                    data.nextShowTime = 0;
                    Failed.Invoke(EAdType.FullScreen, ad.placementName);
                }
                data.ads.Clear();
            }
        }
    }
}