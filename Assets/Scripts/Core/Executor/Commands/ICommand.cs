using System;

namespace Core.Executor.Commands {
	public interface ICommand {
		void Start( Action<ICommand> onFinish );
        void Update( float dt );
	}

	public interface ICommandDebugReport {
		string DebugReport( string indent );
	}
}