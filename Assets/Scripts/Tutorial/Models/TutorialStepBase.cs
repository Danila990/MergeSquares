using System;
using Core.Anchors;
using Core.Conditions;
using Tutorial.View;
using UnityEngine;
using Utils;

namespace Tutorial.Models
{
    [Serializable]
    public class TutorialStepBaseData
    {
        [Tooltip ("[Optional] Set this if you want to skip step for some reason")]
        public ExpressionDesc skipCondition = null;
        [Tooltip ("[Required] Set this to activate step")]
        public ExpressionDesc activationCondition = null;
        [Tooltip ("[Required] Set this to complete step")]
        public ExpressionDesc completionCondition = null;
        [Space]
        [Tooltip ("[Optional] Points to entity, but you need to setup anchor component on it")]
        public EAnchorType anchorType = EAnchorType.None;
        [Tooltip ("[Optional] If there is more then one entity with this type of anchor you can setup id here and on component")]
        public string anchorId;
        [Tooltip ("[Optional] Enables lock layer")]
        public bool lockLayer = false;
        [Tooltip ("[Optional] Enables fade layer")]
        public bool showFader = true;
        [Tooltip ("[Optional] Clear fade on step hide")]
        public bool clearFader;
        [Tooltip ("[Optional] Enables arrow pointer and set it's direction")]
        public EDirection arrowDir = EDirection.None;
        [Tooltip ("[Optional] Set arrow pointer offset - if pivot of target isn't same as target you want to point")]
        public Vector3 arrowOffset = Vector3.zero;
        [Space]
        [Tooltip ("[Optional] Delay before start of this step in sec")]
        public float delayOnStartInSec = 0f;
        [Tooltip ("[Optional] Clue prefab which will spawn in this step")]
        public TutorialClue clue;
        [Tooltip ("[Optional] Clear clue on step hide")]
        public bool clearClue;
        [Tooltip ("[Optional] Use default click on tap layer")]
        public bool waitClick;
        [Tooltip ("[Optional] Close windows before start")]
        public bool closeAllWindows;
    }
    [Serializable]
    public class TutorialStepBase
    {
        [Header("Base params")]
        [SerializeField] public TutorialStepBaseData baseData;

        public TutorialStepBaseData Base => baseData;
    }
}