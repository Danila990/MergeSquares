using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.PointPanel
{
    [Serializable]
    public class StatickLevelData
    {
        public int number;
        public List<int> pointIds;
        public int buttonsCount;
        public int attempts;
    }

    [CreateAssetMenu(fileName = "StatickLevelsRepository", menuName = "Repositories/StatickLevelsRepository")]
    public class StatickLevelsRepository : ScriptableObject
    {
        public bool presavedLevelsEnabled = true;
        public List<StatickLevelData> levels = new List<StatickLevelData>();
    }
}