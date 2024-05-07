using Core.Windows;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class SquaresShopButton : MonoBehaviour
    {
        private WindowManager _windowManager;

        [Inject]
        public void Construct(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void OnClick()
        {
            SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
        }
    }
}