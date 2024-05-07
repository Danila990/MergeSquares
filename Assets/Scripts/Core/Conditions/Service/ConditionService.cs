using System;
using System.Collections.Generic;
using Core.SaveLoad;
using Core.Windows;
using UnityEngine;
using Zenject;

namespace Core.Conditions.Service
{
    public class ConditionAction
    {
        public ConditionActionSo desc;
        public ConditionBase condition;
        public bool waitReload;
    }
    
    public class ConditionService : MonoBehaviour
    {
        [SerializeField] private List<ConditionActionSo> actionDescs;
        
        public bool Ready { get; private set; }
        private List<ConditionAction> _actions = new List<ConditionAction>();
        private List<ConditionAction> _removeActions = new List<ConditionAction>();
        
        private ConditionBuilder _conditionBuilder;
        private SaveService _saveService;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            ConditionBuilder conditionBuilder,
            SaveService saveService,
            WindowManager windowManager
        )
        {
            _conditionBuilder = conditionBuilder;
            _saveService = saveService;
            _windowManager = windowManager;
            _saveService.LoadFinished += OnLoadFinished;
        }

        private void OnDestroy()
        {
            _saveService.LoadFinished -= OnLoadFinished;
            _actions.Clear();
        }

        private void OnLoadFinished(LoadContext context)
        {
            foreach (var action in actionDescs)
            {
                var condition = _conditionBuilder.CreateCondition(action.ExpressionDesc, OnConditionChanged);
                _actions.Add(new ConditionAction
                {
                    desc = action,
                    condition = condition
                });
            }

            Ready = true;
        }

        private void OnConditionChanged(bool value)
        {
            foreach (var action in _actions)
            {
                UpdateActionState(action);
            }

            foreach (var action in _removeActions)
            {
                _actions.Remove(action);
            }
            _removeActions.Clear();
        }

        private void UpdateActionState(ConditionAction action)
        {
            if (action.condition.IsTrue)
            {
                if(!action.waitReload)
                {
                    PerformAction(action.desc);
                    action.waitReload = true;
                    if (action.desc.OneTime)
                    {
                        _removeActions.Add(action);
                    }
                }
            }
            else
            {
                if (action.waitReload)
                {
                    action.waitReload = false;
                }
            }
        }

        private void PerformAction(ConditionActionSo actionDesc)
        {
            switch (actionDesc.Type)
            {
                case EConditionActionType.OpenPopup:
                    _windowManager.ShowWindow(actionDesc.WindowId.ToString());
                    break;
                case EConditionActionType.PlayMusic:
                    if(actionDesc.StopSound.IsPlaying())
                    {
                        actionDesc.StopSound.Pause();
                    }
                    actionDesc.StartSound.Play();
                    break;
            }
        }
    }
}