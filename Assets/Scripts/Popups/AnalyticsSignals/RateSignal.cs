using Core.Signals;
using GameStats;

namespace Popups.AnalyticsSignals
{
    public class RateSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly bool WasRated;

        public RateSignal(bool wasRated)
        {
            WasRated = wasRated;
        }
    }
}
