using Core.Signals;

namespace Core.Windows.AnalyticsSignals
{
    public class PopupClosedSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public string PopupName { get; }

        public PopupClosedSignal(string popupName)
        {
            PopupName = popupName;
        }
    }
}
