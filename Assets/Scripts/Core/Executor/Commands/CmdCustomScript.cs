using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdCustomScript : ICommand {
		private Action<ICommand> onFinishAction;
		private readonly Action<Action> script;

		public CmdCustomScript( Action<Action> script ) {
			this.script = script;
		}

		public void Start( Action<ICommand> onFinish ) {
			onFinishAction = onFinish;
			script( OnFinishScript );
		}
        
        public void Update(float dt) {}

		private void OnFinishScript() {
			onFinishAction( this );
		}

		public override string ToString() {
			return ObjectInfo.Report( this );
		}
	}

	public class CmdCustomScript1<T> : ICommand {
		private Action<ICommand> onFinishAction;
		private Action<Action, T> script;
		private readonly T prm;

		public CmdCustomScript1( Action<Action, T> script, T prm ) {
			this.script = script;
			this.prm = prm;
		}

		public void Start( Action<ICommand> onFinish ) {
			onFinishAction = onFinish;
			script( OnFinishScript, prm );
		}
        
        public void Update(float dt) {}

		private void OnFinishScript() {
			onFinishAction( this );
		}

		public override string ToString() {
			return ObjectInfo.Report( this, typeof( T ).Name );
		}
	}

	public class CmdCustomScript2<T1, T2> : ICommand {
		private Action<ICommand> onFinishAction;
		private Action<Action, T1, T2> script;
		private readonly T1 prm1;
		private readonly T2 prm2;

		public CmdCustomScript2( Action<Action, T1, T2> script, T1 prm1, T2 prm2 ) {
			this.script = script;
			this.prm1 = prm1;
			this.prm2 = prm2;
		}

		public void Start( Action<ICommand> onFinish ) {
			onFinishAction = onFinish;
			script( OnFinishScript, prm1, prm2 );
		}
        
        public void Update(float dt) {}

		private void OnFinishScript() {
			onFinishAction( this );
		}

		public override string ToString() {
			return ObjectInfo.Report( this, typeof( T1 ).Name, typeof( T2 ).Name );
		}
	}
}