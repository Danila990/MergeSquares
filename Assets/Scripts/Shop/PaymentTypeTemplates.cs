using UnityEngine;

namespace Shop
{
    public enum EPaymentTypeTemplate {None, AdHard, FreeAdDisable}

    public class PaymentTypeTemplates : MonoBehaviour
    {
        public static EPaymentType GetNextTypeFor(EPaymentTypeTemplate template, EPaymentType currentType) => template switch
        {
            EPaymentTypeTemplate.AdHard => AdHard(currentType),
            EPaymentTypeTemplate.FreeAdDisable => FreeAdDisable(currentType)
        };

        private static EPaymentType AdHard(EPaymentType currentType) => currentType switch
        {
            EPaymentType.None => EPaymentType.Advertising,
            EPaymentType.Advertising => EPaymentType.Hard,
            EPaymentType.Hard => EPaymentType.Hard
        };

        private static EPaymentType FreeAdDisable(EPaymentType currentType) => currentType switch
        {
            EPaymentType.None => EPaymentType.Free,
            EPaymentType.Free => EPaymentType.Advertising,
            EPaymentType.Advertising => EPaymentType.None
        };
    }
}

