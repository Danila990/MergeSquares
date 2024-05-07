using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.Shop;
using LeadboardScores;
using UnityEngine;
using Zenject;

namespace Core.Windows
{
    public class SkinsPopup : GenericPopupContent
    {
        [SerializeField] private List<SquaresSkinCell> cells = new();
        [SerializeField] private SquaresSkinsManager skinsManager;

        public override string GetWindowId() => "SkinsPopup";

        private GridManager _gridManager;

        [Inject]
        private void Construct(WindowManager windowManager, GridManager gridManager)
        {
            _windowManager = windowManager;
            _gridManager = gridManager;
            InitSkinCells();
        }
        
        public override void Init(object dataToInit, PopupBase popupBase)
        {
            
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {
            
        }
        
        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
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
        
        private void UpdateUi()
        {
            foreach (var cell in cells)
            {
                if (_gridManager.OpenedSkins.Find(s => s.skinType == cell.Skin) != null)
                    cell.SetOpened();
                cell.Select(cell.Skin == _gridManager.CurrentSkin);
            }
        }
        
        private void OnCellCLick(ESquareSkin skin)
        {
            if (_gridManager.OpenedSkins.Find(s => s.skinType == skin) != null)
            {
                SelectSkin(skin);
            }
        }
        
        private void SelectSkin(ESquareSkin skin)
        {
            _gridManager.SetSkin(skin);
            UpdateUi();
        }
    }
}