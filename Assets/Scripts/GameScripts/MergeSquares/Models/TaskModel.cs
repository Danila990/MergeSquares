using System;
using System.Runtime.Serialization;
using LargeNumbers;

    public enum ETaskType
    {
        CollectPoints,
        GetCellWithValue,
        MakeMerges,
        Endless,
        MakeLines
    }
    
    [Serializable]
    public class TaskModelBase
    {
        public ETaskType type;
    }

namespace GameScripts.MergeSquares.Models
{
    [Serializable]
    public class TaskModel : TaskModelBase
    {
        public int value;
    }
}

namespace GameScripts.Game2248
{
    [Serializable]
    public class TaskModel : TaskModelBase
    {
        public LargeNumber valueLarge;
    }
}

