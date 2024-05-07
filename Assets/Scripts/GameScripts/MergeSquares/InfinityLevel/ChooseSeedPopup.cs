using System;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class ChooseSeedParams
    {
        public List<InfinityGridModel> Models;
        public int generateCost = 100;
    }

    public class ChooseSeedPopup : GenericPopupContent
    {
        [SerializeField] private SeedPanel seedPanelPrefab;
        [SerializeField] private SeedPanel ratingPanelPrefab;
        [SerializeField] private Transform timeRoot;
        [SerializeField] private Transform generateddRoot;
        [SerializeField] private GameObject generateButton;
        [SerializeField] private GameObject maxText;
        [SerializeField] private TextMeshProUGUI costText;

        private ChooseSeedParams _seedParams;
        private List<SeedPanel> timePanels = new List<SeedPanel>();

        private GameStatService _gameStatService;
        private GridManager _gridManager;
        private RatingService _ratingService;
        
        [Inject]
        public void Construct(WindowManager windowManager, GameStatService gameStatService, GridManager gridManager, RatingService ratingService)
        {
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _gridManager = gridManager;
            _ratingService = ratingService;
        }

        public override string GetWindowId() => "ChooseSeedPopup";

        public override void Init(object dataToInit, PopupBase popupBase)
        {
            // _gridManager.GenerateInfinityGrid(true, _ratingService.CreateMonthSeed());
            // externalSeedPanel.Init(_gridManager.GetExternalGrid(), OnPanelClick, Delete);

            DrawTimePanels();
            
            if (dataToInit is ChooseSeedParams chooseSeedParams)
            {
                for (int i = 0; i < chooseSeedParams.Models.Count;)
                {
                    var model = chooseSeedParams.Models[i];
                    if (model.isExternal)
                    {
                        chooseSeedParams.Models.Remove(model);
                        continue;
                    }
                    var panel = Instantiate(seedPanelPrefab, generateddRoot);
                    panel.Anchor.Id += i;
                    panel.Init(model, OnPanelClick, Delete);
                    i++;
                }
                _seedParams = chooseSeedParams;
                SetGenerateButton();
                costText.text = chooseSeedParams.generateCost.ToString();
                _ratingService.WeekReset += DrawTimePanels;
                _ratingService.MonthReset += DrawTimePanels;
            }
            else
            {
                popupBase.CloseWindow();
            }
        }

        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
        }

        public void GenerateNew()
        {
            if (_gameStatService.TryDec(EGameStatType.Soft, _seedParams.generateCost))
            {
                var model = _gridManager.GenerateInfinityGrid();
                _gridManager.SaveGridModel(model);
                var panel = Instantiate(seedPanelPrefab, generateddRoot);
                panel.Init(model, OnPanelClick, Delete);
                SetGenerateButton();
            }
            else
            {
                OnClickClose();
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
            }
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {
            _ratingService.WeekReset -= DrawTimePanels;
            _ratingService.MonthReset -= DrawTimePanels;
        }

        private void DrawTimePanels()
        {
            foreach (var ratingP in timePanels)
            {
                Destroy(ratingP.gameObject);
            }
            timePanels.Clear();

            foreach (var rewardData in _ratingService.Data.rewardsInProgress)
            {
                if(rewardData.id == string.Empty)
                    continue;
                var ratingP = Instantiate(ratingPanelPrefab, timeRoot);
                ratingP.Init(rewardData, OnPanelClick, Delete);
                ratingP.SetIsToClaim(false);
                timePanels.Add(ratingP);
            }
            foreach (var rewardData in _ratingService.Data.rewardsToClaim)
            {
                if(rewardData.id == string.Empty)
                    continue;
                var ratingP = Instantiate(ratingPanelPrefab, timeRoot);
                ratingP.Init(rewardData, OnPanelClick, Delete);
                ratingP.SetIsToClaim(true);
                timePanels.Add(ratingP);
            }
        }

        private void OnPanelClick(InfinityGridModel model)
        {
            if (_gameStatService.TryDec(EGameStatType.Ticket, 1))
            {
                _gridManager.InfinityGridData.currentModel = model;
                _gridManager.InfinityGridData.nextValue = model.model.nextValues.First().value;
                _windowManager.ShowWindow(EPopupType.RatingLevel.ToString());
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.Tickets);
            }
            _windowManager.CloseAll(GetWindowId());
        }

        private void SetGenerateButton()
        {
            var isActive = _seedParams.Models.Count < 5;
            generateButton.SetActive(isActive);
            maxText.SetActive(!isActive);
        }

        private void Delete(InfinityGridModel model)
        {
            _gridManager.DeleteInfinityLevel(model);
            SetGenerateButton();
        }
    }
}
