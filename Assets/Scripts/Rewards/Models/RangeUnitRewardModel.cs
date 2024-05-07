using System;
using System.Collections.Generic;

namespace Rewards.Models
{
    [Serializable]
    public class RangeUnitRewardModel
    {
        public List<RandomUnitRewardModel> rewards;
        public int chance;
        public int count;
    }
}