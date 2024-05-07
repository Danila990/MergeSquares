using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Anchors;
using Core.Windows;
using DG.Tweening;
using GameScripts.MergeSquares.Models;
using GameScripts.MergeSquares.Shop;
using GameStats;
using JetBrains.Annotations;
using Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Instructions;
using Zenject;
using EShopMarkers = GameScripts.MergeSquares.Shop.EShopMarkers;

namespace GameScripts.MergeSquares.InfinityLevel
{
    [Serializable]
    public class InfinityGridModel
    {
        public GridModel model;
        public int bestScore;
        public int retryCount;
        public bool isExternal = false;
    }
    [Serializable]
    public class InfinityGridData
    {
        public List<CellData> cells = new();
        public int nextValue;
        public InfinityGridModel currentModel;
        public InfinityGridModel externalModel;
        public List<InfinityGridModel> previousModels = new();
        public List<InfinityGridModel> externalModels = new();
        public GridTaskData task;
    }
    public class InfinityLevelGrid : GridView
    {
        [SerializeField] protected PopupBase popupBase;
        [SerializeField] protected Button stopRemoveWallsButton;
        // [SerializeField] private Button stopChangeButton;
        [SerializeField] protected ResultPopup resultPopupContentPrefab;
        [SerializeField] protected UnitView nextValueView;
        [SerializeField] protected List<Button> lockedButtons;
        [SerializeField] protected Anchor changeButton;
        [SerializeField] protected TextMeshProUGUI restartCostText;
        [SerializeField] protected int restartCost = 80;
        
        protected bool _isNewBestScore;
        protected InfinityGridData _data;
        
        [Inject]
        public void ChildConstruct()
        {
            StateChanged += OnGridStateChanged;
            NewValueCreated += OnNewValueCreated;
            RemoveWallsActivated += OnStartRemoveWalls;
            SquareChangeActivated += OnStartChange;
            Started += OnStarted;
            _nextValueView = nextValueView;
        }
        
        protected void OnStarted()
        {
            _data = _gridManager.InfinityGridData ?? _gridManager.InitInfinityLevel();
            SaveGrid();
            Init(_data.currentModel.model, _data.cells);
            if (_data.nextValue >= 0)
            {
                _nextValue = _data.nextValue;
                _nextValueView.Init(_nextValue);
                _nextValueView.Animator.AnimateScalePingPong();
            }
            else
                ChooseNextValue();
            _isActive = true;
            restartCostText.text = restartCost.ToString();
        }

        protected void OnDestroy()
        {
            StateChanged -= OnGridStateChanged;
            NewValueCreated -= OnNewValueCreated;
            RemoveWallsActivated -= OnStartRemoveWalls;
            SquareChangeActivated -= OnStartChange;
            Started -= OnStarted;
        }
        
        public void OnStartChange(bool status)
        {
            if (status)
            {
                _windowManager.TryShowAndGetWindow(EPopupType.Overlay.ToString(), out PopupBase basePopup);
                var sortingOrder = popupBase.Canvas.sortingOrder;
                basePopup.Canvas.sortingOrder = sortingOrder + 1;
                changeButton.SetSorting(sortingOrder + 2);
            }
            else
            {
                changeButton.ResetSorting();
                _windowManager.CloseAll(EPopupType.Overlay.ToString());
            }
            // stopChangeButton.gameObject.SetActive(status);
        }
        
        [UsedImplicitly]
        public void TestGenericWinOpen()
        {
            ShowEndLevelPopup();
        }
        
        public void SaveGrid(bool force = true)
        {
            _gridManager.SaveInfinityGrid(_data, force);
        }

        public override IEnumerator ChangePosition()
        {
            yield return base.ChangePosition();
            _data.cells.Clear();
            foreach (var dcell in Cells)
            {
                if (!dcell.Value.IsFree)
                {
                    _data.cells.Add(new CellData()
                    {
                        position = dcell.Key,
                        value = dcell.Value.view.Value,
                    });
                }
            }
            StateChanged.Invoke();
            SaveGrid();
            LockButtons(false);
        }

        public void FailDebug()
        {
            FinishGrid();
        }

        public void RestartLevel()
        {
            if (_gameStatService.TryDecWithAnim(EGameStatType.Soft, restartCost))
            {
                ClearGrid(() => {});
                Init(_gridModel, new List<CellData>());
                SaveGridResult(true);
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.InApps);
                popupBase.CloseWindow();
            }
        }
        
        public override void StartChange()
        {
            if (AvailableForClick && CanChanged())
            {
                if (_gameStatService.GetStat(EGameStatType.SquareChanges).GetValue() > 0)
                {
                    _isPositionChanging = true;
                    SetOverlays(_isPositionChanging, cell => !cell.IsWall && !cell.IsFree);
                    SquareChangeActivated.Invoke(_isPositionChanging);
                }
                else
                {
                    SquaresShop.OpenSection(_windowManager, EShopMarkers.Clues);
                    popupBase.CloseWindow();
                }
            }
        }

