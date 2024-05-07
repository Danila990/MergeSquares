using GameTasks;

namespace GameScripts.Game2248.Tasks
{
    public class TaskService : TaskService<TaskData, TaskModel, ETaskDataType>
    {
        protected override void TaskCompleted(TaskData task)
        {
            if(task.isDaily && task.type != ETaskDataType.AllTaskComplete)
            {
                AddStat(ETaskDataType.AllTaskComplete, 1);
            }
        }
        
        protected override void DailyReset(TaskData task)
        {
            if(task.isDaily && task.type == ETaskDataType.AllTaskComplete)
            {
                task.Reset();
            }
        }
    }
}