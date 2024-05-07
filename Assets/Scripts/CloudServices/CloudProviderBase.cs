using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CloudServices
{
    public abstract class CloudProviderBase: MonoBehaviour
    {
        // Ads
        public const string RewardAdRewarded = "rewarded";
        public const string RewardAdClosed = "closed";
        public const string RewardAdError = "error";
        public abstract void StartShowRewardAd(string id);
        public abstract void StartShowFullScreenAd();
        
        public abstract void ShowStickyFromStart();
        public virtual bool IsStartStickyShifted => false;
        
        public Action<string, string, bool> RewardAdFinished = (id, finished, success) => { };
        public Action<bool> FullscreenAdFinished = (finished) => { };
        public Action<bool> StickyChanged = (active) => { };
        public Action OnPause = () => {};
        public Action OnResume = () => {};
        
        public virtual bool IsFullscreenAvailable => true;
        public virtual bool IsRewardedAvailable => true;
        
        public virtual bool IsStickySideAvailable => false;
        
        public virtual bool IsPreloaderPlaying => false;
        public virtual bool IsFullscreenBeforeLevelAvailable => false;
        public virtual bool IsFullscreenAfterLevelAvailable => true;
        
        public virtual int MinLevelToShowFullscreen() => 1;
        public virtual void StartShowStickySideAd(){}
        public virtual void StopShowStickySideAd(){}
        public virtual void RefreshStickySideAd(){}

        // Save load
        public abstract void Load();

        public void LoadOnInit()
        {
            if (CheckAuthState())
            {
                Load();
            }
            else
            {
                Loaded.Invoke("", "");
            }
        }

        public void StartLoad(Func<string> defaultLoad)
        {
            if (CheckAuthState())
            {
                Loaded.Invoke(_loadData, defaultLoad());
            }
            else
            {
                Log($"[{ServiceName}][StartLoad] Use local save");
                Loaded.Invoke("", defaultLoad());
            }
        }

        public abstract void Save(string data);

        public void ClearSaves(Action defaultSaveClear)
        {
            if (CheckAuthState())
            {
                Save("{}");
            }
            defaultSaveClear();

            SceneManager.LoadScene(0);
        }

        public void StartSave(string saveText, Action<string> defaultSave, bool force = false)
        {
            if (OnlyForceSave)
            {
                if (force)
                {
                    Log($"[{ServiceName}][StartSave] Try force save");
                    if (CheckAuthState())
                    {
                        Save(saveText);
                    }
                }
                if(Time.timeSinceLevelLoad - _previousSaveTime > SaveTime)
                {
                    _previousSaveTime = Time.timeSinceLevelLoad;
                    Log($"[{ServiceName}][StartSave] Save to local");
                    defaultSave(saveText);
                }
            }
            else
            {
                if(Time.timeSinceLevelLoad - _previousSaveTime > SaveTime || force)
                {
                    _previousSaveTime = Time.timeSinceLevelLoad;
                    if (CheckAuthState())
                    {
                        Save(saveText);
                    }
                    Log($"[{ServiceName}][StartSave] Save to local");
                    defaultSave(saveText);
                }
            }
        }

        public Action<string, string> Loaded = (data, localData) => { };
        public virtual float SaveTime => 0.1f;
        public virtual bool OnlyForceSave => false;
        
        protected string _loadData = "";
        protected float _previousSaveTime = -1f;


        // Purchases
        public abstract void Purchase(string id);
        public abstract bool TryGetPurchase(string id, out CloudPurchase purchase);
        public void StartPurchase(string id)
        {
            if (CheckAuthState())
            {
                Purchase(id);
            }
        }
        public abstract void StartGetPurchases();
        public abstract void ConsumePurchase(string id);
        
        public Action PurchasesGot = () => { };
        public Action<CloudPurchase, bool> PurchaseChecked = (purchase, present) => { };
        public Action<CloudPurchase, bool> Purchased = (purchase, present) => { };

        // Rate
        public abstract void Rate();
        public abstract void StartRate();
        
        public Action<bool> ReviewChecked = canReview => { };
        public Action<bool> ReviewGot = was => { };
        
        public virtual bool IsPaymentsAvailable => true;
        
        // Auth
        public abstract bool CheckAuthState();
        public abstract void Auth();
        
        public Action<bool> AuthConfirmed = confirmed => { };
        
        // Social
        
        public abstract void Share(string text);
        public abstract void Post(string text);
        public abstract void Invite();
        public abstract void JoinCommunity();
        
        public virtual bool IsSupportsShare => false;
        public virtual bool IsSupportsNativeShare => false;
        public virtual bool IsSupportsNativeInvite => false;
        public virtual bool IsSupportsNativePosts => false;
        public virtual bool IsSupportsNativeCommunityJoin => false;
        public virtual bool CanJoinCommunity => false;
        
        // Leaderboards

        public virtual void Open(string idOrTag, string variant) {}
        public virtual void PublishRecord(string idOrTag, string variant, int value) {}

        public virtual void FetchRating(string idOrTag, string variant)
        {
            // Stub for fetch or not impl error
            RatingFetched.Invoke(idOrTag, variant, -1);
        }
        
        public virtual void FetchLeaderboard(string idOrTag, string variant)
        {
            // Stub for fetch or not impl error
            LeaderBoardFetched.Invoke(idOrTag, variant, -1);
        }
        
        public Action<string, string, int> RatingFetched = (idOrTag, variant, position) => { };

        public Action<string, string, int> LeaderBoardFetched = (idOrTag, variant, position) => { };

        // Other
        public abstract string GetLanguage();
        public abstract void SendAnalyticEvent(string eventName, IDictionary<string, object> eventData);
        public abstract bool IsSdkInit();

        public virtual void GameReady(){}

        public virtual EPlatformType GetPlatformType() => EPlatformType.YANDEX_NAO;
        public virtual ECloudDeviceType GetDeviceType() => ECloudDeviceType.Mobile;
        public abstract string GetCurrency();

        public virtual void GameplayStart() {}

        public virtual void GameplayStop() {}
        
        public virtual void HappyTime() {}

        
        // Debug
        public abstract void SetNeedWatch(bool value);
        public abstract void SetShowLogs(bool value);
        
        public virtual bool NeedWatch => false;
        public virtual bool ShowLog => false;
        
        public virtual string ServiceName => "Base";
        
        protected void Log(string log)
        {
            if (ShowLog)
            {
                Debug.Log(log);
            }
        }
    }
}