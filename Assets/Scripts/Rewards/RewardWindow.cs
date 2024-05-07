using System.Collections.Generic;
using Core.Repositories;
using DG.Tweening;
using Rewards.Models;
using UnityEngine;
using Zenject;

namespace Rewards
{
    public class RewardViewData
    {
        public RewardModel baseReward;
        public int count;

        public bool Mergeable(RewardModel other)
        {
            return baseReward.id == other.id && baseReward.unit == other.unit;
        }
    }
    
    public class RewardWindow : MonoBehaviour
    {
        [SerializeField] private Transform rewardsParent;
        [SerializeField] private RewardView rewardPrefab;
        [SerializeField] private float cellSize;

        private RewardService _rewardService;
        private ResourceRepository _resourceRepository;
        
        [Inject]
        private void Construct(RewardService rewardService, ResourceRepository resourceRepository)
        {
            _rewardService = rewardService;
            _resourceRepository = resourceRepository;
        }

        public void Init(IReadOnlyList<RewardViewData> rewards, bool interval = false, float delay = 0f)
        {
            foreach (var reward in rewards)
            {
                if(interval)
                {
                    var sequence = DOTween.Sequence();
                    sequence.AppendInterval(delay);
                    sequence.onComplete += () => CreateView(reward);
                }
                else
                {
                    CreateView(reward);
                }
            }
        }
        
        public void Init(IReadOnlyList<RewardModel> rewards)
        {
            foreach (var reward in rewards)
            {
                var view = Instantiate(rewardPrefab, rewardsParent);
                var rewardModel = _rewardService.GetById(reward.id);

                var text = rewardModel.rewardType == ERewardType.Unit
                    ? ""
                    : reward.value.ToString();
                    
                view.Init(new RewardViewParams
                {
                    sprite = rewardModel.sprite,
                    text = text,
                    size = cellSize
                });
            }
        }

        public void Clear()
        {
            foreach (var view in rewardsParent.GetComponentsInChildren<RewardView>())
                Destroy(view.gameObject);
        }

        private void CreateView(RewardViewData reward)
        {
            var view = Instantiate(rewardPrefab, rewardsParent);
            var rewardModel = _rewardService.GetById(reward.baseReward.id);

            view.Init(new RewardViewParams
            {
                sprite = rewardModel.sprite,
                text = reward.count.ToString(),
                size = cellSize
            });
        }
    }
}

