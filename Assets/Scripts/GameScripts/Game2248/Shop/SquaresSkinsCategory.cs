using System.Collections.Generic;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using Shop.AnalyticsSignals;
using UnityEngine;
using Zenject;

namespace GameScripts.Game2248.Shop
{
    public class SquaresSkinsCategory : SquaresSkinsCategoryBase
    {
        [SerializeField] private Transform skinsRoot;
        [SerializeField] private SquaresSkinCell skinCellPrefab;
        [SerializeField] private List<SquaresSkinCell> cells = new();
        [SerializeField] private SquaresSkinsManager skinsManager;

        private GridManager _gridManager;
        private GameStatService _gameStatService;
        private SignalBus _signalBus;
        private WindowManager _windowManager;

        [Inject]
        private void Construct(GameStatService gameStatService, GridManager gridManager, SignalBus signalBus, WindowManager windowManager)
        {
            _gameStatService = gameStatService;
            _gridManager = gridManager;
            _signalBus = signalBus;
            _windowManager = windowManager;
            InitSkinCells();
        }

        public void CreateSkinCells()
        {
            var oldCells = skinsRoot.GetComponentsInChildren<SquaresSkinCell>();
            foreach (var cell in oldCells)
            {
                DestroyImmediate(cell.gameObject);
            }
            cells.Clear();
            for (int i = 0; i < skinsManager.Skins.Count; i++)
            {
                var skinCell = Instantiate(skinCellPrefab, skinsRoot);
                cells.Add(skinCell);
            }
        }
        
        private void InitSkinCells()
        {
            var index = 0;
            foreach (var cell in cells)
            {
                if(index < skinsManager.Skins.Count)
                {
                    var skin = skinsManager.Skins[index];
                    cell.Init(index, skin, skinsManager.GetRarity(skin), OnCellCLick);
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
                index++;
            }
            UpdateUi();
        }
        
        private void OnCellCLick(ESquareSkin skin)
        {
            if (_gridManager.OpenedSkins.Contains(skin))
            {
                SelectSkin(skin);
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.Units);
            }
        }

        private void UnlockSkin(ESquareSkin skin)
        {
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, skinsManager.GetElementByEnum(skin).OpenCost))
            {
                _signalBus.Fire(new ShopPurchaseSignal(skinsManager.GetElementByEnum(skin).OpenCost, $"Skin {skin}"));
                _gridManager.OpenSkin(skin);
                SelectSkin(skin);
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
            }
        }

        public override void UpdateUi()
        {
            foreach (var cell in cells)
            {
                if (_gridManager.OpenedSkins.Contains(cell.Skin))
                    cell.SetOpened();
                
                cell.Select(cell.Skin == _gridManager.CurrentSkin);
            }
        }

        private void SelectSkin(ESquareSkin skin)
        {
            _gridManager.SetSkin(skin);
            UpdateUi();
        }
    }
}