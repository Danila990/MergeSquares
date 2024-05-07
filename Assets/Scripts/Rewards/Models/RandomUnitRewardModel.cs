using System;

namespace Rewards.Models
{
    [Serializable]
    public class RandomUnitRewardModel
    {
        public RewardModel baseReward;
        public int chance;
    }
}