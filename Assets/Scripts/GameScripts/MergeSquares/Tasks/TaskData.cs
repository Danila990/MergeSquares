using System;
using GameTasks;

namespace GameScripts.MergeSquares.Tasks
{
    [Serializable]
    public enum ETaskDataType
    {
        WaveCount = 0,
        CritCount = 1,
        LevelCompleteCount = 2,
        SquaresMerged = 3,
        ClueSwapSpent = 4,
        ClueBombSpent = 5,
        AdShowed = 6,
        AllTaskComplete = 7,
    }
    
    [Serializable]
    public class TaskData : TaskData<TaskModel, ETaskDataType>
    {
        public int value;

        public override void Reset()
        {
            base.Reset();
            value = 0;
        }
        
        public override bool AddAndCheck(int amount)
        {
            value += amount;
            if (value >= model.targetValue)
            {
                value = model.targetValue;
                return true;    
            }
            return false;
        }
    }
}