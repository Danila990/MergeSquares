using UnityEngine;

namespace Utils.Instructions {
	public sealed class WaitForever : CustomYieldInstruction {
		public override bool keepWaiting => true;
	}
}