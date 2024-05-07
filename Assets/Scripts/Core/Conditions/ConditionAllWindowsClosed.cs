using Core.Windows;

namespace Core.Conditions
{
    public class ConditionAllWindowsClosed : ConditionBase
    {
        private WindowManager _windowManager;

        public ConditionAllWindowsClosed(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public override bool IsTrue => !_windowManager.IsAnyWindowOpened();
    }
}