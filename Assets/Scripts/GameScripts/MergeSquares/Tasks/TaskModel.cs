using System;
using GameTasks;

namespace GameScripts.MergeSquares.Tasks
{
    [Serializable]
    public class TaskModel : TaskModel<ETaskDataType>
    {
        public int targetValue;
        
        public string DescKey => $"{type.ToString()}Desc";
    }
}