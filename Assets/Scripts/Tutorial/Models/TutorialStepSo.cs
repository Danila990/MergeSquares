using Core.Anchors;
using Core.Conditions;
using UnityEngine;
using Utils;

namespace Tutorial.Models
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Models/TutorialStep")]
    public class TutorialStepSo : ScriptableObject
    {
        public TutorialStepDescType type;
        // Board
        public Vector2Int positionDragStart;
        public Vector2Int positionDragFinish;
        // Dialogue
        public string scriptName;
        public string scriptLabel;
        public bool submitOnStepEnd;
        // Common
        public EAnchorType anchorType = EAnchorType.None;
        public Vector2Int stackFixedTarget;
        public string anchorId;
        public bool lockLayer;
        public bool showFader;
        public EDirection arrowDir = EDirection.None;
        public Vector3 arrowOffset = Vector3.zero;
        // For All
        public float delayOnStartInSec;
        public bool showSkipButton;
        public ExpressionDesc skipCondition;
        public ExpressionDesc activationCondition;
        public ExpressionDesc completionCondition;
    }
}