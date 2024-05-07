using System;

namespace Rewards.Models
{
    [Serializable]
    public class RewardModel
    {
        public string id;
        public string unit;
        public int value;
        public bool isAdditional;
    }
}