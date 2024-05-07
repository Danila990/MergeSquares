using System.Collections;
using System.Collections.Generic;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.InfinityLevel;
using GameStats;
using LeadboardScores;
using UnityEngine;
using Utils;
using Zenject;

namespace Core.Windows
{
    public class OpenChooseSeed : MonoBehaviour
    {
        [SerializeField] private ChooseSeedPopup chooseSeedPrefab;
        
        private WindowManager _windowManager;
        private GameStatService _gameStatService;
        private GridManager _gridManager;
        [SortingLayer] [SerializeField] private string layer = "Default";
        [SerializeField] private int sortingOrder = 200;
        
        [Inject]
        public void Construct(WindowManager windowManager, GameStatService gameStatService, GridManager gridManager)
        {
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _gridManager = gridManager;
        }
        
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
            
            var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            window.Canvas.sortingLayerName = layer;
            window.Canvas.sortingOrder = sortingOrder;
        }
    }
}