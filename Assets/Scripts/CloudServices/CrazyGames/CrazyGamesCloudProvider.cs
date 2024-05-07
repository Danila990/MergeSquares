using System.Collections.Generic;
using System.Runtime.InteropServices;
using CrazyGames;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace CloudServices.CrazyGames
{
    public class CrazyGamesCloudProvider : CloudProviderBase
    {
        [SerializeField] private int defLevelToShowFullscreen = 3;
        [SerializeField] private bool showLog;
        [SerializeField] private bool needWatch;
        
        
        
        private const string SavesKey = "saves";
        private const string Service = "CrazyGamesService";
        private const string ServiceNeedWatch = "CrazyGamesServiceNeedWatch";
        private const string ServiceShowLogs = "CrazyGamesServiceShowLogs";

        private bool _reviewChecked = false;
        private Watcher _watcher;
        private int _delayFirstCalls = -1;
        private bool _startGame;
        private bool _auth;
        private ECloudDeviceType _deviceType;

        private void Start()
        {
            needWatch = PlayerPrefsExt.GetBool(ServiceNeedWatch, needWatch);
            showLog = PlayerPrefsExt.GetBool(ServiceShowLogs, showLog);
            _watcher = new(Service, needWatch);
            DontDestroyOnLoad(this);
            
            CrazySDK.Instance.GetSystemInfo(systemInfo =>
            {
                // TODO: can use locale from here
                // Debug.Log(systemInfo.countryCode);
                switch (systemInfo.device.type)
                {
                    case "desktop":
                        _deviceType = ECloudDeviceType.Desktop;
                        break;
                    case "tablet":
                        _deviceType = ECloudDeviceType.Tablet;
                        break;
                    case "mobile":
                        _deviceType = ECloudDeviceType.Mobile;
                        break;
                }
            });
        }

        #region Auth

        public override void Auth(){}

        public override bool CheckAuthState()
        {
            return false;
        }


        #endregion

        #region Rate

        public override void Rate() {}

        public override void StartRate()
        {
            ReviewChecked.Invoke(false);
        }

        #endregion

        #region Ads
        
        public override void StartShowRewardAd(string id)
        {
            CrazyAds.Instance.beginAdBreakRewarded(() => OnRewardedReward(id), () => OnRewardedClose(id, false));
        }

        public override void StartShowFullScreenAd()
        {
            CrazyAds.Instance.beginAdBreak(() => OnFullscreenClose(true), () => OnFullscreenClose(false));
        }

        private void OnRewardedReward(string id)
        {
            Log($"[CrazyGamesService][OnRewardedReward] OK!");
            RewardAdFinished.Invoke(id, RewardAdRewarded, true);
        }

        private void OnRewardedClose(string id, bool success)
        {
            Log($"[CrazyGamesService][OnRewardedClose] OK!");
            RewardAdFinished.Invoke(id, RewardAdClosed, success);
        }

        private void OnFullscreenClose(bool success)
        {
            Log($"[CrazyGamesService][OnFullscreenClose] OK!");
            FullscreenAdFinished.Invoke(success);
        }
        
        public override bool IsFullscreenBeforeLevelAvailable => true;

        public override bool IsFullscreenAfterLevelAvailable => false;
        
        public override bool IsStickySideAvailable => true;

        public override int MinLevelToShowFullscreen() => defLevelToShowFullscreen;

        public override void StartShowStickySideAd()
        {
            StickyChanged.Invoke(true);
        }
        
        public override void StopShowStickySideAd()
        {
            StickyChanged.Invoke(false);
        }

        public override void RefreshStickySideAd() {}
        
        public override void ShowStickyFromStart() {}
        
        #endregion

        #region SaveLoad
        
        public override void Save(string data){}

        public override void Load(){}
        public override bool OnlyForceSave => false;
        
        #endregion

        #region Purchases

        public override bool IsPaymentsAvailable => false;

        public override void Purchase(string id) {}
        public override bool TryGetPurchase(string id, out CloudPurchase purchase)
        {
            purchase = null;
            return false;
        }

        public override void ConsumePurchase(string id){}
        
        public override void StartGetPurchases()
        {
            PurchasesGot.Invoke();
        }

        #endregion

        #region Other
        
        public override void SendAnalyticEvent(string eventName, IDictionary<string, object> eventData)
        {
            Log($"[CrazyGamesService][SendAnalyticEvent] Event: {eventName} with data: {JsonConvert.SerializeObject(eventData)}");
        }

        public override string GetLanguage() => "en";

        public override bool IsSdkInit()
        {
            return true;
        }

        public override EPlatformType GetPlatformType() => EPlatformType.CRAZY_GAMES_NAO;
        public override ECloudDeviceType GetDeviceType() => _deviceType;
        public override string GetCurrency() => "$";

        public override void GameplayStart()
        {
            CrazyEvents.Instance.GameplayStart();
        }
        
        public override void GameplayStop()
        {
            CrazyEvents.Instance.GameplayStop();
        }

        public override void HappyTime()
        {
            CrazyEvents.Instance.HappyTime();
        }
        
        #endregion


        #region Social

        public override void Share(string text)
        {
            CrazyEvents.Instance.ShowInviteButton(new Dictionary<string, string>());
        }

        public override void Post(string text){}

        public override void Invite()
        {
            if (IsSupportsNativeInvite)
            {
                CrazyEvents.Instance.ShowInviteButton(new Dictionary<string, string>());
            }
        }

        public override void JoinCommunity(){}
        
        public override bool IsSupportsShare => true;
        public override bool IsSupportsNativeInvite => true;

        #endregion
        
        #region Debug

        public override bool NeedWatch => needWatch;
        public override bool ShowLog => showLog;
        public override string ServiceName => "CrazyGamesService";

        public override void SetNeedWatch(bool value)
        {
            PlayerPrefsExt.SetBool(ServiceNeedWatch, value);
            PlayerPrefsExt.Save();
        }

        public override void SetShowLogs(bool value)
        {
            PlayerPrefsExt.SetBool(ServiceShowLogs, value);
            PlayerPrefsExt.Save();
        }

        #endregion
    }
}