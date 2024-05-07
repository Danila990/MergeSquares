using Core.Signals;

namespace Shop.AnalyticsSignals
{
    public class ShopPurchaseSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly int AmountSpent;
        public readonly string ProductName;

        public ShopPurchaseSignal(int amount, string productName)
        {
            AmountSpent = amount;
            ProductName = productName;
        }
    }
}
