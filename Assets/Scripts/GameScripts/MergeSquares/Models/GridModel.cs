using System;
using System.Collections.Generic;
using System.Linq;
using GameScripts.MergeSquares.Models;
using LargeNumbers;
using UnityEngine;


[Serializable]
public class GridModelBase
{
    public int id;
    public Vector2Int size;
    public int reward;
}

namespace GameScripts.MergeSquares.Models
{
    [Serializable]
    public class GridModel : GridModelBase
    {
        public List<NextValue> nextValues = new();
        public TaskModel taskModel;
        
        public List<UnitModel> units = new();
        public Dictionary<Vector2Int, int> UnitPositions => units.ToDictionary(t => t.position, t => t.value);
    }
}

namespace GameScripts.Game2248
{
    [Serializable]
    public class PowUpdateCondition
    {
        public int updatePow;
        public int deletePow = -1;
        public int addPow = -1;
    }
    
    [Serializable]
    public class GridModel : GridModelBase
    {
        public TaskModel taskModel;
        public List<int> startPows = new List<int>();
        public int nextPowUpdate = -1;

        public List<UnitModel> units = new();
        public Dictionary<Vector2Int, LargeNumber> UnitPositions => units.ToDictionary(t => t.position, t => t.largeValue);

        public List<PowUpdateCondition> powUpdateConditions = new List<PowUpdateCondition>();

        public IReadOnlyList<int> StartPows => startPows;
        public IReadOnlyList<UnitModel> Units => units;
        public IReadOnlyList<PowUpdateCondition> PowUpdateConditions => powUpdateConditions;
    }
}