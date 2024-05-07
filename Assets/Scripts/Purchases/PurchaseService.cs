using System;
using System.Collections.Generic;
using Advertising;
using CloudServices;
using Core.SaveLoad;
using Core.Windows;
using GameStats;
using UnityEngine;
using Purchases.AnalyticsSignals;
using Zenject;

namespace Purchases
{
    public class PurchaseService : MonoBehaviour
    {
        [SerializeField] private PurchaseProviderBase purchaseProvider;
        [SerializeField] private List<PurchaseSo> purchases = new();

        [Space] [SerializeField] private Saver saver;
        [SerializeField] private bool verbose;

        public Action PurchaseChanged = () => { };
        public Action<CloudPurchase, PurchaseSo> Purchased = (x, y) => { };

        public bool IsPurchasesAvailable => _cloudService.CloudProvider.IsPaymentsAvailable;

        private AdvertisingService _advertisingService;
        private GameStatService _gameStatService;
        private CloudService _cloudService;
        private SignalBus _signalBus;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            AdvertisingService advertisingService,
            GameStatService gameStatService,
            CloudService cloudService,
            SignalBus signalBus,
            WindowManager windowManager
        )
        {
            _advertisingService = advertisingService;
            _gameStatService = gameStatService;
            _cloudService = cloudService;
            _signalBus = signalBus;
            _windowManager = windowManager;
            saver.DataLoadFinished += OnDataLoadedFinished;
        }
        
        private void Start()
        {
            _cloudService.CloudProvider.Purchased += OnPurchased;
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.Purchased -= OnPurchased;
            saver.DataLoadFinished -= OnDataLoadedFinished;
        }
        
        public void SetVerbose(bool value)
        {
            verbose = value;
        }

        public void StartPurchase(PurchaseSo so)
        {
            Log($"[PurchaseService][StartPurchase] id: {so.id}");

            if (!IsPurchaseAvailable(so))
            {
                return;
            }

            if (!_cloudService.CloudProvider.CheckAuthState())
            {
                Log($"[PurchaseService][StartPurchase] Try login");
                _windowManager.ShowWindow(EPopupType.Auth.ToString());
            }
            else
            {
                Log($"[PurchaseService][StartPurchase] Start yandex Purchase id: {so.id}");
                _cloudService.CloudProvider.StartPurchase(so.id);
            }
        }

        public bool TryCheckAlreadyBought(PurchaseSo so, out bool bought)
        {
            if (purchases.Contains(so))
            {
                if (_cloudService.CloudProvider.TryGetPurchase(so.id, out var purchase))
                {
                    switch (so.type)
                    {
                        case EPurchaseType.DisableAds:
                            bought = _advertisingService.IsAdsDisable;
                            break;
                        case EPurchaseType.GameType:
                            bought = purchaseProvider.TryCheckAlreadyBought(so);
                            break;
                        default:
                            bought = false;
                            break;
                    }

                    return true;
                }
            }

            bought = false;
            return false;
        }
        
        public bool IsPurchaseAvailable(PurchaseSo purchase)
        {
            if (!purchases.Contains(purchase) || !TryGetCost(purchase, out var cost, out var currency))
            {
                return false;
            }

            return IsPurchasesAvailable;
        }

        public bool TryGetCost(PurchaseSo so, out int cost, out string currency)
        {
            if (purchases.Contains(so))
            {
                if (_cloudService.CloudProvider.TryGetPurchase(so.id, out var purchase))
                {
                    cost = purchase.cost;
                    currency = purchase.currencySymbol;
                    return true;
                }
            }

            currency = "";
            cost = 0;

            return false;
        }

        private void OnPurchased(CloudPurchase purchase, bool present)
        {
            if (purchase == null || !present)
            {
                return;
            }
            
            Log($"[PurchaseService][OnPurchased] Got yandex Purchase id: {purchase.id} with present: {present}");
            
            if (TryGetById(purchase.id, out var so))
            {
                Purchase(so, purchase);
                Log($"[PurchaseService][OnPurchased] Consume Purchase id: {purchase.id}");
                _cloudService.CloudProvider.ConsumePurchase(purchase.id);

                PurchaseChanged.Invoke();
                Purchased.Invoke(purchase, so);
            }
        }

        private void Purchase(PurchaseSo so, CloudPurchase purchase)
        {
            switch (so.type)
            {
                case EPurchaseType.DisableAds:
                    Log($"[PurchaseService][Purchase] Disable Ads");
                    _advertisingService.SetAdsDisable();
                    if(_advertisingService.IsAdsDisable)
                    {
                        _signalBus.Fire(new BoughtNoAdsSignal(purchase.cost));
                    }
                    break;
                case EPurchaseType.AddCurrency:
                    Log($"[PurchaseService][Purchase] Add currency type: {so.statType} amount: {so.value}");
                    _gameStatService.TryIncWithAnim(so.statType, so.value);
                    _signalBus.Fire(new CurrencyBoughtSignal(so.statType, purchase.cost, so.value));
                    break;
                case EPurchaseType.GameType:
                    Log($"[PurchaseService][Purchase] Add game type");
                    purchaseProvider.Purchase(so, purchase);
                    break;
                case EPurchaseType.Pack:
                    Log($"[PurchaseService][Purchase] Item pack");
                    foreach (var packItem in so.packedItems)
                    {
                        if(packItem == so)
                        {
                            Log($"[PurchaseService][Purchase] Warning: Self packed item in ({packItem.name})!");
                            continue;
                        }

                        Purchase(packItem, purchase);
                    }
                    break;
            }

            saver.SaveNeeded.Invoke(true);
        }

        private bool TryGetById(string id, out PurchaseSo purchase)
        {
            foreach (var so in purchases)
            {
                if (so.id == id)
                {
                    purchase = so;
                    return true;
                }
            }

            purchase = null;
            return false;
        }
        
        //

        private void OnDataLoadedFinished(LoadContext loadContext)
        {
            foreach (var so in purchases)
            {
                if (_cloudService.CloudProvider.TryGetPurchase(so.id, out var purchase))
                {
                    if(purchase.count > 0)
                    {
                        Log($"[PurchaseService][OnDataLoadedFinished] Got unconsumed purchase: {so.id} count: {purchase.count}");
                        switch (so.type)
                        {
                            case EPurchaseType.DisableAds:
                                _advertisingService.SetAdsDisable();
                                _cloudService.CloudProvider.ConsumePurchase(purchase.id);
                                for (int i = 0; i < purchase.count; i++)
                                {
                                    _cloudService.CloudProvider.ConsumePurchase(purchase.id);
                                }
                                break;
                            default:
                                for (int i = 0; i < purchase.count; i++)
                                {
                                    Purchase(so, purchase);
                                    _cloudService.CloudProvider.ConsumePurchase(purchase.id);
                                }
                                break;
                        }

                        Purchased.Invoke(purchase, so);
                        saver.SaveNeeded.Invoke(true);
                    }
                }
            }
            PurchaseChanged.Invoke();
        }
        
        private void Log(string log)
        {
            if (verbose)
            {
                Debug.Log(log);
            }
        }
    }
}




