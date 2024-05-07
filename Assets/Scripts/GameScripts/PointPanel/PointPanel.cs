using System;
using System.Collections.Generic;
using System.Linq;
using Advertising;
using CloudServices;
using Core.Audio;
using Core.SaveLoad;
using Core.Windows;
using GameScripts.AnalyticsSignals;
using GameStats;
using MergeBoard.UI;
using Plugins.WindowsManager;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace GameScripts.PointPanel
{
    [Serializable]
    public class PointData
    {
        public EPointId id;
        public int index;
        public bool ok;
        public bool busy;

        public PointData(EPointId id, int index)
        {
            this.id = id;
            this.index = index;
        }
    }

    public class PointPanelData
    {
        public List<PointData> points = new();
        public List<EPointId> buttons = new();
        public List<List<PointData>> lines = new();
        public int attempts;
        public bool allOk;

        public PointPanelData()
        {

        }

        public PointPanelData(List<EPointId> points, List<EPointId> buttons, int attempts)
        {
            allOk = false;
            for (int i = 0; i < points.Count; i++)
            {
                this.points.Add(new PointData(points[i], i));
            }
            this.buttons = buttons;
            this.attempts = attempts;
        }

        public List<PointData> GetOkPoints()
        {
            return points.FindAll(p => p.ok);
        }
        
        public void ResetOkPoints()
        {
            foreach (var point in points)
            {
                point.ok = false;
            }
        }

        public List<EPointId> GetFailedPoinsIds(List<PointData> line)
        {
            if (allOk)
                return line.Select(t => t.id).ToList();

            var states = CheckLine(line);
            var result = new List<EPointId>();
            var wrongPlacs = new List<EPointId>();

            for (int i = 0; i < line.Count; i++)
            {
                var id = line[i].id;

                if (states[i] == EPointState.WrongPlace)
                    wrongPlacs.Add(id);

                if (states[i] == EPointState.Fail && !wrongPlacs.Contains(id))
                    result.Add(id);
            }

            return result;
        }

        public List<EPointState> CheckLine(List<PointData> line)
        {
            var res = new List<EPointState>();
            lines.Add(line);

            var filteredPoints = new List<PointData>();
            var filteredLine = new List<PointData>();
            for (var i = 0; i < line.Count; i++)
            {
                if (line[i].id == points[i].id)
                {
                    res.Add(EPointState.Win);
                    points[i].ok = true;
                }
                else
                {
                    filteredPoints.Add(points[i]);
                    filteredLine.Add(line[i]);
                    res.Add(EPointState.Fail);
                }
            }

            if (filteredPoints.Count == 0)
            {
                allOk = true;
                return res;
            }

            foreach (var data in filteredLine)
            {
                var point = filteredPoints.Find(p => p.id == data.id);
                if (point != null)
                {
                    res[data.index] = EPointState.WrongPlace;
                    filteredPoints.Remove(point);
                }
            }

            return res;
        }

        public void Generate() => Generate(Random.Range(3, 6), Random.Range(4, 7), Random.Range(3, Enum.GetValues(typeof(EPointId)).Length - 1));

        public void Generate(int size, int attempts, int typesCount)
        {
            this.attempts = attempts;
            allOk = false;
            foreach (var line in lines)
            {
                line.Clear();
            }
            lines.Clear();

            var except = new List<EPointId>();
            except.Add(EPointId.None);
            buttons = new List<EPointId>(Enum.GetValues(typeof(EPointId)).Cast<EPointId>().ToList()).Shuffle().Except(except).Take(typesCount).ToList();

            points.Clear();
            for (var i = 0; i < size; i++)
            {
                points.Add(new PointData(buttons[Random.Range(0, buttons.Count - 1)], i));
            }
        }

        public void Validate()
        {
            foreach (var point in points)
            {
                if (!buttons.Contains(point.id))
                {
                    Debug.LogError($"[PointPanelData][Validate] find wrong point {point.id} index {point.index}");
                }
            }
        }

        public string GetDataString()
        {
            var res = "";

            foreach (var pointData in points)
            {
                res += $" {pointData.index}:{pointData.id}";
            }

            res += $" -- Buttons {buttons.Count} -- ";

            foreach (var button in buttons)
            {
                res += $" {button}";
            }

            return res;
        }
    }

    [Serializable]
    public class PointLinePrefabData
    {
        public int count;
        public PointLine pointLine;
    }

    [Serializable]
    public class PointPanelSavingData
    {
        public BallSkin currentBallSkin = new BallSkin();
        public int openedSkinsCount = 1;
        public int levelNum = 0;
    }

    public class PointPanel : MonoBehaviour
    {
        [SerializeField] private SoundSource ballEndFly;
        [SerializeField] private SoundSource wrongBall;
        [SerializeField] private List<SoundSource> correctBalls;
        [SerializeField] private SoundSource successLine;
        [SerializeField] private SoundSource unlock;
        [SerializeField] private List<PointLinePrefabData> linePrefabs;
        [SerializeField] private Transform lineContainer;
        [SerializeField] private BottomPanel bottomPanel;
        [SerializeField] private Saver saver;
        [SerializeField] private StatickLevelsRepository levelsRepository;
        [SerializeField] private LevelCounter levelCounter;
        [SerializeField] private Lock lockIcon;
        [SerializeField] private Button skinsButton;
        [SerializeField] private int rewardAdBonus = 50;
        [SerializeField] private int winBonus = 25;
        [SerializeField] private int authAfterLvl = 1;

        public Action OnSetNewSkin = () => { };
        public Action OnNextLevel = () => { };

        public BallSkin CurrentBallSkin => _currentBallSkin;
        public BottomPanel BottomPanel => bottomPanel;
        public PointLine ActiveLine => _lines[_activeLineIndex];
        public int OpenedSkinsCount => _openedSkinsCount;
        public int WinBonus => winBonus;
        // public int SkinCost => skinCost;
        public int ShowedLevel => _levelNum + 1;
        public bool InputBlocked => _inputBlocked;
        public void PlayEndFlySound() => ballEndFly.Play();
        public void PlayWrongBallSound() => wrongBall.Play();
        public void PlaySuccessLineSound() => successLine.Play();
        public void PlayUnlockSound() => unlock.Play();

        private List<PointLine> _lines = new();
        private PointPanelData _data = new();

        private BallSkin _currentBallSkin;
        private int _activeLineIndex;
        private int _openedSkinsCount = 1;
        private int _levelNum = 0;
        private int _correctSoundNum = 0;
        private bool _inputBlocked = false;

        private DummyFactory _dummyFactory;
        private WindowManager _windowManager;
        // private GetItRightTutorial _getItRightTutorial;
        private SignalBus _signalBus;
        private CloudService _cloudService;

        private PointPanelSavingData _savingData = new();

        [Inject]
        private void Construct(DummyFactory dummyFactory,
            WindowManager windowManager/*, GetItRightTutorial getItRightTutorial*/, SignalBus signalBus, CloudService cloudService)
        {
            _dummyFactory = dummyFactory;
            _windowManager = windowManager;
            // _getItRightTutorial = getItRightTutorial;
            _signalBus = signalBus;
            _cloudService = cloudService;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            _windowManager.WindowOpenedEvent += OnWindowEvent;
            _windowManager.WindowClosedEvent += OnWindowEvent;
        }

        private void OnWindowEvent(object sender, WindowOpenEventArgs e)
        {
            if (_windowManager.GetOpenedWindowsCount() == 1)
            {
                _cloudService.CloudProvider.GameplayStop();
            }
        }
        
        private void OnWindowEvent(object sender, WindowCloseEventArgs e)
        {
            if (_windowManager.GetOpenedWindowsCount() == 0)
            {
                _cloudService.CloudProvider.GameplayStart();
            }
        }

        public void TryGenerateAndCheckPoints()
        {
            _data.Generate();
            _data.Validate();
            bottomPanel.SetPoints(_data.buttons, PointClicked);
            Debug.Log($"[PointPanel][TryGenerateAndCheckPoints] got new data {_data.GetDataString()}");
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
            _windowManager.WindowOpenedEvent -= OnWindowEvent;
            _windowManager.WindowClosedEvent -= OnWindowEvent;
        }

        private void Init(PointPanelSavingData savingData)
        {
            _savingData = savingData;
            _openedSkinsCount = savingData.openedSkinsCount;
            _levelNum = savingData.levelNum;
            _currentBallSkin = savingData.currentBallSkin;
            levelCounter.SetLevel(ShowedLevel);

            if (_levelNum <= authAfterLvl)
            {
                OnNextLevel += Auth;
            }

            skinsButton.interactable = _savingData.openedSkinsCount > 1;
            
            SetNewSkin(_currentBallSkin);
            StartGame();
            _cloudService.CloudProvider.GameplayStart();
        }

        public void PlayNextCorrectSound()
        {
            if (_correctSoundNum >= correctBalls.Count)
            {
                _correctSoundNum = 0;
            }
            correctBalls[_correctSoundNum].Play();
            _correctSoundNum++;
        }

        public void OpenNextSkin()
        {
            _openedSkinsCount++;
            _savingData.openedSkinsCount = _openedSkinsCount;
        }

        public void ShowSkinsPanel()
        {
            _windowManager.ShowWindow(EPopupType.BallsCustomizeUi.ToString());
        }

        public void RestartLevel()
        {
            GeneratePoints(true);
        }

        public void ReGenerate()
        {
            GeneratePoints();
        }

        public void NextLevel()
        {
            _levelNum++;
            _savingData.levelNum = _levelNum;
            levelCounter.SetLevel(ShowedLevel);
            GeneratePoints();
            OnNextLevel.Invoke();

            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Started, _levelNum));
        }

        public void SetNewSkin(BallSkin ballSkin)
        {
            _currentBallSkin = ballSkin;
            _savingData.currentBallSkin = ballSkin;
            foreach (var line in _lines)
            {
                line.SetSkin(_currentBallSkin);
            }

            bottomPanel.SetSkin(_currentBallSkin);
            saver.SaveNeeded?.Invoke(true);
            OnSetNewSkin.Invoke();
        }

        private void StartGame()
        {
            // _currentBallSkin = _skinsManager.Skins.First();
            // _currentBallSkin =  _skinsManager.Skins.Find(s => s.skinId == _savingData.currentBallSkin)
            // if (_getItRightTutorial.TutorialEnabled)
            // {
                if (_levelNum <= 0)
                {
                    // _getItRightTutorial.SetTutorialData(ref _data);
                    GeneratePoints();
                    // _getItRightTutorial.StartTutorial(true);
                }
                else
                {
                    GeneratePoints();
                    // if (_openedSkinsCount <= 1)
                    // {
                    //     _getItRightTutorial.StartTutorial(false);
                    // }
                }
            // }
            SetNewSkin(_currentBallSkin);
        }

        private bool TrySetData()
        {
            if (!levelsRepository.presavedLevelsEnabled)
            {
                return false;
            }
            foreach (var level in levelsRepository.levels)
            {
                if (level.number == ShowedLevel)
                {
                    var buttons = new List<EPointId>();

                    var except = new List<EPointId>();
                    except.Add(EPointId.None);
                    buttons = new List<EPointId>(Enum.GetValues(typeof(EPointId)).Cast<EPointId>().ToList()).Shuffle().Except(except).Take(level.buttonsCount).ToList();

                    var points = new List<EPointId>();
                    foreach (var pointId in level.pointIds)
                    {
                        points.Add(buttons[pointId]);
                    }
                    _data = new PointPanelData(points, buttons, level.attempts);

                    return true;
                }
            }

            return false;
        }

        private void Auth()
        {
            if (_levelNum == authAfterLvl)
            {
                OnNextLevel -= Auth;
                // if (!_yandexService.CheckAuthState())
                // {
                //     var authParams = new YandexAuthPopupParams {firstAuth = true};
                //     _windowManager.ShowWindow(EPopupType.Auth.ToString(), new[] { authParams });
                // }
            }
        }
        
        private void PointClicked(EPointId pointId, RectTransform buttonTr)
        {
            if (_inputBlocked)
            {
                return;
            }
            var dummy = _dummyFactory.Create();
            var target = _lines[_activeLineIndex].TargetTransform;
            var targetIndex = _lines[_activeLineIndex].CurrentIndex;

            dummy.SetStartPosition(buttonTr.position);
            dummy.SetStartSizeDelta(buttonTr.sizeDelta);
            dummy.SetSkin(_currentBallSkin, pointId);
            dummy.SetFinishPosition(target.position);
            dummy.SetFinishSizeDelta(target.sizeDelta);
            dummy.Launch();
            dummy.PointReached += () => Finish(pointId, targetIndex);

            if (!_lines[_activeLineIndex].SetNextTarget())
            {
                _inputBlocked = true;
            }
        }

        private void Finish(EPointId pointId, int index)
        {
            PlayEndFlySound();
            _lines[_activeLineIndex].ColorPoint(pointId, _currentBallSkin, index);
            if (_lines[_activeLineIndex].IsFull())
                EndLine();
        }

        private void EndLine()
        {
            _correctSoundNum = 0;
            _inputBlocked = true;
            var state = _data.CheckLine(_lines[_activeLineIndex].LineFill);

            _lines[_activeLineIndex].lineEnd += OnLineEnd;
            _lines[_activeLineIndex].SetEnd(state);
        }

        private void OnLineEnd()
        {
            UpdateBottomPanelButtons();

            if (_data.allOk)
            {
                PlaySuccessLineSound();
                lockIcon.ShowUnlock(Win);
                PlayUnlockSound();
                return;
            }

            _activeLineIndex++;
            if (_activeLineIndex >= _lines.Count)
            {
                Fail();
                return;
            }

            var okPoints = _data.GetOkPoints();
            if (okPoints.Count > 0)
            {
                _lines[_activeLineIndex].SetOpened(okPoints, _currentBallSkin);
            }
            _lines[_activeLineIndex].SetActive();
            _inputBlocked = false;
        }

        private void UpdateBottomPanelButtons()
        {
            var currentLine = _lines[_activeLineIndex].LineFill;
            var nonOk = _data.GetFailedPoinsIds(currentLine);
            bottomPanel.SetFailedCellsOverlaysByIds(nonOk);
        }

        private void Win()
        {
            WinPopupParams winPopupParams =
                new WinPopupParams { showedLevel = ShowedLevel, winBonus = winBonus, giftBonus = rewardAdBonus, ClosePopup = NextLevel };
            object[] args = new[] { winPopupParams };

            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Passed, ShowedLevel));
            _windowManager.ShowWindow(EPopupType.LevelWin.ToString(), args);
            _windowManager.TryGetWindow(EPopupType.LevelWin.ToString(), out WinPopup winPopup);
            winPopup.FullScreenAdsEnables = ShowedLevel >= _cloudService.CloudProvider.MinLevelToShowFullscreen();
            winPopup.Ready();
        }

        private void Fail()
        {
            _signalBus.Fire(new LevelStatusSignal(LevelStatus.Failed, ShowedLevel));

            var failParams = new FailPopupParams
            {
                OnRestarted = RestartLevel,
                OnSkipped = NextLevel
            };
            _windowManager.ShowWindow(EPopupType.LevelFail.ToString(), new[] { failParams });
        }

        private void GeneratePoints(bool isGenerated = false)
        {
            lockIcon.ShowLock();
            _inputBlocked = false;
            if (!isGenerated && !TrySetData())
            {
                TryGenerateAndCheckPoints();
            }

            _activeLineIndex = 0;
            _data.ResetOkPoints();

            bottomPanel.SetPoints(_data.buttons, PointClicked);
            bottomPanel.SetSkin(_currentBallSkin);

            // init lines
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }
            _lines.Clear();

            PointLine linePrefab = linePrefabs.Find(p => p.count == _data.points.Count).pointLine;

            for (int i = 0; i < _data.attempts; i++)
            {
                var line = Instantiate(linePrefab, lineContainer);
                _lines.Add(line);
            }
            foreach (var line in _lines)
            {
                line.Clear();
                line.SetSize(_data.points.Count);
                line.SetSkin(_currentBallSkin);
            }

            _lines.First().SetActive();
            saver.SaveNeeded.Invoke(true);
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new PointPanelSavingData()));
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_savingData);
        }
    }
}