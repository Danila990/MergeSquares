using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdSequence : Sequence, ICommand, ICommandDebugReport {
		public CmdSequence( params ICommand[] cmd ) {
			Enqueue( cmd );
		}

		public new CmdSequence Enqueue( ICommand cmd ) {
			base.Enqueue( cmd );
			return this;
		}

		public void Start( Action<ICommand> onFinish ) {
			AddCommand( new CmdCustom1<ICommand>( onFinish, this ) );
		}
        
        public void Update(float dt) {}

		public override string ToString() {
			return ObjectInfo.Report( this );
		}

		string ICommandDebugReport.DebugReport( string indent ) {
			return ObjectInfo.Report( this, base.DebugReport( indent + Utils.Space ) + indent );
		}
	}
}