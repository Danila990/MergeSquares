using Core.Signals;
using GameStats;

namespace Purchases.AnalyticsSignals
{
    public class CurrencyBoughtSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly int Cost;
        public readonly EGameStatType StatType;
        public readonly int Amount;

        public CurrencyBoughtSignal(EGameStatType statType, int cost, int amount)
        {
            Cost = cost;
            StatType = statType;
            Amount = amount;
        }
    }
}
