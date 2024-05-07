using System;
using System.Collections.Generic;
using GameStats;
using Tutorial.Models;
using UnityEngine;

namespace GameScripts.Game2248.Tutorial
{
    [Serializable]
    public class TutorialStartCondition
    {
        public PointPanel.Tutorial.ETutorialStartType type;
        public int num;
        public EGameStatType gameStatType;
    }
    
    [Serializable]
    public class TutorialDesc : TutorialDescBase<TutorialStep>
    {
        [SerializeField] public List<TutorialStartCondition> startConditions;
        [SerializeField] public List<TutorialStep> steps = new();
        
        public override IReadOnlyList<TutorialStep> Steps => steps;
        public IReadOnlyList<TutorialStartCondition> StartConditions => startConditions;
    }
}