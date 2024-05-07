using Core.Conditions;
using UnityEngine;

namespace Utils.Instructions {
	public class WaitCondition : CustomYieldInstruction {
		public override bool keepWaiting => conditionOrNull == null || !conditionOrNull.IsTrue;

		private ConditionBase conditionOrNull;

		public WaitCondition( ExpressionDesc desc ) {
			// conditionOrNull = ConditionBuilder.CreateConditionOrNull( desc );
		}

		public override void Reset() {
			conditionOrNull?.Dispose();
			conditionOrNull = null;
			base.Reset();
		}
	}
}