        protected override void SetOverlays(bool active, Func<Cell, bool> checkType)
        {
            foreach (var cell in Cells)
            {
                if(checkType(cell.Value))
                {
                    if (active)
                    {
                        cell.Value.anchor.SetSorting(popupBase.Canvas.sortingOrder + 2);
                    }
                    else
                    {
                        cell.Value.anchor.ResetSorting();
                    }
                }
            }
        }

        protected override void ChooseNextValue()
        {
            if (_nextValues.TryWeightRandom(value => value.chance, out NextValue next))
            {
                _nextValue = next.value;
            }
            else
            {
                _nextValue = _nextValues[0].value;
            }
            _nextValueView.Init(_nextValue);
            _nextValueView.Animator.AnimateScalePingPong();
            _data.nextValue = _nextValue;
            SaveGrid();
        }

        protected override void OnCellClick(Cell cell)
        {
            if (cell.IsFree)
                LockButtons(true);
            base.OnCellClick(cell);
        }

        protected override IEnumerator DoCellClick(Cell cell)
        {
            var flyingView = Instantiate(viewPrefab, _nextValueView.transform);
            flyingView.Init(_nextValueView.Value);
            ChooseNextValue();
            yield return new WaitForCallback( callback => { flyingView.Animator.AnimateFlying(cell, callback); } );
            cell.SetFull(flyingView);
            cell.view.Animator.AnimateScalePingPong();

            yield return TryMergeCellsFrom(cell);
            yield return CheckMergeAfterMerge();

            if (AvailableForClick)
            {
                _gridStates.PutCurrentState(CreateGridState());
                _data.cells.Clear();
                foreach (var dcell in Cells)
                {
                    if (!dcell.Value.IsFree)
                    {
                        _data.cells.Add(new CellData()
                        {
                            position = dcell.Key,
                            value = dcell.Value.view.Value,
                        });
                    }
                }
                SaveGrid();
                StateChanged?.Invoke();
            }
            LockButtons(false);
            _canClick = true;
        }

        protected virtual void SaveGridResult(bool ratingUnlocked)
        {
            InfinityGridModel model = null;
            if (_data.currentModel.isExternal)
            {
                model = _data.externalModel;
            }
            else
            {       
                model = _data.previousModels.Find(model => model.model.id == _data.currentModel.model.id);
                if (model == null)
                {
                    model = new InfinityGridModel()
                    {
                        model = _data.currentModel.model
                    };
                    model.isExternal = false;
                    _data.previousModels.Add(model);
                }
            }

            var points = _gameStatService.GetStatValue(EGameStatType.RatingLevelScores);
            if (model.bestScore < points)
            {
                model.bestScore = points;
            }
            
            // if (_data.currentModel.isExternal)
            // {
            //     _gridManager.SaveExternalGridScores(points, );
            // }

            model.retryCount++;
            _gridManager.SetRatingUnlocked(ratingUnlocked);
            SaveGrid();
        }
        
        protected void FinishGrid()
        {
            _isActive = false;
            // _signalBus.Fire(new LevelStatusSignal(LevelStatus.Passed, _currentLevel));
            // Inc retry counter
            // Check best score;
            // var currentPoints = _gameStatService.GetStatValue(EGameStatType.Points);
            // Show popup if its the best (may be with gacha)
            // Save
            SaveGridResult(false);
            ShowEndLevelPopup();
            popupBase.CloseWindow();
        }

        protected void ShowEndLevelPopup(Action callback = null)
        {
            var model = _data.currentModel;
            var resultParams = new InfinityResultParams
            {
                model = model,
                isBestScore = _isNewBestScore,
                ClosePopup =
                    () =>
                    {
                        callback?.Invoke();
                    },
                score = _gameStatService.GetStatValue(EGameStatType.RatingLevelScores),
            };
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = resultPopupContentPrefab,
                dataToInitIt = resultParams,
                isDimmingActive = false,
            };
            _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            _gridManager.InfinityGridData.currentModel = null;
            _gridManager.InfinityGridData.cells.Clear();
            _gameStatService.TrySet(EGameStatType.RatingLevelScores, 0);
            SaveGrid();
        }
        
        protected void OnGridStateChanged()
        {
            if (IsFull())
            {
                FinishGrid();
            }
        }

        protected void LockButtons(bool isLock)
        {
            foreach (var b in lockedButtons)
            {
                b.interactable = !isLock;
            }
        }
        
        protected virtual void OnNewValueCreated(int value)
        {
            _gameStatService.TryIncWithAnim(EGameStatType.RatingLevelScores, value);
            _data.task.currentValue = value;
            StateChanged.Invoke();
            SaveGrid();
        }
        
        protected void OnStartRemoveWalls(bool isRemoving)
        {
            if (isRemoving)
            {
                var overlay = _windowManager.ShowWindow(EPopupType.Overlay.ToString(), isUnique: true);
                overlay.Canvas.sortingOrder = _gridManager.WallOverlaySortingOrder;
                overlay.Canvas.sortingLayerName = "Default";
            }
            else
            {
                _windowManager.CloseAll(EPopupType.Overlay.ToString());
            }
            stopRemoveWallsButton.gameObject.SetActive(isRemoving);
        }
    }
}