using GameStats;
using Shop.AnalyticsSignals;
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class ClueCell : MonoBehaviour
    {
        [SerializeField] private int cost;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private EGameStatType gameStatType;

        private GameStatService _gameStatService;
        private WindowManager _windowManager;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(GameStatService gameStatService, WindowManager windowManager, SignalBus signalBus)
        {
            _gameStatService = gameStatService;
            _windowManager = windowManager;
            _signalBus = signalBus;
            costText.text = cost.ToString();
        }

        public void OnClick()
        {
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, cost))
            {
                _gameStatService.TryInc(gameStatType, 1);
                _signalBus.Fire(new ShopPurchaseSignal(cost, $"Clue: {gameStatType}"));
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
            }
        }
    }
}
