using System;
using System.Collections.Generic;
using Core.Repositories;
using GameStats;
using Rewards.Models;
using UnityEngine;
using Utils;

namespace Rewards
{
    [CreateAssetMenu(fileName = "RewardRepository", menuName = "Repositories/RewardRepository")]
    public class RewardRepository : ScriptableObject
    {
        [SerializeField] private List<RewardResourceModel> rewards;

        public RewardResourceModel GetById(string id) => rewards.GetBy(value => value.id == id);
        public RewardResourceModel GetByStatType(EGameStatType type) => rewards.GetBy(value => value.gameStatType == type);

        public List<RewardModel> ParseCurrencies(string value, bool additional = false)
        {
            var res = new List<RewardModel>();

            foreach (var reward in TableParser.ParseStringList(value))
            {
                var rewardParams = TableParser.ParseStringList(reward, '-');
                var rewardModel = new RewardModel();
                var resourceModel = GetById(rewardParams[0]);
                if(resourceModel != null)
                {
                    rewardModel.id = rewardParams[0];
                    if (resourceModel.rewardType == ERewardType.Unit)
                    {
                        rewardModel.unit = rewardParams[1];
                    }
                    else
                    {
                        rewardModel.value = Convert.ToInt32(rewardParams[1]);
                    }
                    rewardModel.isAdditional = additional;
                    res.Add(rewardModel);
                }
                else
                {
                    Debug.LogError($"[RewardRepository][ParseCurrencies] failed to find with id: {rewardParams[0]} in reward repository. Source: {value}");
                }
            }
            
            return res;
        }
        
        public List<RangeStatRewardModel> ParseRangeCurrencies(string value, bool additional = false)
        {
            var res = new List<RangeStatRewardModel>();

            foreach (var reward in TableParser.ParseStringList(value))
            {
                var rewardParams = TableParser.ParseStringList(reward, '-');
                var rangeRewardModel = new RangeStatRewardModel();
                var rewardModel = new RewardModel();
                var resourceModel = GetById(rewardParams[0]);
                if (resourceModel != null)
                {
                    rewardModel.id = rewardParams[0];
                    if (resourceModel.rewardType == ERewardType.Unit)
                    {
                        rewardModel.unit = rewardParams[1];
                    }
                    else
                    {
                        rewardModel.value = Convert.ToInt32(rewardParams[1]);
                    }
                    rewardModel.isAdditional = additional;
                    rangeRewardModel.baseReward = rewardModel;
                    rangeRewardModel.valueMax = Convert.ToInt32(rewardParams[2]);
                    res.Add(rangeRewardModel);
                }
                else
                {
                    Debug.LogError($"[RewardRepository][ParseCurrencies] failed to find with id: {rewardParams[0]} in reward repository. Source: {value}");
                }
            }
            
            return res;
        }
        
        public List<RewardModel> ParseUnits(string value, bool additional = false)
        {
            var res = new List<RewardModel>();

            foreach (var reward in TableParser.ParseStringList(value))
            {
                var rewardModel = new RewardModel();
                rewardModel.id = "Unit";
                rewardModel.unit = reward;
                rewardModel.isAdditional = additional;
                res.Add(rewardModel);
            }
            
            return res;
        }
        
        public List<RangeUnitRewardModel> ParseRangeUnits(string value, bool additional = false)
        {
            var res = new List<RangeUnitRewardModel>();

            foreach (var rangeReward in TableParser.ParseStringList(value, '@'))
            {
                var rangeRewardParams = TableParser.ParseStringList(rangeReward, '%');
                var rangeUnitRewardModel = new RangeUnitRewardModel();
                rangeUnitRewardModel.count = Convert.ToInt32(rangeRewardParams[0]);
                rangeUnitRewardModel.chance = Convert.ToInt32(rangeRewardParams[1]);
                rangeUnitRewardModel.rewards = new List<RandomUnitRewardModel>();
                foreach (var randomReward in TableParser.ParseStringList(rangeRewardParams[2]))
                {
                    var randomRewardParams = TableParser.ParseStringList(randomReward, '-');
                    var rewardModel = new RewardModel();
                    rewardModel.id = "Unit";
                    rewardModel.unit = randomRewardParams[1];
                    rewardModel.isAdditional = additional;
                    
                    var randomRewardModel = new RandomUnitRewardModel();
                    randomRewardModel.chance = Convert.ToInt32(randomRewardParams[0]);
                    randomRewardModel.baseReward = rewardModel;
                    rangeUnitRewardModel.rewards.Add(randomRewardModel);
                }
                
                res.Add(rangeUnitRewardModel);
            }
            
            return res;
        }
    }
}