using System.Collections.Generic;
using Core.Windows;
using GameStats;
using LeadboardScores;
using Popups;
using Shop;
using Shop.AnalyticsSignals;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public enum ESquareSkin
    {
        external = -1,
        baseSprite = 0,
        bubbleSprite = 1,
        candySprite = 2,
        glassSprite = 3,
        colorFrame = 5,
        goldFrame = 6,
        leavesFrame = 7,
        woodFrame = 8,
        silverFrame = 9,
        skySprite = 10,
        softSprite = 11,
        woodSprite = 12,
    }
    public class SquaresSkinsCategory : SquaresSkinsCategoryBase
    {
        [SerializeField] private Transform skinsRoot;
        [SerializeField] private SquaresSkinCell skinCellPrefab;
        [SerializeField] private List<SquaresSkinCell> cells = new();
        [SerializeField] private SquaresSkinsManager skinsManager;
        [SerializeField] private SkinsCountByRare skinsCountPopup;
        
        private GridManager _gridManager;
        private GameStatService _gameStatService;
        private SignalBus _signalBus;
        private WindowManager _windowManager;

        [Inject]
        private void Construct(GameStatService gameStatService, GridManager gridManager, SignalBus signalBus, WindowManager windowManager, LeadboardScoresService leadboardScoresService)
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
        
        public override void UpdateUi()
        {
            foreach (var cell in cells)
            {
                if (_gridManager.OpenedSkins.Find(s => s.skinType == cell.Skin) != null)
                    cell.SetOpened();
                cell.Select(cell.Skin == _gridManager.CurrentSkin);
            }
        }

        public void ShowSkinsRareCount()
        {
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = skinsCountPopup,
            };
            var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            window.Canvas.sortingOrder = 320;
        }
        
        private void InitSkinCells()
        {
            var index = 0;
            foreach (var cell in cells)
            {
                if(index < skinsManager.Skins.Count)
                {
                    var skin = skinsManager.Skins[index];
                    var skinData = _gridManager.OpenedSkins.Find(s => s.skinType == skin.Skin);
                    var count = -1;
                    if (skinData != null)
                    {
                        count = skinData.count;
                    }
                    cell.Init(index, skin, skinsManager.GetRarity(skin), count, OnCellCLick);
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
            // if (!_gridManager.OpenedSkins.Contains(skin))
            // {
            //     UnlockSkin(skin);
            // }
            // else
            // {
            //     SelectSkin(skin);
            // }
            if (_gridManager.OpenedSkins.Find(s => s.skinType == skin) != null)
            {
                SelectSkin(skin);
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

        private void SelectSkin(ESquareSkin skin)
        {
            _gridManager.SetSkin(skin);
            UpdateUi();
        }
    }
}