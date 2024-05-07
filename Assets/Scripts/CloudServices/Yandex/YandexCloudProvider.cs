using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace CloudServices.Yandex
{
    public class YandexJsonPayments
    {
        public string[] id;
        public string[] title;
        public string[] description;
        public string[] imageURI;
        public string[] priceValue;
        public int[] purchased;
    }

    public class YandexCloudProvider : CloudProviderBase
    {
        [SerializeField] private int minLevelToShowFullscreen = 3;
        [SerializeField] private bool showLog;
        [SerializeField] private bool needWatch;
        [SerializeField] private string language = "ru";
        private const string Service = "YandexService";
        private const string ServiceNeedWatch = "YandexServiceNeedWatch";
        private const string ServiceShowLogs = "YandexServiceShowLogs";

        [DllImport("__Internal")]
        private static extern void ShowRewardAdExtern(string id);

        [DllImport("__Internal")]
        private static extern void ShowFullscreenExtern();

        [DllImport("__Internal")]
        private static extern void SaveExtern(string data);

        [DllImport("__Internal")]
        private static extern void LoadExtern();

        [DllImport("__Internal")]
        private static extern string GetLanguageExtern();

        [DllImport("__Internal")]
        private static extern void RateExtern();

        [DllImport("__Internal")]
        private static extern void CanReviewExtern();

        [DllImport("__Internal")]
        private static extern void PurchaseExtern(string id);
        
        [DllImport("__Internal")]
        private static extern void ConsumePurchaseExtern(string id);

        [DllImport("__Internal")]
        private static extern void GetPurchasesExtern();

        [DllImport("__Internal")]
        private static extern void AuthExtern();

        [DllImport("__Internal")]
        private static extern bool CheckAuthStateExtern();

        [DllImport("__Internal")]
        private static extern void InitSDKExtern();

        [DllImport("__Internal")]
        private static extern bool YandexMetricaSendExtern(string eventName, string eventData);
        
        [DllImport("__Internal")]
        private static extern void GameReadyExtern();

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        public void ShowRewardAd(string id) => ShowRewardAdExtern(id);
        public void ShowFullscreen() => ShowFullscreenExtern();
        public override void Save(string data) => SaveExtern(data);
        public override void Load()
        {
            _watcher.StartWatch("Load");
            Debug.Log($"[YandexService][Load] LoadExtern");
            LoadExtern();
        }

        public override string GetLanguage() => GetLanguageExtern();

        public override void Rate()
        {
            _watcher.StartWatch("Review");
            RateExtern();
        }
        public void CanReview()
        {
            _watcher.StartWatch("CanReview");
            CanReviewExtern();
        }
        public override void Purchase(string id)
        {
            _watcher.StartWatch("Purchase");
            PurchaseExtern(id);
        }
        public override void ConsumePurchase(string id)
        {
            ConsumePurchaseExtern(id);
        }
        public void GetPurchases()
        {
            _watcher.StartWatch("GetPurchases");
            GetPurchasesExtern();
        }
        public override void Auth()
        {
            _watcher.StartWatch("Auth");
            AuthExtern();
        }
        public override bool CheckAuthState()
        {
            var a = CheckAuthStateExtern();
            Log($"[YandexService][CheckAuthState]  state: {a}");
            return a;
        }
        
        public void InitSDK()
        {
            _watcher.StartWatch("InitSDK");
            InitSDKExtern();
        }
        
        public override void SendAnalyticEvent(string eventName, IDictionary<string, object> eventData)
        {
            YandexMetricaSendExtern(eventName, JsonConvert.SerializeObject(eventData));
        }
        
        public override void GameReady()
        {
            GameReadyExtern();
        }
#else
        public void ShowRewardAd(string id)
        {
            var value = 1f;
            var tween = DOTween.To(() => value, x => value = x, 2f, 1);
            tween.onComplete += () =>
            {
                OnRewardAdFinished($"{id}.{RewardAdRewarded}");
                OnRewardAdFinished($"{id}.{RewardAdClosed}");
            };
        }
        
        public override int MinLevelToShowFullscreen() => minLevelToShowFullscreen;

        private bool _debugShowFullscreen;
        public void ShowFullscreen()
        {
            Debug.Log("[YandexCloudProvider][ShowFullscreen] OK!");
            _debugShowFullscreen = !_debugShowFullscreen;
            OnFullscreenAdFinished($"{_debugShowFullscreen.ToString()}");
        }

        public override void Save(string data)
        {
            Debug.LogError($"[YandexService][Save] Empty save for data {data}");
        }

        public override void Load()
        {
            Loaded.Invoke("", PlayerPrefs.GetString("AllData", ""));
        }

        public override string GetLanguage() => language;

        public override void Rate()
        {
            Debug.Log("[RatePopup][Rate] Rating");
            OnReview("true");
        }

        public void CanReview()
        {
            OnCanReview("true");
        }

        public override void Purchase(string id)
        {
            throw new NotImplementedException();
        }

        public override void ConsumePurchase(string id)
        {
            
        }

        public void GetPurchases()
        {
            _purchases.Clear();

            void AddPurchase(string purchaseName, int cost, int amount = 0)
            {
                var purchase = new CloudPurchase
                {
                    id = purchaseName,
                    cost = cost,
                    count = amount,
                    currencySymbol = GetLanguage() == "ru" ? "Ян" : "Yans"
                };
                Log($"[YandexService][GetPurchases] TEST purchase: {JsonConvert.SerializeObject(purchase)}");
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
        }

        public override void Auth() => OnAuth(0);
        public void InitSDK() => OnInitSDK(1);

        public override void SendAnalyticEvent(string eventName, IDictionary<string, object> eventData)
        {
            Log(
                $"[YandexService][SendAnalyticEvent] Event: {eventName} with data: {JsonConvert.SerializeObject(eventData)}");
        }

        public override bool CheckAuthState() => false;
        public override void GameReady() {Debug.Log("[YandexService][GameReady] Ready");}
#endif
        public override bool NeedWatch => needWatch;
        public override bool ShowLog => showLog;
        public override string ServiceName => "YandexService";
        public override string GetCurrency() => "";
        public IReadOnlyDictionary<string, CloudPurchase> Purchases => _purchases;

        private Dictionary<string, CloudPurchase> _purchases = new();
        private bool _reviewChecked = false;
        private Watcher _watcher;
        private int _delayFirstCalls = -1;
        private bool _startGame;
        private bool _isSdkInit;

        private void Start()
        {
            needWatch = PlayerPrefsExt.GetBool(ServiceNeedWatch, needWatch);
            showLog = PlayerPrefsExt.GetBool(ServiceShowLogs, showLog);
            _watcher = new(Service, needWatch);
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            // Задержка вызова метода InitSDK
            if (_delayFirstCalls < 30)
            {
                _delayFirstCalls++;
                if (_delayFirstCalls == 30)
                {
                    if (!_startGame)
                    {
                        _startGame = true;
                        InitSDK();
                    }
                }
            }
        }

        public override bool IsSdkInit()
        {
            return _isSdkInit;
        }

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

        [UsedImplicitly]
        public void OnLoad(string data)
        {
            _watcher.StopWatch("Load");
            Log($"[YandexService][OnLoad]  LoadData: {data}");
            _loadData = data;
            Loaded.Invoke(data, "");
        }

        [UsedImplicitly]
        public void OnPurchase(string str)
        {
            _watcher.StopWatch("Purchase");
            var parsedStr = ParseString(str);
            if(_purchases.TryGetValue(parsedStr[0], out var purchase))
            {
                Purchased.Invoke(purchase, GetBool(Convert.ToInt32(parsedStr[1])));
            }
            else
            {
                Purchased.Invoke(null, GetBool(Convert.ToInt32(parsedStr[1])));
            }
        }

        [UsedImplicitly]
        public void OnPurchaseCheck(string str)
        {
            _watcher.StopWatch("CheckPurchase");
            var parsedStr = ParseString(str);
            if(_purchases.TryGetValue(parsedStr[0], out var purchase))
            {
                PurchaseChecked.Invoke(purchase, GetBool(Convert.ToInt32(parsedStr[1])));
            }
            else
            {
                PurchaseChecked.Invoke(null, GetBool(Convert.ToInt32(parsedStr[1])));
            }
        }

        [UsedImplicitly]
        public void OnPurchasesGot(string data)
        {
            _watcher.StopWatch("GetPurchases");
            var PaymentsData = JsonUtility.FromJson<YandexJsonPayments>(data);
            _purchases.Clear();
            for (int i = 0; i < PaymentsData.id.Length; i++)
            {
                var purchase = new CloudPurchase
                {
                    id = PaymentsData.id[i],
                    title = PaymentsData.title[i],
                    description = PaymentsData.description[i],
                    cost = Convert.ToInt32(PaymentsData.priceValue[i]),
                    count = PaymentsData.purchased[i],
                    currencySymbol = GetLanguage() == "ru" ? "Ян" : "Yans"
                };
                Log($"[YandexService][OnPurchasesGot] purchase: {JsonConvert.SerializeObject(purchase)}");
                _purchases.Add(purchase.id, purchase);
            }

            PurchasesGot.Invoke();
        }

        [UsedImplicitly]
        public void OnCanReview(string result)
        {
            _watcher.StopWatch("CanReview");
            Log($"[YandexService][OnCanReview] result: {result}");
            ReviewChecked.Invoke(GetBool(result));
        }
        
        [UsedImplicitly]
        public void OnReview(string result)
        {
            _watcher.StopWatch("Review");
            Log($"[YandexService][OnReview] result: {result}");
            ReviewGot.Invoke(GetBool(result));
        }

        [UsedImplicitly]
        public void OnAuth(int confirmed)
        {
            Log($"[YandexService][OnAuth] confirmed: {confirmed}");
            _watcher.StopWatch("Auth");
            AuthConfirmed.Invoke(GetBool(confirmed));
        }

        [UsedImplicitly]
        public void OnInitSDK(int result)
        {
            _watcher.StopWatch("InitSDK");
            _isSdkInit = result >= 0;
        }

        [UsedImplicitly]
        public void OnRewardAdFinished(string str)
        {
            var parsedStr = ParseString(str);
            RewardAdFinished.Invoke(parsedStr[0], parsedStr[1], true);
        }
        
        [UsedImplicitly]
        public void OnFullscreenAdFinished(string str)
        {
            FullscreenAdFinished.Invoke(GetBool(str));
        }

        public override void StartRate()
        {
            if (CheckAuthState() && !_reviewChecked)
            {
                _reviewChecked = true;
                CanReview();
            }
            else
            {
#if UNITY_EDITOR
                ReviewChecked.Invoke(true);
#else
                ReviewChecked.Invoke(false);
#endif
            }
        }

        public override void StartShowRewardAd(string id)
        {
            ShowRewardAd(id);
        }

        public override void StartShowFullScreenAd()
        {
            ShowFullscreen();
        }
        
        public override void ShowStickyFromStart(){}

        public override bool TryGetPurchase(string id, out CloudPurchase purchase)
        {
            return _purchases.TryGetValue(id, out purchase);
        }

        public override void StartGetPurchases()
        {
            GetPurchases();
#if UNITY_EDITOR
            // Add test purchases 
            PurchasesGot.Invoke();
#endif
        }
        
        public override void Share(string text)
        {
            if (IsSupportsShare || IsSupportsNativeShare)
            {
                Log($"[YandexService][Share] Try share: {text}");
            }
        }

        public override void Post(string text)
        {
            if (IsSupportsNativePosts)
            {
                Log($"[YandexService][Post] Try post: {text}");
            }
        }

        public override void Invite()
        {
            if (IsSupportsNativeInvite)
            {
                Log($"[YandexService][Invite] Try Invite");
            }
        }

        public override void JoinCommunity()
        {
            if (IsSupportsNativeCommunityJoin && CanJoinCommunity)
            {
                Log($"[YandexService][JoinCommunity] Try join community");
            }
        }

        private List<string> ParseString(string str)
        {
            return str.Split('.').ToList();
        }

        private bool GetBool(int num)
        {
            return num > 0;
        }
        
        private bool GetBool(string value)
        {
            return Convert.ToBoolean(value);
        }

        private void Log(string log)
        {
            if (showLog)
            {
                Debug.Log(log);
            }
        }
    }
}