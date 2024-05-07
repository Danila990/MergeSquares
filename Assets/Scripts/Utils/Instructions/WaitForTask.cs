using System.Threading.Tasks;
using UnityEngine;

namespace Utils.Instructions {
	public sealed class WaitForTask : CustomYieldInstruction {
		public override bool keepWaiting => task != null && !task.IsCompleted;

		private Task task;

		public WaitForTask( Task task ) {
			this.task = task;
		}
	}
}