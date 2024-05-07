using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.SaveLoad;
using Core.Windows;
using DG.Tweening;
using GameScripts.Game2248.Tasks;
using GameStats;
using LargeNumbers;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Instructions;
using Zenject;

namespace GameScripts.Game2248
{
    public enum EBonusType
    {
        None = 0,
        Bomb = 1,
        Plus = 2,
        DestroyByNum = 3,
    }

    public class GridView : MonoBehaviour
    {
        [SerializeField] private Transform linesRoot;
        [SerializeField] private LineRenderer linePrefab;
        [SerializeField] protected UnitView viewPrefab;
        [SerializeField] protected Cell cellPrefab;
        [SerializeField] protected FlexibleGridLayout gridLayout;
        // [SerializeField] private float offset;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        
        public Vector3 LocalPosition => canvasGroup.transform.localPosition;
        public Dictionary<Vector2Int, Cell> Cells => _cells;
        public TaskModel Task => _gridModel.taskModel;
        public GridModel GridModel => _gridModel;

        public Action<bool> OnRemoveSquareActive = value => { };
        public Action<LargeNumber> OnNewValueCreated = value => { };
        public Action LineEnded = () => {};
        public bool AvailableForClick => _isActive && !_gridManager.IsLocked;
        protected Dictionary<Vector2Int, Cell> _cells = new();
        protected LargeNumber _finalValue = LargeNumber.zero;
        protected LargeNumber _activeSum = LargeNumber.zero;
        private int _linePow = 1;
        private int _updatePow = -1;
        private LargeNumber _sameValueOrder = LargeNumber.zero;
        private bool _isActive = true;
        private EBonusType _bonusType = EBonusType.None;
        private bool _isSquareRemoving = false;
        private bool _isSquareSwap = false;
        protected float _lastActivityTime = 0;
        protected bool IsDowned = false;
        private bool _isAnimateScalePulse = false;
        protected List<Cell> _downedCells = new List<Cell>();
        protected List<LineRenderer> _lines = new List<LineRenderer>();
        private List<Vector2Int> _directions =
            new() { new(1, 0), new(0, 1), new(-1, 0), new(0, -1), new(-1, 1), new(1, 1), new(1, -1), new(-1, -1) };

        private IEnumerator _mergeClue;
        private Cell _mergeCell = null;
        private Vector3 _savedLocalScale;
        private List<Cell> _ableMergeCells = new List<Cell>();
        private Cell _firstSwapSquare = null;
        
        private UnitView _nextValueView;
        protected GameStatService _gameStatService;
        protected GameStatLargeService _gameStatLargeService;
        private SaveService _saveService;
        protected GridManager _gridManager;
        private GridStates _gridStates;
        protected GridModel _gridModel;
        private TaskScoresView _taskScoresView;
        private WindowManager _windowManager;
        protected SquaresSpawnController _squaresSpawnController;
        protected TaskService _taskService;

        [Inject]
        public void Construct(
            TaskScoresView taskScoresView,
            UnitView nextValueView,
            GameStatService gameStatService,
            GameStatLargeService gameStatLargeService,
            GridManager gridManager,
            Camera worldCamera,
            SaveService saveService,
            WindowManager windowManager,
            SquaresSpawnController squaresSpawnController,
            TaskService taskService
        )
        {
            _nextValueView = nextValueView;
            _gameStatService = gameStatService;
            _gameStatLargeService = gameStatLargeService;
            _gridManager = gridManager;
            canvas.worldCamera = worldCamera;
            _saveService = saveService;
            _taskScoresView = taskScoresView;
            _windowManager = windowManager;
            _squaresSpawnController = squaresSpawnController;
            _taskService = taskService;
        }

        private void Start()
        {
            _saveService.LoadFinished += OnLoadFinished;
            _mergeClue = AnimateScalePulse();
        }

        public void Init(GridModel gridModel, GridData data = null)
        {
            _gridStates = new GridStates(_gameStatService);

            if (data != null)
            {
                if (data.size == Vector2Int.zero)
                {
                    data.size = gridModel.size;
                }
                // create new model to not overwrite old
                var model = new GridModel()
                {
                    id = gridModel.id,
                    reward = gridModel.reward,
                    size = data.size,
                    taskModel = gridModel.taskModel,
                    nextPowUpdate = gridModel.nextPowUpdate,
                    units = data.cellsLarge.Select(t => new UnitModel() { position = t.position, largeValue = t.valueLarge }).ToList(),
                    startPows = new List<int>(gridModel.StartPows),
                    powUpdateConditions = new List<PowUpdateCondition>(gridModel.PowUpdateConditions),
                };
                _gridModel = model;
                InitCells(model);
            }
            else
            {
                if ((Screen.width >  Screen.height && gridModel.size.x < gridModel.size.y) || (Screen.width <  Screen.height && gridModel.size.x > gridModel.size.y))
                {
                    var newSize = new Vector2Int(gridModel.size.y, gridModel.size.x);
                    gridModel.size = newSize;
                }
                _gridModel = gridModel;
                InitCells(gridModel);
                _gridManager.SaveSize(_gridModel.size);
            }
            
            _lastActivityTime = Time.time;
            TryFindMergePossibilities();
            if(UseHint())
            {
                StartCoroutine(CheckActivity());
            }
        }

