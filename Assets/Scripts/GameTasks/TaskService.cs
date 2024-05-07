using System;
using System.Collections.Generic;
using System.Globalization;
using Core.SaveLoad;
using Notify;
using Rewards;
using Rewards.Models;
using UnityEngine;
using Utils;
using Zenject;

namespace GameTasks
{
    [Serializable]
    public class TaskModel<T> where T : Enum
    {
        public T type;
        // Note: Use id if you want to place to task with same type in one list
        public string id;
        public List<RewardModel> rewards = new();
    }
    
    [Serializable]
    public class TaskData<TModel, T> where TModel : TaskModel<T> where T : Enum
    {
        public T type;
        public string id;
        public bool isDaily;
        public bool claimed;
        public bool completed;
        public TModel model;
        public virtual bool AddAndCheck(int amount) => true;

        public virtual void Reset()
        {
            completed = false;
        }
    }
    
    [Serializable]
    public class TaskServiceData<TTask, TModel, TType> where TTask : TaskData<TModel, TType>, new() where TModel : TaskModel<TType>  where TType : Enum
    {
        public List<TTask> repeatedTasks = new();
        public List<TTask> dailyTasks = new();
        public long currentData = DateTime.Today.Ticks;

        public void ResetDaily()
        {
            foreach (var task in dailyTasks)
            {
                if(task.claimed)
                {
                    task.Reset();
                    task.claimed = false;
                }
            }
        }

        public void InitDaily(List<TModel> models)
        {
            Init(dailyTasks, models, true);
        }
        
        public void InitRepeated(List<TModel> models)
        {
            Init(repeatedTasks, models);
        }
        
        public void LoadDaily(List<TModel> models)
        {
            Load(dailyTasks, models);
        }
        
        public void LoadRepeated(List<TModel> models)
        {
            Load(repeatedTasks, models);
        }

        private void Init(List<TTask> dest, List<TModel> models, bool isDaily = false)
        {
            dest.Clear();
            foreach (var taskModel in models)
            {
                AddTask(dest, taskModel, isDaily);
            }
        }
        
        private void Load(List<TTask> dest, List<TModel> models, bool isDaily = false)
        {
            dest.RemoveAll(d => models.Find(m => m.type.Equals(d.type)) == null);
            foreach (var taskModel in models)
            {
                var task = dest.Find(d => d.type.Equals(taskModel.type));
                if (task != null)
                {
                    task.model = taskModel;
                }
                else
                {
                    AddTask(dest, taskModel, isDaily);
                }
            }
        }

        private void AddTask(List<TTask> dest, TModel model, bool isDaily = false)
        {
            dest.Add(new TTask
            {
                type = model.type,
                id = model.id,
                model = model,
                isDaily = isDaily
            });
        }
    }
    
    public class TaskService<TTask, TModel, TType> : MonoBehaviour where TTask : TaskData<TModel, TType>, new() where TModel : TaskModel<TType> where TType : Enum
    {
        [SerializeField] private List<TModel> repeatedTasks = new();
        [SerializeField] private List<TModel> dailyTasks = new();
        [Space]
        [SerializeField] private Saver saver;
        
        public const string DailyNotify = "DailyTask";
        public const string RepeatNotify = "RepeatTask";

        public IReadOnlyList<TTask> RepeatedTasks => Data.repeatedTasks;
        public IReadOnlyList<TTask> DailyTasks => Data.dailyTasks;

        public Action<TTask> Changed = task => {};
        public Action<TTask> Completed = task => {};
        public Action<TTask> Claimed = task => {};
        public Action Ready = () => {};
        public Action Ticked = () => {};

        protected TaskServiceData<TTask, TModel, TType> Data = new();
        private float _timer = 0f;
        private DateTime _currentData;

        private RewardService _rewardService;
        private NotifyService _notifyService;
        
        [Inject]
        public void Construct(RewardService rewardService, NotifyService notifyService)
        {
            _rewardService = rewardService;
            _notifyService = notifyService;
            
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if(_timer >= 1.0f)
            {
                _timer = 0;
                Ticked.Invoke();
                if(_currentData != DateTime.Today)
                {
                    _currentData = DateTime.Today;
                    ResetDaily();
                }
            }
        }

        public void ResetDaily()
        {
            Data.currentData = DateTime.Today.Ticks;
            Data.ResetDaily();
            foreach (var dailyTask in Data.dailyTasks)
            {
                Changed.Invoke(dailyTask);
                DailyReset(dailyTask);
            }
            saver.SaveNeeded.Invoke(true);
        }
        
        protected virtual void DailyReset(TTask task){}

        public void Claim(TTask task){
            var model = task.model;
            _rewardService.AwardAll(model.rewards);
            task.claimed = task.isDaily;
            if (!task.isDaily)
            {
                task.Reset();
            }
            CheckNotify();
            Claimed.Invoke(task);
            saver.SaveNeeded.Invoke(true);
        }

        public void AddStat(TType type, int amount){
            AddStat(Data.dailyTasks, type, amount);
            AddStat(Data.repeatedTasks, type, amount);
        }
        
        private void AddStat(List<TTask> dest, TType type, int amount)
        {
            var task = dest.GetBy(t => (t.type).Equals(type));
            if (task != null)
            {
                if (!task.completed && task.AddAndCheck(amount))
                {
                    task.completed = true;
                    TaskCompleted(task);
                    Completed.Invoke(task);
                    CheckNotify();
                }
                else
                {
                    Changed.Invoke(task);
                }
                saver.SaveNeeded.Invoke(true);
            }
        }

        protected virtual void TaskCompleted(TTask task){}

        private void CheckNotify()
        {
            _notifyService.SetNotify(new NotifyRef{id = DailyNotify}, NeedClaimAny(Data.dailyTasks));
            _notifyService.SetNotify(new NotifyRef{id = RepeatNotify}, NeedClaimAny(Data.repeatedTasks));
        }

        private bool NeedClaimAny(List<TTask> dest)
        {
            foreach (var task in dest)
            {
                if (task.completed && !task.claimed)
                {
                    return true;
                }
            }

            return false;
        }

        // SaveLoad

        protected virtual TaskServiceData<TTask, TModel, TType> GetDefaultData()
        {
            var data = new TaskServiceData<TTask, TModel, TType>();

            data.InitDaily(dailyTasks);
            data.InitRepeated(repeatedTasks);
            
            return data;
        }

        private void Init(TaskServiceData<TTask, TModel, TType> data, LoadContext context)
        {
            Data = data;
            Data.LoadDaily(dailyTasks);
            Data.LoadRepeated(repeatedTasks);
            _currentData = new DateTime(Data.currentData);
            Ready.Invoke();
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, GetDefaultData()), context);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(Data);
        }
    }
}