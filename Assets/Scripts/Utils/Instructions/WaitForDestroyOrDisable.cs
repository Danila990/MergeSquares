using UnityEngine;

namespace Utils.Instructions {
	public sealed class WaitForDestroyOrDisable : CustomYieldInstruction {
		public override bool keepWaiting => target && target.activeInHierarchy;

		private GameObject target;

		public WaitForDestroyOrDisable( GameObject target ) {
			this.target = target;
		}
	}
}