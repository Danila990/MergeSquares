using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using Core.SaveLoad;
using Core.Windows;
using DG.Tweening;
using GameScripts.MergeSquares.Models;
using GameScripts.MergeSquares.Shop;
using GameScripts.MergeSquares.Tasks;
using GameStats;
using JetBrains.Annotations;
using LeadboardScores;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utils;
using Utils.Instructions;
using Zenject;
using TaskModel = GameScripts.MergeSquares.Models.TaskModel;

namespace GameScripts.MergeSquares
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] protected UnitView viewPrefab;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private FlexibleGridLayout gridLayout;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<NextValue> nextValues = new();
        [SerializeField] private float upgradeDelay;
        [SerializeField] private float cellCritProbability;
        [SerializeField] private int softForMaxMerge = 5;
        [SerializeField] private float animTimeout = 5;

        public Vector3 LocalPosition => canvasGroup.transform.localPosition;
        public Dictionary<Vector2Int, Cell> Cells => _cells;
        public TaskModel Task => _gridModel.taskModel;

        public Action<bool> RemoveWallsActivated = value => { };
        public Action<bool> SquareChangeActivated = value => { };
        public Action<int> NewValueCreated = value => { };
        public Action Merged = () => { };
        public Action StateChanged = () => { };
        public Action Started = () => { };
        public Action OnFreeCellClicked = () => { };

        public bool AvailableForClick => _isActive && !_gridManager.IsLocked;
        // only for debug
        public GridStates GridStates => _gridStates;

        private Dictionary<Vector2Int, Cell> _cells = new();
        private List<Cell> _cellsToCheck = new();
        protected List<NextValue> _nextValues = new();
        protected int _nextValue;
        protected bool _isActive = false;
        private bool _isWallRemoving = false;
        protected bool _isPositionChanging = false;
        protected bool _canClick = true;
        private List<Vector2Int> _directions = new() { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
        private List<Vector2Int> _upgradeDirections =
            new() { new(1, 0), new(0, 1), new(-1, 0), new(0, -1), new(-1, 1), new(1, 1), new(1, -1), new(-1, -1) };
        bool isMerged = false;

        protected UnitView _nextValueView;
        protected GameStatService _gameStatService;
        private SaveService _saveService;
        protected GridManager _gridManager;
        protected GridStates _gridStates;
        protected GridModel _gridModel;
        private TaskScoresView _taskScoresView;
        protected WindowManager _windowManager;
        private TaskService _taskService;
        private LeadboardScoresService _leadboardScoresService;

        private Cell _fromCell;
        private Cell _toCell;

        private Coroutine _mergeCorutine;

        [Inject]
        public void Construct(
            TaskScoresView taskScoresView,
            UnitView nextValueView,
            GameStatService gameStatService,
            GridManager gridManager,
            Camera worldCamera,
            SaveService saveService,
            WindowManager windowManager,
            TaskService taskService,
            LeadboardScoresService leadboardScoresService
        )
        {
            _nextValueView = nextValueView;
            _gameStatService = gameStatService;
            _gridManager = gridManager;
            canvas.worldCamera = worldCamera;
            _saveService = saveService;
            _taskScoresView = taskScoresView;
            _windowManager = windowManager;
            _taskService = taskService;
            _leadboardScoresService = leadboardScoresService;
        }

        private void Start()
        {
            _saveService.LoadFinished += OnLoadFinished;
            Started.Invoke();
        }

        public void Init(GridModel gridModel, List<CellData> cellsData = null)
        {
            _gridStates = new GridStates(_gameStatService);

            if (cellsData != null)
            {
                // create new model to not overwrite old
                var model = new GridModel()
                {
                    id = gridModel.id,
                    nextValues = gridModel.nextValues,
                    size = gridModel.size,
                    taskModel = gridModel.taskModel,
                    units = cellsData.Select(t => new UnitModel() { position = t.position, value = t.value }).ToList()
                };
                InitCells(model);
            }
            else
            {
                InitCells(gridModel);
            }
            _nextValues = gridModel.nextValues;
            _gridModel = gridModel;
            _canClick = true;
        }

        private void OnLoadFinished(LoadContext context)
        {
            SetWallsRemovingButton();
            SetChangingButton();
        }

        private void OnDisable()
        {
            _saveService.LoadFinished -= OnLoadFinished;
        }

        public bool IsFull()
        {
            foreach (var cell in _cells)
            {
                if (cell.Value.IsFree)
                    return false;
            }
            return true;
        }

        public void Block()
        {
            _isActive = false;
        }

        public void Finish(Action callback = null)
        {
            _isActive = false;
            _gridManager.WallsRemovingButton.Main.onClick.RemoveListener(StartRemoveWalls);
            _gridManager.ChangeButton.Main.onClick.RemoveListener(StartChange);
            _gameStatService.TrySetWithAnim(EGameStatType.Points, 0);
            SwipePrevGridAnimated(() => ClearGrid(callback));
        }

        private void SwipePrevGridAnimated(Action callback)
        {
            DOTween.To(() => canvasGroup.alpha, newAlpha =>
            {
                canvasGroup.alpha = newAlpha;
            }, _gridManager.InactiveGridAlpha, _gridManager.GridChangeTime);

            var upMove = DOTween.To(() => canvasGroup.transform.localPosition, newPosition =>
            {
                canvasGroup.transform.localPosition = newPosition;
            }, _gridManager.EndFlyPosition, _gridManager.GridChangeTime);

            upMove.onComplete += () => callback?.Invoke();
        }

        protected void ClearGrid(Action callback)
        {
            var keys = _cells.Keys.ToList();
            for (int i = 0; i < _cells.Count; i++)
            {
                var key = keys[i];
                Destroy(_cells[key].gameObject);
            }
            foreach (var cell in gridLayout.transform.GetComponents<Cell>())
            {
                Destroy(cell.gameObject);
            }
            _cells.Clear();
            _gridStates.Clear();
            _gridManager.SaveGrid(true);

            canvasGroup.transform.localPosition = _gridManager.NextGridLocalPosition;

            callback?.Invoke();
        }

        private bool TryRemoveWall(Cell cell)
        {
            if (cell.IsWall)
            {
                _gridStates.RemoveCellFromStates(cell.gridPosition);
                cell.view.Animator.AnimateDestroy(() => cell.Clear());
                _gridManager.SaveGrid();
                return true;
            }
            return false;
        }

        public void StartRemoveWalls()
        {
            if (AvailableForClick && CanRemoveWall())
            {
                if(_gameStatService.GetStat(EGameStatType.WallsRemoves).GetValue() > 0)
                {
                    _isWallRemoving = true;
                    SetWallsOverlays(_isWallRemoving);
                    RemoveWallsActivated.Invoke(_isWallRemoving);
                }
                else
                {
                    SquaresShop.OpenSection(_windowManager, EShopMarkers.Clues);
                }
            }
        }

        public virtual void StartChange()
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
                }
            }
        }

        public void StopRemoveWalls()
        {
            if (AvailableForClick)
            {
                _isWallRemoving = false;
                SetWallsOverlays(_isWallRemoving);
                RemoveWallsActivated.Invoke(_isWallRemoving);
            }
        }

        public void StopChange(bool afterChange = false)
        {
            if (AvailableForClick)
            {
                if (!afterChange && _fromCell != null && !_fromCell.IsFree)
                {
                    _fromCell.view.SetChangeOverlayActive(false);
                    _fromCell = null;
                }
                _isPositionChanging = false;
                SetOverlays(_isPositionChanging, cell => !cell.IsWall);
                SquareChangeActivated.Invoke(_isPositionChanging);
            }
        }

        public void SetNextValue(int value)
        {
            _nextValue = value;
            _nextValueView.Init(_nextValue);
            _gridManager.SaveNextValue(_nextValue);
        }

        public void SetNewSkin(ESquareSkin skin)
        {
            foreach (var cell in _cells.Values)
            {
                if (!cell.IsFree)
                {
                    cell.view.SetSkin(skin);
                }
            }
            _nextValueView.SetSkin(skin);
        }

        public void SetCells(List<CellData> cells)
        {
            foreach (var saveCell in cells)
            {
                var cell = _cells[saveCell.position];
                if (cell.IsFree)
                {
                    cell.SetFull(Instantiate(viewPrefab));
                    cell.view.Init(saveCell.value);
                }
            }
        }

        public void SetData(GridData data)
        {
            if (data.cells.Count > 0)
                ClearCells();

            SetCells(data.cells);
            _nextValue = data.nextValue;
            _nextValueView.Init(_nextValue);
            _gridManager.SaveGrid();
        }

        public void SwipeActiveGridAnimated()
        {
            DOTween.To(() => canvasGroup.alpha, newAlpha =>
            {
                canvasGroup.alpha = newAlpha;
            }, 1, _gridManager.GridChangeTime);

            var upMove = DOTween.To(() => canvasGroup.transform.localPosition, newPosition =>
            {
                canvasGroup.transform.localPosition = newPosition;
            }, _gridManager.CurrentGridLocalPosition, _gridManager.GridChangeTime);
            upMove.onComplete += () =>
            {
                _taskScoresView.Init(_gridModel.taskModel);
                _gridManager.SetButtonsInteractable(true);
            };

            SetActive();
        }

        public void SetActive()
        {
            _gridManager.WallsRemovingButton.Main.onClick.AddListener(StartRemoveWalls);
            _gridManager.ChangeButton.Main.onClick.AddListener(StartChange);
            SetWallsRemovingButton();
            SetChangingButton();
            _isActive = true;
            _gridStates.PutCurrentState(CreateGridState());
            ChooseNextValue();
        }

        public void SetWallsRemovingButton()
        {
            _gridManager.WallsRemovingButton.SetInteractable(CanRemoveWall() && _gridManager.UnlockedButtons);
        }

        public virtual void SetChangingButton()
        {
            _gridManager.ChangeButton.SetInteractable(CanChanged() && _gridManager.UnlockedButtons);
        }

        public void ClearCells()
        {
            foreach (var cell in _cells)
            {
                cell.Value.Clear();
            }
        }

        [UsedImplicitly]
        public void ReturnToPreviousState()
        {
            if (!AvailableForClick || !_gridStates.CanTake())
                return;
            
            if(_gameStatService.GetStat(EGameStatType.StepBacks).GetValue() > 0)
            {
                ClearCells();
                var state = _gridStates.TakePreviuosState();
                SetCells(state.Cells);

                _gameStatService.TrySetWithAnim(EGameStatType.Points, state.CollectedPoins);
                // _gridManager.StepBackButton.interactable = _gridStates.CanTake();
                _gridManager.SaveGrid(true);
                _gridStates.PutCurrentState(CreateGridState());
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.Clues);
            }
        }

        public bool IsClueActive()
        {
            return _isWallRemoving || _isPositionChanging;
        }

        public virtual IEnumerator ChangePosition()
        {
            if (_fromCell != null && _toCell !=null)
            {
                _fromCell.view.SetChangeOverlayActive(false);
                _gameStatService.TryDec(EGameStatType.SquareChanges, 1);
                _taskService.AddStat(ETaskDataType.ClueSwapSpent, 1);
                
                if (_toCell.view != null)
                {
                    var toView = _toCell.view;
                    var fromView = _fromCell.view;
                    _toCell.SetFree();
                    _fromCell.SetFree();
                    
                    toView.Animator.AnimateFlying(_fromCell);
                    yield return new WaitForCallback( callback => { fromView.Animator.AnimateFlying(_toCell, callback); } );

                    _toCell.SetFull(fromView);
                    _fromCell.SetFull(toView);

                    _toCell.view.Animator.AnimateUpgrade(_toCell.view.Value);
                    _fromCell.view.Animator.AnimateUpgrade(_fromCell.view.Value);
                }
                else
                {
                    var fromView = _fromCell.view;
                    _fromCell.SetFree();
                    
                    yield return new WaitForCallback( callback => { fromView.Animator.AnimateFlying(_toCell, callback); } );
                    
                    _toCell.SetFull(fromView);
                    _toCell.view.Animator.AnimateUpgrade(_toCell.view.Value);
                }

                StopChange(true);
                yield return CheckMergeAfterChange(_fromCell, _toCell);
                
                _gridManager.SaveGrid();

                _fromCell = null;
                _toCell = null;
            }
        }
        
         protected virtual void OnCellClick(Cell cell)
        {
            if (!AvailableForClick || !_canClick)
            {
                return;
            }

            SetChangingButton();

            if (_isWallRemoving)
            {
                if (TryRemoveWall(cell))
                {
                    _gameStatService.TryDec(EGameStatType.WallsRemoves, 1);
                    _taskService.AddStat(ETaskDataType.ClueBombSpent, 1);
                    SetWallsRemovingButton();
                    cell.PlayDestroy();
                    StopRemoveWalls();
                }
                _isWallRemoving = false;
                return;
            }

            if(_isPositionChanging)
            {
                if (!cell.IsFree && !cell.IsWall && _fromCell == null)
                {
                    _fromCell = cell;
                    _fromCell.view.SetChangeOverlayActive(true);
                    SetOverlays(_isPositionChanging, cellToCheck => !cellToCheck.IsWall && !_gridManager.TutorialEnabled);
                    return;
                }

                if (!cell.IsWall && _fromCell != null && cell != _fromCell)
                {
                    _toCell = cell;
                    _isPositionChanging = false;
                    StartCoroutine(ChangePosition());
                }
                
                return;
            }
            
            OnFreeCellClicked.Invoke();

            if (cell.IsFree)
            {
                _canClick = false;
                cell.PlayClick();

                StartCoroutine(DoCellClick(cell));
            }
        }
        
        protected virtual IEnumerator DoCellClick(Cell cell)
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
                _gridManager.SaveGrid();
                StateChanged?.Invoke();
            }
            _canClick = true;
        }

        protected IEnumerator TryMergeCellsFrom(Cell cell)
        {
            var cells = new List<Cell>();
            var needToCheckOneMoreTime = false;

            do
            {
                var needCrit = false;
                cells.Clear();
                var value = CollectCellsBySide(cell.view.Value, cell, cells);
                needToCheckOneMoreTime = cells.Count >= 3;
                if (needToCheckOneMoreTime)
                {
                    if (!isMerged)
                        isMerged = true;
                    var newValue = value * 2;
                    
                    if (TryGetCritValue(value, cells.Count, out var critValue))
                    {
                        newValue = critValue;
                        needCrit = true;
                        if (cell.view.Value == (int) CellValue.Joker)
                        {
                            cell.view.Init(value);
                        }
                    }
                    else
                    {
                        cell.view.Animator.AnimateScalePingPong();
                        cell.view.Init(newValue);
                    }

                    cells.Remove(cell);

                    foreach (var mergedCell in cells)
                    {
                        var view = mergedCell.view;
                        mergedCell.SetFree();
                        view.transform.SetParent(cell.view.transform, true);
                        if (mergedCell == cells[^1]) // Wait for last cell would be merged
                        {
                            if (!needCrit)
                            {
                                _gridManager.PlayMerge();
                            }
                            yield return new WaitForCallback( callback => { view.Animator.AnimateMerge(cell.view, () =>
                            {
                                Destroy(view.gameObject);
                                callback.Invoke();
                            });});
                        }
                        else
                        {
                            view.Animator.AnimateMerge(cell.view, () => Destroy(view.gameObject));
                        }

                        _cellsToCheck.Remove(mergedCell);
                    }
                    _taskService.AddStat(ETaskDataType.SquaresMerged, cells.Count);
                    
                    if (newValue > 2048)
                    {
                        var view = cell.view;
                        cell.SetFree();
                        view.Animator.AnimateDestroy(() => Destroy(view.gameObject));
                        // Add soft for merge 3 or more 2048 squares
                        _gameStatService.TryIncWithAnim(EGameStatType.Soft, softForMaxMerge);
                    }
                    else
                    {
                        if (needCrit)
                        {
                            yield return cell.view.Animator.AnimateCritical(newValue);
                            _taskService.AddStat(ETaskDataType.CritCount, 1);
                        }
                        else
                        {
                            cell.view.Animator.CreateSparksParticles();
                        }

                        if (_isActive)
                        {
                            yield return UpgradeCellsNearWithDelay(cell.gridPosition);
                        }
                    }
                    Merged?.Invoke();
                    NewValueCreated.Invoke(newValue);
                }
            } while (needToCheckOneMoreTime);
        }
        
        private bool TryGetCritValue(int currentValue, int mergedCellsCount, out int value)
        {
            var overMergedCellsCount = mergedCellsCount - 3;
            value = currentValue * 2;

            if (overMergedCellsCount > 0)
            {
                var percent = overMergedCellsCount * cellCritProbability;
                while (percent > 1)
                {
                    value *= 2;
                    percent -= 1;
                }

                var useUpperModifier = CalculateProbability(percent);
                if (useUpperModifier)
                    value *= 2;
            }

            return value > currentValue * 2;
        }

        private bool CalculateProbability(float percent)
        {
            return UnityEngine.Random.Range(0f, 1f) <= percent;
        }

        private IEnumerator CheckMergeAfterChange(Cell from, Cell to)
        {
            if(from.view != null)
            {
                _mergeCorutine = StartCoroutine(TryMergeCellsFrom(from));
                yield return CheckMergeAfterMerge();
            }

            do
            {
                yield return new WaitForSeconds(0.8f);

                if (to.view != null)
                {
                    _mergeCorutine = StartCoroutine(TryMergeCellsFrom(to));
                    yield return CheckMergeAfterMerge();
                }
            } while (isMerged);
            

        }

        protected IEnumerator CheckMergeAfterMerge()
        {
            while (_cellsToCheck.Count > 0)
            {
                var otherCell = _cellsToCheck[0];
                _cellsToCheck.Remove(otherCell);
                if(otherCell.view != null)
                {
                    yield return TryMergeCellsFrom(otherCell);
                }
            }
        }


        private void SetWallsOverlays(bool active)
        {
            SetOverlays(active, cell => cell.IsWall);
        }

        protected virtual void SetOverlays(bool active, Func<Cell, bool> checkType)
        {
            foreach (var cell in _cells)
            {
                if(checkType(cell.Value))
                {
                    if (active)
                    {
                        cell.Value.anchor.SetSorting(_gridManager.WallSortingOrder);
                    }
                    else
                    {
                        cell.Value.anchor.ResetSorting();
                    }
                }
            }
        }

        private bool CanRemoveWall()
        {
            var hasWall = false;
            foreach (var cell in _cells)
            {
                if (cell.Value.IsWall)
                {
                    hasWall = true;
                    break;
                }
            }
            return hasWall;
        }

        protected bool CanChanged()
        {
            var hasValueCells = false;
            foreach (var cell in _cells)
            {
                if (!cell.Value.IsFree && !cell.Value.IsWall)
                {
                    hasValueCells = true;
                    break;
                }
            }
            return hasValueCells;
        }

        protected GridState CreateGridState()
        {
            var stateCells = new List<CellData>();

            foreach (var pair in _cells)
            {
                if (!pair.Value.IsFree)
                    stateCells.Add(new CellData() { position = pair.Key, value = pair.Value.view.Value });
            }

            return new GridState(stateCells, _gameStatService.GetStat(EGameStatType.Points).RealValue);
        }

        private int CollectCellsBySide(int value, Cell start, List<Cell> cells)
        {
            cells.Add(start);
            if (value == (int)CellValue.Joker && cells.Count == 1)
            {
                // find max value chain if Joker is global start of our search
                // collect all chains near
                var valuesCount = new Dictionary<int, int>();
                var cellsOnDirection = new List<Cell>();
                var secondJoker = false;
                foreach (var direction in _directions)
                {
                    var newPos = start.gridPosition + direction;
                    cellsOnDirection.Clear();
                    if (newPos.x >= 0 && newPos.x < gridLayout.GridSize.x && newPos.y >= 0 &&
                        newPos.y < gridLayout.GridSize.y)
                    {
                        var cell = _cells[newPos];
                        if (!cell.IsFree && !cell.IsWall)
                        {
                            if (!cell.IsJoker)
                            {
                                if (!valuesCount.ContainsKey(cell.view.Value))
                                {
                                    valuesCount.Add(cell.view.Value, 0);
                                }

                                CollectCellsBySide(cell.view.Value, cell, cellsOnDirection);
                                valuesCount[cell.view.Value] += cellsOnDirection.Count;
                            }
                            else // Check joker near
                            {
                                secondJoker = true;
                                foreach (var secondDirection in _directions)
                                {
                                    cellsOnDirection.Clear();
                                    var secondNewPos = cell.gridPosition + secondDirection;
                                    if (secondNewPos.x >= 0 && secondNewPos.x < gridLayout.GridSize.x && secondNewPos.y >= 0 &&
                                        secondNewPos.y < gridLayout.GridSize.y)
                                    {
                                        var secondCell = _cells[secondNewPos];
                                        if (!secondCell.IsFree && !secondCell.IsJoker && !secondCell.IsWall)
                                        {
                                            if (!valuesCount.ContainsKey(secondCell.view.Value))
                                            {
                                                valuesCount.Add(secondCell.view.Value, 0);
                                            }

                                            CollectCellsBySide(secondCell.view.Value, secondCell, cellsOnDirection);
                                            valuesCount[secondCell.view.Value] += cellsOnDirection.Count;
                                        }
                                        // ignore more than 2 jokers
                                    }
                                }
                            }
                        }
                    }
                }

                // Debug.Log($"[GridView][CollectCellsBySide] Joker as chain start");
                var maxValue = -1;
                foreach (var pair in valuesCount)
                {
                    // Debug.Log($"[GridView][CollectCellsBySide] Collect {pair.Key} : {pair.Value}");
                    if (pair.Value >= 3 - (secondJoker ? 1 : 0) && pair.Key > maxValue)
                    {
                        maxValue = pair.Key;
                    }
                }
                value = maxValue;
                // Debug.Log($"[GridView][CollectCellsBySide] Value = {value}");

                // return if can't find
                if (value == -1)
                {
                    return -1;
                }
            }
            foreach (var direction in _directions)
            {
                var newPos = start.gridPosition + direction;
                if (newPos.x >= 0 && newPos.x < gridLayout.GridSize.x && newPos.y >= 0 &&
                    newPos.y < gridLayout.GridSize.y)
                {
                    var cell = _cells[newPos];
                    if (!cell.IsFree && !cell.IsWall && (cell.view.Value == value || cell.IsJoker) && !cells.Contains(cell))
                    {
                        // Debug.Log($"[GridView][CollectCellsBySide] Step in chain with value : {value} cells count : {cells.Count}");
                        CollectCellsBySide(value, cell, cells);
                    }
                }
            }
            return value;
        }

        protected virtual void ChooseNextValue()
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
            _gridManager.SaveNextValue(_nextValue);
        }

        private void InitCells(GridModel gridModel)
        {
            foreach (var cell in gridLayout.transform.GetComponents<Cell>())
            {
                Destroy(cell.gameObject);
            }
            
            gridLayout.SetSize(gridModel.size.y, gridModel.size.x);
            var columnCount = 0;
            var rowCount = 0;

            for (int i = 0; i < gridModel.size.x * gridModel.size.y; i++)
            {
                var cell = Instantiate(cellPrefab, gridLayout.transform);
                var gridPosition = new Vector2Int(columnCount, rowCount);
                var rectTransform = (RectTransform)cell.transform;
                cell.localPosition = rectTransform.localPosition;
                cell.worldPosition = rectTransform.position;
                cell.gridPosition = gridPosition;

                if (gridModel.UnitPositions.TryGetValue(gridPosition, out var value))
                {
                    cell.SetFull(Instantiate(viewPrefab));
                    cell.view.Init(value);
                    cell.view.Animator.AnimateScalePingPong();
                }
                else
                {
                    cell.SetFree();
                }

                cell.Clicked += OnCellClick;
                _cells.Add(gridPosition, cell);

                columnCount++;
                columnCount %= gridLayout.GridSize.x;
                if (columnCount == 0)
                {
                    rowCount++;
                    rowCount %= gridLayout.GridSize.y;
                }
            }
        }

        private IEnumerator UpgradeCellsNearWithDelay(Vector2Int position)
        {
            var cells = GetCellsToUpgrade(position);

            if (cells.Count > 0)
            {
                _gridManager.PlayWave();
                _taskService.AddStat(ETaskDataType.WaveCount, 1);
                _cellsToCheck.AddRange(cells);
                foreach (var cell in cells)
                {
                    var newValue = cell.view.Value * 2;
                    cell.view.Init(newValue);
                    cell.view.Animator.AnimateUpgrade(newValue);
                    cell.view.Animator.AnimateScalePingPong();
                    NewValueCreated.Invoke(newValue);
                    StateChanged.Invoke();
                }
                yield return new WaitForSeconds(upgradeDelay);
            }
        }

        private List<Cell> GetCellsToUpgrade(Vector2Int position)
        {
            var cells = GetNumericCellsNear(position);
            return cells.Where(c => !c.IsFree && c.view.Value <= _cells[position].view.Value / 2).ToList();
        }

        private List<Cell> GetNumericCellsNear(Vector2Int position)
        {
            var result = new List<Cell>();

            foreach (var dir in _upgradeDirections)
            {
                var newPos = position + dir;
                if (newPos.x >= 0 && newPos.x < gridLayout.GridSize.x && newPos.y >= 0 &&
                    newPos.y < gridLayout.GridSize.y)
                {
                    var cell = _cells[newPos];

                    if (!cell.IsFree && !cell.IsWall && !cell.IsJoker)
                        result.Add(cell);
                }
            }

            return result;
        }

        private Cell GetCellByPosition(Vector2 position)
        {
            return _cells[CalculateGridPosition(position)];
        }

        private Vector2Int CalculateGridPosition(Vector3 worldPosition)
        {
            var size = new Vector3(
                _cells[new Vector2Int(1, 0)].worldPosition.x - _cells[new Vector2Int(0, 0)].worldPosition.x,
                _cells[new Vector2Int(0, 1)].worldPosition.y - _cells[new Vector2Int(0, 0)].worldPosition.y
            );
            var xSize = size.x * gridLayout.GridSize.x;
            var ySize = size.y * gridLayout.GridSize.y;
            var corner = gridLayout.transform.position - new Vector3(xSize / 2, ySize / 2);
            var position = worldPosition - corner;
            var xPosition = (int)Mathf.Clamp(position.x / size.x, 0f, gridLayout.GridSize.x - 1);
            var yPosition = (int)Mathf.Clamp(position.y / size.y, 0f, gridLayout.GridSize.y - 1);
            return new Vector2Int(xPosition, yPosition);
        }
    }
}