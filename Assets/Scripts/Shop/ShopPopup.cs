using Core.Windows;
using System;
using UnityEngine;
using Zenject;

namespace Shop
{
    public class ShopPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private ShopScrollRectMarkers scrollRectMarkers;
        
        public Action Inited;
        
        private WindowManager _windowManager;
        
        [Inject]
        public void Construct(WindowManager windowManager)
        {
            popupBase.Inited += Init;
        
            _windowManager = windowManager;
        }
        
        private void OnDestroy()
        {
            popupBase.Inited -= Init;
        }
        
        public void OnMarketTabButton()
        {
            scrollRectMarkers.ScrollToMarker(EShopMarkers.Market);
        }
        
        public void OnResourcesTabButton()
        {
            scrollRectMarkers.ScrollToMarker(EShopMarkers.Resources);
        }
        
        public void CallOfferWindow(int purchasedItemValue)
        {
            // TODO: If we have a window with an offer to buy 'hard' currency, call this window at this point.
            // var initArgs = new SoftPopupInitArgs
            // {
            //     popupForShop = true,
            //     purchasedItemValue = purchasedItemValue
            // };
            // OfferSoftPopup.CreateWithArgs(initArgs, _windowManager);
        }
        
        private void Init()
        {
            Inited?.Invoke();
        }
    }
}
