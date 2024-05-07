using System;
using GameTasks;
using LargeNumbers;

namespace GameScripts.Game2248.Tasks
{
    [Serializable]
    public class TaskModel : TaskModel<ETaskDataType>
    {
        public bool isLargeValue;
        public int targetValue;
        public LargeNumber targetLargeValue;
        
        public string DescKey => $"{type.ToString()}Desc";
    }
}