using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdWaitInterval : ICommand {
		private Action<ICommand> onFinish;
		private readonly float durationInSec;
		private float elapsedTimeInSec;

		public CmdWaitInterval( float durationInSec ) {
			this.durationInSec = durationInSec;
		}

		public void Start( Action<ICommand> onFinish ) {
			this.onFinish = onFinish;
            // new CmdWaitTask1<float>(async delay => await Task.Delay((int)(delay * 1000f)), desc.delayOnStartInSec)
		}

        public void Update(float dt) {}

        // private void OnUpdate( EvUpdate ev ) {
		// 	elapsedTimeInSec += ev.deltaTime;
		// 	if ( elapsedTimeInSec >= durationInSec ) {
		// 		onFinish?.Invoke( this );
		// 	}
		// }

		public override string ToString() {
			return ObjectInfo.Report( this, durationInSec );
		}
	}
}