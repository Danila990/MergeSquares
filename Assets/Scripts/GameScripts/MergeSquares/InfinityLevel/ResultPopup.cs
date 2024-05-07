using System;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using DG.Tweening;
using GameScripts.MergeSquares.Shop;
using GameStats;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class InfinityResultParams
    {
        public InfinityGridModel model;
        public Action ClosePopup;
        public bool isBestScore;
        public int score;
    }
    
    public class ResultPopup : GenericPopupContent
    {
        [SerializeField] private TextMeshProUGUI scores;
        [SerializeField] private GameObject bestText;
        [SerializeField] private TextMeshProUGUI restartCostText;
        [SerializeField] protected int restartCost = 80;
        [SerializeField] private TextMeshProUGUI coins;
        [SerializeField] private List<GameObject> notActiveZero;
        [SerializeField] private List<GameObject> activeZero;

        private InfinityResultParams resultParams;
        
        private GameStatService _gameStatService;
        private GridManager _gridManager;
        
        [Inject]
        public void Construct(WindowManager windowManager, GameStatService gameStatService, GridManager gridManager)
        {
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _gridManager = gridManager;
        }

        public override string GetWindowId() => "ResultPopup";

        public override void Init(object dataToInit, PopupBase popupBase)
        {
            if (dataToInit is InfinityResultParams resultParams)
            {
                this.resultParams = resultParams;
                if (resultParams.score > 0)
                {
                    DOTween.To(() => resultParams.score, newScores =>
                    {
                        scores.text = newScores.ToString();
                    }, 0, 2f);
                    bestText.SetActive(resultParams.isBestScore);
                    var bonusScore = resultParams.score / 10;
                    _gameStatService.TryIncWithAnim(EGameStatType.Soft, bonusScore);
                    coins.text = $"{bonusScore}";
                    foreach (var go in activeZero)
                    {
                        go.SetActive(false);
                    }
                }
                else
                {
                    bestText.SetActive(false);
                    foreach (var go in notActiveZero)
                    {
                        go.SetActive(false);
                    }
                    foreach (var go in activeZero)
                    {
                        go.SetActive(true);
                    }
                }
                restartCostText.text = restartCost.ToString();
                Debug.Log($"OK!!");
            }
            else
            {
                Debug.Log($"ERROR!!");
                popupBase.CloseWindow();
            }
        }

        public void Restart()
        {
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, restartCost))
            {
                _gridManager.InfinityGridData.currentModel = resultParams.model;
                _gridManager.InfinityGridData.nextValue = resultParams.model.model.nextValues.First().value;
                _windowManager.ShowWindow(EPopupType.RatingLevel.ToString());
                _windowManager.CloseAll(GetWindowId());
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
                _windowManager.CloseAll(GetWindowId());
            }
        }

        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {
            
        }
    }
}