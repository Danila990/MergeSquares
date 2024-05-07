using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CloudServices;
using Core.SaveLoad;
using Core.Windows;
using DG.Tweening;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.InfinityLevel;
using GameScripts.MergeSquares.Shop;
using GameStats;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LeadboardScores
{
    public enum ELeadboardAddType
    {
        None = 0,
        Merge = 1,
        Soft = 2,
    }

    [Serializable]
    public class LeadboardRewardData : ClaimRewardData
    {
        public int scores;
    }
    
    [Serializable]
    public class LeadboardServiceData
    {
        public long weekData = DateTime.Today.Ticks;
        public long monthData = DateTime.Today.Ticks;
        public List<LeadboardRewardData> rewardsToClaim = new();
        public List<LeadboardRewardData> rewardsInProgress = new();
    }

    // [Serializable]
    // public class AddTypeCount
    // {
    //     public ELeadboardAddType type;
    //     public int count;
    // }
    
    public class LeadboardScoresService : MonoBehaviour
    {
        [SerializeField] private Saver saver;
        [SerializeField] private SquaresSkinsManager skinsRepo;
        // [SerializeField] private List<AddTypeCount> addTypes;
        // [SerializeField] private LeadboarsFlyScores flyPrefab;
        [SerializeField] private Transform target;
        [SerializeField] private Transform root;
        [SerializeField] private GameObject leadboardScoresPanel;
        [SerializeField] private TextMeshProUGUI leadboardAddScoresText;
        [SerializeField] private Image coinAddIcon;
        [SerializeField] private List<RatingRewardWithPlaceData> weekRewards = new();
        [SerializeField] private float monthRewardFactor = 6;

        public const string MonthId = "Month";
        public const string WeekId = "Week";
        
        public Action RewardsUpdated = () => {};
        public Action WeekReset = () => {};
        public Action Ticked = () => {};
        public Action MonthReset = () => {};
        public LeadboardServiceData Data => _data;

        private int prevValue = 0;
        [SerializeField] private LeadboardServiceData _data;
        private DateTime _currentWeekData;
        private DateTime _currentMonthData;
        private Calendar _calendar;
        private float _timer = 0f;

        private Coroutine animationAdd;
        // private Tween tween;
        // private Tween colorTween;
        // private bool isAnimActive = false;
        private Color saveColorText;
        private Color saveColorCoin;

        private GridManager _gridManager;
        private GameStatService _gameStatService;
        private CloudService _cloudService;
        
        [Inject]
        public void Construct(GridManager gridManager, GameStatService gameStatService, CloudService cloudService)
        {
            _gridManager = gridManager;
            _gameStatService = gameStatService;
            _cloudService = cloudService;
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            _cloudService.CloudProvider.LeaderBoardFetched += OnLeaderboardFetched;
            _calendar = new CultureInfo("en-US").Calendar;
        }

        private void Start()
        {
            prevValue = _gameStatService.GetStatValue(EGameStatType.Soft);
            leadboardScoresPanel.SetActive(false);
            _gameStatService.StatChanged += StatChanged;
            saveColorText = leadboardAddScoresText.color;
            saveColorCoin = coinAddIcon.color;
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
                        _data.rewardsInProgress.Add(new LeadboardRewardData()
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
                        _data.rewardsInProgress.Add(new LeadboardRewardData()
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

        private void OnDisable()
        {
            _cloudService.CloudProvider.LeaderBoardFetched -= OnLeaderboardFetched;
            _gameStatService.StatChanged -= StatChanged;
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }

        public int GetCurrentPosition(bool isMonth)
        {
            foreach (var reward in _data.rewardsInProgress)
            {
                if (reward.isMonth == isMonth)
                    return reward.placeInRating;
            }

            return -1;
        }
        
        public int GetCurrentScores(bool isMonth)
        {
            foreach (var reward in _data.rewardsInProgress)
            {
                if (reward.isMonth == isMonth)
                    return reward.scores;
            }
            
            return -1;
        }

        public float GetMultiplier()
        {
            float multiplier = 1 + (0.01f * _gridManager.CurrentLevel);
            foreach (var skin in _gridManager.OpenedSkins)
            {
                var skinData = skinsRepo.GetElementByEnum(skin.skinType);
                var skinRarity = skinsRepo.GetRarity(skinData.Rarity);
                // multiplier += skinRarity.multiplierFor100 * (skin.count / 100f);
                multiplier += skinRarity.multiplier + (skin.count * skinRarity.multiplier / 100f);
            }

            multiplier *= 5;
            return multiplier;
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
                _data.rewardsInProgress.Add(new LeadboardRewardData
                {
                    isMonth = false,
                    id = CreateWeekId(),
                    placeInRating = 0,
                    rewardsToClaim = CalcRewards(false, 0)
                });
            }
        }

        public void StartAddScores(ELeadboardAddType type, Vector3 pos, int baseScores)
        {
            // var typeCount = addTypes.Find(t => t.type == type);
            // if (typeCount == null)
            // {
            //     Debug.LogError($"[LeadboardScoresService][StartAddScores] Error: not fount count for type {type}");
            // }
            switch (type)
            {
                case ELeadboardAddType.Soft:
                    var scores = CalculateScores(baseScores);

                    if (animationAdd != null)
                    {
                        StopCoroutine(animationAdd);
                        EndAnim(scores);
                    }
                    leadboardScoresPanel.SetActive(true);
                    leadboardAddScoresText.text = scores.ToString();
                    animationAdd = StartCoroutine(AddAnim(scores));
                    
                    break;
                default:
                    break;
            }
        }
        
        private IEnumerator AddAnim(int scores)
        {
            var time = 2;
            var endTime = Time.time + time;
            var stat = _gameStatService.GetStat(EGameStatType.LeadboardScores);
            var timeCounter = 0f;

            // var saveColorText = leadboardAddScoresText.color;
            // var saveColorCoin = coinAddIcon.color;
            
            while (endTime >= Time.time)
            {
                var newCount = (int)Mathf.Lerp(scores, 0, timeCounter / time);
                leadboardAddScoresText.text = newCount.ToString();
                stat.TrySetLocalDelta(scores - newCount);
                timeCounter += Time.deltaTime;
                var newColorA = Mathf.Lerp(1, 0, timeCounter / time);
                coinAddIcon.color = new Color(saveColorCoin.r, saveColorCoin.g, saveColorCoin.b, newColorA);
                leadboardAddScoresText.color =
                    new Color(saveColorText.r, saveColorText.g, saveColorText.b, newColorA);
                
                yield return null;
            }

            EndAnim(scores);
        }
        
        

        private void EndAnim(int scores)
        {
            AddScores(scores);
            var stat = _gameStatService.GetStat(EGameStatType.LeadboardScores);
            stat.TryResetLocalDelta();
            leadboardScoresPanel.SetActive(false);
            coinAddIcon.color = saveColorCoin;
            leadboardAddScoresText.color = saveColorText;
            animationAdd = null;
        }

        private void StatChanged(EGameStatType type, int value)
        {
            if (type == EGameStatType.Soft)
            {
                var realValue = _gameStatService.GetStat(EGameStatType.Soft).RealValue;
                if (realValue > prevValue)
                {
                    StartAddScores(ELeadboardAddType.Soft, Vector3.zero, (realValue - prevValue));
                }
                prevValue = realValue;
            }
        }

        private void OnLeaderboardFetched(string idOrTag, string variant, int position)
        {
            if (position != -1)
            {
                var rewards = CalcRewards(idOrTag == MonthId, position);
                var data = _data.rewardsInProgress.Find(d => d.id == variant);
                // if (data == null)
                // {
                //     data = _data.rewardsToClaim.Find(d => d.id == variant);
                // }

                if (data != null)
                {
                    data.rewardsToClaim = rewards;
                }
                else
                {
                    _data.rewardsInProgress.Add(new LeadboardRewardData()
                    {
                        isMonth = idOrTag == MonthId,
                        id = variant,
                        placeInRating = position,
                        rewardsToClaim = rewards,
                        scores = 0
                    });
                }
                RewardsUpdated.Invoke();
                saver.SaveNeeded.Invoke(true);
            }
        }

        private int CreateWeekSeed()
        {
            var week = _calendar.GetWeekOfYear(_currentWeekData, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday) + 1;
            return 10000 * week + _currentWeekData.Year;
        }
        
        private int CreateMonthSeed()
        {
            var month = _calendar.GetMonth(_currentMonthData) + 1;
            return 1000000 * month + _currentWeekData.Year;
        }
        
        private string CreateWeekId() => CreateWeekSeed().ToString();
        
        private string CreateMonthId() => CreateMonthSeed().ToString();
        
        private void AddScores(int scores)
        {
            _gameStatService.TryInc(EGameStatType.LeadboardScores, scores);
            foreach (var reward in _data.rewardsInProgress)
            {
                reward.scores += scores;
            }
        }
        
        private void Init(LeadboardServiceData data, LoadContext context)
        {
            _data = data;
            _currentWeekData = new DateTime(_data.weekData);
            _currentMonthData = new DateTime(_data.monthData);
            _cloudService.CloudProvider.FetchLeaderboard(WeekId, CreateWeekId());
            _cloudService.CloudProvider.FetchLeaderboard(MonthId, CreateMonthId());
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new LeadboardServiceData()), context);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
        
        private int CalculateScores(int scoreIn)
        {
            return (int)(scoreIn * GetMultiplier());
        }
    }
}
