using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdWaitEvent : ICommand {
		private Action<ICommand> _onFinish;
        private readonly Func<bool> _checkWaitFinished;
        private bool _complete;

        public CmdWaitEvent(Func<bool> checkWaitFinished)
        {
            _complete = false;
            _checkWaitFinished = checkWaitFinished;
        }
		
        public void Start( Action<ICommand> onFinish ) {
			_onFinish = onFinish;
		}

        public void Update(float dt)
        {
            if (_checkWaitFinished != null && _checkWaitFinished.Invoke() && !_complete)
            {
                _onFinish( this );
                _complete = true;
            }
        }

		public override string ToString() {
			return "CmdWaitEvent";
		}
	}
}