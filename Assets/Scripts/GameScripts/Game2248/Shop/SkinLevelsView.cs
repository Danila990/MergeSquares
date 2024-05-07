using System;
using Core.Localization;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using JetBrains.Annotations;
using Mono.CSharp;
using TMPro;
using UnityEngine;
using Utils;
using Zenject;

namespace GameScripts.Game2248.Shop
{
    public class SkinLevelsView : MonoBehaviour
    {
        [SerializeField] private EGameStatType levelType;
        [SerializeField] private EGameStatType experienceType;
        [SerializeField] private SlicedFilledImage fill;
        [SerializeField] private TextMeshProUGUI progress;
        [SerializeField] private LocalizeUi level;
        [SerializeField] private LocalizeUi summonSmall;
        [SerializeField] private LocalizeUi summonLarge;
        [SerializeField] private TextMeshProUGUI smallCost;
        [SerializeField] private TextMeshProUGUI largeCost;
        [Space]
        [SerializeField] private int smallCostAmount;
        [SerializeField] private int largeCostAmount;
        [SerializeField] private int summonSmallAmount;
        [SerializeField] private int summonLargeAmount;
        [SerializeField] private string currentNamespace;

        private GameStatService _gameStatService;
        private GameStatLeveled _gameStatLeveled;
        private WindowManager _windowManager;
        
        [Inject]
        public void Construct(GameStatService gameStatService, GameStatLeveled gameStatLeveled, WindowManager windowManager)
        {
            _gameStatService = gameStatService;
            _gameStatLeveled = gameStatLeveled;
            _windowManager = windowManager;
            _gameStatLeveled.Changed += OnChanged;
        }

        private void Start()
        {
            UpdateView();
        }

        private void OnDestroy()
        {
            _gameStatLeveled.Changed -= OnChanged;
        }
        
        [UsedImplicitly]
        public void OnSummonClick(bool isSmall)
        {
            if (_windowManager.TryGetWindow<SummonSkinsPopup>(EPopupType.SummonPopup.ToString(), out var summonSkinsPopup))
            {
                if (!summonSkinsPopup.CanSummon())
                {
                    return;
                }
            }
            var cost = isSmall ? smallCostAmount : largeCostAmount;
            var amount = isSmall ? summonSmallAmount : summonLargeAmount;
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, cost))
            {
                _gameStatService.TryIncWithAnim(experienceType, amount);
                var summonParams = new SummonSkinsPopupParams{Count = amount};
                _windowManager.ShowWindow(EPopupType.SummonPopup.ToString(), new[] { summonParams });
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
            }
        }
        
        [UsedImplicitly]
        public void OnSummonClickFromWindow(bool isSmall)
        {
            if (_windowManager.TryGetWindow<SummonSkinsPopup>(EPopupType.SummonPopup.ToString(), out var summonSkinsPopup))
            {
                if(summonSkinsPopup.CanSummon())
                {
                    var cost = isSmall ? smallCostAmount : largeCostAmount;
                    if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, cost))
                    {
                        _gameStatService.TryIncWithAnim(experienceType,
                            isSmall ? summonSmallAmount : summonLargeAmount);
                        summonSkinsPopup.Summon(isSmall ? summonSmallAmount : summonLargeAmount);
                    }
                    else
                    {
                        SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
                    }
                }
            }
        }
        
        private void OnChanged(GameStatLeveledData data)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            level.UpdateArgs(new []{(_gameStatLeveled.GetLevel(levelType) + 1).ToString()});
            summonSmall.UpdateArgs(new []{summonSmallAmount.ToString()});
            summonLarge.UpdateArgs(new []{summonLargeAmount.ToString()});
            smallCost.text = smallCostAmount.ToString();
            largeCost.text = largeCostAmount.ToString();
            var current = _gameStatLeveled.GetLevelCurrent(levelType);
            var max = _gameStatLeveled.GetLevelMax(levelType);
            progress.text = $"{current}/{max}";
            fill.fillAmount = (float) current / max;
        }
    }
}