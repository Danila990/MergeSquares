using System;

namespace Core.Executor.Conditions {
	public class ConditionFunc : ICondition {
		private readonly Func<bool> checker;
		private readonly bool invert;

		public ConditionFunc( Func<bool> checker, bool invert = false ) {
			this.checker = checker;
			this.invert = invert;
		}

		public bool IsTrue {
			get { return invert != checker.Invoke(); }
		}
	}
}