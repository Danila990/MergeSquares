using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advertising;
using CloudServices;
using Core.Audio;
using Core.SaveLoad;
using TMPro;
using UnityEngine;
using Zenject;
using GameScripts.AnalyticsSignals;
using GameStats;
using GameScripts.MergeSquares.Models;
using Core.Windows;
using UnityEngine.UI;
using Core.Localization;
using Core.Repositories;
using GameScripts.MergeSquares.InfinityLevel;
using GameScripts.MergeSquares.Shop;
using GameScripts.MergeSquares.Tasks;
using GameScripts.MergeSquares.Tutorial;
using JetBrains.Annotations;
using Tutorial;
using UI;
using Utils;
using Enum = System.Enum;
using Random = UnityEngine.Random;
using TaskModel = GameScripts.MergeSquares.Models.TaskModel;

namespace GameScripts.MergeSquares
{
    [Serializable]
    public class GridTaskData
    {
        public int currentValue = 0;
        public ETaskType type;
    }

    [Serializable]
    public class CellData
    {
        public Vector2Int position;
        public int value = -1;
    }

    [Serializable]
    public class SkinSaveData
    {
        public ESquareSkin skinType;
        public int count = 1;
    }

    [Serializable]
    public class GridData
    {
        public InfinityGridData infinityGridData = new();
        public List<CellData> cells = new();
        public int nextValue;
        public int level;
        public bool ratingUnlocked;
        public List<SkinSaveData> openedSkins;
        public ESquareSkin currentSkin;
        private GridTaskData task = new();

        public GridTaskData Task
        {
            get { return task; }
            set { task = value; }
        }
    }

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private ResourceRepository _resourceRepository;
        [SerializeField] private SoundSource merge;
        [SerializeField] private SoundSource wave;
        [SerializeField] private SoundSource slide;
        [SerializeField] private TaskScoresView taskScoresView;
        [SerializeField] private List<GridView> grids;
        [SerializeField] private LocalizeUi levelView;
        [SerializeField] private int minPoints;
        [SerializeField] private int maxPoints;
        [SerializeField] private float gridChangeTime;
        [SerializeField] private float inactiveGridAlpha;
        [SerializeField] private Vector2 endFlyPosition;
        // [SerializeField] private float targetViewFlyTime = 1f;
        [SerializeField] private RectTransform targetView;
        [SerializeField] private Saver saver;
        [SerializeField] private MergeSquaresLevelRepository levelRepository;
        [SerializeField] private ButtonGroup wallsRemovingGroup;
        [SerializeField] private ButtonGroup changeGroup;
        [SerializeField] private ButtonGroup shopGroup;
        [SerializeField] private Button tasksButton;
        [SerializeField] private Button stopRemoveWallsButton;
        [SerializeField] private Button stopChangeButton;
        [SerializeField] private int wallOverlaySortingOrder;
        [SerializeField] private int wallSortingOrder;
        [SerializeField] private int authAfterLevel = 2;
        [SerializeField] private SquaresSkinsManager _squaresSkinsManager;
        
        public void PlayMerge() => merge.Play();
        public void PlayWave() => wave.Play();
        public void PlaySlide() => slide.Play();

        [Space][SerializeField] private TMP_InputField cheatLevelInput;
        
        public Action OnNextGrid = () => { };
        public Action OnInited = () => { };
        public Action OnStartLevel = () => { };
        public Action OnFreeCellClicked = () => { };

        public int CurrentLevel => _currentLevel;
        public float GridChangeTime => gridChangeTime;
        public float InactiveGridAlpha => inactiveGridAlpha;
        public (int, int) MinMaxPoints => (minPoints, maxPoints);
        public Vector3 EndFlyPosition => endFlyPosition;
        public Vector2 NextGridLocalPosition => _nextGridLocalPosition;
        public Vector3 CurrentGridLocalPosition => _currentGridLocalPosition;
        public GridView CurrentGridView => grids[_currentGrid];
        public ESquareSkin CurrentSkin => _currentSkin;
        public List<SkinSaveData> OpenedSkins => _openedSkins;
        public ButtonGroup WallsRemovingButton => wallsRemovingGroup;
        public TaskScoresView TaskScoresView => taskScoresView;

