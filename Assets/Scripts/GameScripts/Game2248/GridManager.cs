using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Advertising;
using Core.Anchors;
using Core.Audio;
using Core.Localization;
using Core.Repositories;
using Core.SaveLoad;
using Core.Windows;
using DG.Tweening;
using GameScripts.AnalyticsSignals;
using GameScripts.Game2248.Tasks;
using GameStats;
using JetBrains.Annotations;
using LargeNumbers;
using LuckyWheel;
using QFSW.QC.Actions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace GameScripts.Game2248
{
    [Serializable]
    public class GridTaskData : GameScripts.MergeSquares.GridTaskData
    {
        public LargeNumber CurrentValueLarge { get; set; } = LargeNumber.zero;
    }
    
    [Serializable]
    public class CellData
    {
        public Vector2Int position;
        public LargeNumber valueLarge = LargeNumber.zero;
    }
    
    public class GridData
    {
        public GridTaskData task = new();
        public List<CellData> cellsLarge = new();
        public int level;
        public Vector2Int size = Vector2Int.zero;
        public List<ESquareSkin> openedSkins;
        public ESquareSkin currentSkin;
        public float scaleProgress;
        public float giftProgress;
    }
    
    [Serializable]
    public class BonusView
    {
        public EBonusType bonusType;
        public Sprite sprite;
    }

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private ResourceRepository resourceRepository;
        [SerializeField] private SoundSource crit;
        [SerializeField] private SoundSource endLine;
        [SerializeField] private SoundSource wave;
        [SerializeField] private SoundSource slide;
        [SerializeField] private TaskScoresView taskScoresView;
        [SerializeField] private List<GridView> grids;
        [SerializeField] private LocalizeUi levelView;
        [SerializeField] private float gridChangeTime;
        [SerializeField] private float inactiveGridAlpha;
        [SerializeField] private Vector2 endFlyPosition;
        [SerializeField] private RectTransform targetView;
        [SerializeField] private Saver saver;
        [SerializeField] private LevelRepository levelRepository;
        [SerializeField] private Button squareRemovingButton;
        // [SerializeField] private Button stopRemoveWallsButton;
        [SerializeField] private Button swapSquaresButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private GameObject stopSwapSquaresButton;
        [SerializeField] private GameObject stopSquareRemovingButton;
        [SerializeField] private int wallOverlaySortingOrder;
        [SerializeField] private int wallSortingOrder;
        [SerializeField] private int authAfterLevel = 2;
        [SerializeField] private Camera _camera;
        [SerializeField] private float timeToClue;
        [SerializeField] private float animationPulseDuration;
        [SerializeField] private AnimationCurve scaleAnimationCurve;
        [SerializeField] private AnimationCurve clueAnimationCurve;
        [SerializeField] private UnitViewAnimParams animParams;
        [SerializeField] private UnitView nextSquare;
        [SerializeField] private GameObject lockLayer;
        [SerializeField] private Image ratingScale;
        [SerializeField] private Button ratingLevelButton;
        [SerializeField] private Image bonusView;
        [SerializeField] private ParticleSystem bonusEndParticles;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [SerializeField] private bool isRatingActive;
        [SerializeField] private List<BonusView> bonusesSprites = new List<BonusView>();

        public void PlayCrit() => crit.Play();
        public void PlayWave() => wave.Play();
        public void PlaySlide() => slide.Play();

        [Space][SerializeField] private TMP_InputField cheatLevelInput;
        
        public Action OnNextGrid = () => { };
        public Action OnInited = () => { };
        public Action OnStartLevel = () => { };
        
        public float AnimationPulseDuration => animationPulseDuration;
        public UnitViewAnimParams AnimParams => animParams;
        public AnimationCurve ScaleAnimationCurve => scaleAnimationCurve;
        public AnimationCurve ClueAnimationCurve => clueAnimationCurve;
        public ResourceRepository ResourceRepository => resourceRepository;
        public int CurrentLevel => _currentLevel;
        public float GridChangeTime => gridChangeTime;
        public float TimeToClue => timeToClue;
        public float InactiveGridAlpha => inactiveGridAlpha;
        public Vector3 EndFlyPosition => endFlyPosition;
        public Vector2 NextGridLocalPosition => _nextGridLocalPosition;
        public Vector3 CurrentGridLocalPosition => _currentGridLocalPosition;
        
        public TaskScoresView TaskScoresView => taskScoresView;
        public GridView CurrentGridView => grids[_currentGrid];

        public ESquareSkin CurrentSkin => _currentSkin;
        public List<ESquareSkin> OpenedSkins => _openedSkins;
        public Button SquareRemovingButton => squareRemovingButton;
        public Button SwapSquaresButton => swapSquaresButton;
        public Camera Camera => _camera;
        public int WallSortingOrder => wallSortingOrder;
        public bool IsLocked { get; set; } = false;
        public int NextLevel { get; set; } = -1;
        public int CurrentPow { get; set; } = 1;
        public float GiftProgress { get; set; } = 0;
        public bool LevelEnds { get; private set; } = false;
        public LargeNumber MaxSquare { get; set; } = new LargeNumber(2);
        
        public bool UnlockedButtons => _openedSkins.Count > 1;

        private ESquareSkin _currentSkin;
        private List<ESquareSkin> _openedSkins;
        private int _currentGrid = 0;
        private int _currentLevel = 1;
        private bool _clearGridSave = false;
        private bool _winPopupShowing;
        private Vector2 _targetViewAnchor;
        private Vector2 _targetViewPosition;
        private Vector3 _nextGridLocalPosition;
        private Vector3 _currentGridLocalPosition;
        private GridData _savingData = new GridData();
        private float _scaleFull = 100;
        private float _scaleProgress = 0;

        private SignalBus _signalBus;
        private GameStatService _gameStatService;
        private GameStatLargeService _gameStatLargeService;
        private WindowManager _windowManager;
        private TaskService _taskService;
        private AdvertisingService _advertisingService;
        private WheelService _wheelService;

        [Inject]
        public void Construct(WindowManager windowManager, GameStatService gameStatService,
            GameStatLargeService gameStatLargeService, SignalBus signalBus,
            SquaresSpawnController squaresSpawnController, TaskService taskService, AdvertisingService advertisingService, WheelService wheelService
        )
        {
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            cheatLevelInput.onEndEdit.AddListener(OnCheatEndEdit);

            _signalBus = signalBus;
            _gameStatService = gameStatService;
            _gameStatLargeService = gameStatLargeService;
            _windowManager = windowManager;
            _taskService = taskService;
            _advertisingService = advertisingService;
            _wheelService = wheelService;
            _advertisingService.RewardAdRewarded += AdFinished;
            _advertisingService.FullScreenAdRewarded += AdFinished;
            _wheelService.SetGiftProgress += value => GiftProgress = value;

            foreach (var grid in grids)
            {
                grid.OnNewValueCreated += OnNewValueCreated;
                grid.LineEnded += OnLineEnded;
            }
        }

        private void Start()
        {
            _targetViewAnchor = targetView.anchoredPosition;
            _targetViewPosition = targetView.position;
            _currentGridLocalPosition = grids[0].LocalPosition;
            _nextGridLocalPosition = grids[1].LocalPosition;
            taskScoresView.UpdateSkin(_currentSkin);
            SetBonusView(EBonusType.None);
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
            
            _advertisingService.RewardAdRewarded -= AdFinished;
            _advertisingService.FullScreenAdRewarded -= AdFinished;

            foreach (var grid in grids)
            {
                grid.OnNewValueCreated -= OnNewValueCreated;
                grid.LineEnded -= OnLineEnded;
            }
        }

        [UsedImplicitly]
        public void SetExternalSkin()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var values = new List<int> {2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048};
            var filePath = Application.dataPath + "/" + "StreamingAssets" + "/";
            bool TryGetSprite(string path, out Sprite sprite)
            {
                Debug.Log($"[GridManager][TryGetSprite] path: {path}");
                if (File.Exists(path))
                {
                    Debug.Log($"[GridManager][TryGetSprite] found file");
                    var fileData = File.ReadAllBytes(path);
                    var tex = new Texture2D(2, 2);
                    //..this will auto-resize the texture dimensions.
                    tex.LoadImage(fileData);
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    return true;
                }

                sprite = null;
                return false;
            }
            Debug.Log($"[GridManager][SetExternalSkin] **************** START LOAD sprites");
            foreach (var value in values)
            {
                var path = filePath + value + ".png";
                if (TryGetSprite(path, out var sprite))
                {
                    resourceRepository.AddSquareImage2248(value, sprite, false);
                }
            }
            Debug.Log($"[GridManager][SetExternalSkin] **************** START LOAD frame");
            var framePath = filePath + "frame.png";
            Debug.Log($"[GridManager][SetExternalSkin] path: {framePath}");
            if (TryGetSprite(framePath, out var spriteFrame))
            {
                foreach (var value in values)
                {
                    resourceRepository.AddSquareImage2248(value, spriteFrame, true);
                }
            }
            SetSkin(ESquareSkin.external);
#endif
        }

        private void Init(GridData savingData)
        {
            _openedSkins = savingData.openedSkins;
            SetScaleProgress(savingData.scaleProgress);
            GiftProgress = savingData.giftProgress;
            _savingData = savingData;
            _currentLevel = savingData.level;

            _currentSkin = _savingData.currentSkin;
            SetButtons();
            SetFirstGrid();
            OnInited.Invoke();
        }
        
        public void PlayEndLine() => endLine.Play();

        public void ShowNextSquare() => nextSquare.gameObject.SetActive(true);
        
        public void HideNextSquare() => nextSquare.gameObject.SetActive(false);
        
        public void UpdateNextSquare(LargeNumber value) => nextSquare.Init(value);

        public void AddScaleProgress(float value) => SetScaleProgress(value + _scaleProgress);
        public void SaveSize(Vector2Int value) => _savingData.size = value;

        public void SetScaleProgress(float value)
        {
            if (!isRatingActive)
            {
                return;
            }
            if (ratingLevelButton.gameObject.activeInHierarchy)
            {
                return;
            }

            var startScale = _scaleProgress;
            
            var scaleFulling = DOTween.To(() => startScale, newAlpha =>
            {
                ratingScale.fillAmount = newAlpha / _scaleFull;
            }, value, 1f);

            scaleFulling.OnKill(() =>
            {
                if (_scaleProgress >= _scaleFull)
                {
                    SetScaleProgress(0);
                    ratingLevelButton.gameObject.SetActive(true);
                }
            });
            _scaleProgress = value;
            _savingData.scaleProgress = _scaleProgress;
            SaveGrid();
        }

        public void RemoveSquare()
        {
            CurrentGridView.StartRemoveSquare();
        }

        public void SwapSquares()
        {
            CurrentGridView.StartSwapSquares();
        }

        public void SetBonusView(EBonusType bonusType)
        {
            if(bonusType == EBonusType.None)
                bonusView.gameObject.SetActive(false);
            else
            {
                var bonusViewSprite = bonusesSprites.Find(b => b.bonusType == bonusType).sprite;
                if (!bonusView.GameObject().activeInHierarchy)
                {
                    bonusView.gameObject.SetActive(true);
                    StartCoroutine(AnimateBonus());
                }
                bonusView.sprite = bonusViewSprite;
            }
        }

        public void FlyBonus(Vector3 position, Action callback)
        {
            var startPos = bonusView.transform.position;
            var move = DOTween.To(() => bonusView.transform.position, newPosition =>
            {
                bonusView.transform.position = newPosition;
            }, position, 0.5f);
        
            move.onKill += () =>
            {
                bonusEndParticles.transform.position = bonusView.transform.position;
                bonusEndParticles.Play();
                bonusView.transform.position = startPos;
                SetBonusView(EBonusType.None);
                callback?.Invoke();
            };
        }

        public void StartRatingLevel()
        {
            if (!CurrentGridView.AvailableForClick)
            {
                return;
            }
            ratingLevelButton.gameObject.SetActive(false);
            _windowManager.ShowWindow(EPopupType.RatingLevel.ToString());
            _windowManager.TryGetWindow(EPopupType.RatingLevel.ToString(), out RatingLevelGrid bonusLevelGrid);
            bonusLevelGrid.InitCells(new GridModel()
            {
                id = -1,
                size = new Vector2Int(5, 7),
                taskModel = new TaskModel()
                {
                    type = ETaskType.CollectPoints,
                    valueLarge = new LargeNumber(100),
                },
                units = new List<UnitModel>(),
                startPows = new List<int>(){1,2,3,4,5}
            });
        }
        
        public void RestorePlatePosition()
        {
            targetView.anchoredPosition = _targetViewAnchor;
            targetView.position = _targetViewPosition;
        }

        public void SetSkin(ESquareSkin skin)
        {
            _currentSkin = skin;
            _savingData.currentSkin = _currentSkin;
            CurrentGridView.SetNewSkin(skin);
            taskScoresView.UpdateSkin(skin);
            SaveGrid();
        }

        public void ActivateClueDestroyOverlay()
        {
            stopSquareRemovingButton.SetActive(true);
            lockLayer.SetActive(true);
            squareRemovingButton.GetComponent<Anchor>().SetSorting(sortingLayerName, sortingOrder);
            foreach (var cell in CurrentGridView.Cells)
            {
                cell.Value.anchor.SetSorting(sortingLayerName, sortingOrder);
                cell.Value.view.Canvas.overrideSorting = false;
            }
        }
        
        public void ActivateClueSwapOverlay()
        {
            stopSwapSquaresButton.SetActive(true);
            lockLayer.SetActive(true);
            swapSquaresButton.GetComponent<Anchor>().SetSorting(sortingLayerName, sortingOrder);
            foreach (var cell in CurrentGridView.Cells)
            {
                cell.Value.anchor.SetSorting(sortingLayerName, sortingOrder);
                cell.Value.view.Canvas.overrideSorting = false;
            }
        }

        public void DeactivateClueOverlay()
        {
            lockLayer.SetActive(false);
            stopSquareRemovingButton.SetActive(false);
            stopSwapSquaresButton.SetActive(false);
            shopButton.interactable = UnlockedButtons;
            squareRemovingButton.interactable = UnlockedButtons;
            swapSquaresButton.interactable = UnlockedButtons;
            swapSquaresButton.GetComponent<Anchor>().ResetSorting();
            squareRemovingButton.GetComponent<Anchor>().ResetSorting();
            foreach (var cell in CurrentGridView.Cells)
            {
                cell.Value.anchor.ResetSorting();
                var view = cell.Value.view;
                if (view != null)
                {
                    view.Canvas.overrideSorting = true;
                }
            }
        }
        
        public void OpenSkin(ESquareSkin skin)
        {
            _openedSkins.Add(skin);
            _savingData.openedSkins = _openedSkins;
        }
        //
        // public void StopRemoveWalls()
        // {
        //     CurrentGridView.StopRemoveWalls();
        // }

        public LargeNumber GetNearest2PowValue(LargeNumber x, out int pow)
        {
            if (x <= LargeNumber.zero)
            {
                Debug.LogError($"[GridManager][GetNearest2PowValue] Invalid value - 0");
            }
            var operateX = MaxSquare;
            var res = MaxSquare;
            var counter = 0;
            if (x > MaxSquare)
            {
                while (operateX < x)
                {
                    operateX *= 2;
                    counter++;
                }

                var sepOperate = operateX / 2;
                var minDif = x - sepOperate;
                var maxDif = operateX - x;

                if (maxDif <= minDif)
                {
                    res = operateX;
                }
                else
                {
                    res = sepOperate;
                }

            }
            else if(x < MaxSquare)
            {
                while (operateX > x)
                {
                    operateX /= 2;
                    counter++;
                }
                
                var doubOperate = operateX * 2;
                var minDif = x - operateX;
                var maxDif = doubOperate - x;
                if (minDif < maxDif)
                {
                    res = operateX;
                }
                else
                {
                    res = doubOperate;
                }
            }
            else
            {
                res = new LargeNumber(MaxSquare);
            }
            pow = (int)Math.Log(res, 2);
            return res;
        }

        public void SaveGrid()
        {
            _savingData.cellsLarge.Clear();
            if (!_clearGridSave)
            {
                SaveCells();
            }
            else
            {
                _savingData.task.CurrentValueLarge = LargeNumber.zero;
                var delta = _gameStatLargeService.GetStatValue(EGameStatType.Points);
                _gameStatLargeService.TrySet(EGameStatType.Points, LargeNumber.zero);
                _gameStatLargeService.TryAddLocalDelta(EGameStatType.Points, delta);
            }
            saver.SaveNeeded.Invoke(true);
        }
        
        public void ShowSquareSliderAdd(SquareSliderParams sliderParams)
        {
            _windowManager.EnsureOpen(EPopupType.SquaresSliderNew.ToString(), new[] { sliderParams });
        }
        
        public void ShowSquareSliderRemove(SquareSliderParams sliderParams)
        {
            _windowManager.EnsureOpen(EPopupType.SquaresSliderRemove.ToString(), new[] { sliderParams });
        }

        private void SetFirstGrid()
        {
            GridModel model = null;
            if(!levelRepository.TryGetById(_currentLevel, ref model))
            {
                model = GenerateLevel(_currentLevel);
            }
            _savingData.task.type = model.taskModel.type;
            if (_savingData.cellsLarge.Count > 0)
            {
                CurrentGridView.Init(model, _savingData);
            }
            else
            {
                CurrentGridView.Init(model);
            }
        
            levelView.UpdateArgs(new[] { _currentLevel.ToString() });
            // CurrentGridView.SwipeActiveGridAnimated();
            CurrentGridView.SetActive();
            _gameStatLargeService.TrySet(EGameStatType.TargetPoints, model.taskModel.valueLarge);
            if (_savingData.task.type == ETaskType.GetCellWithValue)
                _gameStatLargeService.TrySet(EGameStatType.Points, MaxSquare);
            taskScoresView.Init(model.taskModel, _gameStatLargeService.GetStat(EGameStatType.Points).RealValue);

            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Started, _currentLevel));
        }
        
        public void NextGridDebug()
        {
            FinishGrid();
        }

        private void NextGrid()
        {
            SetButtons();
            SetEnvironmentInteractable(false);
            _clearGridSave = false;
            
            _gameStatLargeService.ResetLocalDeltaWithAnim(EGameStatType.Points);
            taskScoresView.AnimateWin();
            _taskService.AddStat(ETaskDataType.LevelCompleteCount, 1);
        
            _currentGrid++;
            if (_currentGrid >= grids.Count)
                _currentGrid = 0;
            CurrentGridView.gameObject.SetActive(true);
            GridModel model = null;
            if(!levelRepository.TryGetById(_currentLevel, ref model))
            {
                model = GenerateLevel(_currentLevel);
            }
            _savingData.task.type = model.taskModel.type;
            MaxSquare = new LargeNumber(2);
            CurrentPow = 1;
            CurrentGridView.Init(model);
            _gameStatLargeService.TrySet(EGameStatType.TargetPoints, model.taskModel.valueLarge);
            PlaySlide();
            CurrentGridView.SwipeActiveGridAnimated();
            _savingData.level = _currentLevel; 
            levelView.UpdateArgs(new[] { _currentLevel.ToString() });
            RestorePlatePosition();
            OnNextGrid.Invoke();
            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Started, _currentLevel));
        }
        
        private void SetEnvironmentInteractable(bool interactable)
        {
            IsLocked = !interactable;
            // SetButtonsInteractable(!interactable);
            if (!interactable)
                CurrentGridView.SetNotActive();
        }

        public void SetButtons()
        {
            shopButton.interactable = UnlockedButtons;
            squareRemovingButton.interactable = UnlockedButtons;
            swapSquaresButton.interactable = UnlockedButtons;
        }

        private void RestartGrid()
        {
            StartGridWithCurrentLevel();
        }

        private void SkipGrid()
        {
            _currentLevel++;
            _savingData.level++;
        
            StartGridWithCurrentLevel();
        }

        private void StartGridWithCurrentLevel()
        {
            _savingData.task.CurrentValueLarge = LargeNumber.zero;
            var currentPoints = _gameStatLargeService.GetStatValue(EGameStatType.Points);
            CurrentGridView.Finish(() => IsLocked = false);
            NextGrid();
            DownPoints(currentPoints);
            _gameStatLargeService.TrySetWithAnim(EGameStatType.Points, LargeNumber.zero);
            SaveGrid();
        }

        private IEnumerator AnimateBonus()
        {
            float t = 0;
            var mod = 2;
            while (bonusView.GameObject().activeInHierarchy)
            {
                if (t >= AnimationPulseDuration / mod)
                {
                    t = 0;
                }
                bonusView.transform.localScale = Vector3.one * scaleAnimationCurve.Evaluate(t / animationPulseDuration * mod);
                t += Time.deltaTime;
                yield return null;
            }
        }
        // private void OnStatChanged(EGameStatType type, LargeNumber value)
        // {
        //     
        // }

        private void FinishGrid()
        {
            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Passed, _currentLevel));
            SetEnvironmentInteractable(false);
            if(NextLevel > 0)
            {
                _currentLevel = NextLevel;
            }
            else
            {
                _currentLevel++;
            }
            _savingData.level = _currentLevel;
            _clearGridSave = true;
            var currentPoints = _gameStatLargeService.GetStatValue(EGameStatType.Points);
            SaveGrid();
            taskScoresView.AnimateWin(OnAnimationFinished);
        
            void OnAnimationFinished()
            {
                ShowWinPopup(() =>
                {
                    CurrentGridView.Finish(() =>
                    {
                        IsLocked = false;
                        LevelEnds = false;
                        OnStartLevel.Invoke();
                    });
                    NextGrid();
                    DownPoints(currentPoints);
                });
            }
        }

        private void DownPoints(LargeNumber points)
        {
            if (taskScoresView.TaskType == ETaskType.GetCellWithValue)
            {
                StartCoroutine(DownSquaresTarget(points));
            }
            else
            {
                _gameStatLargeService.TrySetWithAnim(EGameStatType.Points, LargeNumber.zero);
            }
        }

        private void SaveCells()
        {
            foreach (var pair in CurrentGridView.Cells)
            {
                if (!pair.Value.IsFree)
                {
                    _savingData.cellsLarge.Add(new CellData { position = pair.Key, valueLarge = pair.Value.view.Value });
                }
            }
        }

        private IEnumerator DownSquaresTarget(LargeNumber points)
        {
            var duration = 1f;
            var steps = (int)Math.Sqrt(points);
            var timer = 0f;
            var stepTime = duration / steps;
            var stepCounter = 1;
        
            while (timer < duration && points > 2)
            {
                timer += Time.deltaTime;
                if (timer >= stepTime * stepCounter)
                {
                    stepCounter++;
                    points /= 2;
                    taskScoresView.UpdateScore(points);
                }
                yield return null;
            }
            yield return null;
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new GridData()
            {
                level = 1,
                currentSkin = ESquareSkin.baseSprite,
                openedSkins = new List<ESquareSkin> { ESquareSkin.baseSprite },
            }));
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_savingData);
        }

        private void OnNewValueCreated(LargeNumber value)
        {
            switch (_savingData.task.type)
            {
                case ETaskType.GetCellWithValue:
                    _gameStatLargeService.TrySetWithAnim(EGameStatType.Points, MaxSquare);
                    if (_savingData.task.CurrentValueLarge < MaxSquare)
                    {
                        _savingData.task.CurrentValueLarge = MaxSquare;
                        taskScoresView.UpdateScore(MaxSquare);
                    }
                    if (MaxSquare >= CurrentGridView.Task.valueLarge)
                    {
                        LevelEnds = true;
                    }

                    break;
                case ETaskType.MakeLines:
                    _gameStatLargeService.TryIncWithAnim(EGameStatType.Points, new LargeNumber(1));
                    _savingData.task.CurrentValueLarge = value;
                    if (_gameStatLargeService.GetStatLarge(EGameStatType.Points).RealValue
                        >= _gameStatLargeService.GetStatLarge(EGameStatType.TargetPoints).RealValue)
                    {
                        LevelEnds = true;
                    }

                    break;
                case ETaskType.CollectPoints:

                    _gameStatLargeService.TryIncWithAnim(EGameStatType.Points, value);
                    _savingData.task.CurrentValueLarge = value;

                    if (_gameStatLargeService.GetStatLarge(EGameStatType.Points).RealValue
                        >= _gameStatLargeService.GetStatLarge(EGameStatType.TargetPoints).RealValue)
                    {
                        LevelEnds = true;
                    }

                    break;
                case ETaskType.Endless:
                    _gameStatLargeService.TryIncWithAnim(EGameStatType.Points, value);
                    _savingData.task.CurrentValueLarge = value;
                    break;
            }

            AddScaleProgress(34);
            SaveGrid();
        }

        private void OnLineEnded()
        {
            StartCoroutine(CheckOnLevelEnd());
        }

        private IEnumerator CheckOnLevelEnd()
        {
            if (!LevelEnds)
            {
                yield return CurrentGridView.UpdatePows();
                if (!CurrentGridView.TryFindMergePossibilities())
                {
                    yield return new WaitForSeconds(2f);
                    ShowFailPopup();
                }
                else
                {
                    CurrentGridView.SetActive();
                }
            }
            else
            {
                FinishGrid();
            }
        }

        private void ShowWinPopup(Action callback = null)
        {
            if (!_winPopupShowing)
            {
                _winPopupShowing = true;
                // var showedLevel = _currentLevel - 1; // decrement because next level id saves before
                // GridModel model = null;
                // if(!levelRepository.TryGetById(showedLevel, ref model))
                // {
                //     model = GenerateLevel(showedLevel);
                // }
                var prevProgress = GiftProgress;
                GiftProgress += 0.25f;
                _savingData.giftProgress = GiftProgress;
                var winParams = new WinPopupParams
                {
                    showedLevel = _currentLevel - 1,
                    winBonus = CurrentGridView.GridModel.reward,
                    giftBonus = CurrentGridView.GridModel.reward * 3,
                    ClosePopup =
                        () =>
                        {
                            _winPopupShowing = false;
                            callback?.Invoke();
                        },
                    wheelId = "1",
                    startGiftProgress = prevProgress,
                    giftProgress = GiftProgress,
                };
                _windowManager.ShowWindow(EPopupType.LevelWin.ToString(), new[] { winParams });
                WinPopup winPopup;
                _windowManager.TryGetWindow(EPopupType.LevelWin.ToString(), out winPopup);
                winPopup.FullScreenAdsEnables = UnlockedButtons;
                winPopup.Ready();
            }
        }

        private void ShowFailPopup()
        {
            var failParams = new FailPopupParams
            {
                OnRestarted = RestartGrid,
                OnSkipped = SkipGrid
            };
        
            _windowManager.ShowWindow(EPopupType.LevelFail.ToString(), new[] { failParams });
            FailPopup failPopup;
            _windowManager.TryGetWindow(EPopupType.LevelFail.ToString(), out failPopup);
            failPopup.FullScreenAdsEnables = UnlockedButtons;
        }
        
        private void AdFinished()
        {
            _taskService.AddStat(ETaskDataType.AdShowed, 1);
        }
        
        private void OnCheatEndEdit(string cheat)
        {
            if (Int32.TryParse(cheat, out var level) && level >= 0)
            {
                NextLevel = level;
            }
            else
            {
                NextLevel = -1;
            }
        }

        private GridModel GenerateLevel(int seed)
        {
            var rnd = new LCRandom( seed );
            var ableTasks = new List<ETaskType>
                { ETaskType.CollectPoints, ETaskType.MakeLines, ETaskType.GetCellWithValue };
            var type = ableTasks[rnd.Range32(0, ableTasks.Count)];
            var valueLarge = LargeNumber.zero;
            var lvlMultiplier = 1 + _currentLevel * 0.1f;

            switch (type)
            {
                case ETaskType.CollectPoints:
                    valueLarge = new LargeNumber(rnd.Range32(1, 6) * 1000 * lvlMultiplier);
                    break;
                case ETaskType.MakeLines:
                    valueLarge =  new LargeNumber((int)(rnd.Range32(5, 16) * lvlMultiplier));
                    break;
                case ETaskType.GetCellWithValue:
                    int minTargetPow = 6 + (int)lvlMultiplier;
                    int maxTargetPow = 11 + (int)lvlMultiplier;
                    valueLarge = new LargeNumber(Math.Pow(2, rnd.Range32(minTargetPow, maxTargetPow)));
                    break;
            }
            

            var pows = new List<int>();
            var minSpawnPow = 1 + (int)lvlMultiplier;
            var maxSpawnPow = rnd.Range32(3,6) + (int)lvlMultiplier;

            for (int i = minSpawnPow; i <= maxSpawnPow; i++)
            {
                pows.Add(i);
            }

            var genReward = rnd.Range32(1, 5) * 10;
            
            var model = new GridModel
            {
                startPows = pows,
                id = seed,
                size = new Vector2Int(rnd.Range32(4, 8), rnd.Range32(5, 9)),
                taskModel = new TaskModel
                {
                    type = type,
                    valueLarge = valueLarge,
                },
                units = new List<UnitModel>(),
                reward = genReward,
            };
            return model;
        }
    }
}