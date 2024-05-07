using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.LogViewer {
	public class LogViewerHandler : ILogHandler {
		private readonly ILogHandler unityHandler;
		private readonly global::Utils.LogViewer.LogViewer logViewer;
		private readonly Regex regex;

		public LogViewerHandler( global::Utils.LogViewer.LogViewer logViewer, Regex regex ) {
			unityHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = this;
			this.logViewer = logViewer;
			this.regex = regex;
		}

		public void LogFormat( LogType logType, Object context, string format, params object[] args ) {
			var message = args[ 0 ] as string;
			var (module, method) = GetMeta( message );
			if ( module != null ) {
				var cfg = logViewer.GetLogViewConfig( module );
				if ( method != null ) {
					cfg = logViewer.GetLogViewConfig( module, method ) ?? cfg;
				}
				if ( cfg != null ) {
					var color = ColorUtility.ToHtmlStringRGBA( cfg.color );
					message = $"<color=#{color}>{message}</color>";
					args[ 0 ] = message;
					if ( logViewer.AreRestHidden ) {
						unityHandler.LogFormat( logType, context, format, args );
					}
				}
			}

			if ( !logViewer.AreRestHidden ) {
				unityHandler.LogFormat( logType, context, format, args );
			}
		}

		public void LogException( Exception exception, Object context ) {
			unityHandler.LogException( exception, context );
		}

		private (string, string) GetMeta( string message ) {
			var res = regex.Matches( message );
			if ( res.Count > 1 ) {
				return ( res[ 0 ].Value, res[ 1 ].Value );
			}
			if ( res.Count > 0 ) {
				return ( res[ 0 ].Value, null );
			}
			return ( null, null );
		}
	}
}