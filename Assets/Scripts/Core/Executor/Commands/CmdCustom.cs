using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdCustom : ICommand {
		private Action action;

		public CmdCustom( Action action ) {
			this.action = action;
		}

		public void Start( Action<ICommand> onFinish ) {
			action();
			onFinish( this );
		}
        
        public void Update(float dt) {}
        
		public override string ToString() {
			return ObjectInfo.Report( this );
		}
	}

	public class CmdCustom1<T> : ICommand {
		private readonly Action<T> action;
		private readonly T prm;

		public CmdCustom1( Action<T> action, T prm ) {
			this.action = action;
			this.prm = prm;
		}

		public void Start( Action<ICommand> onFinish ) {
			action( prm );
			onFinish( this );
		}

        public void Update(float dt) {}
        
		public override string ToString() {
			return ObjectInfo.Report( this, typeof( T ).Name );
		}
	}

	public class CmdCustom2<T1, T2> : ICommand {
		private readonly Action<T1, T2> action;
		private readonly T1 prm1;
		private readonly T2 prm2;

		public CmdCustom2( Action<T1, T2> action, T1 prm1, T2 prm2 ) {
			this.action = action;
			this.prm1 = prm1;
			this.prm2 = prm2;
		}

		public void Start( Action<ICommand> onFinish ) {
			action( prm1, prm2 );
			onFinish( this );
		}
        
        public void Update(float dt) {}

		public override string ToString() {
			return ObjectInfo.Report( this, typeof( T1 ).Name, typeof( T2 ).Name );
		}
	}

	public class CmdCustom3<T1, T2, T3> : ICommand {
		private readonly Action<T1, T2, T3> action;
		private readonly T1 prm1;
		private readonly T2 prm2;
		private readonly T3 prm3;

		public CmdCustom3( Action<T1, T2, T3> action, T1 prm1, T2 prm2, T3 prm3 ) {
			this.action = action;
			this.prm1 = prm1;
			this.prm2 = prm2;
			this.prm3 = prm3;
		}

		public void Start( Action<ICommand> onFinish ) {
			action( prm1, prm2, prm3 );
			onFinish( this );
		}
        
        public void Update(float dt) {}
	}
}