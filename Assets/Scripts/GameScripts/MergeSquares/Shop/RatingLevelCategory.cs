using System;
using System.Collections;
using System.Collections.Generic;
using Core.SaveLoad;
using Core.Windows;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.Shop;
using GameScripts.MergeSquares.Tasks;
using GameStats;
using LeadboardScores;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class RatingLevelCategory : MonoBehaviour
    {
        [SerializeField] private GameObject unlockButton;
        [SerializeField] private ChooseSeedPopup chooseSeedPrefab;
        [SerializeField] private GameObject startButton;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private int cost = 100;
        // [SerializeField] private TextMeshProUGUI weekPositionText;
        // [SerializeField] private TextMeshProUGUI monthPositionText;
        // [SerializeField] private TextMeshProUGUI weekScoresText;
        // [SerializeField] private TextMeshProUGUI monthScoresText;

        private WindowManager _windowManager;
        private GameStatService _gameStatService;
        private GridManager _gridManager;

        [Inject]
        public void Construct(WindowManager windowManager, GameStatService gameStatService, GridManager gridManager)
        {
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _gridManager = gridManager;
            
            _gameStatService.StatChanged += OnStatChanged;
        }

        private void OnDestroy()
        {
            _gameStatService.StatChanged -= OnStatChanged;
        }

        public void Start()
        {
            SetButtons();
            costText.text = cost.ToString();
            // SetLeadboard();
        }

        public void OnClickUnlock()
        {
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, 100))
            {
                _gridManager.SetRatingUnlocked(true);
                SetButtons();
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
            }
        }

        // public void ShowAllRewardsMonth()
        // {
        //     ShowAllRewards(true);
        // }
        //
        // public void ShowAllRewardsWeek()
        // {
        //     ShowAllRewards(false);
        // }

        public void OnClickStart()
        {
            if (_gridManager.InfinityGridData.currentModel != null && _gridManager.InfinityGridData.currentModel.model.size.x > 0)
            {
                _windowManager.ShowWindow(EPopupType.RatingLevel.ToString());
                return;
            }
            
            var data = new ChooseSeedParams
            {
                Models = _gridManager.InfinityGridData.previousModels,
            };
            
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = chooseSeedPrefab,
                dataToInitIt = data
            };
            
            _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
        }

        // private void ShowAllRewards(bool isMonth)
        // {
        //     var pos = _leadboardScoresService.GetCurrentPosition(isMonth);
        //     var allRewardParams = new AllRewardsParams()
        //     {
        //         position = pos,
        //         GetRewards = _leadboardScoresService.CalcRewards,
        //     };
        //     var popupParams = new GenericPopupParams
        //     {
        //         prefabToCreate = allRewardsPopupPrefab,
        //         dataToInitIt = allRewardParams,
        //     };
        //     var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
        //     window.Canvas.sortingOrder = 320;
        // }

        // private void SetLeadboard()
        // {
        //     weekPositionText.text = _leadboardScoresService.GetCurrentPosition(false).ToString();
        //     monthPositionText.text = _leadboardScoresService.GetCurrentPosition(true).ToString();
        //     weekScoresText.text = _leadboardScoresService.GetCurrentScores(false).ToString();
        //     monthScoresText.text = _leadboardScoresService.GetCurrentScores(true).ToString();
        // }
        
        private void OnStatChanged(EGameStatType type, int value)
        {
            if (type == EGameStatType.LeadboardScores)
            {
                SetButtons();
            }
        }

        private void SetButtons()
        {
            // unlockButton.SetActive(!_gridManager.RatingUnlocked);
            // startButton.SetActive(_gridManager.RatingUnlocked);
        }
    }
}
