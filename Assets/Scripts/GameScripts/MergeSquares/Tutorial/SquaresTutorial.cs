using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Anchors;
using Core.Conditions;
using Core.Executor.Commands;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using Tutorial;
using Tutorial.Models;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.MergeSquares.Tutorial
{
    public class SquaresTutorial : TutorialBase<TutorialDesc, TutorialStep>
    {
        private bool _cellClickSet;
        private bool _cellFullSet;
        
        private Action _onStepFinish = () => {};
        private Action _onClick = () => {};
        private Anchor _anchor;
        private List<Anchor> _anchors = new();
        private Button _lockLayerButton;
        private SummonSkinsPopup summonSkinsPopup;

        private readonly TutorialProvider _provider;

        public SquaresTutorial(TutorialProvider provider)
        {
            _provider = provider;
            Completed += OnCompleted;
            Deactivated += OnDeactivated;
            Ready += OnReady;
        }
        
        private void OnReady(ATutorial obj)
        {
            _tutorialView.Clicked += OnClicked;
        }
        
        private void OnClicked()
        {
            if (_currentStepDesc.Base.waitClick && IsActive())
            {
                _onClick.Invoke();
            }
        }

        private void OnCompleted(ATutorial tutorial)
        {
            Hide();
            _tutorialView.Clicked -= OnClicked;
            Completed -= OnCompleted;
            Deactivated -= OnDeactivated;
            Ready -= OnReady;
        }

        private void OnDeactivated(ATutorial tutorial)
        {
            Hide();
        }

        protected override void SetupView()
        {
            _tutorialView.DefaultMode(_currentStepDesc);
        }

        private void Hide()
        {
            _provider.GridManager.SetButtonsInteractable(true);
            _anchor = null;
            
            _provider.SettingsButton.ResetSorting();
            
            if(_desc.type != ETutorialDescType.SoloChange)
            {
                foreach (var cell in _provider.GridManager.CurrentGridView.Cells.Values)
                {
                    cell.anchor.ResetSorting();
                }
            }
            _anchors.Clear();
            _tutorialView.AddFader(false);
            _tutorialView.ResetAllClues();
        }

        protected override void BuildCondition(ETutorialConditionType type)
        {
            switch (type)
            {
                case ETutorialConditionType.Activation:
                    if (_currentStepIndex == 0)
                    {
                        _activationCondition = new TutorialActivationCondition(_desc.StartConditions,
                            _provider.GridManager, _provider.GameStatService, _provider.TutorialService);
                    }
                    else
                    {
                        _activationCondition = new ConditionTrue();
                    }
                    break;
                case ETutorialConditionType.Complete:
                    _completionCondition = GetCompleteCondition(_currentStepDesc);
                    break;
                case ETutorialConditionType.Skip:
                    _skipCondition = new ConditionFalse();
                    break;
            }
        }
        
        public ConditionBase GetCompleteCondition(TutorialStep step)
        {
            ConditionBase res = new ConditionTrue();
            if (_currentStepDesc.Base.waitClick)
            {
                res = new ConditionEventTriggered();
                _onClick = ((ConditionEventTriggered)res).OnEvent;
            }
            var trigger = new ConditionEventTriggered();
            CheckStat(step);
            _provider.SettingsButton.SetSorting(_provider.SortingLayerName, _provider.SortingOrder);
            _tutorialView.AddCommand(new CmdCustom(BlockAllCells));
            switch (step.stepType)
            {
                case ETutorialType.SquareClick:
                    _onStepFinish = trigger.OnEvent;
                    SquareClickStep(step);
                    res = trigger;
                    break;
                case ETutorialType.ButtonClick:
                    _onStepFinish = trigger.OnEvent;
                    ButtonClickStep(step);
                    res = trigger;
                    break;
                case ETutorialType.ClearKeptAnchors:
                    ClearKeptAnchorsStep(step);
                    break;
                case ETutorialType.AddShowedAnchor:
                    AddShowedAnchorStep(step);
                    break;
                case ETutorialType.SetSummonChances:
                    _onStepFinish = trigger.OnEvent;
                    _provider.StartCoroutine(SetSummonChancesStep(step));
                    res = trigger;
                    break;
            }
            
            return res;
        }

        private IEnumerator SetSummonChancesStep(TutorialStep step)
        {
            yield return GetSummonPopup(10f);
            summonSkinsPopup.Summon(10, step.skinsChances);
            summonSkinsPopup.EndSummon += _onStepFinish.Invoke;
        }
        
        private IEnumerator GetSummonPopup(float time)
        {
            var wait = Time.time + time;
            SummonSkinsPopup summonSkinsPopup;
            while (Time.time <= wait)
            {
                if (_provider.WindowManager.TryGetWindow(EPopupType.SummonPopup.ToString(), out summonSkinsPopup))
                {
                    this.summonSkinsPopup = summonSkinsPopup;
                    yield break;
                }
                yield return null;
            }
            Debug.LogError($"[Tutorial][GetSummonPopup] Summon popup not found in {time} seconds");
        }

        private void AddShowedAnchorStep(TutorialStep step)
        {
            _provider.FingerController.Hide();
            _provider.GridManager.CurrentGridView.Cells[step.squareClickPos].anchor.SetSorting(_provider.SortingLayerName, _provider.SortingOrder);
            _anchors.Add(_provider.GridManager.CurrentGridView.Cells[step.squareClickPos].anchor);
        }

        private void ClearKeptAnchorsStep(TutorialStep step)
        {
            _provider.FingerController.Hide();
            _anchors.Clear();
        }

        private void CheckStat(TutorialStep step)
        {
            if (step.needsStat != EGameStatType.None && _provider.GameStatService.Get(step.needsStat) < step.statCount)
            {
                _provider.GameStatService.TrySet(step.needsStat, step.statCount);
            }
        }

        private void ButtonClickStep(TutorialStep step)
        {
            _provider.AnchorService.TryGetAnchor(step.Base.anchorType, out _anchor, step.Base.anchorId);
            var buttonGroup = _anchor.GetComponent<ButtonGroup>();
            // _provider.TutorialService.CurrentAnchor = _anchor;
            if(buttonGroup != null)
            {
                buttonGroup.Main.onClick.AddListener(OnButtonClick);
                buttonGroup.SetInteractable(true);
            }
            else
            {
                var button = _anchor.GetComponent<Button>();
                button.onClick.AddListener(OnButtonClick);
                button.interactable = true;
            }
        }

        private void OnButtonClick()
        {
            _anchor.ResetSorting();
            // _provider.TutorialService.CurrentAnchor = null;
            var buttonGroup = _anchor.GetComponent<ButtonGroup>();
            if(buttonGroup != null)
            {
                buttonGroup.Main.onClick.RemoveListener(OnButtonClick);
            }
            else
            {
                var button = _anchor.GetComponent<Button>();
                button.onClick.RemoveListener(OnButtonClick);
            }
            _onStepFinish.Invoke();
        }

        private void BlockAllCells()
        {
            var cells = _provider.GridManager.CurrentGridView.Cells;
            var keys = cells.Keys.ToList();
            foreach (var key in keys)
            {
                if (!_anchors.Contains(cells[key].anchor))
                {
                    cells[key].anchor.ResetSorting();
                }
            }
        }

        private void SquareClickStep(TutorialStep step)
        {
            var cells = _provider.GridManager.CurrentGridView.Cells;
            var keys = cells.Keys.ToList();
            _cellFullSet = false;
            _cellClickSet = false;

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == step.squareClickPos)
                {
                    cells[keys[i]].anchor.SetSorting(_provider.SortingLayerName, _provider.SortingOrder);
                    if (cells[keys[i]].IsFree)
                    {
                        cells[keys[i]].FullSet += OnTutorialClicked;
                        _cellFullSet = true;
                    }
                    else
                    {
                        cells[keys[i]].Clicked += OnTutorialClicked;
                        _cellClickSet = true;
                    }
                    
                    _anchor = cells[keys[i]].anchor;
                    if (step.keepAfterStep)
                    {
                        _anchors.Add(_anchor);
                    }
                }
                else if (!_anchors.Contains(cells[keys[i]].anchor))
                {
                    cells[keys[i]].anchor.ResetSorting();
                }
            }

            _provider.GridManager.CurrentGridView.SetNextValue(step.squareValue);
            _tutorialView.AddFinger(_anchor, _currentStepDesc);
        }

        private void OnTutorialClicked(Cell cell)
        {
            if (_cellClickSet)
            {
                cell.Clicked -= OnTutorialClicked;
                _cellClickSet = false;
            }
            if (_cellFullSet)
            {
                cell.FullSet -= OnTutorialClicked;
                _cellFullSet = false;
            }
            _onStepFinish.Invoke();
        }
    }
}