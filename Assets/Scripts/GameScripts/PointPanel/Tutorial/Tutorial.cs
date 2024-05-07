using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Anchors;
using Core.Conditions;
using Core.Executor.Commands;
using GameScripts.MergeSquares;
using GameStats;
using Tutorial;
using Tutorial.Models;
using Tutorial.View;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace GameScripts.PointPanel.Tutorial
{
    public class Tutorial : TutorialBase<TutorialDesc, TutorialStep>
    {
        private bool _cellClickSet;
        private bool _cellFullSet;
        
        private Action _onStepFinish;
        private Action _onClick;
        private Anchor _anchor;
        private TutorialClue _clue;
        private Button _lockLayerButton;
        private List<TutorialClue> clues = new List<TutorialClue>();
        
        private readonly TutorialProvider _provider;

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
                            _provider.PointPanel, _provider.GameStatService, _provider.WindowManager);
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
                case ETutorialType.PanelClick:
                    _onStepFinish = trigger.OnEvent;
                    PointClickStep(step);
                    res = trigger;
                    break;
                case ETutorialType.ButtonClick:
                    _onStepFinish = trigger.OnEvent;
                    ButtonClickStep(step);
                    res = trigger;
                    break;
            }
            
            return res;
        }

        private void PointClickStep(TutorialStep step)
        {
            var cells = _provider.PointPanel.BottomPanel.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == step.statCount)
                {
                    var cell = cells[i];
                    _tutorialView.AddCommand(new CmdCustom(() =>
                    {
                        cell.SetClickable(true);
                        cell.SetOverrideSorting(true);
                        // cells[i].tutorialClicked += OnTutorialClicked;
                        _anchor = cell.Anchor;
                        var button = cell.GetComponent<Button>();
                        button.onClick.AddListener(OnPointClicked);
                        button.interactable = true;
                        _tutorialView.AddFinger(_anchor, _currentStepDesc);
                    }));
                }
                cells[i].SetClickable(false);
                cells[i].SetOverrideSorting(false);
            }
        }
        
        private void OnPointClicked()
        {
            var cells = _provider.PointPanel.BottomPanel.Cells;
            if (_anchor != null)
            {
                _anchor.ResetSorting();
                var button = _anchor.GetComponent<Button>();
                button.onClick.RemoveListener(OnButtonClick);
            }
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