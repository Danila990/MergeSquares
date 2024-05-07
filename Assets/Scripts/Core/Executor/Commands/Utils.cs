using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.Executor.Commands {
	internal static class Utils {
		public const string Delimiter = "\n";
		public const string Space = "\t";
		public const string ListSeparator = ",";

		public static string DebugReport( [CanBeNull] object obj, string indent ) {
			if ( obj == null ) {
				return "<empty>";
			}
			if ( obj is ICommandDebugReport rep ) {
				return rep.DebugReport( indent );
			}
			return obj.ToString();
		}

		public static string DebugReport( IEnumerable<ICommand> list, string indent, string separator ) {
			var builder = new StringBuilder();
			var tail = false;
			foreach ( var cmd in list ) {
				if ( tail ) {
					builder.Append( separator );
				}
				builder.Append( indent ).Append( DebugReport( cmd, indent ) );
				tail = true;
			}
			return builder.ToString();
		}

		[Conditional( "EXECUTOR_LOG_ENABLED" ), Conditional( "UNITY_EDITOR" )]
		public static void LogCommand( ICommand cmd ) {
			// Debug.Log( $"{Time.realtimeSinceStartup} Run command {cmd}" );
		}
	}
}