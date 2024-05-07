using System;
using GameTasks;
using LargeNumbers;

namespace GameScripts.Game2248.Tasks
{
    [Serializable]
    public enum ETaskDataType
    {
        LinesCount = 0,
        LevelCompleteCount = 1,
        SquaresDestroyed = 2,
        ClueSwapSpent = 3,
        ClueBombSpent = 4,
        BonusLevelCompleteCount = 5,
        AdShowed = 6,
        AllTaskComplete = 7,
        TotalAmountReached = 8,
        BonusesUsed = 9,
    }
    
    [Serializable]
    public class TaskData : TaskData<TaskModel, ETaskDataType>
    {
        public int value;
        public LargeNumber largeValue;

        public override void Reset()
        {
            base.Reset();
            value = 0;
            largeValue = LargeNumber.zero;
        }
        
        public override bool AddAndCheck(int amount)
        {
            return model.isLargeValue ? AddAndCheckLarge(amount) : AddAndCheckCommon(amount);
        }
        
        public bool AddAndCheckLarge(int amount)
        {
            largeValue += amount;
            if (largeValue >= model.targetLargeValue)
            {
                largeValue = model.targetLargeValue;
                return true;
            }
            return false;
        }
        
        public bool AddAndCheckCommon(int amount)
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