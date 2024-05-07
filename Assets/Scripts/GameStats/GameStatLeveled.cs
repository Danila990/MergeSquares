using System;
using System.Collections.Generic;
using Core.SaveLoad;
using UnityEngine;
using Zenject;

namespace GameStats
{
    [Serializable]
    public class GameStatLeveledData
    {
        public EGameStatType levelType;
        public EGameStatType experienceType;
        public List<int> experienceForLevel = new();
    }
    
    public class GameStatLeveled : MonoBehaviour
    {
        [SerializeField] private List<GameStatLeveledData> levels = new();

        public Action<GameStatLeveledData> Changed = data => {};

        private GameStatService _gameStatService;
        private SaveService _saveService;
        
        [Inject]
        public void Construct(GameStatService gameStatService, SaveService saveService)
        {
            _gameStatService = gameStatService;
            _saveService = saveService;
            _saveService.LoadFinished += OnLoadFinished;
        }

        private void OnDestroy()
        {
            _saveService.LoadFinished -= OnLoadFinished;
            foreach (var data in levels)
            {
                var experienceStat = _gameStatService.GetStat(data.experienceType);
                experienceStat.StatChanged -= OnStatChanged;
            }
        }

        public int GetLevel(EGameStatType levelType) => _gameStatService.Get(levelType);

        public int GetLevelCurrent(EGameStatType levelType)
        {
            var data = FindDataByLevelType(levelType);
            if (data != null)
            {
                return _gameStatService.Get(data.experienceType);
            }

            return 0;
        }
        
        public int GetLevelMax(EGameStatType levelType)
        {
            var data = FindDataByLevelType(levelType);
            if (data != null)
            {
                var level = GetLevel(levelType);
                if (level < data.experienceForLevel.Count)
                {
                    return data.experienceForLevel[level];
                }
                return data.experienceForLevel[^1];
            }

            return 0;
        }

        private void OnLoadFinished(LoadContext context)
        {
            // Need to tune exp stats
            foreach (var data in levels)
            {
                var levelStat = SetupStat(data.levelType, data.experienceForLevel.Count - 1);
                var experienceStat = SetupStat(data.experienceType, data.experienceForLevel[levelStat.realValue]);
                experienceStat.StatChanged += OnStatChanged;
            }
        }

        private void OnStatChanged(AGameStatVariable<int> baseStat)
        {
            if (baseStat is GameStatVariable stat)
            {
                var data = FindDataByExperienceType(stat.type);
                if(data != null)
                {
                    var level = GetLevel(data.levelType);
                    if (stat.GetValue() >= data.experienceForLevel[level])
                    {
                        _gameStatService.TryInc(data.levelType, 1);
                        _gameStatService.TrySetWithAnim(data.experienceType, 0);
                        var newLevel = GetLevel(data.levelType);
                        SetupStat(data.experienceType, data.experienceForLevel[newLevel]);
                    }
                    Changed.Invoke(data);
                }
            }
        }

        private GameStatVariable SetupStat(EGameStatType type, int limit)
        {
            var stat = _gameStatService.GetStat(type);
            stat.limited = true;
            stat.limit = limit;
            stat.realValue = stat.realValue > stat.limit
                ? stat.limit
                : stat.realValue;
            return stat;
        }
        
        private GameStatLeveledData FindDataByLevelType(EGameStatType levelType)
        {
            foreach (var data in levels)
            {
                if (data.levelType == levelType)
                {
                    return data;
                }
            }

            return null;
        }
        
        private GameStatLeveledData FindDataByExperienceType(EGameStatType experienceType)
        {
            foreach (var data in levels)
            {
                if (data.experienceType == experienceType)
                {
                    return data;
                }
            }

            return null;
        }
    }
}