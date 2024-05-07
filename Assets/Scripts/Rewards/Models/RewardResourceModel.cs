using System;
using GameStats;
using UnityEngine;
using Utils.Attributes;

namespace Rewards.Models
{
    [Serializable]
    public class RewardResourceModel
    {
        public string id;
        
        public ERewardType rewardType;
        
        [EnumConditionalHide(nameof(rewardType), ERewardType.GameStat, true)]
        public EGameStatType gameStatType;

        public Sprite sprite;
    }
}