using Core.Windows;
using Plugins.WindowsManager;

namespace Core.Conditions
{
    public class ConditionWindowOpened : ConditionBase
    {
        private readonly string _id;
        private WindowManager _windowManager;
        private bool _completed;

        public ConditionWindowOpened(EPopupType id, WindowManager windowManager)
        {
            _id = id.ToString();
            _windowManager = windowManager;
            _windowManager.WindowOpenedEvent += OnWindowOpened;
            _windowManager.WindowClosedEvent += OnWindowClosed;
        }

        public override void Dispose()
        {
            _windowManager.WindowOpenedEvent -= OnWindowOpened;
            _windowManager.WindowClosedEvent -= OnWindowClosed;
            base.Dispose();
        }

        private void OnWindowClosed(object sender, WindowCloseEventArgs windowCloseEventArgs)
        {
            if (windowCloseEventArgs.WindowId == _id)
            {
                MarkChanged();
            }
        }

        private void OnWindowOpened(object sender, WindowOpenEventArgs windowOpenEventArgs)
        {
            if (windowOpenEventArgs.Window.WindowId == _id)
            {
                MarkChanged();
            }
        }

        public override bool IsTrue => IsOpen();

        private bool IsOpen()
        {
            var window = _windowManager.GetWindow(_id);
            return window?.IsActive() ?? false;
        }
    }
}