        public int WallOverlaySortingOrder => wallOverlaySortingOrder;
        public InfinityGridData InfinityGridData => _savingData.infinityGridData;
        
        public ButtonGroup ChangeButton => changeGroup;
        public int WallSortingOrder => wallSortingOrder;
        public bool RatingUnlocked => _ratingUnlocked;
        public bool IsLocked { get; set; } = false;
        public int NextLevel { get; set; } = -1;
        public bool UnlockedButtons => _currentLevel > 7 || _tutorialService.CheckTutorialFinished(tutorial => ((SquaresTutorial)tutorial).TutorialDesc.type == ETutorialDescType.WallChangeShop);
        public bool TutorialEnabled => _tutorialService.HasActiveTutorial;

        private ESquareSkin _currentSkin;
        private List<SkinSaveData> _openedSkins;
        private int _currentGrid = 0;
        private int _currentLevel = 1;
        private bool _clearGridSave = false;
        private bool _levelFinished = false;
        private bool _winPopupShowing;
        public bool _ratingUnlocked = false;
        private Vector2 _targetViewAnchor;
        private Vector2 _targetViewPosition;
        private Vector3 _nextGridLocalPosition;
        private Vector3 _currentGridLocalPosition;
        private GridData _savingData = new();

        private SignalBus _signalBus;
        private GameStatService _gameStatService;
        private WindowManager _windowManager;
        private TutorialService _tutorialService;
        private CloudService _cloudService;
        private TaskService _taskService;
        private AdvertisingService _advertisingService;
        private RatingService _ratingService;

        [Inject]
        public void Construct(
            WindowManager windowManager,
            GameStatService gameStatService,
            SignalBus signalBus,
            TutorialService tutorialService,
            CloudService cloudService,
            TaskService taskService,
            AdvertisingService advertisingService,
            RatingService ratingService
        )
        {
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            cheatLevelInput.onEndEdit.AddListener(OnCheatEndEdit);

            _signalBus = signalBus;
            _gameStatService = gameStatService;
            _windowManager = windowManager;
            _tutorialService = tutorialService;
            _cloudService = cloudService;
            _taskService = taskService;
            _advertisingService = advertisingService;
            _ratingService = ratingService;
            _advertisingService.RewardAdRewarded += AdFinished;
            _advertisingService.FullScreenAdRewarded += AdFinished;

            foreach (var grid in grids)
            {
                grid.StateChanged += OnGridStateChanged;
                grid.NewValueCreated += OnNewValueCreated;
                grid.RemoveWallsActivated += OnStartRemoveWalls;
                grid.SquareChangeActivated += OnStartChange;
                grid.Merged += OnMerged;
                grid.OnFreeCellClicked += FreeCellClicked;
            }

            _tutorialService.TutorialEnded += OnTutorialEnded;
            _gameStatService.StatChanged += OnStatChanged;
        }

        private void Start()
        {
            _targetViewAnchor = targetView.anchoredPosition;
            _targetViewPosition = targetView.position;
            _currentGridLocalPosition = grids[0].LocalPosition;
            _nextGridLocalPosition = grids[1].LocalPosition;
            taskScoresView.UpdateSkin(_currentSkin);
            
            shopGroup.SetInteractable(UnlockedButtons);
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
            
            _gameStatService.StatChanged -= OnStatChanged;
            _advertisingService.RewardAdRewarded -= AdFinished;
            _advertisingService.FullScreenAdRewarded -= AdFinished;

            foreach (var grid in grids)
            {
                grid.StateChanged -= OnGridStateChanged;
                grid.NewValueCreated -= OnNewValueCreated;
                grid.RemoveWallsActivated -= OnStartRemoveWalls;
                grid.SquareChangeActivated -= OnStartChange;
                grid.Merged -= OnMerged;
                grid.OnFreeCellClicked -= FreeCellClicked;
            }
            _tutorialService.TutorialEnded -= OnTutorialEnded;
        }

        private void Init(GridData savingData)
        {
            _openedSkins = savingData.openedSkins;
            _savingData = savingData;

            if (_savingData.infinityGridData.previousModels.Count <= 0)
            {
                var model = GenerateInfinityGrid();
                SaveGridModel(model);
            }

            _currentLevel = savingData.level;
            SetFirstGrid();
            levelView.UpdateArgs(new[] { _currentLevel.ToString() });
            _currentSkin = _savingData.currentSkin;
            _ratingUnlocked = _savingData.ratingUnlocked;
            CurrentGridView.SetNewSkin(_currentSkin);
            OnInited.Invoke();
        }

