using Core.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Windows.AnalyticsSignals
{
    public class PopupOpenedSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public string PopupName { get; }

        public PopupOpenedSignal(string popupName)
        {
            PopupName = popupName;
        }
    }
}
