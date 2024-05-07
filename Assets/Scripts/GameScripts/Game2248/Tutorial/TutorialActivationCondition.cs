using System.Collections.Generic;
using Core.Conditions;
using GameStats;
using UnityEngine;

namespace GameScripts.Game2248.Tutorial
{
    public class TutorialActivationCondition : ConditionBase
    {
        private IReadOnlyList<TutorialStartCondition> _conditions;
        private GridManager _gridManager;
        private GameStatService _gameStatService;

        public override bool Updatable => true;

        public TutorialActivationCondition(IReadOnlyList<TutorialStartCondition> conditions, GridManager gridManager, GameStatService gameStatService)
        {
            _conditions = conditions;
            _gridManager = gridManager;
            _gameStatService = gameStatService;
        }

        private bool CheckStartConditions()
        {
            foreach (var condition in _conditions)
            {
                switch (condition.type)
                {
                    case PointPanel.Tutorial.ETutorialStartType.Level:
                        if (condition.num != _gridManager.CurrentLevel || _gridManager.IsLocked)
                        {
                            return false;
                        }
                        break;
                    case PointPanel.Tutorial.ETutorialStartType.StatsCount:
                        if (condition.num > _gameStatService.GetStatValue(condition.gameStatType))
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