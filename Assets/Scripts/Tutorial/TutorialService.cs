using System;
using System.Collections.Generic;
using Core.Anchors;
using Core.Conditions;
using Core.SaveLoad;
using Tutorial.AnalyticsSignals;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Zenject;

namespace Tutorial
{
    [Serializable]
    public class TutorialData
    {
        public string id;
        public bool passed;
    }

    [Serializable]
    public class TutorialServiceData
    {
        public List<TutorialData> tutorials = new();
    }

    public class TutorialService : MonoBehaviour
    {
        [SerializeField] private bool tutorialEnabled;
        [SerializeField] private bool verbose;
        [SerializeField] private TutorialView tutorialView;
        [SerializeField] private TutorialProviderBase tutorialProvider;
        [Space] [SerializeField] private Saver saver;

        public bool IsLocked { get; set; }
        public Action TutorialEnded = () => {};
        public bool HasActiveTutorial => _currentTutorial != null;
        public bool WaitingTutorials => _activeTutorials.Count > 0;
        
        private TutorialServiceData _data;

        private List<ATutorial> _activeTutorials = new();
        private List<ATutorial> _removeTutorials = new();
        private ATutorial _currentTutorial;

        private bool _canStart;
        
        private ConditionBuilder _conditionBuilder;
        private SaveService _saveService;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(ConditionBuilder conditionBuilder, SaveService saveService, SignalBus signalBus)
        {
            _conditionBuilder = conditionBuilder;
            _saveService = saveService;
            _signalBus = signalBus;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            _saveService.LoadFinished += OnLoadFinished;
        }

        public bool CheckTutorialFinished(Func<ATutorial, bool> checker)
        {
            var res = true;
            foreach (var activeTutorial in _activeTutorials)
            {
                if (checker(activeTutorial))
                {
                    // If Check after tutorial ended event
                    res = _removeTutorials.Contains(activeTutorial);
                    break;
                }
            }
            
            return res;
        }

        private void Start()
        {
            _canStart = true;
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
            _saveService.LoadFinished -= OnLoadFinished;
        }

        private void Update()
        {
            if (_activeTutorials.Count > 0 && _canStart)
            {
                foreach (var activeTutorial in _activeTutorials)
                {
                    activeTutorial.Update(Time.deltaTime);
                }
                foreach (var tutorial in _removeTutorials)
                {
                    _activeTutorials.Remove(tutorial);
                }
                _removeTutorials.Clear();
            }
        }

        private void Init(TutorialServiceData data, LoadContext context)
        {
            _data = data;
        }

        private void OnDeactivated(ATutorial tutorial)
        {
            if (_currentTutorial != null && _currentTutorial == tutorial)
            {
                var savesLocked = _currentTutorial.LockSaves();
                _currentTutorial = null;
                UnlockSaves(savesLocked);
            }
        }
        
        private void OnTutorialStateChanged(ATutorial tutorial) {
            if (_currentTutorial != null && _currentTutorial != tutorial)
            {
                return;
            }

            if ( _currentTutorial != null && _currentTutorial.IsCompleted() )
            {
                var savesLocked = _currentTutorial.LockSaves();
                _data.tutorials.Add(new TutorialData{id = _currentTutorial.Name(), passed = true});
                _removeTutorials.Add(_currentTutorial);
                
                _currentTutorial.StateChanged -= OnTutorialStateChanged;
                _currentTutorial = null; // Tutorial will be disposed with aspect removal. Completed tutorial could be repeated again by some conditions.
                UnlockSaves(savesLocked);
                TutorialEnded.Invoke();
            }
            if ( _currentTutorial == null && !IsLocked ) {
                foreach (var activeTutorial in _activeTutorials)
                {
                    if ( activeTutorial.IsReady() ) {
                        _currentTutorial = activeTutorial;
                        _signalBus.Fire(new TutorialStepSignal(0, _currentTutorial.Name()));
                        if (_currentTutorial.LockSaves())
                        {
                            _saveService.SaveLocked = true;
                            Log($"[TutorialService][OnTutorialStateChanged] Saves locked");
                        }
                        _currentTutorial.ChangeState(ETutorialState.Active);
                        break;
                    }
                }
            }
            else if ( _currentTutorial != null && _currentTutorial.IsReady() ) {
                _currentTutorial.ChangeState(ETutorialState.Active);
            }
        }

        private void UnlockSaves(bool savesLocked)
        {
            if (savesLocked)
            {
                _saveService.SaveLocked = false;
                Log($"[TutorialService][UnlockSaves] Saves unlocked");
                _saveService.ForceSave();
            }
            else
            {
                saver.SaveNeeded.Invoke(true);
            }
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new TutorialServiceData()), context);
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
        
        private void OnLoadFinished(LoadContext context)
        {
            _activeTutorials.Clear();
            
            if (!tutorialEnabled)
            {
                return;
            }

            foreach (var tutorial in tutorialProvider.Init(_data, context, this))
            {
                tutorial.Deactivated += OnDeactivated;
                tutorial.StateChanged += OnTutorialStateChanged;
                _activeTutorials.Add(tutorial);
                tutorial.Init(_conditionBuilder, tutorialView, _signalBus, verbose);
            }
        }
        
        private void Log(string log)
        {
            if (verbose)
            {
                Debug.Log(log);
            }
        }
    }
}