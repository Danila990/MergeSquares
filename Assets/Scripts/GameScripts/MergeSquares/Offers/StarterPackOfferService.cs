using Offers;
using Offers.Model;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.Offers
{
    public class StarterPackOfferService : MonoBehaviour
    {
        [SerializeField] private int offerShowLevel;
        [SerializeField] private OfferModel offerModel;

        private bool _popupShowed;
        
        private GridManager _gridManager;
        private OfferService _offerService;

        [Inject]
        private void Construct(
            GridManager gridManager,
            OfferService offerService
            ) 
        {
            _gridManager = gridManager;
            _offerService = offerService;

            _gridManager.OnStartLevel += HandleLevelChanged;
            _offerService.Inited += HandleOfferServiceInited;
        }

        protected virtual void OnDestroy()
        {
            _gridManager.OnStartLevel -= HandleLevelChanged;
            _offerService.Inited -= HandleOfferServiceInited;
        }

        private void HandleLevelChanged()
        {
            UpdateLevelState();
        }

        private void HandleOfferServiceInited()
        {
            UpdateLevelState();
        }

        private void UpdateLevelState()
        {
            var level = _gridManager.CurrentLevel;
            TryShowOffer(level);
        }

        private bool TryShowOffer(int level)
        {
            var model = offerModel;
            if (!_offerService.IsOfferCreationAvailable(model))
            {
                var cratedOffers = _offerService.GetActiveOffersByModel(model);
                if(cratedOffers.Count > 0 && !_popupShowed)
                {
                    _popupShowed = true;
                    _offerService.TryShowOfferPopup(cratedOffers[0]);
                }
                return false;
            }
            
            if(level % offerShowLevel != 0)
            {
                return false;
            }

            var createdOffer = _offerService.CreateOffer(model);
            _popupShowed = true;
            return createdOffer != null;
        }
    }
}
