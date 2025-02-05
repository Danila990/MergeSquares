
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif

namespace Plugins.WindowsManager
{
	/// <summary>
	/// Вспомогательный компонент, вешается на текущую сцену, если есть отложенные окна, чтобы корректно
	/// удалять их в случае, если сцена закрывается до того, как будут активированы и закрыты отложенные окна.
	/// </summary>
	[DisallowMultipleComponent]
	public class WindowManagerLocalSceneHelper : MonoBehaviour
	{
		public UnityEvent DestroyEvent { get; } = new UnityEvent();

		private void OnDestroy()
		{
			DestroyEvent.Invoke();
			DestroyEvent.RemoveAllListeners();
		}
	}

	[DisallowMultipleComponent]
	public abstract class WindowManagerBase : MonoBehaviour, IWindowManager
	{
		private struct DelayedWindow : IComparable<DelayedWindow>
		{
			private readonly long _timestamp;

			public DelayedWindow(IWindow window, bool isUnique, bool overlap)
			{
				Window = window;
				IsUnique = isUnique;
				Overlap = overlap;
				_timestamp = DateTime.Now.Ticks;
			}

			public IWindow Window { get; }
			public bool IsUnique { get; }
			public bool Overlap { get; }

			public int CompareTo(DelayedWindow other)
			{
				if (other.IsUnique && !IsUnique) return 1;
				if (!other.IsUnique && IsUnique) return -1;
				if (_timestamp > other._timestamp) return 1;
				if (_timestamp < other._timestamp) return -1;
				return 0;
			}
		}

		private Dictionary<string, Window> _windowsMap = new Dictionary<string, Window>();
		private Dictionary<string, Window> _createdWindowsMap = new Dictionary<string, Window>();

		protected readonly List<IWindow> _openedWindows = new List<IWindow>();
		private readonly SortedSet<DelayedWindow> _delayedWindows = new SortedSet<DelayedWindow>();
		private bool _isUnique;

		private WindowManagerLocalSceneHelper _sceneHelper;

#pragma warning disable 649
		[SerializeField] protected Window[] _windows = new Window[0];
		[SerializeField] protected Window[] _createdWindows = new Window[0];
#pragma warning restore 649

		protected virtual int StartCanvasSortingOrder { get; } = 0;
		protected virtual int StepCanvasSortingOrder { get; } = 1;

		protected virtual void Awake()
		{
			_windowsMap = _windows.ToDictionary(window => window.WindowId, window => window);
			_createdWindowsMap = _createdWindows.ToDictionary(window => window.WindowId, window => window);
		}

		public event EventHandler<WindowOpenEventArgs> WindowOpenedEvent;

		public event EventHandler<WindowCloseEventArgs> WindowClosedEvent;

		public int CloseAll(params object[] args)
		{
			var windows = GetWindows(args);
			foreach (var window in windows)
			{
				window.Close(true);
			}

			return windows.Length;
		}

		public IWindow GetWindow(object arg)
		{
			return GetWindows(arg).FirstOrDefault();
		}

		public IWindow[] GetWindows(params object[] args)
		{
			var allWindows = new[] {_openedWindows, _delayedWindows.Select(delayed => delayed.Window)}
				.SelectMany(enumerable => enumerable).ToArray();
			if (args.Length <= 0)
			{
				return allWindows;
			}

			HashSet<IWindow> windows = new HashSet<IWindow>();
			foreach (var arg in args)
			{
				switch (arg)
				{
					case string stringArg:
						windows.UnionWith(allWindows.Where(window => window.WindowId == stringArg));
						break;
					case Type typeArg:
						if (!typeof(IWindow).IsAssignableFrom(typeArg))
							throw new NotSupportedException("IWindow types supported only.");
						windows.UnionWith(allWindows.Where(window => window.GetType() == typeArg));
						break;
					default:
						throw new NotSupportedException("GetWindows() received unsupported argument. " +
						                                "Strings WindowId and IWindow types supported only.");
				}
			}

			return windows.ToArray();
		}

		public IWindow ShowWindow(string windowId, object[] args = null, bool isUnique = false, bool overlap = false)
		{
			if (!_windowsMap.TryGetValue(windowId, out var prefab))
			{ 
                Debug.LogFormat("Window with Id {0} isn't registered in Manager.", windowId);
                if (!_createdWindowsMap.TryGetValue(windowId, out var createdWindow))
                {
                    Debug.LogFormat("Fallback on created window with Id {0} failed.", windowId);
                    return null;
                }
                return ShowCreatedWindow(windowId);
			}

			if (!_sceneHelper)
			{
				_sceneHelper = new GameObject(@"WindowManagerLocalSceneHelper",
						typeof(WindowManagerLocalSceneHelper))
					.GetComponent<WindowManagerLocalSceneHelper>();
				_sceneHelper.DestroyEvent.AddListener(OnDestroyScene);
			}

			var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			instance.name = windowId;

			var window = instance.GetComponent<IWindow>();
			Assert.IsNotNull(window, "Window prefab must implements IWindow.");

			InitWindow(window, args ?? Array.Empty<object>());

			if (_isUnique || isUnique && _openedWindows.Count > 0)
			{
				window.Canvas.gameObject.SetActive(false);
				_delayedWindows.Add(new DelayedWindow(window, isUnique, overlap));
			}
			else
			{
				DoApplyWindow(window, isUnique, overlap);
			}

			return window;
		}
		
