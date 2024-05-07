using System;
using Core.Anchors;
using Core.Conditions.Service;
using Core.Windows;
using GameStats;
using Utils.Attributes;

namespace Core.Conditions {
	[Serializable]
	public class ConditionDesc {
        public string conditionTag = "A";
		public EConditionType type;
        [EnumConditionalHide(nameof(type), EConditionType.WindowOpened, true)]
		public EPopupType windowId;
        public EAnchorType anchorType;
        public string anchorId;
		public EEventTriggerId eventId;
		public EGameStatType gameStatId;
		public EGameStatOperation operation;
        public int num;
        // For Nani
        public string scriptName;
        public ConditionDescExtension extension;
    }
}