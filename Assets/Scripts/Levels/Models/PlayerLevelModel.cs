using System;
using System.Collections.Generic;
using Rewards;
using Rewards.Models;

namespace Levels.Models
{
    [Serializable]
    public struct PlayerLevelModel
    {
        public int id;
        public int experience;
        public List<RewardModel> rewards;
    }
}