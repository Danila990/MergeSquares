using System;
using Utils;

namespace Core.Executor.Commands {
	public static class CmdEvent {
		public static ICommand Broadcast<T>( T ev ) where T : struct {
			return new CmdBroadcastEvent<T>( ev );
		}

		private class CmdBroadcastEvent<T> : ICommand where T : struct {
			private readonly T ev;

			public CmdBroadcastEvent( T ev ) {
				this.ev = ev;
			}

			public void Start( Action<ICommand> onFinish ) {
				// App.The.Events.Broadcast( ev );
				onFinish( this );
			}
            
            public void Update(float dt) {}

			public override string ToString() {
				return ObjectInfo.Report( this, typeof( T ).Name );
			}
		}
	}
}