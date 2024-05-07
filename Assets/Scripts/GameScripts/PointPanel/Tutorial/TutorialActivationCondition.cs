using System.Collections.Generic;
using Core.Conditions;
using Core.Windows;
using GameStats;

namespace GameScripts.PointPanel.Tutorial
{
    public class TutorialActivationCondition : ConditionBase
    {
        private IReadOnlyList<TutorialStartCondition> _conditions;
        private PointPanel _pointPanel;
        private GameStatService _gameStatService;
        private WindowManager _windowManager;

        public override bool Updatable => true;

        public TutorialActivationCondition(IReadOnlyList<TutorialStartCondition> conditions, PointPanel pointPanel, GameStatService gameStatService, WindowManager windowManager)
        {
            _conditions = conditions;
            _pointPanel = pointPanel;
            _gameStatService = gameStatService;
            _windowManager = windowManager;
        }

        private bool CheckStartConditions()
        {
            foreach (var condition in _conditions)
            {
                switch (condition.type)
                {
                    case ETutorialStartType.Level:
                        if (condition.num != _pointPanel.ShowedLevel || _pointPanel.InputBlocked)
                        {
                            return false;
                        }
                        break;
                    case ETutorialStartType.StatsCount:
                        if (condition.num > _gameStatService.GetStatValue(condition.gameStatType))
                        {
                            return false;
                        }
                        break;
                    case ETutorialStartType.AllWindowsClosed:
                        if (_windowManager.IsAnyWindowOpened())
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
        
        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if(CheckStartConditions())
            {
                MarkChanged();
            }
        }

        public override bool IsTrue => CheckStartConditions();
    }
}