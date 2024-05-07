using System.Collections.Generic;
using System.Text;

namespace Core.Executor.Commands {
	public class Sequence {
		private readonly Queue<ICommand> commandsQueue = new();
		private ICommand currentCommandOrNull;
		private ICommand nextCommandOrNull;
		private bool inExecuteCycle;

        protected bool startIfNull = true;

        public void Clear() => commandsQueue.Clear();

		public void AddCommand( ICommand command ) {
			if ( currentCommandOrNull != null || !startIfNull ) {
				commandsQueue.Enqueue( command );
			}
			else {
				StartNext( command );
			}
		}

		public void AddCommand( params ICommand[] list ) {
			Enqueue( list );
			Ensure();
		}

		protected void Enqueue( ICommand command ) {
			commandsQueue.Enqueue( command );
		}

		protected void Enqueue( IEnumerable<ICommand> list ) {
			foreach ( var command in list ) {
				commandsQueue.Enqueue( command );
			}
		}

		protected void Ensure() {
			if ( currentCommandOrNull == null && commandsQueue.Count > 0 ) {
				StartNext( commandsQueue.Dequeue() );
			}
		}

		protected string DebugReport( string indent ) {
			if ( currentCommandOrNull == null && commandsQueue.Count == 0 ) {
				return indent + "<empty>";
			}
			var builder = new StringBuilder();
			if ( currentCommandOrNull != null ) {
				builder.Append( indent );
				builder.Append( Utils.DebugReport( currentCommandOrNull, indent ) );
				if ( commandsQueue.Count > 0 ) {
					builder.Append( Utils.ListSeparator );
				}
			}
			builder.Append( Utils.DebugReport( commandsQueue, indent, Utils.ListSeparator ) );
			return builder.ToString();
		}

		// cycle is used instead of implicit recursion (execute <-> onCommandComplete) in order to have readable
		// (not too long) stack trace when 'instant' commands are involved
		protected void StartNext( ICommand command ) {
			nextCommandOrNull = command;
			inExecuteCycle = true;
			while ( nextCommandOrNull != null ) {
				currentCommandOrNull = nextCommandOrNull;
				nextCommandOrNull = null;
				Utils.LogCommand( currentCommandOrNull );
				currentCommandOrNull.Start( OnCommandComplete );
			}
			inExecuteCycle = false;
		}

        public void Update(float dt)
        {
            if (currentCommandOrNull != null)
            {
                currentCommandOrNull.Update(dt);
            }
        }

		private void OnCommandComplete( ICommand command ) {
			if ( currentCommandOrNull == command ) {
				currentCommandOrNull = null;
				if ( 0 < commandsQueue.Count ) {
					nextCommandOrNull = commandsQueue.Dequeue();
					if ( !inExecuteCycle ) {
						StartNext( nextCommandOrNull );
					}
				}
			}
		}
	}
}