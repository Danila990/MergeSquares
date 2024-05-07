using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GamePush;
using GP_Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace CloudServices.GamePush
{
    [Serializable]
    public class GamePushAdsStartConfig
    {
        public int Level;
        public EPlatformType Type;
    }
    public class GamePushCloudProvider : CloudProviderBase
    {
        [SerializeField] private int defLevelToShowFullscreen = 3;
        [SerializeField] private List<GamePushAdsStartConfig> levelsToShowFullscreen = new();
        [SerializeField] private bool showLog;
        [SerializeField] private bool needWatch;
        [SerializeField] private bool onlyForceSave;
        [SerializeField] private EPlatformType editorPlatform;
        private const string SavesKey = "saves";
        private const string Service = "GamePushService";
        private const string ServiceNeedWatch = "GamePushServiceNeedWatch";
        private const string ServiceShowLogs = "GamePushServiceShowLogs";

        private Dictionary<string, CloudPurchase> _purchases = new();
        private Dictionary<string, CloudPurchase> _cloudPurchases = new();
        private bool _reviewChecked = false;
        private Watcher _watcher;
        private int _delayFirstCalls = -1;
        private bool _startGame;

        private void Start()
        {
            needWatch = PlayerPrefsExt.GetBool(ServiceNeedWatch, needWatch);
            showLog = PlayerPrefsExt.GetBool(ServiceShowLogs, showLog);
            _watcher = new(Service, needWatch);
            DontDestroyOnLoad(this);

            // Player
            GP_Player.OnLoginComplete += OnLoginComplete;
            GP_Player.OnLoginError += OnLoginError;
            GP_Player.OnLoadComplete += OnLoadComplete;
            GP_Player.OnLoadError += OnLoadError;
            GP_Player.OnPlayerFetchFieldsComplete += OnPlayerFetchFieldsComplete;
            GP_Player.OnPlayerFetchFieldsError += OnPlayerFetchFieldsError;
            GP_Player.OnSyncComplete += OnSyncComplete;
            GP_Player.OnSyncError += OnSyncError;

            // Payments
            GP_Payments.OnFetchProducts += OnFetchProducts;
            GP_Payments.OnFetchProductsError += OnFetchProductsError;
            GP_Payments.OnConsumeSuccess += OnConsumeSuccess;
            GP_Payments.OnConsumeError += OnConsumeError;
            GP_Payments.OnPurchaseSuccess += OnPurchaseSuccess;
            GP_Payments.OnPurchaseError += OnPurchaseError;
            GP_Payments.OnFetchPlayerPurchases += OnFetchPlayerPurchases;

            // Ads
            GP_Ads.OnFullscreenClose += OnFullscreenClose;

            GP_Game.OnPause += OnPauseCallback;
            GP_Game.OnResume += OnResumeCallback;
            
            GP_LeaderboardScoped.OnFetchPlayerRatingTagVariant += OnFetchPlayerRating;
        }

        private void OnDestroy()
        {
            // Player
            GP_Player.OnLoginComplete -= OnLoginComplete;
            GP_Player.OnLoginError -= OnLoginError;
            GP_Player.OnLoadComplete -= OnLoadComplete;
            GP_Player.OnLoadError -= OnLoadError;
            GP_Player.OnPlayerFetchFieldsComplete -= OnPlayerFetchFieldsComplete;
            GP_Player.OnPlayerFetchFieldsError -= OnPlayerFetchFieldsError;
            GP_Player.OnSyncComplete -= OnSyncComplete;
            GP_Player.OnSyncError -= OnSyncError;

            // Payments
            GP_Payments.OnFetchProducts -= OnFetchProducts;
            GP_Payments.OnFetchProductsError -= OnFetchProductsError;
            GP_Payments.OnConsumeSuccess -= OnConsumeSuccess;
            GP_Payments.OnConsumeError -= OnConsumeError;
            GP_Payments.OnPurchaseSuccess -= OnPurchaseSuccess;
            GP_Payments.OnPurchaseError -= OnPurchaseError;
            GP_Payments.OnFetchPlayerPurchases -= OnFetchPlayerPurchases;

            // Ads
            GP_Ads.OnFullscreenClose -= OnFullscreenClose;
            
            GP_Game.OnPause -= OnPauseCallback;
            GP_Game.OnResume -= OnResumeCallback;
            
            GP_LeaderboardScoped.OnFetchPlayerRatingTagVariant -= OnFetchPlayerRating;
        }

        #region Auth

        public override void Auth()
        {
            _watcher.StartWatch("Auth");
            GP_Player.Login();
        }

        public override bool CheckAuthState()
        {
            return GP_Player.IsLoggedIn();
        }

        private void OnLoginComplete()
        {
            OnAuth(true);
            GP_Player.FetchFields();
        }

        public void OnAuth(bool confirmed)
        {
            Debug.Log($"[GamePushService][OnAuth] confirmed: {confirmed}");
            _watcher.StopWatch("Auth");
            AuthConfirmed.Invoke(confirmed);
        }

        private void OnLoginError() => OnAuth(false);

        #endregion

        #region Rate

        [DllImport("__Internal")]
        private static extern void GPRateExtern();

        [DllImport("__Internal")]
        private static extern bool GPCanRequestReviewExtern();

        [DllImport("__Internal")]
        private static extern bool GPIsAlreadyReviewedExtern();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public override void Rate()
        {
            Debug.Log("[RatePopup][Rate] Rating");
            OnGPReviewSuccess(5);
        }
#else
        public override void Rate()
        {
            if (GPCanRequestReviewExtern())
            {
                _watcher.StartWatch("Review");
                GPRateExtern();
            }
        }
#endif

        public override void StartRate()
        {
            if (CheckAuthState() && !_reviewChecked)
            {
                _reviewChecked = true;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                ReviewChecked.Invoke(true);
#else
                ReviewChecked.Invoke(GPCanRequestReviewExtern());
#endif
            }
            else
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                ReviewChecked.Invoke(true);
#else
                ReviewChecked.Invoke(false);
#endif
            }
        }

        [UsedImplicitly]
        public void OnGPReviewSuccess(int rate)
        {
            _watcher.StopWatch("Review");
            Log($"[GamePushService][OnReviewSuccess] result: {rate.ToString()}");
            ReviewGot.Invoke(true);
        }

        [UsedImplicitly]
        public void OnGPReviewError(string result)
        {
            _watcher.StopWatch("Review");
            Log($"[GamePushService][OnReviewError] error: {result}");
            ReviewGot.Invoke(false);
        }

        #endregion

        #region Ads
        
        public override void StartShowRewardAd(string id)
        {
            GP_Ads.ShowRewarded(id, OnRewardedReward, () => { }, success => OnRewardedClose(id, success));
        }

        public override void StartShowFullScreenAd()
        {
            GP_Ads.ShowFullscreen();
        }
        
        public override void ShowStickyFromStart()
        {
            if (GP_Ads.IsStickyAvailable() && !GP_Ads.IsStickyPlaying() && GetPlatformType() != EPlatformType.CRAZY_GAMES)
            {
                GP_Ads.ShowSticky();
            }
        }
        
        public override bool IsStartStickyShifted
        {
            get
            {
                switch (GetPlatformType())
                {
                    case EPlatformType.None:
                    case EPlatformType.GAME_DISTRIBUTION:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private void OnRewardedReward(string id)
        {
            Log($"[GamePushService][OnRewardedReward] OK!");
            RewardAdFinished.Invoke(id, RewardAdRewarded, true);
        }

        private void OnRewardedClose(string id, bool success)
        {
            Log($"[GamePushService][OnRewardedClose] OK!");
            RewardAdFinished.Invoke(id, RewardAdClosed, success);
        }

        private void OnFullscreenClose(bool success)
        {
            Log($"[GamePushService][OnFullscreenClose] OK!");
            FullscreenAdFinished.Invoke(success);
        }

        public override bool IsFullscreenAvailable => GP_Ads.IsFullscreenAvailable();
        public override bool IsRewardedAvailable => GP_Ads.IsRewardedAvailable();
        public override bool IsPreloaderPlaying => GP_Ads.IsPreloaderAvailable() && GP_Ads.IsPreloaderPlaying();
        public override bool IsFullscreenBeforeLevelAvailable
        {
            get
            {
                switch (GetPlatformType())
                {
                    case EPlatformType.CRAZY_GAMES:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override bool IsFullscreenAfterLevelAvailable
        {
            get
            {
                switch (GetPlatformType())
                {
                    case EPlatformType.CRAZY_GAMES:
                        return false;
                    default:
                        return true;
                }
            }
        }
        
        public override int MinLevelToShowFullscreen() {
            var level = defLevelToShowFullscreen;
            foreach (var config in levelsToShowFullscreen)
            {
                if (config.Type == GetPlatformType())
                {
                    level = config.Level;
                    break;
                }
            }
            return level;
        }
        
        #endregion

        #region SaveLoad
        
        public override void Save(string data)
        {
            GP_Player.Set(SavesKey, data);
            GP_Player.Sync();
        }

        public override void Load()
        {
            GP_Player.Load();
            GP_Player.FetchFields();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            OnPlayerFetchFieldsComplete(null);
#endif
        }
        
        private void OnLoadComplete() => Log($"[GamePushService][OnLoadComplete] OK!");
        private void OnLoadError() => Loaded.Invoke("", "");

        private void OnPlayerFetchFieldsComplete(List<PlayerFetchFieldsData> fields)
        {
            var data = GP_Player.GetString(SavesKey);
            Log($"[GamePushService][OnPlayerFetchFieldsComplete]  LoadData: {data}");
            _loadData = data;
            Loaded.Invoke(data, "");
        }

        private void OnPlayerFetchFieldsError() => Log($"[GamePushService][OnPlayerFetchFieldsError] ERROR!");

        private void OnSyncComplete() => Log($"[GamePushService][OnSyncComplete] OK!");
        private void OnSyncError() => Log($"[GamePushService][OnSyncError] ERROR!");

        public override bool OnlyForceSave => onlyForceSave;
        
        #endregion

        #region Purchases

        public override bool IsPaymentsAvailable => GP_Payments.IsPaymentsAvailable();

        public override void Purchase(string id)
        {
            _watcher.StartWatch("Purchase");
            GP_Payments.Purchase(id);
        }

        public override void ConsumePurchase(string id)
        {
            GP_Payments.Consume(id);
        }

        public void GetPurchases()
        {
            _purchases.Clear();
            _cloudPurchases.Clear();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            void AddPurchase(string purchaseName, int cost, int amount = 0)
            {
                var purchase = new CloudPurchase
                {
                    id = purchaseName,
                    cost = cost,
                    count = amount
                };
                Log($"[GamePushService][GetPurchases] TEST purchase: {JsonConvert.SerializeObject(purchase)}");
                _purchases.Add(purchase.id, purchase);
            }

            AddPurchase("disable_ads", 19);
            AddPurchase("coins_small", 5);
            AddPurchase("coins_wallet", 10);
            AddPurchase("coins_cup", 20);
            AddPurchase("coins_safe", 40);
            AddPurchase("coins_small_sale", 3);
            AddPurchase("coins_wallet_sale", 5);
            AddPurchase("coins_cup_sale", 10);
            AddPurchase("coins_safe_sale", 20);
            AddPurchase("unknown_inApp", 19);
            AddPurchase("starterpack_sale", 24);
            AddPurchase("starterpack", 38);
            AddPurchase("softoffer", 24);
            AddPurchase("softoffer_sale", 38);
#else
            Log($"[GamePushService][GetPurchases] Try get purchases");
            GP_Payments.Fetch();
#endif
        }

        [DllImport("__Internal")]
        public static extern void GPFetchProductsExtern();
        
        [UsedImplicitly]
        public void OnGPFetchProductsExtern(string data)
        {
            Log($"[GamePushService][OnFetchProductsExtern] got products: {data}");
            OnFetchProducts(GP_JSON.GetList<FetchProducts>(data));
        }
        private void OnFetchProducts(List<FetchProducts> products)
        {
            Log($"[GamePushService][OnFetchProducts] got response with products count: {products.Count}");
            for (int i = 0; i < products.Count; i++)
            {
                var purchase = new CloudPurchase
                {
                    cloudId = products[i].id.ToString(),
                    id = products[i].tag,
                    title = products[i].name,
                    description = products[i].description,
                    cost = products[i].price,
                    count = 0,
                    currency = products[i].currency,
                    currencySymbol = products[i].currencySymbol
                };
                Log($"[GamePushService][OnFetchProducts] purchase: {JsonConvert.SerializeObject(purchase)}");
                _purchases.Add(purchase.id, purchase);
                _cloudPurchases.Add(purchase.cloudId, purchase);
            }

            PurchasesGot.Invoke();
        }

        private void OnFetchProductsError() => Log($"[GamePushService][OnFetchProductsError] ERROR!");
        private void OnConsumeSuccess(string id) => Log($"[GamePushService][OnConsumeSuccess] OK!");
        private void OnConsumeError() => Log($"[GamePushService][OnConsumeError] ERROR!");

        private void OnPurchaseSuccess(string id)
        {
            _watcher.StopWatch("Purchase");

            if (_purchases.TryGetValue(id, out var purchase))
            {
                Purchased.Invoke(purchase, true);
            }
            else
            {
                Purchased.Invoke(null, false);
            }
        }

        private void OnPurchaseError()
        {
            _watcher.StopWatch("Purchase");
            Purchased.Invoke(null, false);
        }

        private void OnFetchPlayerPurchases(List<FetchPlayerPurchases> purchases)
        {
            for (int i = 0; i < purchases.Count; i++)
            {
                if (_cloudPurchases.TryGetValue(purchases[i].productId.ToString(), out var purchase))
                {
                    purchase.count++;
                }

                Log(
                    $"[GamePushService][OnFetchPlayerPurchases] try add purchase with id: {purchases[i].productId.ToString()}");
            }
        }

        public override bool TryGetPurchase(string id, out CloudPurchase purchase)
        {
            return _purchases.TryGetValue(id, out purchase);
        }

        public override void StartGetPurchases()
        {
            GetPurchases();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Add test purchases 
            PurchasesGot.Invoke();
#endif
        }

        #endregion

        #region Other

        public override void SendAnalyticEvent(string eventName, IDictionary<string, object> eventData)
        {
            GP_Analytics.Goal(eventName, JsonConvert.SerializeObject(eventData));
        }

        public override string GetLanguage() => GP_Language.ConvertToString(GP_Language.Current());

        public override bool IsSdkInit()
        {
            return true;
        }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public override EPlatformType GetPlatformType() => editorPlatform;
#else
        public override EPlatformType GetPlatformType() => (EPlatformType)GP_Platform.Type();
#endif
        public override string GetCurrency() => _purchases.Values.ToArray()[0].currencySymbol;


        private void OnPauseCallback()
        {
            OnPause.Invoke();
        }
        private void OnResumeCallback()
        {
            OnResume.Invoke();
        }
        
        #endregion


        #region Social

        public override void Share(string text)
        {
            if (IsSupportsShare || IsSupportsNativeShare)
            {
                GP_Socials.Share(text);
            }
        }

        public override void Post(string text)
        {
            if (IsSupportsNativePosts)
            {
                GP_Socials.Post(text);
            }
        }

        public override void Invite()
        {
            if (IsSupportsNativeInvite)
            {
                GP_Socials.Invite();
            }
        }

        public override void JoinCommunity()
        {
            if (IsSupportsNativeCommunityJoin && CanJoinCommunity)
            {
                GP_Socials.JoinCommunity();
            }
        }
        
        public override bool IsSupportsShare => GP_Socials.IsSupportsShare();
        public override bool IsSupportsNativeShare => GP_Socials.IsSupportsNativeShare();
        public override bool IsSupportsNativeInvite => GP_Socials.IsSupportsNativeInvite();
        public override bool IsSupportsNativePosts => GP_Socials.IsSupportsNativePosts();
        public override bool IsSupportsNativeCommunityJoin => GP_Socials.IsSupportsNativeCommunityJoin();
        public override bool CanJoinCommunity => GP_Socials.CanJoinCommunity();

        #endregion
        
        #region Leaderboards

        public override void Open(string idOrTag, string variant) => GP_LeaderboardScoped.Open(idOrTag, variant);
        public override void PublishRecord(string idOrTag, string variant, int value) => GP_LeaderboardScoped.PublishRecord(idOrTag, variant, true, "value", (float)value);

        public override void FetchRating(string idOrTag, string variant)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Stub for fetch or not impl error
            OnFetchPlayerRating(string.Empty, variant, 1000);
#else
            GP_LeaderboardScoped.FetchPlayerRating(idOrTag, variant, "value");
#endif
        }
        
        public override void FetchLeaderboard(string idOrTag, string variant)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Stub for fetch or not impl error
            OnFetchPlayerLeaderboard(string.Empty, variant, 1000);
#else
            GP_LeaderboardScoped.FetchPlayerRating(idOrTag, variant, "value");
#endif
        }

        private void OnFetchPlayerRating(string idOrTag, string variant, int value)
        {
            RatingFetched.Invoke(idOrTag, variant, value);
        }
        
        private void OnFetchPlayerLeaderboard(string idOrTag, string variant, int value)
        {
            LeaderBoardFetched.Invoke(idOrTag, variant, value);
        }
        
        #endregion
        
        #region Debug

        public override bool NeedWatch => needWatch;
        public override bool ShowLog => showLog;
        public override string ServiceName => "GamePushService";

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