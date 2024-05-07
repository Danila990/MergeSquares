using System;
using System.Linq;
using UnityEngine;

namespace Core.Conditions {
	public class ConditionContainer : ConditionBase {
		private readonly ConditionBase[] conditions;
		private readonly EConditionOperation operation;

		public ConditionContainer( ConditionBase[] conditions, EConditionOperation operation ) {
			this.conditions = conditions;
			foreach ( var condition in conditions ) {
				condition.Init( ValidateCompletion );
			}
			this.operation = operation;
		}

		public override bool IsTrue => CheckConditions();

		public override void Dispose() {
			foreach ( var condition in conditions ) {
				condition.Dispose();
			}
			Array.Clear( conditions, 0, conditions.Length );
			base.Dispose();
		}

		private void ValidateCompletion( bool completed ) {
			MarkChanged();
		}

		private bool CheckConditions() {
			var completed = false;
			switch ( operation ) {
				case EConditionOperation.Or:
					completed = conditions.Any( x => x.IsTrue );
					break;
				case EConditionOperation.And:
					completed = conditions.All( x => x.IsTrue );
					break;
				case EConditionOperation.Not:
					completed = !conditions.Any( x => x.IsTrue );
					break;
				default:
					Debug.LogWarningFormat( "Unknown operation type {0}", operation );
					break;
			}
			return completed;
		}
	}
}