using System;
using Core.Windows;
using Purchases;
using UnityEngine;
using Utils.Attributes;

namespace Offers.Model
{
    [Serializable]
    [CreateAssetMenu(menuName = "Offers/OfferModel", fileName = "OfferModel")]
    public class OfferModel : ScriptableObject
    {
        [SerializeField] private string id;
        [BoolConditionalHide(nameof(isHasPurchase), hideInInspector: true, inverse: true)]
        [SerializeField] protected bool isSingle;
        [Space]
        [Header("TimeLimit")]
        [SerializeField] private bool isTimeLimitEnabled;
        [BoolConditionalHide(nameof(isTimeLimitEnabled), hideInInspector: true)]
        [SerializeField] private double timeLimit;
        [Space]
        [Header("Offer Window")]
        [SerializeField] protected EPopupType offerPopupType;
        [SerializeField] protected bool isShowPopupOnCreate;
        [Header("Purchase")]
        [SerializeField] protected bool isHasPurchase;
        [BoolConditionalHide(nameof(isHasPurchase), hideInInspector: true)]
        [SerializeField] protected PurchaseSo _purchase;
        [BoolConditionalHide(nameof(isHasPurchase), hideInInspector: true)]
        [SerializeField] protected PurchaseSo salePurchase;

        public string Id => id;
        public bool IsSingle => isSingle || isHasPurchase;
        public bool IsTimeLimitEnabled => isTimeLimitEnabled;
        public double TimeLimit => timeLimit;

        public EPopupType OfferPopupType => offerPopupType;
        public bool IsShowPopupOnCreate => isShowPopupOnCreate;

        public bool IsHasPurchase => isHasPurchase;
        public PurchaseSo Purchase => _purchase;
        public PurchaseSo SalePurchase => salePurchase;

        public bool IsOfferAvailable()
        {
            if (isHasPurchase)
            {
                if (_purchase == null || salePurchase == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
