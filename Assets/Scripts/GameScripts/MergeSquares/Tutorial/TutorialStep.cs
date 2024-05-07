using System;
using System.Collections.Generic;
using GameScripts.MergeSquares.Shop;
using GameStats;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Utils.Attributes;

namespace GameScripts.MergeSquares.Tutorial
{
    [Serializable]
    public class TutorialStep : TutorialStepBase
    {
        [Header("Squares params")]
        public ETutorialType stepType;
        [EnumConditionalHide(nameof(stepType), ETutorialType.SquareClick, true)] public Vector2Int squareClickPos;
        [EnumConditionalHide(nameof(stepType), ETutorialType.ShowClue, true)] public TutorialClue tutorialCluePrefab;
        public int squareValue;
        public EGameStatType needsStat;
        public int statCount;
        [EnumConditionalHide(nameof(stepType), ETutorialType.SquareClick, true)] public bool keepAfterStep = false;
        [EnumConditionalHide(nameof(stepType), ETutorialType.SetSummonChances, true)] public List<ESquareSkin> skinsChances;
    }
}