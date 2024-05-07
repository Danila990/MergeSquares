using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.Shop;
using GameStats;
using LeadboardScores;
using TMPro;
using UnityEngine;
using Zenject;

namespace Popups
{
    public class SkinsCountByRare : GenericPopupContent
    {
        [SerializeField] private UnitView unitViewPrefab;
        [SerializeField] private SquaresSkinsManager skinsManager;
        [SerializeField] private Transform root;
        [SerializeField] private TextMeshProUGUI skinsMultiplier;

        public override string GetWindowId() => "skinsCountByRare";
        
        private GridManager _gridManager;
        private LeadboardScoresService _leadboardScoresService;

        [Inject]
        private void Construct(WindowManager windowManager, GridManager gridManager, LeadboardScoresService leadboardScoresService)
        {
            _windowManager = windowManager;
            _gridManager = gridManager;
            _leadboardScoresService = leadboardScoresService;
            skinsMultiplier.text = $": {_leadboardScoresService.GetMultiplier()}";
        }

        public override void Init(object dataToInit, PopupBase popupBase)
        {
            foreach (var skin in skinsManager.Skins)
            {
                if (_gridManager.OpenedSkins.Find(s => s.skinType == skin.Skin) == null)
                {
                    var unitView = Instantiate(unitViewPrefab, root);
                    unitView.gameObject.SetActive(true);
                    unitView.Init(2);
                    unitView.SetSkin(skin.Skin);
                    unitView.SetSecret(false);
                }
            }
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {
            
        }

        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
        }
    }
}