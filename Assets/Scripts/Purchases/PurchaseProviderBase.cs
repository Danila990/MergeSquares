using CloudServices;
using UnityEngine;

namespace Purchases
{
    public abstract class PurchaseProviderBase : MonoBehaviour
    {
        public abstract bool TryCheckAlreadyBought(PurchaseSo so);
        public abstract void Purchase(PurchaseSo so, CloudPurchase purchase);
    }
}