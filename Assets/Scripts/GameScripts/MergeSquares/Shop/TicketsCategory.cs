using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class TicketsCategory : MonoBehaviour
    {
        [SerializeField] private int cost = 100;

        private GameStatService _gameStatService;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(GameStatService gameStatService, WindowManager windowManager)
        {
            _gameStatService = gameStatService;
            _windowManager = windowManager;
        }

        public void OnClick(int count)
        {
            var allCost = cost * count;
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, allCost))
            {
                _gameStatService.TryIncWithAnim(EGameStatType.Ticket, count);
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.Coins);
            }
        }
    }
}
