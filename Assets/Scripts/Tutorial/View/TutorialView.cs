using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Anchors;
using Core.Conditions;
using Core.Conditions.Commands;
using Core.Executor;
using Core.Executor.Commands;
using Core.Windows;
using DG.Tweening;
using DG.Tweening.Core;
using Tutorial.Models;
using UnityEngine;
using Utils;
using Zenject;

namespace Tutorial.View
{
    public class TutorialView : MonoBehaviour
    {
        [Tooltip ("[Required] Pointer to objects in tutorial")]
        [SerializeField] private FingerController fingerController;
        [Tooltip ("[Optional] Screen dimming")]
        [SerializeField] private Transform fader;
        [Tooltip ("[Optional] Layer from which OnClick method called (setup in editor button callback).")]
        [SerializeField] private GameObject tapHandler;
        [Tooltip ("[Optional] If you need time to play anims before player can input sth you can block all input for the time")]
        [SerializeField] private GameObject startLockLayer;
        [Tooltip ("[Optional] Regular lock layer that filters unnecessary input")]
        [SerializeField] private GameObject lockLayer;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;

        public Action Clicked = () => {};
        
        public bool IsVisible { get; set; }
        private Executor _executor = new("TutorialStep", false);

        private TutorialStepBase _currentStep;
        private TutorialStepBase _scheduledStep;
        private TutorialStepBase _previousStep;
        private bool _inProgress;
        private Anchor _anchor;
        private TutorialClue _clue;
        private Dictionary<string, TutorialClue> _clues = new();
        private bool _verbose;
        private float _timer;
        private Tweener move;

        private AnchorService _anchorService;
        private ConditionBuilder _conditionBuilder;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            AnchorService anchorService,
            ConditionBuilder conditionBuilder,
            WindowManager windowManager
        )
        {
            _anchorService = anchorService;
            _conditionBuilder = conditionBuilder;
            _windowManager = windowManager;
        }

        private void Awake()
        {
            ActivateTouchHandling(false);
        }

