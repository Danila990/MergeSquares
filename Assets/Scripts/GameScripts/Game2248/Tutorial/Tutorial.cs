using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Anchors;
using Core.Conditions;
using Core.Windows;
using GameScripts.Game2248.Shop;
using GameStats;
using Tutorial;
using Tutorial.Models;
using Tutorial.View;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.Instructions;
using Object = UnityEngine.Object;

namespace GameScripts.Game2248.Tutorial
{
    public class Tutorial : TutorialBase<TutorialDesc, TutorialStep>
    {
        // private bool _cellClickSet;
        // private bool _cellFullSet;
        
        private Action _onStepFinish;
        private Action _onClick;
        private Anchor _anchor;
        private TutorialClue _clue;
        private Button _lockLayerButton;
        private List<TutorialClue> clues = new List<TutorialClue>();
        // private List<Anchor> _anchors = new();

        private readonly TutorialProvider _provider;
        private SummonSkinsPopup summonSkinsPopup;

        public Tutorial(TutorialProvider provider)
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
            if (_currentStepDesc.Base.waitClick)
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
            foreach (var clue in clues)
            {
                Object.Destroy(clue.GameObject());
            }

            clues.Clear();
            if (_provider.TutorialGrid != null)
            {
                _provider.TutorialGrid.Close();
            }
            _provider.GridManager.SetButtons();
        }

        private void OnDeactivated(ATutorial tutorial)
        {
            Hide();
        }
        
        private void Hide()
        {
            _anchor = null;
            
            _provider.SettingsButton.ResetSorting();
            _tutorialView.AddFader(false);
            _tutorialView.ResetAllClues();
        }

        protected override void SetupView()
        {
            _tutorialView.DefaultMode(_currentStepDesc);
        }
        
        protected override void BuildCondition(ETutorialConditionType type)
        {
            switch (type)
            {
                case ETutorialConditionType.Activation:
                    if (_currentStepIndex == 0)
                    {
                        _activationCondition = new TutorialActivationCondition(_desc.StartConditions,
                            _provider.GridManager, _provider.GameStatService);
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
            switch (step.stepType)
            {
                case ETutorialType.TutorialPanel:
                    TutorialPanel(step);
                    break;
                case ETutorialType.SquareLine:
                    _onStepFinish = trigger.OnEvent;
                    SquareLine(step);
                    res = trigger;
                    break;
                case ETutorialType.ButtonClick:
                    _onStepFinish = trigger.OnEvent;
                    ButtonClickStep(step);
                    res = trigger;
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

        private void SquareLine(TutorialStep step)
        {
            var model = new GridModel()
            {
                id = step.gridModel.id,
                size = step.gridModel.size,
                taskModel = step.gridModel.taskModel,
                units = new List<UnitModel>(step.gridModel.Units),
                startPows = new List<int>(step.gridModel.StartPows)
            };
                
            _provider.TutorialGrid.InitCells(model);
            _provider.TutorialGrid.OnLineEnd += OnLineEnd;

            var anchors = new List<Anchor>();
            foreach (var unit in model.Units)
            {
                var cell = _provider.TutorialGrid.filledCells.Find(c => c.gridPosition == unit.position);
                anchors.Add(cell.anchor);
            }
            _tutorialView.AddFingerPath(anchors, _currentStepDesc);
            _provider.TutorialGrid.SetActive();
        }

        private void TutorialPanel(TutorialStep step)
        {
            _provider.WindowManager.TryShowAndGetWindow(EPopupType.TutorialOffer.ToString(), out TutorialGrid tutorialGrid);
            _provider.TutorialGrid = tutorialGrid;
        }

        private void OnLineEnd()
        {
            _provider.TutorialGrid.OnLineEnd -= OnLineEnd;
            // _anchor.ResetSorting();
            _onStepFinish.Invoke();
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
            _anchor.SetSorting(_provider.SortingLayerName, _provider.SortingOrder);
            var button = _anchor.GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
            button.interactable = true;
            _tutorialView.AddFinger(_anchor, _currentStepDesc);
        }

        private void OnButtonClick()
        {
            _anchor.ResetSorting();
            var button = _anchor.GetComponent<Button>();
            button.onClick.RemoveListener(OnButtonClick);
            _onStepFinish.Invoke();
        }
    }
}