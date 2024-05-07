using System;
using LargeNumbers;
using UnityEngine;

namespace GameScripts.MergeSquares.Models
{
    [Serializable]
    public class UnitModel
    {
        public Vector2Int position;
        public int value;
    }
}

namespace GameScripts.Game2248
{
    [Serializable]
    public class UnitModel
    {
        public Vector2Int position;
        public LargeNumber largeValue;
    }
}