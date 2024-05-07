using System.Collections.Generic;
using Offers;
using Offers.Model;
using UnityEngine;
using Utils;
using Zenject;

namespace GameScripts.MergeSquares.Offers
{
    public class SoftOfferService : MonoBehaviour
    {
        [SerializeField] private List<OfferModel> offerModels = new();
        [SerializeField] private bool oneAtTime;
        [SerializeField] private float chanceToAddAll = 0.2f;

        private OfferService _offerService;

        [Inject]
        private void Construct(
            OfferService offerService
            )
        {
            _offerService = offerService;
            _offerService.Inited += OnOfferServiceInited;
        }

        protected virtual void OnDestroy()
        {
            _offerService.Inited -= OnOfferServiceInited;
        }

        private void OnOfferServiceInited()
        {
            TryShowOffer();
        }

        private bool TryShowOffer()
        {
            foreach (var offerModel in offerModels)
            {
                var active = _offerService.GetActiveOffersByModel(offerModel);
                if (active.Count > 0)
                {
                    return false;
                }
            }
            
            if(oneAtTime && Random.Range(0f, 1f) > chanceToAddAll)
            {
                var model = offerModels.GetRandom();
                return TryShowOffer(model);
            }
            
            var any = false;
            foreach (var offerModel in offerModels)
            {
                var res = TryShowOffer(offerModel);
                if (res)
                {
                    any = true;
                }
            }

            return any;
        }

        private bool TryShowOffer(OfferModel offerModel)
        {
            var model = offerModel;
            if (!_offerService.IsOfferCreationAvailable(model))
            {
                return false;
            }

            var createdOffer = _offerService.CreateOffer(model);
            return createdOffer != null;
        }
    }
}