        [UsedImplicitly]
        public void UnlockAll()
        {
            NextLevel = 41;
            NextGridDebug();
            NextLevel = -1;

            foreach (var value in Enum.GetValues(typeof(ESquareSkin)))
            {
                OpenSkin((ESquareSkin)value);
            }
            _gameStatService.TryIncWithAnim(EGameStatType.Soft, 5000);
        }

        public void FreeCellClicked() => OnFreeCellClicked.Invoke();

        [UsedImplicitly]
        public void SetExternalSkin()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var values = new List<int> {0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048};
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
                    _resourceRepository.AddSquareImage(value, sprite, false);
                }
            }
            Debug.Log($"[GridManager][SetExternalSkin] **************** START LOAD frame");
            var framePath = filePath + "frame.png";
            Debug.Log($"[GridManager][SetExternalSkin] path: {framePath}");
            if (TryGetSprite(framePath, out var spriteFrame))
            {
                foreach (var value in values)
                {
                    _resourceRepository.AddSquareImage(value, spriteFrame, true);
                }
            }
            SetSkin(ESquareSkin.external);
#endif
        }

        public void LoadTestLevel()
        {
            CurrentGridView.Finish(() =>
            {
                IsLocked = false;
                _levelFinished = false;
                OnStartLevel.Invoke();
            });
            SetEnvironmentInteractable(false);
            _clearGridSave = false;

            _gameStatService.ResetLocalDeltaWithAnim(EGameStatType.Points);
            taskScoresView.AnimateWin();

            _currentGrid++;
            if (_currentGrid >= grids.Count)
            {
                _currentGrid = 0;
            }
            var units = new List<UnitModel>();
            units.Add(new UnitModel(){position = new Vector2Int(1,1), value = 2});
            units.Add(new UnitModel(){position = new Vector2Int(2,1), value = 2});
            units.Add(new UnitModel(){position = new Vector2Int(3,2), value = 4});
            units.Add(new UnitModel(){position = new Vector2Int(4,2), value = 4});
            units.Add(new UnitModel(){position = new Vector2Int(1,3), value = 2});
            units.Add(new UnitModel(){position = new Vector2Int(2,3), value = 8});
            units.Add(new UnitModel(){position = new Vector2Int(3,3), value = 8});
            var nextValues = new List<NextValue>();
            nextValues.Add(new NextValue {value = 2, chance = 1});
            var model = new GridModel
            {
                id = 123,
                nextValues = nextValues,
                size = new Vector2Int(6, 6),
                taskModel = new TaskModel
                {
                    type = ETaskType.Endless,
                    value = 100
                },
                units = units,
                reward = 20
            };
            _savingData.Task.type = model.taskModel.type;
            CurrentGridView.Init(model);
            _gameStatService.TrySet(EGameStatType.TargetPoints, model.taskModel.value);
            PlaySlide();
            CurrentGridView.SwipeActiveGridAnimated();
            _savingData.level = _currentLevel; 
            levelView.UpdateArgs(new[] { _currentLevel.ToString() });
            RestorePlatePosition();
            OnNextGrid.Invoke();
            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Started, _currentLevel));
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
            SaveGrid(true);
        }

        public void OpenSkin(ESquareSkin skin)
        {
            SkinSaveData newSkin = new SkinSaveData()
            {
                skinType = skin,
                count = 1,
            };
            _openedSkins.Add(newSkin);
            _savingData.openedSkins = _openedSkins;
        }
        
        public void OpenSkinByRarity(ESkinRarity rarity)
        {
            var skins = _squaresSkinsManager.Skins.ToList().FindAll(s => s.Rarity == rarity);
            ESquareSkin skin = ESquareSkin.baseSprite;
            foreach (var s in skins)
            {
                if (_openedSkins.Find(os => os.skinType == s.Skin) == null)
                {
                    OpenSkin(s.Skin);
                    return;
                }
            }
            Debug.LogWarning($"[GridManager][OpenSkinByRarity] Not found unlockable skins for rarity: {rarity}");
        }

        public void AddSkin(ESquareSkin skinType)
        {
            var skin = _openedSkins.Find(s => s.skinType == skinType);
            if (skin != null)
            {
                skin.count++;
            }
            _savingData.openedSkins = _openedSkins;
        }

        [UsedImplicitly]
        public void StopRemoveWalls()
        {
            CurrentGridView.StopRemoveWalls();
        }
        
        [UsedImplicitly]
        public void StopSquareChange()
        {
            CurrentGridView.StopChange();
        }

        public void SetRatingUnlocked(bool isUnlocked)
        {
            _ratingUnlocked = isUnlocked;
            _savingData.ratingUnlocked = _ratingUnlocked;
        }

        private void SetFirstGrid()
        {
            var model = levelRepository.GetById(_currentLevel, GenerateLevel(_currentLevel));
            _savingData.Task.type = model.taskModel.type;
            
            if (_savingData.cells.Count > 0)
                CurrentGridView.Init(model, _savingData.cells);
            else
                CurrentGridView.Init(model);

            // CurrentGridView.SwipeActiveGridAnimated();
            CurrentGridView.SetActive();
            _gameStatService.TrySet(EGameStatType.TargetPoints, model.taskModel.value);
            taskScoresView.Init(model.taskModel, _gameStatService.GetStat(EGameStatType.Points).RealValue);

            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Started, _currentLevel));
        }

        public void NextGridDebug()
        {
            // _squaresTutorial.TutorialEndDebug();
            FinishGrid();
            CurrentGridView.GridStates.Clear();
        }

        private void NextGrid()
        {
            SetEnvironmentInteractable(false);
            _clearGridSave = false;

            _gameStatService.ResetLocalDeltaWithAnim(EGameStatType.Points);
            taskScoresView.AnimateWin();
            _taskService.AddStat(ETaskDataType.LevelCompleteCount, 1);

            _currentGrid++;
            if (_currentGrid >= grids.Count)
            {
                _currentGrid = 0;
            }
            
            var model = levelRepository.GetById(_currentLevel, GenerateLevel(_currentLevel));
            _savingData.Task.type = model.taskModel.type;
            
            CurrentGridView.Init(model);
            _gameStatService.TrySet(EGameStatType.TargetPoints, model.taskModel.value);
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
            SetButtonsInteractable(interactable);
            if (!interactable)
                CurrentGridView.Block();
        }

        public void SetButtonsInteractable(bool value)
        {
            if (!value)
            {
                wallsRemovingGroup.SetInteractable(false);
                changeGroup.SetInteractable(false);
                shopGroup.SetInteractable(false);
                tasksButton.interactable = false;
            }
            else
            {
                CurrentGridView.SetWallsRemovingButton();
                CurrentGridView.SetChangingButton();
                shopGroup.SetInteractable(UnlockedButtons);
                tasksButton.interactable = true;
            }
        }

        public void RestartGrid()
        {
            StartGridWithCurrentLevel();
        }

        public void RestartGridClue()
        {
            if (_gameStatService.TryDec(EGameStatType.LevelRestarts, 1))
            {
                _clearGridSave = true;
                _signalBus.Fire(new LevelStatusSignal(LevelStatus.Failed, _currentLevel));
                SaveGrid();
                ShowFailPopup();
            }
            else
            {
                SquaresShop.OpenSection(_windowManager, EShopMarkers.Clues);
            }
        }

        private void SkipGrid()
        {
            _currentLevel++;
            _savingData.level++;

            StartGridWithCurrentLevel();
        }

        private void StartGridWithCurrentLevel()
        {
            _savingData.Task.currentValue = 0;
            var currentPoints = _gameStatService.GetStatValue(EGameStatType.Points);
            CurrentGridView.Finish(() => IsLocked = false);
            NextGrid();
            DownPoints(currentPoints);
            _gameStatService.TrySetWithAnim(EGameStatType.Points, 0);
            SaveGrid();
        }

        private void OnStatChanged(EGameStatType type, int value)
        {
            if (type == EGameStatType.WallsRemoves || type == EGameStatType.StepBacks)
            {
                SetButtonsInteractable(true);
            }
        }

        private void FinishGrid()
        {
            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Passed, _currentLevel));
            SetEnvironmentInteractable(false);
            _levelFinished = true;
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
            var currentPoints = _gameStatService.GetStatValue(EGameStatType.Points);
            SaveGrid(true);
            taskScoresView.AnimateWin(OnAnimationFinished);

            void OnAnimationFinished()
            {
                ShowWinPopup(() =>
                {
                    CurrentGridView.Finish(() =>
                    {
                        IsLocked = false;
                        _levelFinished = false;
                        OnStartLevel.Invoke();
                    });
                    NextGrid();
                    DownPoints(currentPoints);
                });
            }
        }

        private void DownPoints(int points)
        {
            if (taskScoresView.TaskType == ETaskType.GetCellWithValue)
            {
                StartCoroutine(DownSquaresTarget(points));
            }
            else
            {
                _gameStatService.TrySetWithAnim(EGameStatType.Points, 0);
            }
        }

        public void SaveInfinityGrid(InfinityGridData data, bool force = true)
        {
            _savingData.infinityGridData = data;
            saver.SaveNeeded.Invoke(force);
        }

        public void SaveGrid(bool force = false)
        {
            _savingData.cells.Clear();
            if (!_clearGridSave)
            {
                SaveCells();
            }
            else
            {
                _savingData.Task.currentValue = 0;
                var delta = _gameStatService.GetStatValue(EGameStatType.Points);
                _gameStatService.TrySet(EGameStatType.Points, 0);
                _gameStatService.TryAddLocalDelta(EGameStatType.Points, delta);
            }
            saver.SaveNeeded.Invoke(force);
        }

        public void ClearInfinityLevelDebug()
        {
            _savingData.infinityGridData = InitInfinityLevel();
            _savingData.infinityGridData.currentModel = null;
        }

        public InfinityGridModel GetExternalGrid(int seed)
        {
            // var model = GenerateInfinityGrid(true, _ratingService.CreateMonthSeed());
            foreach (var externalModel in _savingData.infinityGridData.externalModels)
            {
                if (externalModel.model.id == seed)
                {
                    InfinityGridData.externalModel = externalModel;
                    return externalModel;
                }
            }
            var model = GenerateInfinityGrid(true, seed);
            model.isExternal = true;
            InfinityGridData.externalModel = model;
            SaveGridModel(model);
            return model;
        }

        // public void SaveExternalGridScores(int scores, InfinityGridModel model)
        // {
        //     var data = _ratingService.Data.rewardsInProgress.Find(rd => rd.id == model.model.id.ToString());
        //     dawd
        // }

        public void SaveNextValue(int value)
        {
            _savingData.nextValue = value;
            // SaveGrid();
        }
        
        public void DeleteInfinityLevel(InfinityGridModel model)
        {
            _savingData.infinityGridData.previousModels.Remove(model);
            saver.SaveNeeded.Invoke(true);
        }

        public InfinityGridData InitInfinityLevel()
        {
            var newModel = GenerateLevel(Random.Range(0, 999));
            var model = new InfinityGridModel
            {
                model = newModel,
            };
            var infinityGridData = new InfinityGridData
            {
                currentModel = null,
                task = new MergeSquares.GridTaskData
                {
                    type = ETaskType.Endless
                },
            };
            infinityGridData.previousModels.Add(model);
            infinityGridData.nextValue = newModel.nextValues.First().value;
            return infinityGridData;
        }

        private IEnumerator DownSquaresTarget(int points)
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

        private void OnTutorialEnded()
        {
            SetButtonsInteractable(true);
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new GridData()
            {
                level = 1,
                currentSkin = ESquareSkin.baseSprite,
                // openedSkins = new List<ESquareSkin> { ESquareSkin.baseSprite },
                openedSkins = new List<SkinSaveData> { new SkinSaveData()
                {
                    skinType = ESquareSkin.baseSprite,
                    count = 1,
                } },

                infinityGridData = InitInfinityLevel()
            }));
        }
        
        private void SaveCells()
        {
            foreach (var pair in CurrentGridView.Cells)
            {
                if (!pair.Value.IsFree)
                {
                    _savingData.cells.Add(new CellData { position = pair.Key, value = pair.Value.view.Value });
                }
            }
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_savingData);
        }

        private void OnNewValueCreated(int value)
        {
            switch (_savingData.Task.type)
            {
                case ETaskType.GetCellWithValue:
                    _gameStatService.TrySetWithAnim(EGameStatType.Points, value);
                    
                    if (!_levelFinished && _savingData.Task.currentValue < value)
                    {
                        _savingData.Task.currentValue = value;
                        taskScoresView.UpdateScore(value);

                        if (value >= CurrentGridView.Task.value)
                        {
                            FinishGrid();
                        }
                    }
                    break;
                case ETaskType.CollectPoints:
                    _gameStatService.TryIncWithAnim(EGameStatType.Points, value);
                    _savingData.Task.currentValue = value;

                    if (_gameStatService.GetStat(EGameStatType.Points).RealValue
                        >= _gameStatService.GetStat(EGameStatType.TargetPoints).RealValue)
                    {
                        FinishGrid();
                    }
                    break;
                case ETaskType.Endless:
                    _gameStatService.TryIncWithAnim(EGameStatType.Points, value);
                    _savingData.Task.currentValue = value;
                    break;

            }
            SaveGrid();
        }

        private void OnMerged()
        {
            switch (_savingData.Task.type)
            {
                case ETaskType.MakeMerges:
                    _gameStatService.TryInc(EGameStatType.Points, 1);
                    _savingData.Task.currentValue++;

                    if (_gameStatService.GetStat(EGameStatType.Points).RealValue
                        >= _gameStatService.GetStat(EGameStatType.TargetPoints).GetValue())
                    {
                        FinishGrid();
                    }
                    break;
            }
        }

        private void OnGridStateChanged()
        {
            if (CurrentGridView.IsFull())
            {
                if (CurrentGridView.Task.type == ETaskType.Endless)
                {
                    FinishGrid();
                }
                else
                {
                    _clearGridSave = true;
                    _signalBus.Fire(new LevelStatusSignal(LevelStatus.Failed, _currentLevel));
                    SaveGrid();
                    ShowFailPopup();
                }
            }
        }

        private void OnStartRemoveWalls(bool isRemoving)
        {
            if (isRemoving)
            {
                var overlay = _windowManager.ShowWindow(EPopupType.Overlay.ToString(), isUnique: true);
                overlay.Canvas.sortingOrder = wallOverlaySortingOrder;
                overlay.Canvas.sortingLayerName = "Default";
            }
            else
            {
                _windowManager.CloseAll(EPopupType.Overlay.ToString());
            }
            stopRemoveWallsButton.gameObject.SetActive(isRemoving);
        }

        public void OnStartChange(bool status)
        {
            if (status)
            {
                var window = 
                    _windowManager.ShowWindow(EPopupType.Overlay.ToString(), isUnique: true);
                window.Canvas.sortingOrder = wallOverlaySortingOrder;
                window.Canvas.sortingLayerName = "Default";
            }
            else
            {
                _windowManager.CloseAll(EPopupType.Overlay.ToString());
            }
            stopChangeButton.gameObject.SetActive(status);
        }

        public InfinityGridModel GenerateInfinityGrid(bool isGeneratedSeed = false, int seed = 0)
        {
            if (!isGeneratedSeed)
            {
                seed = Random.Range(0, 999);
                var errorCounter = 100;
                while (_savingData.infinityGridData.previousModels.Find(model => model.model.id == seed) != null && errorCounter > 0)
                {
                    seed = Random.Range(0, 999);
                    errorCounter--;
                }
            }
            
            var model = new InfinityGridModel()
            {
                model = GenerateLevel(seed),
                bestScore = 0,
                retryCount = 0,
            };
            return model;
        }
        
        public void SaveGridModel(InfinityGridModel model)
        {
            if(model.isExternal)
            {
                _savingData.infinityGridData.externalModels.Add(model);
            }
            else
            {
                _savingData.infinityGridData.previousModels.Add(model);
            }
            SaveInfinityGrid( _savingData.infinityGridData);
        }
        

        private void ShowWinPopup(Action callback = null)
        {
            if (!_winPopupShowing)
            {
                _winPopupShowing = true;
                var showedLevel = _currentLevel - 1; // decrement because next level id saves before
                var model = levelRepository.GetById(showedLevel, GenerateLevel(showedLevel));
                var winParams = new WinPopupParams
                {
                    showedLevel = showedLevel,
                    winBonus = model.reward,
                    giftBonus = model.reward * 3,
                    ClosePopup =
                        () =>
                        {
                            // if (currentLevel == authAfterLevel && !_yandexService.CheckAuthState())
                            // {
                            //     var authParams = new YandexAuthPopupParams {firstAuth = true};
                            //     _windowManager.ShowWindow(EPopupType.Auth.ToString(), new[] { authParams });
                            // }
                            _winPopupShowing = false;
                            callback?.Invoke();
                        }
                };
                _windowManager.ShowWindow(EPopupType.LevelWin.ToString(), new[] { winParams });
                WinPopup winPopup;
                _windowManager.TryGetWindow(EPopupType.LevelWin.ToString(), out winPopup);
                winPopup.FullScreenAdsEnables = showedLevel >= _cloudService.CloudProvider.MinLevelToShowFullscreen();
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
            failPopup.FullScreenAdsEnables = _currentLevel >= _cloudService.CloudProvider.MinLevelToShowFullscreen();;
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

        public GridModel GenerateLevel(int seed)
        {
            var rnd = new LCRandom( seed );
            var nextValues = new List<NextValue>();
            MaybeGenerateNextValue(1, 1, rnd, nextValues);
            MaybeGenerateNextValue(2, 20, rnd, nextValues);
            MaybeGenerateNextValue(4, 15, rnd, nextValues);
            MaybeGenerateNextValue(8, 10, rnd, nextValues);
            MaybeGenerateNextValue(16, 5, rnd, nextValues);
            MaybeGenerateNextValue(32, 2, rnd, nextValues);
            MaybeGenerateNextValue(64, 1, rnd, nextValues);
            var ableTasks = new List<ETaskType>
                { ETaskType.CollectPoints, ETaskType.MakeMerges, ETaskType.GetCellWithValue };
            var taskType = ableTasks[rnd.Range32(0, ableTasks.Count)];
            var taskValue = 0;
            var lvlMultiplier = 1 + _currentLevel * 0.1f;
            var size = new Vector2Int(rnd.Range32(4, 5), rnd.Range32(5, 7));
            if (_cloudService.CloudProvider.GetDeviceType() == ECloudDeviceType.Desktop)
            {
                (size.x, size.y) = (size.y, size.x);
            }
            var wallsPart = 0.01 * rnd.Range32(3, 11);
            var wallsCount = wallsPart * size.x * size.y;
            var startUnits = new List<UnitModel>();
            var fullPositions = new List<Vector2Int>();
            var x = 0;
            var y = 0;
            Vector2Int pos = Vector2Int.zero;
            for (int i = 0; i < wallsCount; i++)
            {
                for (int j = 0; j < 1000; j++)
                {   
                    x = rnd.Range32(0, size.x + 1);
                    y = rnd.Range32(0, size.y + 1);
                    pos = new Vector2Int(x, y);
                    if (!fullPositions.Contains(pos))
                    {
                        fullPositions.Add(pos);
                        break;
                    }
                }
                
                var wall = new UnitModel()
                {
                    position = pos,
                    value = 0,
                };
                startUnits.Add(wall);
            }
                
            switch (taskType)
            {
                case ETaskType.CollectPoints:
                    taskValue = (int)(rnd.Range32(1, 6) * 1000 * lvlMultiplier);
                    break;
                case ETaskType.MakeMerges:
                    taskValue =  (int)(rnd.Range32(5, 10) * lvlMultiplier);
                    break;
                case ETaskType.GetCellWithValue:
                    int minTargetPow = 7;
                    int maxTargetPow = 11;
                    taskValue = (int)Math.Pow(2, rnd.Range32(minTargetPow, maxTargetPow));
                    break;
            }

            var model = new GridModel
            {
                id = seed,
                nextValues = nextValues,
                size = size,
                taskModel = new TaskModel
                {
                    type = taskType,
                    value = taskValue
                },
                units = startUnits,
                reward = rnd.Range32(1, 4) * 10
            };
            return model;
        }

        private void MaybeGenerateNextValue(int value, int mult, LCRandom rnd, List<NextValue> list)
        {
            var next = new NextValue
            {
                value = value
            };
            next.chance = rnd.Range32(0, 2) == 1 ? rnd.Range32(1, 11) * mult : 0;
            if (next.chance != 0)
            {
                list.Add(next);
            }
        }
    }
}