		public IWindow ShowCreatedWindow(string windowId, bool isUnique = false, bool overlap = false)
		{
			if (!_createdWindowsMap.TryGetValue(windowId, out var window))
			{
				Debug.LogErrorFormat("Window with Id {0} isn't registered in Manager.", windowId);
				return null;
			}

			if (_isUnique || isUnique && _openedWindows.Count > 0)
			{
				window.gameObject.SetActive(false);
				_delayedWindows.Add(new DelayedWindow(window, isUnique, overlap));
			}
			else
			{
				DoApplyWindow(window, isUnique, overlap);
			}

			return window;
		}

		public bool RegisterWindow(Window windowPrefab, bool overrideExisting = false)
		{
			var windowId = windowPrefab.WindowId;
			if (_windowsMap.ContainsKey(windowId))
			{
				if (!overrideExisting) return false;

				CloseAll(windowId);
				_windowsMap[windowId] = windowPrefab;
			}
			else
			{
				_windowsMap.Add(windowId, windowPrefab);
			}

			return true;
		}

		public bool UnregisterWindow(string windowId)
		{
			return _windowsMap.Remove(windowId);
		}

		private void OnDestroyScene()
		{
			_delayedWindows.Clear();
			_openedWindows.Clear();

			_sceneHelper.DestroyEvent.RemoveListener(OnDestroyScene);
			_sceneHelper = null;
		}

		private void DoApplyWindow(IWindow window, bool isUnique, bool overlap)
		{
			_isUnique = isUnique;

			window.CloseWindowEvent += OnCloseWindow;
			window.DestroyWindowEvent += OnDestroyWindow;
			window.ResetClosed();

			if (!window.HasOwnCanvas)
			{
				window.Canvas.sortingOrder = StartCanvasSortingOrder + (_openedWindows.Count * StepCanvasSortingOrder);
			}

			var overlappedWindow = _openedWindows.LastOrDefault();
			_openedWindows.Add(window);

			if (window.IsActive())
			{
				Debug.LogError("Window must be inactive in initial time.");
			}
			else if (!window.IsActiveOrActivated())
			{
				window.Activate();
			}

			if (overlap && overlappedWindow != null && overlappedWindow.IsActiveOrActivated())
			{
				overlappedWindow.Deactivate();
			}

			WindowOpenedEvent?.Invoke(this, new WindowOpenEventArgs(window));
		}

		protected virtual void InitWindow(IWindow window, object[] args)
		{
			window.SetArgs(args);
		}

		private void OnCloseWindow(object sender, WindowResultEventArgs args)
		{
			var window = (IWindow) sender;
			var index = _openedWindows.IndexOf(window);
			var overlappedWindow = index > 0 ? _openedWindows.ElementAt(index - 1) : null;
			_openedWindows.Remove(window);
			window.CloseWindowEvent -= OnCloseWindow;
			window.DestroyWindowEvent -= OnDestroyWindow;

			if (!CreatedContains(window.WindowId))
			{
				if (window.IsInactiveOrDeactivated())
				{
					Destroy(window.Canvas.gameObject);
				}
				else
				{
					window.ActivatableStateChangedEvent += OnWindowDeactivateHandler;
				}
			}

			if (overlappedWindow != null && overlappedWindow.IsInactiveOrDeactivated())
			{
				overlappedWindow.Activate();
			}

			_isUnique = false;
			WindowClosedEvent?.Invoke(this, new WindowCloseEventArgs(window.WindowId, args?.Data));

			_delayedWindows.ToList().ForEach(call =>
			{
				if (_isUnique || call.IsUnique && _openedWindows.Count > 0) return;

				_delayedWindows.Remove(call);

				call.Window.Canvas.gameObject.SetActive(true);
				DoApplyWindow(call.Window, call.IsUnique, call.Overlap);
			});
		}

		private static void OnWindowDeactivateHandler(object sender, EventArgs args)
		{
			var activatableStateChangedEventArgs = (ActivatableStateChangedEventArgs) args;
			if (activatableStateChangedEventArgs.CurrentState != ActivatableState.Inactive) return;
			var activatable = (IActivatable) sender;
			activatable.ActivatableStateChangedEvent -= OnWindowDeactivateHandler;
			Destroy(((IWindow) activatable).Canvas.gameObject);
		}

		private void OnDestroyWindow(object sender, WindowResultEventArgs args)
		{
			var window = (IWindow) sender;
			Debug.LogWarningFormat("Window {0} was destroyed outside of Close() method.", window.WindowId);
			OnCloseWindow(sender, args);
		}

		private bool CreatedContains(string windowId) => _createdWindowsMap.ContainsKey(windowId);
		
#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Window Manager")]
		private static void FindAndSelectWindowManager()
		{
			var instance = Resources.FindObjectsOfTypeAll<WindowManagerBase>().FirstOrDefault();
			if (!instance)
			{
				LoadAllPrefabs();
				instance = Resources.FindObjectsOfTypeAll<WindowManagerBase>().FirstOrDefault();
			}

			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of WindowManager.");
		}

		private static void LoadAllPrefabs()
		{
			Directory.GetDirectories(Application.dataPath, @"Resources", SearchOption.AllDirectories)
				.Select(s => Directory.GetFiles(s, @"*.prefab", SearchOption.TopDirectoryOnly))
				.SelectMany(strings => strings.Select(Path.GetFileNameWithoutExtension))
				.Distinct().ToList().ForEach(s => Resources.LoadAll(s));
		}
#endif
	}
}