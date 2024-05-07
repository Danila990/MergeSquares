using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Core.Executor.Commands {
	public class CmdParallel : ICommand, ICommandDebugReport {
		private Action<ICommand> onFinishAction;
		private readonly List<ICommand> commands;

		public CmdParallel() {
			commands = new List<ICommand>();
		}

		public CmdParallel( params ICommand[] cmd ) {
			commands = cmd.ToList();
		}

		public CmdParallel Enqueue( ICommand cmd ) {
			commands.Add( cmd );
			return this;
		}

		public void Start( Action<ICommand> onFinish ) {
			if ( commands.Count == 0 ) {
				onFinish( this );
				return;
			}
			onFinishAction = onFinish;
			var commandsToStart = commands.ToArray();
			foreach ( var cmd in commandsToStart ) {
				cmd.Start( CheckFinish );
			}
		}
        
        public void Update(float dt) {}

		private void CheckFinish( ICommand cmd ) {
			var idx = commands.IndexOf( cmd );
			if ( idx >= 0 ) {
				commands[idx] = commands[commands.Count - 1];
				commands.RemoveAt( commands.Count - 1 );
				if ( commands.Count == 0 ) {
					onFinishAction( this );
				}
			}
		}

		public override string ToString() {
			return ObjectInfo.Report( this );
		}

		string ICommandDebugReport.DebugReport( string indent ) {
			if ( commands.Count == 0 ) {
				return ObjectInfo.Report( this, indent + Utils.Space + "<empty>" + indent );
			}
			return ObjectInfo.Report( this, Utils.DebugReport( commands, indent + Utils.Space, Utils.ListSeparator ) + indent );
		}
	}
}