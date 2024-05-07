using Advertising;
using CloudServices;
using JetBrains.Annotations;
using Purchases;
using UnityEngine;
using Zenject;

namespace GameScripts.PointPanel
{
    public class NoAdsPopup : MonoBehaviour
    {
        [SerializeField] private GameObject noAdsTile;
        [SerializeField] private GameObject alreadyDisabledTile;
        [SerializeField] private GameObject buyButton;
        [SerializeField] private PurchaseSo adsDisableSo;

        private PurchaseService _purchaseService;

        [Inject]
        public void Construct(PurchaseService purchaseService, AdvertisingService advertisingService)
        {
            _purchaseService = purchaseService;
            SetWindow(advertisingService.IsAdsDisable);
        }

        [UsedImplicitly]
        public void BuyNoAds()
        {
            _purchaseService.StartPurchase(adsDisableSo);
        }

        private void SetWindow(bool isAdsDisable)
        {
            // noAdsTile.SetActive(!isAdsDisable);
            buyButton.SetActive(!isAdsDisable);
            alreadyDisabledTile.SetActive(isAdsDisable);
        }
    }
}