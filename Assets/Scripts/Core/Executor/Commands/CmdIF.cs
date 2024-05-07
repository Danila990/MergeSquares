using System;
using Core.Executor.Conditions;
using Utils;

namespace Core.Executor.Commands {
	public class CmdIF : ICommand, ICommandDebugReport {
		private const int NONE = 0;
		private const int TRUE = 1;
		private const int FALSE = -1;

		private Action<ICommand> onFinishAction;
		private readonly ICondition condition;
		private readonly ICommand cmdTrueOrNull;
		private readonly ICommand cmdFalseOrNull;
		private int debugBranch;

		public CmdIF( ICondition condition, ICommand cmdTrueOrNull, ICommand cmdFalseOrNull = null ) {
			this.condition = condition;
			this.cmdTrueOrNull = cmdTrueOrNull;
			this.cmdFalseOrNull = cmdFalseOrNull;
			debugBranch = NONE;
		}

		public void Start( Action<ICommand> onFinish ) {
			onFinishAction = onFinish;
			if ( condition.IsTrue ) {
				debugBranch = TRUE;
				if ( cmdTrueOrNull != null ) {
					cmdTrueOrNull.Start( OnFinishBranch );
				}
				else {
					onFinishAction( this );
				}
			}
			else {
				debugBranch = FALSE;
				if ( cmdFalseOrNull != null ) {
					cmdFalseOrNull.Start( OnFinishBranch );
				}
				else {
					onFinishAction( this );
				}
			}
		}
        
        public void Update(float dt) {}

		private void OnFinishBranch( ICommand cmd ) {
			onFinishAction( this );
		}

		public override string ToString() {
			return ObjectInfo.Report( this );
		}

		string ICommandDebugReport.DebugReport( string indent ) {
			switch ( debugBranch ) {
				case TRUE:
					return ObjectInfo.Report( this, "<was TRUE>" + indent + Utils.Space + Utils.DebugReport( cmdTrueOrNull, indent + Utils.Space ) + indent );
				case FALSE:
					return ObjectInfo.Report( this, "<was FALSE>" + indent + Utils.Space + Utils.DebugReport( cmdFalseOrNull, indent + Utils.Space ) + indent );
				default:
					return ObjectInfo.Report( this,
						indent + Utils.Space + "true:" + indent + Utils.Space + Utils.Space + Utils.DebugReport( cmdTrueOrNull, indent + Utils.Space + Utils.Space ) +
						indent + Utils.Space + "false:" + indent + Utils.Space + Utils.Space + Utils.DebugReport( cmdFalseOrNull, indent + Utils.Space + Utils.Space ) + indent
					);
			}
		}
	}
}