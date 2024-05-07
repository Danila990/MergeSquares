using System;
using System.Collections.Generic;
using Core.SaveLoad;
using Core.Windows;
using GameStats;
using Levels.AnalyticsSignals;
using Levels.Models;
using Levels.Repositories;
using Rewards;
using Rewards.Models;
using UnityEngine;
using Zenject;

namespace Levels
{
    [Serializable]
    public class LevelData
    {
        public int level = 1;
        public int rewardedLevel = 1;
    }

    public class LevelService : MonoBehaviour
    {
        [SerializeField] private Saver saver;

        [SerializeField] private PlayerLevelRepository playerLevelRepository;

        public event Action<int> LevelChanged = level => { };
        public event Action<int> LevelRewardedChanged = level => { };
        public event Action<int> ExperienceChanged = experience => { };
        
        public int MaxExperience => _level.MaxValue;
        public int Experience => _level.Value;
        public int Level => _level.Level;
        public bool Ready { get; private set; }

        private LevelData _levelData;
        private LevelGameStatVariable _level = new LevelGameStatVariable();
        private List<RewardModel> _rewards = new List<RewardModel>();

        private WindowManager _windowManager;
        private GameStatService _gameStatService;
        private RewardService _rewardService;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(WindowManager windowManager, GameStatService gameStatService, RewardService rewardService, SignalBus signalBus)
        {
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _rewardService = rewardService;
            _signalBus = signalBus;

            saver.DataLoaded += OnDataLoaded;
            saver.DataLoadFinished += OnDataLoadedFinished;
            saver.DataSaved += OnDataSaved;
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataLoadFinished -= OnDataLoadedFinished;
            saver.DataSaved -= OnDataSaved;
        }

        public void TakeReward(bool adWatched)
        {
            if (_levelData.level <= _levelData.rewardedLevel)
            {
                return;
            }

            var level = playerLevelRepository.GetById(_levelData.rewardedLevel);
            _rewards.Clear();
            foreach (var reward in level.rewards)
            {
                if (reward.isAdditional)
                {
                    if (adWatched)
                    {
                        _rewards.Add(reward);
                    }
                }
                else
                {
                    _rewards.Add(reward);
                }
            }
            _rewardService.AwardWithPopup(_rewards);
            _levelData.rewardedLevel++;
            LevelRewardedChanged.Invoke(_levelData.rewardedLevel);
            saver.SaveNeeded.Invoke(true);

            if (adWatched)
            {
                _signalBus.Fire(new PlayerLevelUpOfferSignal(_levelData.rewardedLevel));
            }
        }

        public bool HasReward(out PlayerLevelModel model)
        {
            if(_levelData != null)
            {
                model = playerLevelRepository.GetById(_levelData.rewardedLevel);
                return _levelData.level > _levelData.rewardedLevel;
            }

            model = new PlayerLevelModel();
            return false;
        }

        private void OnStatChanged(EGameStatType type, int value)
        {
            ExperienceChanged.Invoke(value);
        }
        
        private void OnLevelChanged(EGameStatType type, int level)
        {
            Console.WriteLine();
            var currentLevel = _levelData.level;

            _levelData.level = level;
            var newLevel = playerLevelRepository.GetById(_levelData.level);

            _level.UpdateStatMax(newLevel.experience);
            LevelChanged.Invoke(_levelData.level);
            saver.SaveNeeded.Invoke(true);

            var id = EPopupType.OfferLvlUp.ToString();
            _windowManager.ShowWindow(id);
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            _levelData = saver.Unmarshal(data, new LevelData());
            var currentLevel = playerLevelRepository.GetById(_levelData.level);
            _level.Init(new LevelGameStatVariableParams
            {
                currentLevel = _levelData.level,
                statMax = currentLevel.experience,
                type = EGameStatType.Experience,
                gameStatService = _gameStatService
            });
            _level.StatChanged += OnStatChanged;
            _level.LevelChanged += OnLevelChanged;
            Ready = true;
        }

        private void OnDataLoadedFinished(LoadContext loadContext)
        {
            _level.ReportState();
        }

        private string OnDataSaved()
        {
            var marshal = saver.Marshal(_levelData);
            return marshal;
        }
    }
}