using System;
using GameStats;
using Tutorial.Models;
using UnityEngine;

namespace GameScripts.PointPanel.Tutorial
{
    [Serializable]
    public class TutorialStep : TutorialStepBase
    {
        [Header("Tutorial params")]
        public ETutorialType stepType;
        public EGameStatType needsStat;
        public int statCount;
    }
}