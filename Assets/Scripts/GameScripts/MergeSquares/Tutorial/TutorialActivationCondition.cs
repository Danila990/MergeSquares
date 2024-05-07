using System.Collections.Generic;
using System.Linq;
using Core.Conditions;
using GameStats;
using Tutorial;

namespace GameScripts.MergeSquares.Tutorial
{
    public class TutorialActivationCondition : ConditionBase
    {
        private IReadOnlyList<TutorialStartCondition> _conditions;
        private GridManager _gridManager;
        private GameStatService _gameStatService;
        private TutorialService _tutorialService;

        public override bool Updatable => true;

        public TutorialActivationCondition(IReadOnlyList<TutorialStartCondition> conditions, GridManager gridManager, GameStatService gameStatService, TutorialService tutorialService)
        {
            _conditions = conditions;
            _gridManager = gridManager;
            _gameStatService = gameStatService;
            _tutorialService = tutorialService;
        }

        private bool CheckStartConditions()
        {
            foreach (var condition in _conditions)
            {
                switch (condition.type)
                {
                    case ETutorialStartType.Level:
                        if (condition.num != _gridManager.CurrentLevel || _gridManager.IsLocked)
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
                    case ETutorialStartType.OtherTutorialNotFinished:
                        if (_tutorialService.CheckTutorialFinished(tutorial => ((SquaresTutorial)tutorial).TutorialDesc.type == condition.finishedTutorial))
                        {
                            return false;
                        }
                        break;
                    case ETutorialStartType.LevelGreater:
                        if (_gridManager.CurrentLevel <= condition.num || _gridManager.IsLocked)
                        {
                            return false;
                        }
                        break;
                    case ETutorialStartType.SquareCountOnGridGreater:
                        var count = _gridManager.CurrentGridView.Cells.Values.Count(c => !c.IsFree && !c.IsWall);
                        if (count <= condition.num || _gridManager.IsLocked)
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