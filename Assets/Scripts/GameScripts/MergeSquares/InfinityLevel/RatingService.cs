using System;
using System.Collections.Generic;
using System.Globalization;
using CloudServices;
using Core.SaveLoad;
using GameScripts.MergeSquares.Shop;
using GameStats;
using JetBrains.Annotations;
using UnityEngine;
using Utils.Attributes;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    [Serializable]
    public enum ERewardViewType
    {
        Stat = 0,
        UnitSkin = 1
    }
    
    [Serializable]
    public class RewardData
    {
        public ERewardViewType type;
        [EnumConditionalHide(nameof(type), ERewardViewType.Stat, true)] 
        public EGameStatType statType;
        [EnumConditionalHide(nameof(type), ERewardViewType.UnitSkin, true)]
        public ESkinRarity rarity;
        public int baseAmount;

        public RewardData(RewardData from)
        {
            type = from.type;
            statType = from.statType;
            rarity = from.rarity;
            baseAmount = from.baseAmount;
        }
    }
    
    [Serializable]
    public class RatingRewardWithPlaceData
    {
        public List<RewardData> rewardDeltas = new();
        public int placeInRatingMore;
        public int placeInRatingLess;
        public int step;
    }
    
    [Serializable]
    public class ClaimRewardData
    {
        public string id;
        public bool isMonth;
        public int placeInRating;
        public List<RewardData> rewardsToClaim = new();
    }
    
    [Serializable]
    public class RatingServiceData
    {
        public long weekData = DateTime.Today.Ticks;
        public long monthData = DateTime.Today.Ticks;
        public List<ClaimRewardData> rewardsToClaim = new();
        public List<ClaimRewardData> rewardsInProgress = new();
    }
    
    public class RatingService : MonoBehaviour
    {
        [SerializeField] private Saver saver;
        [SerializeField] private float monthRewardFactor = 6;
        [SerializeField] private List<RatingRewardWithPlaceData> weekRewards = new();
        
        public const string MonthId = "Month";
        public const string WeekId = "Week";

        public IReadOnlyList<RatingRewardWithPlaceData> WeekRewards => weekRewards;
        public float MonthRewardFactor => monthRewardFactor;

        public Action Ticked = () => {};
        public Action MonthReset = () => {};
        public Action WeekReset = () => {};
        public Action RewardsUpdated = () => {};

        public RatingServiceData Data => _data;

        //TODO: remove SerializeField
        [SerializeField] private RatingServiceData _data;
        private float _timer = 0f;
        private DateTime _currentWeekData;
        private DateTime _currentMonthData;
        private Calendar _calendar;
        
        private CloudService _cloudService;
        private GameStatService _gameStatService;

        [Inject]
        public void Construct(
            CloudService cloudService,
            GameStatService gameStatService
        )
        {
            _gameStatService = gameStatService;
            _cloudService = cloudService;
            _cloudService.CloudProvider.RatingFetched += OnRatingFetched;

            saver.DataLoaded += OnDataLoaded;
            saver.DataLoadFinished += OnDataLoadedFinished;
            saver.DataSaved += OnDataSaved;
            
            _calendar = new CultureInfo("en-US").Calendar;
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.RatingFetched -= OnRatingFetched;
            
            saver.DataLoaded -= OnDataLoaded;
            saver.DataLoadFinished -= OnDataLoadedFinished;
            saver.DataSaved -= OnDataSaved;
        }

        [UsedImplicitly]
        public void FinishWeek()
        {
            var thisWeek = _calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            var savedWeek = _calendar.GetWeekOfYear(_currentWeekData, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            // if (thisWeek != savedWeek)
            // {
            foreach (var rewardData in _data.rewardsInProgress)
            {
                if (!rewardData.isMonth && rewardData.placeInRating > 0)
                {
                    _data.rewardsToClaim.Add(rewardData);
                }
            }
            foreach (var rewardData in _data.rewardsToClaim)
            {
                if (!rewardData.isMonth)
                {
                    _data.rewardsInProgress.Remove(rewardData);
                }
            }
            Debug.Log($"WeekReset");
            WeekReset.Invoke();

            // }
            _currentWeekData = DateTime.Now;
            _data.weekData = DateTime.Today.Ticks;
            saver.SaveNeeded.Invoke(true);

            if (_data.rewardsInProgress.FindAll(rd => !rd.isMonth).Count <= 0)
            {
                _data.rewardsInProgress.Add(new ClaimRewardData
                {
                    isMonth = false,
                    id = CreateWeekId(),
                    placeInRating = 0,
                    rewardsToClaim = CalcRewards(false, 0)
                });
            }
        }
        
        private void Update()
        {
            // return;
            _timer += Time.deltaTime;
            if(_timer >= 1.0f)
            {
                _timer = 0;
                Ticked.Invoke();
                if(_currentWeekData != DateTime.Today)
                {
                    var thisWeek = _calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                    var savedWeek = _calendar.GetWeekOfYear(_currentWeekData, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                    if (thisWeek != savedWeek)
                    {
                        foreach (var rewardData in _data.rewardsInProgress)
                        {
                            if (!rewardData.isMonth && rewardData.placeInRating > 0)
                            {
                                _data.rewardsToClaim.Add(rewardData);
                            }
                        }
                        foreach (var rewardData in _data.rewardsToClaim)
                        {
                            if (!rewardData.isMonth)
                            {
                                _data.rewardsInProgress.Remove(rewardData);
                            }
                        }
                        Debug.Log($"WeekReset");
                        WeekReset.Invoke();

                    }
                    _currentWeekData = DateTime.Now;
                    _data.weekData = DateTime.Today.Ticks;
                    saver.SaveNeeded.Invoke(true);

                    if (_data.rewardsInProgress.FindAll(rd => !rd.isMonth).Count <= 0)
                    {
                        _data.rewardsInProgress.Add(new ClaimRewardData
                        {
                            isMonth = false,
                            id = CreateWeekId(),
                            placeInRating = 0,
                            rewardsToClaim = CalcRewards(false, 0)
                        });
                    }
                }
                
                if(_currentMonthData != DateTime.Today)
                {
                    var thisMonth = _calendar.GetMonth(DateTime.Today);
                    var savedMonth = _calendar.GetMonth(_currentMonthData);
                    if (thisMonth != savedMonth)
                    {
                        foreach (var rewardData in _data.rewardsInProgress)
                        {
                            if (rewardData.isMonth && rewardData.placeInRating > 0)
                            {
                                _data.rewardsToClaim.Add(rewardData);
                            }
                        }
                        foreach (var rewardData in _data.rewardsToClaim)
                        {
                            if (rewardData.isMonth)
                            {
                                _data.rewardsInProgress.Remove(rewardData);
                            }
                        }
                        MonthReset.Invoke();
                    }
                    _currentMonthData = DateTime.Now;
                    _data.monthData = DateTime.Today.Ticks;
                    saver.SaveNeeded.Invoke(true);

                    if (_data.rewardsInProgress.FindAll(rd => rd.isMonth).Count <= 0)
                    {
                        _data.rewardsInProgress.Add(new ClaimRewardData
                        {
                            isMonth = true,
                            id = CreateMonthId(),
                            placeInRating = 0,
                            rewardsToClaim = CalcRewards(false, 01)
                        });
                    }
                }
            }
        }

        public void OpenTable(bool isMonth, bool isCurrent)
        {
            _cloudService.CloudProvider.Open(isMonth ? MonthId : WeekId, CreateWeekSeed(isCurrent).ToString());
        }
        
        public List<RewardData> CalcRewards(bool isMonth, int position)
        {
            var res = new List<RewardData>();
            foreach (var baseDelta in weekRewards[0].rewardDeltas)
            {
                res.Add(new RewardData(baseDelta));
            }

            foreach (var weekReward in weekRewards)
            {
                if (weekReward.placeInRatingMore != -1 && position < weekReward.placeInRatingMore)
                {
                    // TODO: check if steps = 0
                    var steps = 0;
                    if (position < weekReward.placeInRatingLess)
                    {
                        steps = (weekReward.placeInRatingMore - weekReward.placeInRatingLess) / weekReward.step;
                    }
                    else
                    {
                        steps = (weekReward.placeInRatingMore - position) / weekReward.step + 1;
                    }
                    foreach (var data in res)
                    {
                        foreach (var rewardDelta in weekReward.rewardDeltas)
                        {
                            if (rewardDelta.type == data.type/* && (rewardDelta.statType == data.statType || rewardDelta.rarity == data.rarity)*/)
                            {
                                switch (rewardDelta.type)
                                {
                                    case ERewardViewType.Stat:
                                        if (rewardDelta.statType == data.statType)
                                        {
                                            data.baseAmount += steps * (isMonth
                                                ? rewardDelta.baseAmount * (int)monthRewardFactor
                                                : rewardDelta.baseAmount);
                                        }
                                        break;
                                    case ERewardViewType.UnitSkin:
                                        if(rewardDelta.rarity == data.rarity)
                                            data.baseAmount += (isMonth
                                                ? rewardDelta.baseAmount * (int)monthRewardFactor
                                                : rewardDelta.baseAmount);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            
            return res;
        }

        public bool RemoveReward(string id)
        {
            var rewardData = _data.rewardsToClaim.Find(r => r.id == id);
            if(rewardData == null)
                return false;
            else
            {
                _data.rewardsToClaim.Remove(rewardData);
                saver.SaveNeeded.Invoke(true);
                return true;
            }
        }

        public void PublishValueAndCheckRating(bool isMonth, int value, long startTimestamp)
        {
            var valueWeek = _calendar.GetWeekOfYear(new DateTime(startTimestamp), CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            var currentWeek = _calendar.GetWeekOfYear(_currentWeekData, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            if (valueWeek != currentWeek)
            {
                return;
            }

            var idOrTag = isMonth ? MonthId : WeekId;
            var variant = isMonth ? CreateMonthId() : CreateWeekId();
            _cloudService.CloudProvider.PublishRecord(idOrTag, variant, value);
            _cloudService.CloudProvider.FetchRating(idOrTag, variant);
        }
        
        public int CreateWeekSeed(bool current = true)
        {
            var week = _calendar.GetWeekOfYear(_currentWeekData, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            return 10000 * (current ? week : week > 0 ? week - 1 : week) + (!current && week == 0 ? _currentWeekData.Year - 1 : _currentWeekData.Year);
            // return DateTime.Now.Minute;
        }
        
        public int CreateMonthSeed(bool current = true)
        {
            var month = _calendar.GetMonth(_currentMonthData);
            return 1000000 * (current ? month : month > 0 ? month - 1 : month) + (!current && month == 0 ? _currentWeekData.Year - 1 : _currentWeekData.Year);
            // return DateTime.Now.Minute;
        }

        private void OnRatingFetched(string idOrTag, string variant, int position)
        {
            if (position != -1)
            {
                var rewards = CalcRewards(idOrTag == MonthId, position);
                var data = _data.rewardsInProgress.Find(d => d.id == variant);
                // if (data == null)
                // {
                    // data = _data.rewardsToClaim.Find(d => d.id == variant);
                // }

                if (data != null)
                {
                    data.rewardsToClaim = rewards;
                }
                else
                {
                    _data.rewardsInProgress.Add(new ClaimRewardData
                    {
                        isMonth = idOrTag == MonthId,
                        id = variant,
                        placeInRating = position,
                        rewardsToClaim = rewards
                    });
                }
                RewardsUpdated.Invoke();
                saver.SaveNeeded.Invoke(true);
            }
        }

        private string CreateWeekId() => CreateWeekSeed().ToString();
        
        private string CreateMonthId() => CreateMonthSeed().ToString();
        
        private void Init(RatingServiceData data, LoadContext context)
        {
            _data = data;
            _currentWeekData = new DateTime(_data.weekData);
            _currentMonthData = new DateTime(_data.monthData);
            _cloudService.CloudProvider.FetchRating(WeekId, CreateWeekId());
            _cloudService.CloudProvider.FetchRating(MonthId, CreateMonthId());
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new RatingServiceData()), context);
        }

        private void OnDataLoadedFinished(LoadContext loadContext)
        {
            // AddOfflineProgress(loadContext);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}
