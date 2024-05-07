using System;
using UnityEngine;

namespace Utils.Instructions {
	public sealed class WaitForCallback : CustomYieldInstruction {
		public override bool keepWaiting => keepWaiting_;
		private bool keepWaiting_;

		public WaitForCallback( Action<Action> func ) {
			keepWaiting_ = true;
			func( () => keepWaiting_ = false );
		}
	}
}