using System;
using System.Collections.Generic;
using GameScripts.Game2248.Shop;
using GameStats;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Utils.Attributes;

namespace GameScripts.Game2248.Tutorial
{
    [Serializable]
    public class TutorialStep : TutorialStepBase
    {
        [Header("Tutorial params")]
        public ETutorialType stepType;
        public GridModel gridModel;
        public EGameStatType needsStat;
        public int statCount;
        public float waitTime;
        public bool keepAfterStep = false;
        public List<ESquareSkin> skinsChances;
    }
}