        private void OnLoadFinished(LoadContext context)
        {
            // SetWallsRemovingButton();
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

        public void SetNotActive() => _isActive = false;
        
        public void SetActive() => _isActive = true;
        public virtual bool UseHint() => true;

        public void Finish(Action callback = null)
        {
            StopClue();
            _isActive = false;
            _gameStatService.TrySetWithAnim(EGameStatType.Points, 0);
            SwipePrevGridAnimated(() => ClearGrid(callback));
        }
        
        public bool TryFindMergePossibilities()
        {
            _ableMergeCells.Clear();
            foreach (var pair in _cells)
            {
                if (pair.Value.IsFree)
                {
                    continue;
                }
                foreach (var direction in _directions)
                {
                    var newPos = pair.Key + direction;
                    if (newPos.x >= 0 && newPos.x < gridLayout.GridSize.x && newPos.y >= 0 &&
                        newPos.y < gridLayout.GridSize.y && !_cells[newPos].IsFree)
                    {
                        var other = _cells[newPos];
                        if (other.view.Value == pair.Value.view.Value)
                        {
                            _ableMergeCells.Add(pair.Value);
                            _ableMergeCells.Add(other);
                            _savedLocalScale = other.transform.localScale;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void StartRemoveSquare()
        {
            if (AvailableForClick/* && CanRemoveWall()*/)
            {
                if(_gameStatService.GetStat(EGameStatType.SquareRemoves).RealValue > 0 && !_isSquareRemoving)
                {
                    _isSquareRemoving = true;
                    // SetWallsOverlays(_isSquareRemoving);
                    OnRemoveSquareActive?.Invoke(_isSquareRemoving);
                    _gridManager.ActivateClueDestroyOverlay();
                }
                else if (_isSquareRemoving)
                {
                    _gridManager.DeactivateClueOverlay();
                    StopRemoveSquare();
                }
                else
                {
                    GameScripts.MergeSquares.Shop.SquaresShop.OpenSection(_windowManager, GameScripts.MergeSquares.Shop.EShopMarkers.Clues);
                }
            }
        }
        
        public void ClearLine()
        {
            _downedCells.Clear();
            _finalValue = LargeNumber.zero;
            _activeSum = LargeNumber.zero;
        }

        public void StartSwapSquares()
        {
            if (AvailableForClick /* && CanRemoveWall()*/)
            {
                if(_gameStatService.GetStat(EGameStatType.SquareSwap).RealValue > 0 && !_isSquareSwap)
                {
                    _isSquareSwap = true;
                    // SetWallsOverlays(_isSquareRemoving);
                    _gridManager.ActivateClueSwapOverlay();
                }
                else if (_isSquareSwap)
                {
                    StopSwapSquares();
                }
                else
                {
                    GameScripts.MergeSquares.Shop.SquaresShop.OpenSection(_windowManager, GameScripts.MergeSquares.Shop.EShopMarkers.Clues);
                }
            }
        }

        public void StopSwapSquares()
        {
            if(_firstSwapSquare != null)
            {
                _firstSwapSquare.view.SetChangeOverlayActive(false);
                _firstSwapSquare = null;
            }
            _gridManager.IsLocked = false;
            _gridManager.DeactivateClueOverlay();
            _isSquareSwap = false;
            _firstSwapSquare = null;
            _lastActivityTime = Time.time;
            TryFindMergePossibilities();
        }

        public void StopRemoveSquare()
        {
            _gridManager.IsLocked = false;
            _isSquareRemoving = false;
            OnRemoveSquareActive?.Invoke(_isSquareRemoving);
            _lastActivityTime = Time.time;
            TryFindMergePossibilities();
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
        }

        public void SetCells(List<CellData> cells)
        {
            foreach (var saveCell in cells)
            {
                var cell = _cells[saveCell.position];
                if (cell.IsFree)
                {
                    cell.SetFull(Instantiate(viewPrefab));
                    cell.view.Init(saveCell.valueLarge);
                }
            }
        }
        
        public void SetX2Square(Cell cell)
        {
            var newValue = cell.view.Value * 2;
            _gridManager.CurrentGridView._finalValue = newValue;
            _gridManager.MaxSquare = newValue;
            _linePow++;
            cell.Clear();
            cell.SetFull(Instantiate(viewPrefab));
            cell.view.Init(newValue);
            cell.view.Animator.AnimateScalePingPong();
            UpdateMax();
            OnNewValueCreated.Invoke(newValue);
        }

        public IEnumerator UpdatePows()
        {
            for (int i = _gridManager.CurrentPow + 1; i <= _linePow; i++)
            {
                yield return UpdatePow(i);
            }

            if (_linePow > _gridManager.CurrentPow)
                _gridManager.CurrentPow = _linePow;
        }
        
        public IEnumerator ClearRemovedCells(int pow)
        {
            var needsDown = false;
            var removeValue = new LargeNumber(Math.Pow(2, pow));
            foreach (var cell in _cells)
            {
                if(!cell.Value.IsFree)
                {
                    if (cell.Value.view.Value == removeValue)
                    {
                        cell.Value.Clear();
                        needsDown = true;
                    }
                }
            }

            if (needsDown)
            {
                yield return MoveCellsDown();
                yield return FullFreeCells();
            }
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
                if (_gridModel.taskModel.type == ETaskType.GetCellWithValue)
                {
                    _taskScoresView.Init(_gridModel.taskModel, _gridManager.MaxSquare);
                }
                else
                {
                    _taskScoresView.Init(_gridModel.taskModel, _gameStatLargeService.GetStatValue(EGameStatType.Points));
                }
                // _gridManager.SetButtonsInteractable(true);
            };
        
            SetActive();
        }

        public void ClearCells()
        {
            foreach (var cell in _cells)
            {
                cell.Value.Clear();
            }
        }

        protected IEnumerator AnimateScalePulse(AnimationCurve curve = null)
        {
            _isAnimateScalePulse = true;
            float t = 0;
            if (curve == null)
            {
                curve = _gridManager.ScaleAnimationCurve;
            }
            
            while (true)
            {
                if (t >= _gridManager.AnimationPulseDuration)
                {
                    t = 0;
                }
                _ableMergeCells[0].view.transform.localScale = Vector3.one * curve.Evaluate(t / _gridManager.AnimationPulseDuration);
                _ableMergeCells[1].view.transform.localScale = Vector3.one * curve.Evaluate(t / _gridManager.AnimationPulseDuration);

                t += Time.deltaTime;
                
                yield return null;
            }
        }
        
        protected void StopClue()
        {
            if (_isAnimateScalePulse)
            {
                _ableMergeCells[0].view.transform.localScale = _savedLocalScale;
                _ableMergeCells[1].view.transform.localScale = _savedLocalScale;
                StopCoroutine(_mergeClue);
                _isAnimateScalePulse = false;
            }
        }

        protected void OnPointerDown(Cell cell)
        {
            StopClue();
            if (!AvailableForClick)
            {
                return;
            }

            StartCoroutine(PointerDown(cell));
        }

        private IEnumerator PointerDown(Cell cell)
        {
            if (_isSquareRemoving && _gameStatService.TryDecWithAnim(EGameStatType.SquareRemoves, 1))
            {
                yield return new WaitForCallback( callback => { cell.view.Animator.AnimateDestroy(callback);});
                cell.Clear();
                _gridManager.IsLocked = true;
                _gridManager.DeactivateClueOverlay();
                yield return MoveCellsDown();
                yield return FullFreeCells();
                _taskService.AddStat(ETaskDataType.ClueBombSpent, 1);
                _taskService.AddStat(ETaskDataType.SquaresDestroyed, 1);
                StopRemoveSquare();
            }
            else if (_isSquareSwap)
            {
                if (_firstSwapSquare == null)
                {
                    _firstSwapSquare = cell;
                    cell.view.SetChangeOverlayActive(true);
                }
                else if(_firstSwapSquare == cell)
                {
                    _firstSwapSquare.view.SetChangeOverlayActive(false);
                    _firstSwapSquare = null;
                }
                else
                {
                    _gameStatService.TryDecWithAnim(EGameStatType.SquareSwap, 1);
                    _firstSwapSquare.view.SetChangeOverlayActive(false);
                    var toView = cell.view;
                    var fromView = _firstSwapSquare.view;
                    cell.SetFree();
                    _firstSwapSquare.SetFree();
                    toView.Animator.SetSorting(1000);
                    fromView.Animator.SetSorting(1000);
                    toView.Animator.AnimateFlying(_firstSwapSquare);
                    yield return new WaitForCallback( callback => { fromView.Animator.AnimateFlying(cell, callback); } );
                    toView.Animator.ResetSorting();
                    fromView.Animator.ResetSorting();
                    cell.SetFull(fromView);
                    _firstSwapSquare.SetFull(toView);

                    cell.view.Animator.AnimateScalePingPong();
                    _firstSwapSquare.view.Animator.AnimateScalePingPong();
                    
                    _gridManager.IsLocked = true;
                    _taskService.AddStat(ETaskDataType.ClueSwapSpent, 1);
                    StopSwapSquares();
                    _gridManager.SaveGrid();
                }
            }
            else if (!IsDowned)
            {
                IsDowned = true;
                _downedCells.Add(cell);
                _finalValue = cell.view.Value;
                _activeSum = _finalValue;
                var newLine = Instantiate(linePrefab, linesRoot);
                newLine.positionCount = 1;
                newLine.SetPosition(0, cell.transform.position);
                newLine.endColor = cell.view.ImageData.color;
                newLine.startColor = cell.view.ImageData.color;
                _lines.Add(newLine);
                cell.view.Animator.AnimateScalePingPong();
                cell.view.SetSelectLight(true);
                cell.PlayClick();
                // StartCoroutine(LineFollowPointer());
            }
        }
        
        protected void OnPointerEnter(Cell cell)
        {
            if (IsDowned)
            {
                var prevCell = _downedCells.Last();
                if (Math.Abs(prevCell.gridPosition.x - cell.gridPosition.x) <= 1 &&
                    Math.Abs(prevCell.gridPosition.y - cell.gridPosition.y) <= 1)
                {
                    if (_downedCells.Contains(cell))
                    {
                        if (_downedCells.Count > 1 && _downedCells[_downedCells.Count - 2] == cell)
                        {
                            _activeSum -= _downedCells.Last().view.Value;
                            _finalValue = _gridManager.GetNearest2PowValue(_activeSum, out _linePow);
                            _gridManager.UpdateNextSquare(_finalValue);
                            _downedCells.Last().view.SetSelectLight(false);
                            _downedCells.Remove(_downedCells.Last());
                            Destroy(_lines[_lines.Count - 1].gameObject);
                            _lines.RemoveAt(_lines.Count - 1);
                            _lines.Last().positionCount = 1;
                            cell.view.Animator.AnimateScalePingPong();
                            cell.PlayClick();
                            CheckBonuses();
                            if (_downedCells.Count > 1)
                            {
                                _gridManager.UpdateNextSquare(_finalValue);
                            }
                            else
                            {
                                _gridManager.HideNextSquare();
                            }
                        }
                    }
                    else if (cell.view.Value == prevCell.view.Value || (cell.view.Value == prevCell.view.Value * 2 && _downedCells.Count > 1))
                    {
                        _downedCells.Add(cell);
                        _activeSum += cell.view.Value;
                        _finalValue = _gridManager.GetNearest2PowValue(_activeSum, out _linePow);
                        _gridManager.ShowNextSquare();
                        _gridManager.UpdateNextSquare(_finalValue);
                        _lines.Last().positionCount = 2;
                        _lines.Last().SetPosition(1, cell.transform.position);
                        var newLine = Instantiate(linePrefab, linesRoot);
                        newLine.positionCount = 1;
                        newLine.SetPosition(0, cell.transform.position);
                        newLine.endColor = cell.view.ImageData.color;
                        newLine.startColor = cell.view.ImageData.color;
                        _lines.Add(newLine);
                        CheckBonuses();
                        cell.view.Animator.AnimateScalePingPong();
                        cell.view.SetSelectLight(true);
                        cell.PlayClick();
                    }
                }
            }
        }

        protected virtual void OnPointerUp(Cell cell)
        {
            if(!IsDowned)
                return;
            
            IsDowned = false;
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }
            _lines.Clear();

            _gridManager.HideNextSquare();
            if (_downedCells.Count > 1)
            {
                _gridManager.IsLocked = true;
                var needsX2Offer = false;
                var pow = 1;
                _mergeCell = _downedCells.Last();
                if (_gridManager.MaxSquare < _finalValue)
                {
                    _gridManager.MaxSquare = _finalValue;
                    foreach (var c in Cells.Values)
                    {
                        c.view.SetMaxFrame(c.view.Value == _gridManager.MaxSquare);
                    }
                    _gridManager.GetNearest2PowValue(_finalValue, out pow);
                }
                else if(_gridManager.MaxSquare == _finalValue)
                {
                    needsX2Offer = true;
                }
                StartCoroutine(FlyParticles(needsX2Offer, pow));
                _gridManager.PlayEndLine();
                // _gameStatLargeService.TryIncWithAnim(EGameStatType.Points, finalValue);

            }

            else
            {
                _downedCells[0].view.SetSelectLight(false);
                cell.PlayClick();
                _downedCells.Clear();
                _finalValue = LargeNumber.zero;
                _activeSum = LargeNumber.zero;
            }
        }

        protected virtual IEnumerator FlyParticles(bool needsX2Offer, int pow)
        {
            var lastCell = _downedCells.Last();
            SetNotActive();
            var endColor = _gridManager.ResourceRepository.GetSquare2248ImageByPow(pow).color;
            var flyTime = _gridManager.AnimParams.time * 2f;

            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                StartCoroutine(_downedCells[i].view.ParticleController.FlyToTarget(flyTime,
                    lastCell.transform, _downedCells[i].view.ImageData.color, endColor, () => { }));
                StartCoroutine(_downedCells[i].view.AnimateByTime(_gridManager.AnimParams));
            }

            StartCoroutine(lastCell.view.ParticleController.FlyToTarget(flyTime,
                lastCell.transform, lastCell.view.ImageData.color, endColor, () => { }));
            yield return lastCell.view.AnimateByTime(_gridManager.AnimParams);
            var lastView = lastCell.view;
            lastCell.SetFull(Instantiate(viewPrefab));
            lastCell.view.Init(_finalValue, _finalValue == _gridManager.MaxSquare);
            lastCell.view.ParticleController.RunAppear();
            yield return lastCell.view.AnimateByTime(new UnitViewAnimParams()
            {
                time = _gridManager.AnimParams.time,
                aColor = 1,
                startRotation = _gridManager.AnimParams.endRotation,
                endRotation = 0,
                startScale = _gridManager.AnimParams.endScale,
                endScale = 1
            });
            Destroy(lastView.gameObject);
            yield return EndLine(needsX2Offer);
        }

        protected void UpdateMax()
        {
            foreach (var c in Cells.Values)
            {
                if (!c.IsFree)
                {
                    c.view.SetMaxFrame(c.view.Value == _gridManager.MaxSquare);
                }
            }
        }
        
        
        protected virtual IEnumerator FixGrid()
        {
            _taskService.AddStat(ETaskDataType.SquaresDestroyed, _downedCells.Count - 1);
            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                _downedCells[i].Clear();
            }
            yield return MoveCellsDown();
            yield return FullFreeCells();
            _lastActivityTime = Time.time;
            OnNewValueCreated.Invoke(_finalValue);
        }

        protected IEnumerator MoveCellsDown()
        {
            var cells = _cells.Values.ToList();
            var animTime = 0f;
            for (int i = cells.Count - 1; i >= 0; i--)
            {
                if (cells[i].IsFree)
                {
                    TryMoveDown(cells[i]);
                }
                else
                {
                    animTime = cells[i].view.Animator.FlyingDuration;
                }
            }
            yield return new WaitForSeconds(animTime);
        }
        
        protected virtual bool CheckBonuses()
        {
            if (_downedCells.Count < 3)
            {
                _bonusType = EBonusType.None;
                return false;
            }

            var incCount = 0;
            var maxIncCount = 0;
            var sameVount = 0;
            var maxSameCount = 0;
            var prevCellValue = LargeNumber.zero;
            foreach (var dcell in _downedCells)
            {
                if (dcell.view.Value > prevCellValue)
                {
                    incCount++;
                }
                else
                {
                    incCount = 1;
                }
                
                if (dcell.view.Value == prevCellValue)
                {
                    sameVount++;
                }
                else
                {
                    sameVount = 1;
                }

                prevCellValue = dcell.view.Value;
                if (maxIncCount < incCount)
                    maxIncCount = incCount;
                if (maxSameCount < sameVount)
                {
                    maxSameCount = sameVount;
                    _sameValueOrder = dcell.view.Value;
                }
            }

            if (maxIncCount == 3)
            {
                _bonusType = EBonusType.Bomb;
            }
            else if (maxIncCount > 3)
            {
                _bonusType = EBonusType.Plus;
            }
            else if (maxSameCount > 4)
            {
                _bonusType = EBonusType.DestroyByNum;
            }
            else
            {
                _bonusType = EBonusType.None;
            }

            _gridManager.SetBonusView(_bonusType);
            if (_bonusType == EBonusType.None)
            {
                return false;
            }
            return true;
        }

        protected virtual void FullDownedCell(Cell cell, Cell nextCell)
        {
            cell.SetFull(Instantiate(viewPrefab));
            cell.view.InitInvisible(nextCell.view.Value);
            if (nextCell == _mergeCell)
            {
                _mergeCell = cell;
            }
            var nextCellView = nextCell.view;
            nextCell.SetFree();
            nextCellView.Animator.AnimateFlying(cell, () =>
            {
                cell.view.SetVisible();
                cell.view.SetMaxFrame(cell.view.Value == _gridManager.MaxSquare);
                Destroy(nextCellView.gameObject);
            });
        }

        protected virtual IEnumerator EndLine(bool needsX2Offer)
        {
            OnNewValueCreated.Invoke(_finalValue);
            _taskService.AddStat(ETaskDataType.SquaresDestroyed, _downedCells.Count - 1);
            _taskService.AddStat(ETaskDataType.LinesCount, 1);
            if (needsX2Offer)
            {
                _windowManager.TryShowAndGetWindow(EPopupType.X2Offer.ToString(), out X2Offer offer);
                yield return new WaitForCallback( callback => { offer.Init(_downedCells.Last(), callback); } );
            }
            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                _downedCells[i].Clear();
            }
            ClearLine();
            yield return MoveCellsDown();
            yield return FullFreeCells();
            yield return PlayBonus();
            yield return MoveCellsDown();
            yield return FullFreeCells();
            _lastActivityTime = Time.time;
            LineEnded.Invoke();
            if (!_gridManager.LevelEnds)
            {
                _gridManager.IsLocked = false;
            }
        }

        protected IEnumerator PlayBonus()
        {
            if (_bonusType != EBonusType.None)
            {
                yield return new WaitForCallback(callback => _gridManager.FlyBonus(_mergeCell.transform.position, callback));
                switch (_bonusType)
                {
                    case EBonusType.Bomb:
                        yield return BombBonus();
                        break;
                    case EBonusType.Plus:
                        yield return PlusBonus();
                        break;
                    case EBonusType.DestroyByNum:
                        yield return DestroyByNumBonus();
                        break;
                }
            }
            _sameValueOrder = LargeNumber.zero;
            _bonusType = EBonusType.None;
        }

        private void FlyBonusCell(Vector2Int pos, float flyTime, Color endColor)
        {
            FlyBonusCell(_cells[pos], flyTime, endColor);
        }

        private void FlyBonusCell(Cell cell, float flyTime, Color endColor)
        {
            StartCoroutine(cell.view.ParticleController.FlyToTarget(flyTime,
                _mergeCell.transform, cell.view.ImageData.color, endColor, () => { }));
            StartCoroutine(cell.view.AnimateByTime(_gridManager.AnimParams));
        }

        private void WaveBonus(ref LargeNumber sum, ref List<Cell> cellsToClear)
        {
            sum = _mergeCell.view.Value;
            cellsToClear.Clear();
            
            int xMin = Math.Max(_mergeCell.gridPosition.x - 1, 0);
            int xMax = _mergeCell.gridPosition.x + 2 <= _gridModel.size.x
                ? _mergeCell.gridPosition.x + 2
                : _gridModel.size.x;

            int yMin = Math.Max(_mergeCell.gridPosition.y - 1, 0);
            int yMax = _mergeCell.gridPosition.y + 2 <= _gridModel.size.y
                ? _mergeCell.gridPosition.y + 2
                : _gridModel.size.y;

            var compareValue = _gridManager.MaxSquare / 2;
            for (int i = xMin; i < xMax; i++)
            {
                for (int j = yMin; j < yMax; j++)
                {
                    var pos = new Vector2Int(i, j);
                    if (_cells[pos].view.Value < compareValue)
                    {
                        SetX2Square(_cells[pos]);
                    }
                }
            }
        }

        private IEnumerator BombBonus()
        {
            LargeNumber sum = LargeNumber.zero;
            var flyTime = _gridManager.AnimParams.time * 2f;

            int xMin = Math.Max(_mergeCell.gridPosition.x - 1, 0);
            int xMax = _mergeCell.gridPosition.x + 2 <= _gridModel.size.x
                ? _mergeCell.gridPosition.x + 2
                : _gridModel.size.x;

            int yMin = Math.Max(_mergeCell.gridPosition.y - 1, 0);
            int yMax = _mergeCell.gridPosition.y + 2 <= _gridModel.size.y
                ? _mergeCell.gridPosition.y + 2
                : _gridModel.size.y;

            var cellsToClear = new List<Cell>();
            var endColor = _gridManager.ResourceRepository.GetSquare2248ImageByPow(_gridManager.CurrentPow).color;

            for (int i = xMin; i < xMax; i++)
            {
                for (int j = yMin; j < yMax; j++)
                {
                    var pos = new Vector2Int(i, j);
                    sum += _cells[pos].view.Value;
                    cellsToClear.Add(_cells[pos]);
                }
            }

            if (_gridManager.GetNearest2PowValue(sum, out var newPow) > _mergeCell.view.Value)
            {
                foreach (var cell in cellsToClear)
                {
                    FlyBonusCell(cell, flyTime, endColor);
                }
            }
            else
            {
                WaveBonus(ref sum, ref cellsToClear);
            }

            yield return new WaitForSeconds(flyTime);
            yield return EndBonus(sum, cellsToClear);
        }

        private IEnumerator PlusBonus()
        {
            LargeNumber sum = LargeNumber.zero;
            var flyTime = _gridManager.AnimParams.time * 2f;
            var cellsToClear = new List<Cell>();
            var endColor = _gridManager.ResourceRepository.GetSquare2248ImageByPow(_gridManager.CurrentPow).color;

            var x = _mergeCell.gridPosition.x;
            var y = _mergeCell.gridPosition.y;

            for (int i = 0; i < _gridModel.size.x; i++)
            {
                var pos = new Vector2Int(i, y);
                cellsToClear.Add(_cells[pos]);
                // FlyBonusCell(pos, flyTime, endColor);
                sum += _cells[pos].view.Value;
            }
            for (int j = 0; j < _gridModel.size.y; j++)
            {
                var pos = new Vector2Int(x, j);
                if (pos == _mergeCell.gridPosition)
                {
                    continue;
                }
                cellsToClear.Add(_cells[pos]);
                // FlyBonusCell(pos, flyTime, endColor);
                sum += _cells[pos].view.Value;
            }
            
            if (_gridManager.GetNearest2PowValue(sum, out var newPow) > _mergeCell.view.Value)
            {
                foreach (var cell in cellsToClear)
                {
                    FlyBonusCell(cell, flyTime, endColor);
                }
            }
            else
            {
                WaveBonus(ref sum, ref cellsToClear);
            }
            
            yield return new WaitForSeconds(flyTime);
            yield return EndBonus(sum, cellsToClear);
        }

        private IEnumerator DestroyByNumBonus()
        {
            LargeNumber sum = _mergeCell.view.Value;
            var flyTime = _gridManager.AnimParams.time * 2f;
            var cellsToClear = new List<Cell>();
            var endColor = _gridManager.ResourceRepository.GetSquare2248ImageByPow(_gridManager.CurrentPow).color;

            cellsToClear.Add(_mergeCell);
            FlyBonusCell(_mergeCell.gridPosition, flyTime, endColor);
            
            foreach (var cell in _cells.Values)
            {
                if (cell.view.Value == _sameValueOrder)
                {
                    cellsToClear.Add(cell);
                    // FlyBonusCell(cell, flyTime, endColor);
                    sum += _sameValueOrder;
                }
            }
            
            if (_gridManager.GetNearest2PowValue(sum, out var newPow) > _mergeCell.view.Value)
            {
                foreach (var cell in cellsToClear)
                {
                    FlyBonusCell(cell, flyTime, endColor);
                }
            }
            else
            {
                WaveBonus(ref sum, ref cellsToClear);
            }
            
            yield return new WaitForSeconds(flyTime);
            yield return EndBonus(sum, cellsToClear);
        }

        private IEnumerator EndBonus(LargeNumber sum, List<Cell> cellsToClear)
        {
            _taskService.AddStat(ETaskDataType.SquaresDestroyed, cellsToClear.Count);
            _taskService.AddStat(ETaskDataType.BonusesUsed, 1);
            foreach (var cell in cellsToClear)
            {
                cell.Clear();
            }
            _gridManager.PlayEndLine();
            var newValue = _gridManager.GetNearest2PowValue(sum, out var newPow);
            _linePow = newPow;
            _mergeCell.Clear();
            _mergeCell.SetFull(Instantiate(viewPrefab));
            _mergeCell.view.Init(newValue);
            _mergeCell.view.ParticleController.RunAppear();
            yield return _mergeCell.view.AnimateByTime(new UnitViewAnimParams()
            {
                time = _gridManager.AnimParams.time,
                aColor = 1,
                startRotation = _gridManager.AnimParams.endRotation,
                endRotation = 0,
                startScale = _gridManager.AnimParams.endScale,
                endScale = 1
            });
            if (_gridManager.MaxSquare < newValue)
            {
                _gridManager.MaxSquare = newValue;
                UpdateMax();
            }
            else if(_gridManager.MaxSquare == newValue)
            {
                _mergeCell.view.SetMaxFrame(true);
            }
            OnNewValueCreated.Invoke(newValue);
        }

        private void TryMoveDown(Cell cell)
        {
            var nextCell = cell;
            var nextY = cell.gridPosition.y;
            while (true)
            {
                nextY -= 1;
                if (nextY < 0 || nextY >= gridLayout.GridSize.y)
                {
                    break;
                }
                nextCell = _cells[new Vector2Int(cell.gridPosition.x, nextY)];
                if (!nextCell.IsFree)
                {
                    FullDownedCell(cell, nextCell);
                    break;
                }
            }
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

        private void ClearGrid(Action callback)
        {
            var keys = _cells.Keys.ToList();
            for (int i = 0; i < _cells.Count; i++)
            {
                var key = keys[i];
                Destroy(_cells[key].gameObject);
            }
            _cells.Clear();
            _gridStates.Clear();
            _gridManager.SaveGrid();
        
            canvasGroup.transform.localPosition = _gridManager.NextGridLocalPosition;
        
            callback?.Invoke();
        }

        private IEnumerator UpdatePow(int pow)
        {
            PowUpdateCondition powCondition = null;
            if(_updatePow != -1 && _updatePow <= _gridManager.CurrentPow)
            {
                for (int i = _updatePow; i <= _gridManager.CurrentPow; i++)
                {
                    yield return _squaresSpawnController.AddNext();
                    yield return _squaresSpawnController.RemoveMin();
                }

                _updatePow = _gridManager.CurrentPow + 1;
                _gridModel.nextPowUpdate = _updatePow;
            }
            else
            {
                powCondition =
                    _gridModel.PowUpdateConditions.ToList().Find(p => p.updatePow == pow);
                if (powCondition != null)
                {
                    yield return AddSpawn(powCondition.addPow);
                    yield return WaitNextFrameAndUpdate(powCondition, () => {});
                }
            }
        }

        private IEnumerator WaitNextFrameAndUpdate(PowUpdateCondition powCondition, Action OnEnd)
        {
            yield return new WaitForSeconds(0.5f);
            yield return RemoveSpawn(powCondition.deletePow);
            OnEnd.Invoke();
        }

        private IEnumerator AddSpawn(int pow)
        {
            if (pow > 0)
            {
                yield return _squaresSpawnController.TryAddSpawn(pow);
            }
        }
        
        private IEnumerator RemoveSpawn(int pow)
        {
            if (pow > 0)
            {
                yield return _squaresSpawnController.TryRemoveSpawn(pow);
                yield return ClearRemovedCells(pow);
            }
        }

        private void InitCells(GridModel gridModel)
        {
            gridLayout.SetSize(gridModel.size.y, gridModel.size.x);
            var columnCount = 0;
            var rowCount = 0;
            _squaresSpawnController.SetSpawns(gridModel.StartPows);
            _updatePow = -1;

            for (int i = 0; i < gridModel.size.x * gridModel.size.y; i++)
            {
                var cell = Instantiate(cellPrefab, gridLayout.transform);
                var gridPosition = new Vector2Int(columnCount, rowCount);
                var rectTransform = (RectTransform)cell.transform;
                cell.localPosition = rectTransform.localPosition;
                cell.worldPosition = rectTransform.position;
                cell.gridPosition = gridPosition;
                cell.SetFull(Instantiate(viewPrefab));

                LargeNumber value = LargeNumber.zero;
                var position = new Vector2Int(columnCount, rowCount);
                if (!gridModel.UnitPositions.TryGetValue(position, out value))
                {
                    value = _squaresSpawnController.GetRandomValue();
                }
                CheckMaxSquare(gridModel, value);
                cell.view.Init(value);
                cell.view.Animator.AnimateScalePingPong();

                cell.PointerDowned += OnPointerDown;
                cell.PointerUp += OnPointerUp;
                cell.PointerEnter += OnPointerEnter;

                _cells.Add(gridPosition, cell);

                columnCount++;
                columnCount %= gridLayout.GridSize.x;
                if (columnCount == 0)
                {
                    rowCount++;
                    rowCount %= gridLayout.GridSize.y;
                }
            }

            while (!TryFindMergePossibilities())
            {
                foreach (var cell in _cells)
                {
                    cell.Value.Clear();
                    var value = _squaresSpawnController.GetRandomValue();
                    cell.Value.SetFull(Instantiate(viewPrefab));
                    cell.Value.view.Init(value);
                    CheckMaxSquare(gridModel, value);
                    cell.Value.view.Animator.AnimateScalePingPong();
                }
            }
            
            
            if (gridModel.PowUpdateConditions.Count < 1)
            {
                if (gridModel.nextPowUpdate != -1)
                {
                    _updatePow = gridModel.nextPowUpdate;
                }
                else
                {
                    _updatePow = _gridManager.CurrentPow + 2;
                    _gridModel.nextPowUpdate = _updatePow;
                }
            }

            foreach (var cell in _cells.Values)
            {
                cell.view.SetMaxFrame(cell.view.Value == _gridManager.MaxSquare);
            }

            _gridManager.SaveGrid();
        }

        private void CheckMaxSquare(GridModel gridModel, LargeNumber value)
        {
            if (_gridManager.MaxSquare < value)
            {
                if (gridModel.taskModel.type == ETaskType.GetCellWithValue)
                {
                    while (gridModel.taskModel.valueLarge <= value)
                    {
                        value /= 2;
                    }

                    if (_gridManager.MaxSquare < value)
                    {
                        _gridManager.MaxSquare = value;
                    }
                }
                else
                {
                    _gridManager.MaxSquare = value;
                }
                        
                _gridManager.GetNearest2PowValue(value, out var pow);
                _gridManager.CurrentPow = pow;
            }
        }

        private IEnumerator LineFollowPointer()
        {
            while (IsDowned)
            {
                var pointerPosition = _gridManager.Camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                _lines.Last().SetPosition(1,  new Vector3(pointerPosition.x, pointerPosition.y, 0));
                yield return null;
            }
        }
        
        private IEnumerator CheckActivity()
        {
            while (_isActive)
            {
                if (!IsDowned && !_isSquareRemoving && !_isSquareSwap &&
                    Time.time > _lastActivityTime + _gridManager.TimeToClue && !_isAnimateScalePulse)
                {
                    _mergeClue = AnimateScalePulse(_gridManager.ClueAnimationCurve);
                    StartCoroutine(_mergeClue);
                }

                yield return null;
            }
        }

        protected virtual IEnumerator FullFreeCells()
        {
            var cells = _cells.Values.ToList().Where(c => c.IsFree).ToList();
            foreach(var cell in cells)
            {
                cell.SetFull(Instantiate(viewPrefab));
                var val = _squaresSpawnController.GetRandomValue();
                cell.view.Init(val, val == _gridManager.MaxSquare);
                cell.view.SetMaxFrame(cell.view.Value == _gridManager.MaxSquare);

                var localPosition = cell.view.transform.localPosition;
                localPosition = new Vector3(
                    localPosition.x,
                    localPosition.y + ((cell.transform as RectTransform).sizeDelta.y * _gridModel.size.y),
                    localPosition.z);
                cell.view.transform.localPosition = localPosition;

                if(cell == cells.Last())
                {
                    yield return new WaitForCallback(callback => { cell.view.Animator.AnimateFlying(cell, callback); });
                }
                else
                {
                    cell.view.Animator.AnimateFlying(cell);
                }
            }
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