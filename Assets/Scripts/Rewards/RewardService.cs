using System;
using System.Collections.Generic;
using Core.Windows;
using GameStats;
using Rewards.Models;
using UnityEngine;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace Rewards
{
    public class RewardService : MonoBehaviour
    {
        [SerializeField] private RewardRepository rewardRepository;

        private GameStatService _gameStatService;
        private WindowManager _windowManager;

        [Inject]
        private void Construct(GameStatService gameStatService, WindowManager windowManager)
        {
            _gameStatService = gameStatService;
            _windowManager = windowManager;
        }

        public RewardResourceModel GetById(string id) => rewardRepository.GetById(id);
        public RewardResourceModel GetByStatType(EGameStatType type) => rewardRepository.GetByStatType(type);

        public void AwardWithPopup(IReadOnlyList<RewardModel> rewards, object[] addArgs = null)
        {
            object[] args;
            var listArgs = new List<object>();
            
            listArgs.Add(new RewardPopupParams{rewards = (List<RewardModel>)rewards});

            if (addArgs != null)
            {
                listArgs.AddRange(addArgs);
            }

            args = listArgs.ToArray();
            
            _windowManager.ShowWindow(
                EPopupType.Reward.ToString(),
                args
            );
        }

        public void AwardAll(IReadOnlyList<RewardModel> rewards)
        {
            foreach (var rewardModel in rewards)
            {
                Award(rewardModel);
            }
        }

        public void Award(RewardModel model)
        {
            var reward = rewardRepository.GetById(model.id);
            switch (reward.rewardType)
            {
                case ERewardType.GameStat:
                    _gameStatService.TryIncWithAnim(reward.gameStatType, model.value);
                    break;
            }
        }
        
        public bool TryConvertReward(RangeStatRewardModel model, out RewardModel rewardModel)
        {
            var res = false;
            rewardModel = null;
            var reward = rewardRepository.GetById(model.baseReward.id);
            switch (reward.rewardType)
            {
                case ERewardType.GameStat:
                    var value = Random.Range(Convert.ToInt32(model.baseReward.value), model.valueMax);
                    rewardModel = new RewardModel
                    {
                        id = model.baseReward.id,
                        value = value,
                        isAdditional = model.baseReward.isAdditional
                    };
                    res = true;
                    break;
                default:
                    Debug.LogWarning($"[RewardService][Award] cant find resource for range stat model with id: {model.baseReward.id}");
                    break;
            }
            return res;
        }
        
        public bool TryConvertReward(RangeUnitRewardModel model, out List<RewardModel> rewardModels)
        {
            var res = false;
            rewardModels = null;
            for (int i = 0; i < model.count; i++)
            {
                var drop = Random.Range(0, 100);

                if (drop > model.chance)
                {
                    Debug.Log($"[RewardService][Award] random not on your side: chance {model.chance} must be >= {drop} in try: {i + 1}");
                    continue;
                }

                if (model.rewards.TryWeightRandom(m => m.chance, out var unit))
                {
                    var rewardResource = rewardRepository.GetById(unit.baseReward.id);
                    switch (rewardResource.rewardType)
                    {
                        case ERewardType.Unit:
                            rewardModels ??= new List<RewardModel>();
                            rewardModels.Add(new RewardModel
                            {
                                id = unit.baseReward.id,
                                unit = unit.baseReward.unit,
                                isAdditional = unit.baseReward.isAdditional
                            });
                            res = true;
                            break;
                        default:
                            Debug.LogWarning($"[RewardService][Award] cant find resource for range unit model with id: {unit.baseReward.id}");
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[RewardService][Award] TryWeightRandom error: {unit.baseReward.id}");
                }
            }
            return res;
        }
        
        public bool TryGetGameStat(RewardModel model, out GameStatContainer gameStat)
        {
            gameStat = new GameStatContainer();

            var reward = rewardRepository.GetById(model.id);
            if(reward.rewardType != ERewardType.GameStat)
            {
                return false;
            }

            gameStat.type = reward.gameStatType;
            gameStat.value = model.value;

            return true;
        }
    }
}