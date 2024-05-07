using System;
using System.Collections.Generic;
using GameStats;
using Tutorial.Models;
using UnityEngine;
using Utils.Attributes;

namespace GameScripts.MergeSquares.Tutorial
{
    [Serializable]
    public enum ETutorialDescType
    {
        Merge = 0,
        Joker = 1,
        WallChangeShop = 2,
        SoloChange = 3
    }
    
    [Serializable]
    public class TutorialStartCondition
    {
        public ETutorialStartType type;
        [EnumConditionalHide(nameof(type), ETutorialStartType.OtherTutorialNotFinished, true)] public ETutorialDescType finishedTutorial;
        public int num;
        [EnumConditionalHide(nameof(type), ETutorialStartType.StatsCount, true)] public EGameStatType gameStatType;
    }
    
    [Serializable]
    public class TutorialDesc : TutorialDescBase<TutorialStep>
    {
        [SerializeField] public ETutorialDescType type;
        [SerializeField] public List<TutorialStartCondition> startConditions;
        [SerializeField] public List<TutorialStep> steps = new();
        
        public override IReadOnlyList<TutorialStep> Steps => steps;
        public IReadOnlyList<TutorialStartCondition> StartConditions => startConditions;
    }
}