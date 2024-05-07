using CloudServices;
using Core.SaveLoad;
using Core.Windows;
using Offers.Model;
using Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Zenject;

namespace Offers
{
    [Serializable]
    public class OfferData
    {
        public string offerId;
        public long startTimestamp;
        public bool isTimeLimitEnabled;
        public double timeLimit;
        
        public TimeSpan ActiveTime
        {
            get => Timestamp.CalculateTimeDiff(startTimestamp);
        }

        public TimeSpan RemainingTime
        {
            get
            {
                var limitSpan = TimeSpan.FromSeconds(timeLimit);
                var remainingTime = limitSpan - ActiveTime;
                if(remainingTime.TotalSeconds < 0)
                {
                    remainingTime = TimeSpan.Zero;
                }

                return remainingTime;
            }
        }
    }
    
    [Serializable]
    public class OfferServiceData
    {
        public List<OfferData> offers = new();
    }
    
    public class OfferService : MonoBehaviour
    {
        [SerializeField] private Saver saver;
        [SerializeField] private List<OfferModel> models = new();
        public bool IsInited => _isInited;
        public event Action<OfferData> OfferCreated = (x) => { };
        public event Action<OfferData> OfferDestroyed = (x) => { };
        public event Action OffersStateChanged = () => { };
        public event Action Inited = () => { };

        private OfferServiceData _data = new();
        private Timer _updateTimer = new();
        private bool _isInited = false;

        private const float OFFER_UPDATE_INTERVAL = 0.1f;

        private WindowManager _windowManager;
        private PurchaseService _purchaseService;

        [Inject]
        private void Construct(
            DiContainer diContainer,
            WindowManager windowManager,
            PurchaseService purchaseService
        )
        {
            _windowManager = windowManager;
            _purchaseService = purchaseService;

            saver.DataSaved += OnDataSaved;
            saver.DataLoaded += OnDataLoaded;
            saver.DataLoadFinished += OnDataLoadFinished;

            _purchaseService.Purchased += OnPurchased;

            _updateTimer.TimerFinished += OnUpdateTimerFinished;
        }

        protected virtual void OnDestroy()
        {
            saver.DataSaved -= OnDataSaved;
            saver.DataLoaded -= OnDataLoaded;
            saver.DataLoadFinished -= OnDataLoadFinished;
            
            _purchaseService.Purchased -= OnPurchased;

            _updateTimer.TimerFinished -= OnUpdateTimerFinished;
        }

        protected virtual void Start()
        {
            _updateTimer.Init(OFFER_UPDATE_INTERVAL);
        }

        protected virtual void Update()
        {
            _updateTimer.Update(Time.deltaTime);
        }

        public OfferData CreateOffer(OfferModel offerModel)
        {
            if (!IsOfferCreationAvailable(offerModel))
            {
                Debug.Log($"[OfferService] Error: Unable to create offer from model ({offerModel.GetType().Name})");
                return null;
            }

            var offer = new OfferData
            {
                isTimeLimitEnabled = offerModel.IsTimeLimitEnabled,
                offerId = offerModel.Id,
                startTimestamp = Timestamp.GetTicks(),
                timeLimit = offerModel.IsTimeLimitEnabled ? offerModel.TimeLimit : 0
            };
            _data.offers.Add(offer);
            
            if (offerModel.IsShowPopupOnCreate)
            {
                TryShowOfferPopup(offer);
            }

            OfferCreated.Invoke(offer);
            OffersStateChanged.Invoke();

            SaveOffers();
            return offer;
        }

        public void DestroyOffer(OfferData offer)
        {
            if (!_data.offers.Contains(offer))
            {
                return;
            }

            _data.offers.Remove(offer);

            OfferDestroyed.Invoke(offer);
            OffersStateChanged.Invoke();

            SaveOffers();
        }

