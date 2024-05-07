using GameStats;

namespace Shop
{
    public enum EPaymentType {None, Free, Advertising, Soft, Hard, Money }
    
    public static class PaymentType
    {
        public static EGameStatType ToEGameStatType(EPaymentType paymentType) => paymentType switch
        {
            EPaymentType.Soft => EGameStatType.Soft,
            EPaymentType.Hard => EGameStatType.Hard,
            _ => EGameStatType.None
        };
    }
}

