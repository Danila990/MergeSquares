using Core.Signals;

namespace Purchases.AnalyticsSignals
{
    public class BoughtNoAdsSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly int Cost;

        public BoughtNoAdsSignal(int cost)
        {
            Cost = cost;
        }
    }
}
