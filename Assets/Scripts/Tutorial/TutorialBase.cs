using System;
using Core.Conditions;
using Core.StateMachine;
using JetBrains.Annotations;
using Tutorial.AnalyticsSignals;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Zenject;

namespace Tutorial
{
    public abstract class ATutorial
    {
        protected bool _isReady;
        protected bool _isCompleted;
        protected bool _isActive;
        
        public bool IsReady() => _isReady;
        public bool IsCompleted() => _isCompleted;
        public bool IsActive() => _isActive;
        
        public virtual string Name() => "Name unset";
        public virtual bool LockSaves() => false;

        public Action<ATutorial> StateChanged = tutorial => { };
        public Action<ATutorial> Completed = tutorial => { };
        public Action<ATutorial> Deactivated = tutorial => { };
        public Action<ATutorial> Ready = tutorial => { };

        public abstract void Init(ConditionBuilder conditionBuilder, TutorialView tutorialView, SignalBus signalBus, bool verbose);
        public abstract void Update(float deltaTime);
        public abstract void ChangeState(ETutorialState state);
        
    }
    public class TutorialBase<TDesc, TStep> : ATutorial
        where TDesc : TutorialDescBase<TStep>
        where TStep : TutorialStepBase
    {
        protected TDesc _desc;
        protected TStep _currentStepDesc;
        protected ConditionBase _skipCondition;
        protected ConditionBase _activationCondition;
        protected ConditionBase _completionCondition;
        protected int _currentStepIndex = 0;
        private IState<ETutorialState> _rootState;
        private bool _timerCompleted;
        private float _elapsedTime;
        
        public override string Name() => _desc.name;
        public override bool LockSaves() => _desc.lockSaves;
        public TutorialView TutorialView => _tutorialView;
        public TDesc TutorialDesc => _desc;

        private bool _wasShown;
        private bool _verbose;

        protected ConditionBuilder _conditionBuilder;
        protected TutorialView _tutorialView;
        private SignalBus _signalBus;

        public void SetDesc(TDesc desc)
        {
            _desc = desc;
        }
        
        public override void Init(ConditionBuilder conditionBuilder, TutorialView tutorialView, SignalBus signalBus, bool verbose)
        {
            _conditionBuilder = conditionBuilder;
            _tutorialView = tutorialView;
            _signalBus = signalBus;
            InitStateMachine();
            _verbose = verbose;
            _rootState.Verbose = _verbose;
            _tutorialView.SetVerbose(_verbose);
            _rootState.Prefix = _desc.name;
            ChangeState(ETutorialState.None);
            Ready.Invoke(this);
        }
        
        public override void Update(float deltaTime)
        {
            _rootState.Update(deltaTime);
        }

        public override void ChangeState(ETutorialState state)
        {
            _rootState.ChangeState(state);
            StateChanged.Invoke(this);
        }
        
        private void InitStateMachine()
        {
            _rootState = new StateMachineBuilder<ETutorialState>()
                .State(ETutorialState.None)
                .Enter(state =>
                {
                    _currentStepDesc = null;
                    DisposeConditions();
                    DisposeTimer();
                    if (_desc.forceStart)
                    {
                        _rootState.Update(0f);
                    }
                })
                .Condition(() => _currentStepIndex >= _desc.Steps.Count && !_tutorialView.IsVisible, state => ChangeState(ETutorialState.Completed))
                .Condition(() => _currentStepIndex < _desc.Steps.Count && !_tutorialView.IsVisible, state =>
                {
                    _currentStepDesc = _desc.Steps[_currentStepIndex];
                    BuildCondition(ETutorialConditionType.Skip);
                    BuildCondition(ETutorialConditionType.Activation);
                    ChangeState(ETutorialState.Wait);
                    if (_desc.forceStart)
                    {
                        _rootState.Update(0f);
                    }
                })
                .End()
                .State(ETutorialState.Wait)
                .Enter(state =>
                {
                    DisposeTimer();
                    if (_desc.forceStart)
                    {
                        _rootState.Update(0f);
                    }
                })
                .Condition(() => CheckCondition(_skipCondition), state => PromoteStep())
                .Condition(() => CheckCondition(_activationCondition), state => ChangeState(ETutorialState.Ready))
                .End()
                .State(ETutorialState.Ready)
                .Enter(state => {_isReady = true; })
                .Exit(state => { _isReady = false; })
                .Condition(() => CheckCondition(_skipCondition), state => PromoteStep())
                .Condition(() => CheckCondition(_activationCondition, true), state => ChangeState(ETutorialState.Wait))
                .End()
                .State(ETutorialState.Active)
                .Enter(state =>
                {
                    if (_currentStepDesc != null)
                    {
                        _isActive = true;
                        SetupView();
                        BuildCondition(ETutorialConditionType.Complete);
                        ShowStep(_currentStepDesc);
                    }
                })
                .Exit(state =>
                {
                    _isActive = false;
                    if (_completionCondition != null)
                    {
                        _completionCondition.Dispose();
                        _completionCondition = null;
                    }
                })
                .Condition(() => CheckCondition(_completionCondition) && _tutorialView.IsVisible, state =>
                {
                    HideStep();
                    PromoteStep();
                })
                .Condition(() => CheckCondition(_activationCondition, true), state =>
                {
                    Deactivated.Invoke(this);
                    HideStep();
                    ChangeState(ETutorialState.Wait);
                })
                .End()
                .State(ETutorialState.Cooldown)
                .Enter(state => { DisposeTimer(); })
                .Update((state, deltaTime) =>
                {
                    _elapsedTime += deltaTime;
                    _timerCompleted = _elapsedTime >= _currentStepDesc?.Base.delayOnStartInSec;
                })
                .Condition(() => CheckCondition(_skipCondition), state => ChangeState(ETutorialState.Completed))
                .Condition
                (
                    () => !CheckCondition(_skipCondition) && CheckCondition(_activationCondition),
                    state =>
                    {
                        DisposeTimer();
                        ChangeState(ETutorialState.Wait);
                    }
                )
                .Condition(
                    () => !CheckCondition(_skipCondition) && !CheckCondition(_activationCondition) && _timerCompleted,
                    state => ChangeState(ETutorialState.Ready))
                .End()
                .State(ETutorialState.Completed)
                .Enter(state =>
                {
                    Log($"[Tutorial][State] {_desc.name} Completed!");
                    _isCompleted = true;
                    DisposeConditions();
                    Completed.Invoke(this);
                })
                .Exit(state => { _isCompleted = false; })
                .End()
                .Build();
        }

        protected virtual void BuildCondition(ETutorialConditionType type)
        {
            switch (type)
            {
                case ETutorialConditionType.Activation:
                    if (_currentStepDesc.Base.activationCondition != null && !_currentStepDesc.Base.activationCondition.IsEmpty && _activationCondition == null)
                    {
                        _activationCondition = _conditionBuilder.CreateCondition(_currentStepDesc.Base.activationCondition);
                    }
                    break;
                case ETutorialConditionType.Complete:
                    if (_currentStepDesc.Base.completionCondition != null && !_currentStepDesc.Base.completionCondition.IsEmpty)
                    {
                        _completionCondition = _conditionBuilder.CreateCondition(_currentStepDesc.Base.completionCondition);
                    }
                    break;
                case ETutorialConditionType.Skip:
                    if (_currentStepDesc.Base.skipCondition != null && !_currentStepDesc.Base.skipCondition.IsEmpty && _skipCondition == null)
                    {
                        _skipCondition = _conditionBuilder.CreateCondition(_currentStepDesc.Base.skipCondition);
                    }
                    break;
            }
        }

        protected virtual void SetupView()
        {
            _tutorialView.DefaultMode(_currentStepDesc);
        }

        private bool CheckCondition(ConditionBase condition, bool inverse = false) =>
            condition != null && (inverse ? !condition.IsTrue : condition.IsTrue);

        private void PromoteStep()
        {
            _currentStepIndex += 1;
            _signalBus.Fire(new TutorialStepSignal(_currentStepIndex, _desc.name));
            Log($"[Tutorial][PromoteStep] {_desc.name} New index: {_currentStepIndex}");
            ChangeState(ETutorialState.None);
        }

        private void HideStep()
        {
            if (_wasShown)
            {
                _wasShown = false;
                _tutorialView.Reset();
            }
        }

        private void ShowStep(TStep desc)
        {
            if (!_tutorialView.TryShow(desc, _currentStepIndex == 0 && _desc.forceStart))
            {
                ChangeState(ETutorialState.Wait);
            }
            else
            {
                _wasShown = true;
            }
        }

        private void DisposeTimer()
        {
            _elapsedTime = 0;
            _timerCompleted = false;
        }

        [UsedImplicitly]
        private void DisposeConditions()
        {
            if (_skipCondition != null)
            {
                _skipCondition.Dispose();
                _skipCondition = null;
            }

            if (_completionCondition != null)
            {
                _completionCondition.Dispose();
                _completionCondition = null;
            }

            if (_activationCondition != null)
            {
                _activationCondition.Dispose();
                _activationCondition = null;
            }
        }
        
        private void Log(string log)
        {
            if (_verbose)
            {
                Debug.Log(log);
            }
        }
    }
}