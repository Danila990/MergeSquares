using System;
using Core.SaveLoad;
using UnityEngine;
using Utils;
using Zenject;

namespace GameStats
{
    [Serializable]
    public class GameStatReloaderData
    {
        public long timestamp;
    }

    public class GameStatReloader : MonoBehaviour
    {
        [SerializeField] private EGameStatType type;
        [SerializeField] private int reloadTime;
        [SerializeField] private int reloadAmount;
        [SerializeField] private float reloadTickTime = 1f;
        [Space] [SerializeField] private Saver saver;

        public bool ActiveReload() => _startTick;

        public event Action<int> ReloadTimerChanged = timeLeft => { };
        public event Action<bool> ReloadActiveChanged = active => { };

        private float _timer;
        private bool _startTick;
        private GameStatReloaderData _data;

        private GameStatService _gameStatService;

        [Inject]
        public void Construct(GameStatService gameStatService)
        {
            _gameStatService = gameStatService;
            saver.DataLoaded += OnDataLoaded;
            saver.DataLoadFinished += OnDataLoadedFinished;
            saver.DataSaved += OnDataSaved;
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataLoadFinished -= OnDataLoadedFinished;
            saver.DataSaved -= OnDataSaved;
        }

        public void OnEnable()
        {
            _gameStatService.StatChanged += OnStatChanged;
        }

        public void OnDisable()
        {
            _gameStatService.StatChanged -= OnStatChanged;
        }

        private void Update()
        {
            if (_startTick)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    _timer = reloadTickTime;
                    var delta = (int) Timestamp.CalculateTimeDiff(_data.timestamp).TotalSeconds;
                    if (delta >= reloadTime)
                    {
                        Reload(_gameStatService.GetStat(type));
                    }

                    ReloadTimerChanged.Invoke(reloadTime - delta);
                }
            }
        }

        public int GetReloadAmount()
        {
            var delta = (int) Timestamp.CalculateTimeDiff(_data.timestamp).TotalSeconds;
            return reloadTime - delta > 0 ? reloadTime - delta : reloadTime;
        }

        public void Init(GameStatReloaderData data, LoadContext context)
        {
            _data = data;
        }
        
        private void OnStatChanged(EGameStatType type, int value)
        {
            if (this.type == type)
            {
                var stat = _gameStatService.GetStat(type);
                if (!stat.MaxReached() && !_startTick)
                {
                    _startTick = true;
                    ReloadActiveChanged.Invoke(_startTick);
                    _data.timestamp = Timestamp.GetTicks();
                    _timer = 0;
                    saver.SaveNeeded(false);
                }
            }
        }

        private void Reload(GameStatVariable stat)
        {
            _gameStatService.TryInc(type, reloadAmount);
            if (!stat.MaxReached())
            {
                _data.timestamp = Timestamp.GetTicks();
                saver.SaveNeeded(false);
            }
            else
            {
                _startTick = false;
                ReloadActiveChanged.Invoke(_startTick);
            }
        }

        private void AddOfflineProgress(LoadContext loadContext)
        {
            var contextTimeLeft = (int) loadContext.playerOfflineTime.TotalSeconds;
            var timeLeft = (int) Timestamp.CalculateTimeDiff(_data.timestamp).TotalSeconds;
            timeLeft = timeLeft > contextTimeLeft ? timeLeft : contextTimeLeft;
            var stat = _gameStatService.GetStat(type);
            while (timeLeft > 0 && !stat.MaxReached())
            {
                if (timeLeft > reloadTime)
                {
                    timeLeft -= reloadTime;
                    Reload(stat);
                }
                else
                {
                    var delta = reloadTime - timeLeft;
                    timeLeft = 0;
                    if (delta <= 0)
                    {
                        Reload(stat);
                    }
                    else
                    {
                        var timeSpan = TimeSpan.FromSeconds(delta);
                        _data.timestamp = Timestamp.GetTicks(timeSpan);
                        saver.SaveNeeded(true);
                    }
                }
            }

            _startTick = !stat.MaxReached();
            ReloadActiveChanged.Invoke(_startTick);
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new GameStatReloaderData()), context);
        }

        private void OnDataLoadedFinished(LoadContext loadContext)
        {
            AddOfflineProgress(loadContext);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}