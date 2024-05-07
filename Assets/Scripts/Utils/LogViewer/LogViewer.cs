using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Utils.LogViewer {
	[Serializable]
	public class LogViewConfig {
		public string module;
		public string method;

		public Color color;
		// public LogType logType;

		public bool Check( string moduleToCheck, string methodToCheck = null ) {
			var moduleEquals = !String.IsNullOrEmpty( module ) && module == moduleToCheck;
			var methodEmpty = String.IsNullOrEmpty( method );
			var methodEquals = !methodEmpty && method == methodToCheck || String.IsNullOrEmpty( methodToCheck ) && methodEmpty;
			return moduleEquals && methodEquals;
		}
	}

	[Serializable]
	[CreateAssetMenu( fileName = "LogViewer", menuName = "TruePilots/Utils/LogViewer" )]
	public class LogViewer : ScriptableObject {
		[SerializeField] private bool hideUncoloredLogs;
		[SerializeField] private List<LogViewConfig> configs;

		public bool AreRestHidden => hideUncoloredLogs;

		private Dictionary<string, LogViewConfig> cache = new Dictionary<string, LogViewConfig>();
		private LogViewerHandler handler;

		public LogViewConfig GetLogViewConfig( string module, string method = null ) {
			var key = MakeKey( module, method );
			if ( !cache.ContainsKey( key ) ) {
				var cfg = configs.Find( c => c.Check( module, method ) );
				// Add null cfg to cache as we don't want to repeat search
				cache.Add( key, cfg );
				return cfg;
			}
			return cache[ key ];
		}

		private void OnEnable() {
#if UNITY_EDITOR
			handler = new LogViewerHandler( this, new Regex( @"\[[a-zA-Z]+\]" ) );
#endif
		}

		private string MakeKey( string module, string method ) {
			var res = module;

			if ( !String.IsNullOrEmpty( method ) ) {
				res += method;
			}

			return res;
		}
	}
}