using System;
using System.Collections.Generic;
using Core.Localization;
using GameTasks;
using UnityEngine;
using Zenject;

namespace GameScripts.Game2248.Tasks
{
    public class TaskPopup : TaskPopupBase
    {
        [SerializeField] private TaskView taskViewPrefab;
        [SerializeField] private Transform dailyRoot;
        [SerializeField] private Transform repeatRoot;
        [SerializeField] private LocalizeUi dailyTimer;
        
        private TaskService _taskService;

        [Inject]
        private void Construct(TaskService taskService)
        {
            _taskService = taskService;
            _taskService.Ticked += OnTicked;
        }

        private void OnDestroy()
        {
            ClearTabs();
            _taskService.Ticked -= OnTicked;
        }

        private void OnTicked()
        {
            var deltaTime = new DateTime(DateTime.Today.Add(TimeSpan.FromDays(1)).Ticks - DateTime.Now.Ticks);
            dailyTimer.UpdateArgs(new[] {$"{deltaTime.Hour:D2} : {deltaTime.Minute:D2} : {deltaTime.Second:D2}"});
        }

        protected override void UpdateTabs()
        {
            ClearTabs();
            UpdateTab(new List<TaskData>(_taskService.RepeatedTasks), repeatRoot);
            UpdateTab(new List<TaskData>(_taskService.DailyTasks), dailyRoot);
        }

        private void UpdateTab(List<TaskData> source, Transform root)
        {
            source.Sort((t1, t2) => 
                (t1.completed ? -1 : 0) + (t1.claimed ? 3 : 0) - ((t2.completed ? -1 : 0) + (t2.claimed ? 3 : 0)));
            foreach (var taskData in source)
            {
                var view = Instantiate(taskViewPrefab, root);
                view.Init(taskData);
                _taskService.Changed += view.OnChanged;
                _taskService.Completed += view.OnChanged;
                _taskService.Claimed += view.OnChanged;
                view.Claimed += OnClaimed;
            }
        }

        private void ClearTabs()
        {
            ClearTab(repeatRoot);
            ClearTab(dailyRoot);
        }
            
        private void ClearTab(Transform root)
        {
            foreach (var view in root.GetComponentsInChildren<TaskView>())
            {
                _taskService.Changed -= view.OnChanged;
                _taskService.Completed -= view.OnChanged;
                _taskService.Claimed -= view.OnChanged;
                view.Claimed -= OnClaimed;
                Destroy(view.gameObject);
            }
        }

        private void OnClaimed(TaskData data)
        {
            _taskService.Claim(data);
        }
    }
}