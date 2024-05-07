using Core.Windows;
using Purchases;
using TMPro;
using UnityEngine;
using Utils;
using Zenject;

namespace Offers.UI
{
    public class StarterPackOfferPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [Space]
        [SerializeField] private TextMeshProUGUI saleAmount;
        [SerializeField] private GameObject saleMark;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI oldPriceText;

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

            popupBase.Disposed += OnPopupDisposed;
            popupBase.ShowArgsGot += OnPopupArgs;

            _offerService.OfferDestroyed += OnOfferDestroyed;
        }

        private void OnDestroy()
        {
            SetOfferInstance(null);

            popupBase.Disposed -= OnPopupDisposed;
            popupBase.ShowArgsGot -= OnPopupArgs;

            _offerService.OfferDestroyed -= OnOfferDestroyed;
        }

        private void Update()
        {
            if(_offer == null)
            {
                return;
            }

            UpdateOfferTimer(_offer);
        }

        private void OnPopupDisposed(PopupBaseCloseType closeType)
        {
            SetOfferInstance(null);
        }

        private void OnPopupArgs(object[] popupArgs)
        {
            if(popupArgs.Length <= 0 || popupArgs[0] is not OfferData offer)
            {
                Debug.LogWarning($"[StarterPackOfferPopup] Error: Failed to initialize offer popup with instance!");
                return;
            }

            SetOfferInstance(offer);
        }

        private void SetOfferInstance(OfferData offerData)
        {
            _offer = offerData;

            var offerTimeRemaining= _offer?.RemainingTime;
            if (offerTimeRemaining == null)
            {
                timeText.gameObject.SetActive(false);
                return;
            }

            if(_offer == null)
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
            if(saleMark != null && oldCost != 0)
            {
                saleMark.SetActive(saleAmountValue > float.Epsilon);
            }

            if (saleAmount != null && oldCost != 0)
            {
                saleAmount.text = $"{saleAmountValue:F0}%";
            }
            oldPriceText.text = $"{oldCost} {oldCurrency}";
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

        private void OnOfferDestroyed(OfferData offerData)
        {
            if (_offer == null || offerData != _offer)
            {
                return;
            }

            popupBase.CloseWindow();
        }

        public void OnBuyButton()
        {
            if(_offer == null)
            {
                return;
            }

            _offerService.ClaimOffer(_offer);
        }
    }
}