        public bool IsOfferCreationAvailable(OfferModel offersModel)
        {
            if (offersModel == null)
            {
                return false;
            }

            if (!models.Contains(offersModel))
            {
                return false;
            }

            if (!offersModel.IsOfferAvailable())
            {
                return false;
            }

            if (offersModel.IsSingle)
            {
                var activeModelOffers = GetActiveOffersByModel(offersModel);
                if (activeModelOffers.Count > 0)
                {
                    return false;
                }
            }

            if (offersModel.IsHasPurchase)
            {
                if (!_purchaseService.IsPurchaseAvailable(offersModel.SalePurchase))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryShowOfferPopup(OfferData offer)
        {
            var offerModel = GetModel(offer);
            if (offerModel == null)
            {
                return false;
            }

            if (offerModel == null || offerModel.OfferPopupType == EPopupType.None)
            {
                return false;
            }

            var window = _windowManager.ShowWindow(offerModel.OfferPopupType.ToString(), new object[] { offer });
            return window != null;
        }

        public void ClaimOffer(OfferData offer)
        {
            if (IsOfferDataShouldBeDestroyed(offer))
            {
                return;
            }

            var offerModel = GetModel(offer);
            if (offerModel == null)
            {
                return;
            }
            if (offerModel.IsHasPurchase)
            {
                _purchaseService.StartPurchase(offerModel.SalePurchase);
                return;
            }

            DestroyOffer(offer);
        }
        
        public IList<OfferData> GetActiveOffersByModel(OfferModel offersModel)
        {
            return _data.offers.Where(x => x.offerId == offersModel.Id).ToList();
        }

        public OfferModel GetModel(OfferData data)
        {
            return models.Find(m => m.Id == data.offerId);
        }

        public int GetCreatedOffersCount(string offerId)
        {
            return _data.offers.FindAll(o => o.offerId == offerId).Count;
        }

        private void SaveOffers()
        {
            saver.SaveNeeded.Invoke(true);
        }

        private void UpdateOffersState()
        {
            if (_data.offers.Count <= 0)
            {
                return;
            }

            var offersToDestroy = _data.offers.Where(x => IsOfferDataShouldBeDestroyed(x)).ToList();
            if (!offersToDestroy.Any())
            {
                return;
            }

            _data.offers.RemoveAll(x => offersToDestroy.Contains(x));
            foreach (var destroyedOffer in offersToDestroy)
            {
                OfferDestroyed.Invoke(destroyedOffer);
            }

            SaveOffers();

            OffersStateChanged.Invoke();
        }

        private bool IsOfferDataShouldBeDestroyed(OfferData offer)
        {
            if (offer.isTimeLimitEnabled && offer.ActiveTime.TotalSeconds >= offer.timeLimit)
            {
                return true;
            }

            if (models.Find(m => m.Id == offer.offerId) == null)
            {
                return true;
            }
            
            var offerModel = GetModel(offer);
            if (offerModel == null || offerModel != null && !offerModel.IsOfferAvailable())
            {
                return true;
            }

            return false;
        }

        private void OnPurchased(CloudPurchase purchase, PurchaseSo so)
        {
            var offers = _data.offers.Where(x =>
                {
                    var offerModel = GetModel(x);
                    return offerModel != null && offerModel.IsHasPurchase && offerModel.SalePurchase == so;
                })
                .OrderBy(x => x.ActiveTime);

            if (!offers.Any())
            {
                return;
            }

            var offer = offers.FirstOrDefault();
            DestroyOffer(offer);
        }

        private void OnUpdateTimerFinished()
        {
            UpdateOffersState();
            _updateTimer?.Reset();
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }

        public void Init(OfferServiceData data, LoadContext context)
        {
            _data = data;
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            
            Init(saver.Unmarshal(data, new OfferServiceData()), context);
        }

        private void OnDataLoadFinished(LoadContext context)
        {
            _isInited = true;

            OffersStateChanged.Invoke();
            Inited.Invoke();
        }
    }
}