        private void Update()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
            }

            _executor?.Update(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_inProgress)
            {
                return;
            }

            if (_currentStep == null && IsVisible && _scheduledStep == null)
            {
                Hide();
            }
            else if (_scheduledStep != null)
            {
                _previousStep = null;
                _currentStep = _scheduledStep;
                _scheduledStep = null;
                _executor.Start();
            }
        }

        private void OnDestroy()
        {
            _executor.Dispose();
        }

        public void SetVerbose(bool value)
        {
            _verbose = value;
        }

        public void OnClick()
        {
            if (IsVisible && !_inProgress && _currentStep != null)
            {
                Clicked.Invoke();
            }
        }

        public bool TryShow(TutorialStepBase desc, bool force = false)
        {
            if (_scheduledStep != null)
            {
                if (force)
                {
                    _inProgress = false;
                    IsVisible = true;
                    _executor.Clear();
                    Hide();
                }
                else
                {
                    Log($"[TutorialView][TryShow] Another step has been already scheduled");
                    return false;
                }
            }

            _scheduledStep = desc;
            if (force)
            {
                _scheduledStep = null;
                _previousStep = null;
                _currentStep = desc;
                _executor.Start();
            }
            return true;
        }

        public void Reset()
        {
            _previousStep = _currentStep;
            _currentStep = null;
        }

        public TutorialView Prepare()
        {
            _executor.Dispose();
            _executor = new Executor("TutorialStep", false);

            if(_verbose)
            {
                _executor.AddCommand(new CmdLogMessage("[TutorialView][Show] Tutorial appearance started"));
            }
            _executor.AddCommand(new CmdCustom1<bool>(LockInput, true));
            _executor.AddCommand(new CmdCustom(FaderCleanUp));
            _executor.AddCommand(new CmdCustom(fingerController.Hide));
            _executor.AddCommand(new CmdCustom(() => IsVisible = false));
            _executor.AddCommand(new CmdCustom(() => _inProgress = true));
            
            return this;
        }
        
        public TutorialView AddFader(TutorialStepBase desc)
        {
            if (desc.Base.showFader)
            {
                AddFader(true);
            }
            return this;
        }
        
        public TutorialView CloseWindow(TutorialStepBase desc)
        {
            if (desc.Base.closeAllWindows)
            {
                _windowManager.CloseAll();
            }
            return this;
        }

        
        public TutorialView AddFader(bool active, bool instant = false)
        {
            if (instant)
            {
                fader.gameObject.SetActive(active);
            }
            else
            {
                _executor.AddCommand(new CmdCustom1<bool>(fader.gameObject.SetActive, active));
            }
            return this;
        }
        
        public TutorialView AddLock(TutorialStepBase desc)
        {
            if(desc.Base.lockLayer)
            {
                _executor.AddCommand(new CmdCustom1<bool>(lockLayer.gameObject.SetActive, true));
            }
            return this;
        }
        
        public TutorialView AddClick(TutorialStepBase desc)
        {
            if(desc.Base.waitClick)
            {
                _executor.AddCommand(new CmdCustom1<bool>(ActivateTouchHandling, true));
            }
            return this;
        }
        
        public TutorialView AddCheckActivationCondition(ConditionBase condition)
        {
            if (condition != null)
            {
                _executor.AddCommand(new CmdWaitCondition(condition));
            }
            return this;
        }
        
        public TutorialView AddDelay(float delayInSec)
        {
            _executor.AddCommand(new CmdCustom(() => _timer = delayInSec));
            _executor.AddCommand(new CmdWaitEvent(() => _timer <= 0f));
            return this;
        }
        
        public TutorialView AddClue(TutorialClue cluePrefab)
        {
            if(cluePrefab != null)
            {
                _executor.AddCommand(new CmdCustom(() =>
                {
                    if (_clues.TryGetValue(cluePrefab.name, out var clue))
                    {
                        Destroy(clue.gameObject);
                        _clues.Remove(cluePrefab.name);
                    }

                    var newClue = Instantiate(cluePrefab);
                    _clues.Add(cluePrefab.name, newClue);
                    _clue = newClue;
                }));
                _executor.AddCommand(new CmdWaitEvent(() => _clue.Completed));
            }
            return this;
        }
        
        public TutorialView HideFinger()
        {
            _executor.AddCommand(new CmdCustom(() => fingerController.Hide()));
            return this;
        }
        
        public TutorialView ResetClue(TutorialClue cluePrefab)
        {
            _executor.AddCommand(new CmdCustom(() =>
            {
                if (_clues.TryGetValue(cluePrefab.name, out var clue))
                {
                    Destroy(clue.gameObject);
                    _clues.Remove(cluePrefab.name);
                }
            }));
            return this;
        }
        
        public TutorialView ResetAllClues(bool instant = true)
        {
            var resetClues = new Action(() =>
            {
                foreach (var clue in _clues.Values)
                {
                    Destroy(clue.gameObject);
                }

                _clues.Clear();
            });
            if (instant)
            {
                resetClues.Invoke();
            }
            else
            {
                _executor.AddCommand(new CmdCustom(resetClues));
            }
            return this;
        }
        
        public TutorialView AddFinger(TutorialStepBase desc)
        {
            _executor.AddCommand(new CmdCustom1<TutorialStepBase>(UpdateAnchor, desc));
            return this;
        }

        public TutorialView AddFinger(Anchor anchor, TutorialStepBase desc)
        {
            _executor.AddCommand(new CmdCustom2<Anchor, TutorialStepBase>(UpdateAnchor, anchor, desc));
            return this;
        }

        public TutorialView AddFingerPath(List<Anchor> anchors, TutorialStepBase desc)
        {
            _executor.AddCommand(new CmdCustom2<List<Anchor>, TutorialStepBase>(UpdatePath, anchors, desc));
            return this;
        }

        public TutorialView FinishPreparing()
        {
            _executor.AddCommand(new CmdCustom1<bool>(LockInput, false));
            _executor.AddCommand(new CmdCustom(() => _inProgress = false));
            _executor.AddCommand(new CmdCustom(() => IsVisible = true));
            if(_verbose)
            {
                _executor.AddCommand(new CmdLogMessage($"[TutorialView][Show] Tutorial appearance completed"));
            }
            return this;
        }
        
        public TutorialView AddCommand(ICommand command)
        {
            _executor.AddCommand(command);
            return this;
        }

        public TutorialView DefaultMode(TutorialStepBase desc)
        {
            Prepare();
            AddFader(desc);
            CloseWindow(desc);
            if(desc.Base.activationCondition is {IsEmpty: false})
            {
                AddCheckActivationCondition(_conditionBuilder.CreateCondition(desc.Base.activationCondition));
            }
            AddClue(desc.Base.clue);
            AddFinger(desc);
            AddClick(desc);
            AddLock(desc);
            AddDelay(desc.Base.delayOnStartInSec);
            FinishPreparing();
            return this;
        }

        private void UpdatePath(List<Anchor> anchors, TutorialStepBase desc)
        {
            fingerController.Show();
            SetNewTarget(anchors, desc, 1);
        }

        private void SetNewTarget(List<Anchor> anchors, TutorialStepBase desc, int targetIndex)
        {
            move = DOTween.To(() => anchors[targetIndex - 1].TargetPosition, newPosition =>
            {
                fingerController.SetScreenPosition(newPosition + desc.Base.arrowOffset);
            }, anchors[targetIndex].TargetPosition, 1f);
        
            move.onComplete += () =>
            {
                var newIndex = 1;
                if (targetIndex < anchors.Count - 1)
                {
                    newIndex += targetIndex;
                }
                SetNewTarget(anchors, desc, newIndex);
            };
        }

        // private IEnumerator MovePath(Anchor[] anchors, TutorialStepBase desc)
        // {
        //     isMoving = true;
        //     foreach (var anchor in anchors)
        //     {
        //         anchor.SetSorting(sortingLayerName, sortingOrder);
        //     }
        //
        //     while (isMoving)
        //     {
        //         yield return null;
        //     }
        // }

        private void Hide()
        {
            move.Kill();
            if (IsVisible)
            {
                if(_verbose)
                {
                    _executor.AddCommand(new CmdLogMessage("[TutorialView][Hide] Tutorial hide started"));
                }
                _executor.AddCommand(new CmdCustom1<bool>(ActivateTouchHandling, false));
                _executor.AddCommand(new CmdCustom1<bool>(LockInput, false));
                _executor.AddCommand(new CmdCustom1<bool>(lockLayer.gameObject.SetActive, false));

                // Reset anchor sorting
                if (_anchor != null)
                {
                    _anchor.ResetSorting();
                    _anchor = null;
                }
                
                if (_previousStep != null && _previousStep.Base.clearClue)
                {
                    if(_previousStep.Base.clue != null)
                    {
                        if (_clues.TryGetValue(_previousStep.Base.clue.name, out var clue))
                        {
                            Destroy(clue.gameObject);
                        }

                        _clues.Remove(_previousStep.Base.clue.name);
                    }

                    if (_clues.Count == 1)
                    {
                        Destroy(_clues.First().Value.gameObject);
                        _clues.Clear();
                    }
                }
                
                _executor.AddCommand(new CmdCustom(() => _inProgress = true));
                
                // Some async commands could be here

                _executor.AddCommand(new CmdCustom(() => _inProgress = false));
                if(_previousStep != null && _previousStep.Base.clearFader)
                {
                    _executor.AddCommand(new CmdCustom(FaderCleanUp));
                }
                _executor.AddCommand(new CmdCustom(fingerController.StopSequence));
                _executor.AddCommand(new CmdCustom(fingerController.Hide));
                _executor.AddCommand(new CmdCustom(() => { IsVisible = false; }));
                if(_verbose)
                {
                    _executor.AddCommand(new CmdLogMessage("[TutorialView][Hide] Tutorial hide completed"));
                }
                if (!_executor.StartIfNull)
                {
                    _executor.Start();
                }
            }
        }

        private void FaderCleanUp()
        {
            fader.gameObject.SetActive(false);
        }

        private void UpdateAnchor(TutorialStepBase desc)
        {
            if (desc.Base.anchorType != EAnchorType.None)
            {
                if (!_anchorService.TryGetAnchor(desc.Base.anchorType, out _anchor, desc.Base.anchorId))
                {
                    Log(
                        $"[TutorialView][UpdateAnchor] Anchor: {desc.Base.anchorType} with id: {desc.Base.anchorId} not found");
                    return;
                }

                UpdateAnchor(_anchor, desc);
            }
        }
        
        private void UpdateAnchor(Anchor anchor, TutorialStepBase desc)
        {
            move.Kill();
            anchor.SetSorting(sortingLayerName, sortingOrder);
            fingerController.Show();
            UpdateFinger(anchor.TargetPosition, desc);
        }
        
        private void UpdateFinger(Vector3 fingerPosition, TutorialStepBase desc)
        {
            fingerController.Show();
            fingerController.SetScreenPosition(fingerPosition + desc.Base.arrowOffset);
            switch (desc.Base.arrowDir)
            {
                case EDirection.Up:
                    fingerController.Upside();
                    break;
                case EDirection.Left:
                    fingerController.LeftHand();
                    break;
                case EDirection.Down:
                    fingerController.Downside();
                    break;
                default:
                    fingerController.RightHand();
                    break;
            }

            fingerController.StartLoopTapAnimation();
        }

        private void LockInput(bool value)
        {
            startLockLayer.gameObject.SetActive(value);
        }

        private void ActivateTouchHandling(bool visible)
        {
            tapHandler.SetActive(visible);
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