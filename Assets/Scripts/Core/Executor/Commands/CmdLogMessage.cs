using System;
using UnityEngine;
using Utils;

namespace Core.Executor.Commands {
	public class CmdLogMessage : ICommand {
		private readonly string msg;

		public CmdLogMessage( string msg ) {
			this.msg = msg;
		}

		public void Start( Action<ICommand> onFinish ) {
			Debug.Log( msg );
			onFinish( this );
		}
        
        public void Update(float dt) {}

		public override string ToString() {
			return ObjectInfo.Report( this, "\"" + msg.Substring( 0, Math.Min( 32, msg.Length ) ) + "\"" );
		}
	}
}