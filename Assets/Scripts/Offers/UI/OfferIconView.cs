using System;
using Offers.Model;
using Purchases;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Attributes;
using Zenject;

namespace Offers.UI
{
    public class OfferIconView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountToAdd;
        [SerializeField] private TextMeshProUGUI saleAmount;
        [SerializeField] private GameObject saleMark;
        [Space]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI oldPriceText;
        [Space]
        [SerializeField] private bool isOfferModelReference;
        [BoolConditionalHide(nameof(isOfferModelReference), hideInInspector: true)]
        [SerializeField] private OfferModel offerModelReference;

        private OfferService _offerService;
        private PurchaseService _purchaseService;

        private OfferData _offer;

        [Inject]
        private void Construct(
            OfferService offerService,
            PurchaseService purchaseService
        )
        {
            _offerService = offerService;
            _purchaseService = purchaseService;
            
            UpdateVisibilityState();
            TryInitializeReferenceOffer();
        }

        private void OnEnable()
        {
            _offerService.OfferDestroyed += OnOfferDestroyed;
        }

        private void OnDisable()
        {
            _offerService.OfferDestroyed -= OnOfferDestroyed;

        }

        private void Update()
        {
            if (_offer == null)
            {
                return;
            }

            UpdateOfferTimer(_offer);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!isOfferModelReference || offerModelReference == null)
            {
                return;
            }

            if (!offerModelReference.IsSingle)
            {
                Debug.LogError($"[OfferIconView] offerModelReference should be single offer type!");
                offerModelReference = null;
            }
        }

#endif

        public void Initialzie(OfferData offerData)
        {
            Clear();

            _offer = offerData;
            if(_offer == null)
            {
                return;
            }

            UpdateOfferView(_offer);
            UpdateVisibilityState();
        }

        public void Clear()
        {
            _offer = null;
            UpdateVisibilityState();
        }

        private void UpdateOfferView(OfferData offerData)
        {
            var offerTimeRemaining = _offer?.RemainingTime;
            if (offerTimeRemaining == null)
            {
                timeText.gameObject.SetActive(false);
                return;
            }

            if (_offer == null)
            {
                return;
            }

            timeText.gameObject.SetActive(true);
            UpdateOfferTimer(_offer);

            var model = _offerService.GetModel(offerData);
            var saleAmountValue = 0f; 
            if (model != null && !model.IsHasPurchase || !_purchaseService.TryGetCost(model.SalePurchase, out var cost, out var currency))
            {
                cost = 0;
                currency = string.Empty;
            }
            if(icon != null)
            {
                icon.sprite = model.SalePurchase.icon;
            }
            if(amountToAdd != null)
            {
                amountToAdd.text = $"+{model.SalePurchase.value}";
            }
            saleAmountValue = cost;
            priceText.text = $"{cost} {currency}";

            oldPriceText.gameObject.SetActive(true);
            if (model != null && !model.IsHasPurchase || !_purchaseService.TryGetCost(model.Purchase, out var oldCost, out var oldCurrency))
            {
                oldPriceText.gameObject.SetActive(false);
                oldCost = 0;
                oldCurrency = string.Empty;
            }
            if (oldCost != 0 && saleAmountValue > float.Epsilon)
            {
                saleAmountValue = 100 * (1 - saleAmountValue / oldCost);
            }

            oldPriceText.text = $"{oldCost} {oldCurrency}";

            if(saleMark != null && oldCost != 0)
            {
                saleMark.SetActive(saleAmountValue > float.Epsilon);
            }

            if (saleAmount != null && oldCost != 0)
            {
                saleAmount.text = $"{saleAmountValue:F0}%";
            }
        }

        private void UpdateOfferTimer(OfferData offer)
        {
            var remainingTime = offer?.RemainingTime;
            if (remainingTime == null)
            {
                return;
            }

            timeText.text = TimeUtils.SecondsToHMSFormat(remainingTime.Value);
        }

        private void UpdateVisibilityState()
        {
            if (rootObject == null)
            {
                return;
            }

            rootObject.SetActive(_offer != null);
        }

        private bool TryInitializeReferenceOffer()
        {
            if (_offer != null)
            {
                return false;
            }

            if (!TryFindReferenceOffer(out var offerData))
            {
                return false;
            }

            Initialzie(offerData);
            return true;
        }

        private bool TryFindReferenceOffer(out OfferData offer)
        {
            offer = null;

            if (!isOfferModelReference || offerModelReference == null)
            {
                return false;
            }

            var availableOffers = _offerService.GetActiveOffersByModel(offerModelReference);
            offer = availableOffers.FirstOrDefault();
            return offer != null;
        }

        private void OnOfferServiceInited()
        {
            TryInitializeReferenceOffer();
        }

        private void OnOfferCreated(OfferData offerData)
        {
            var model = _offerService.GetModel(offerData);
            if (!isOfferModelReference || offerModelReference == null || model != null && model != offerModelReference)
            {
                return;
            }

            if(_offer != null)
            {
                return;
            }

            Initialzie(offerData);
        }

        private void OnOfferDestroyed(OfferData offerData)
        {
            if(offerData == _offer)
            {
                Clear();
            }
        }

        public void OnBuyButton()
        {
            if (_offer == null)
            {
                return;
            }

            _offerService.ClaimOffer(_offer);
        }
    }
}
