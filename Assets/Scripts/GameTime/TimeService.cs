using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Conditions;
using Core.SaveLoad;
using Notify;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace GameTime
{
    [Serializable]
    public class SavedData
    {
        public string id;
        public long date;

        public SavedData(string id, DateTime date)
        {
            this.id = id;
            this.date = date.Ticks;
        }
    }
    
    [Serializable]
    public class TimeServiceData
    {
        public List<SavedData> dates = new List<SavedData>();
    }

    public class TimeService : MonoBehaviour
    {
        [SerializeField] private Saver saver;

        public Action Tick = () => {};
        public Action NewDayStarting = () => {};

        
        private TimeServiceData _data = new TimeServiceData();
        
        private DateTime _nowDay;
        private float _updateTimer;
        private int _updateDelay = 1;
        private NotifyRef _notifyRef = new NotifyRef();

        private Dictionary<string, DateTime> _targetTimes = new Dictionary<string, DateTime>();

        private NotifyService _notifyService;

        private void Start()
        {
            _nowDay = DateTime.Now.Date;
        }
        
        [Inject]
        public void Construct(NotifyService notifyService)
        {
            _notifyService = notifyService;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
        
        private void Update()
        {
            _updateTimer -= Time.deltaTime;
            if (_updateTimer <= 0)
            {
                _updateTimer = _updateDelay;
                Tick.Invoke();
                CheckDayChange();
            }
        }

        public bool TryGetTimeTarget(string id, ref DateTime returnValue)
        {
            if (_targetTimes.Keys.Contains(id))
            {
                returnValue = _targetTimes[id];
                return true;
            }
            return false;
        }
        
        public void SetTimeTarget(string id, DateTime dateTime)
        {
            if (_targetTimes.Keys.Contains(id))
            {
                _targetTimes[id] = dateTime;
                var savedData = _data.dates.Find(d => d.id == id);
                if (savedData != null)
                {
                    savedData.date = dateTime.Ticks;
                }
                else
                {
                    _data.dates.Add(new SavedData(id, dateTime));
                }
            }
            else
            {
                _targetTimes.Add(id, dateTime);
                _data.dates.Add(new SavedData(id, dateTime));
            }
            saver.SaveNeeded.Invoke(true);
        }

        public bool TryAddTimeTarget(string id, DateTime dateTime)
        {
            if (_targetTimes.Keys.Contains(id))
            {
                return false;
            }
            else
            {
                _targetTimes.Add(id, dateTime);
                _data.dates.Add(new SavedData(id, dateTime));
                saver.SaveNeeded.Invoke(true);
                return true;
            }
        }
        
        public bool TryRemoveTimeTarget(string id)
        {
            if (!_targetTimes.Keys.Contains(id))
            {
                return false;
            }
            else
            {
                _targetTimes.Remove(id);
                _data.dates.Remove(_data.dates.Find(d => d.id == id));
                saver.SaveNeeded.Invoke(true);
                return true;
            }
        }

        private void CheckDayChange()
        {
            if (_nowDay != DateTime.Now.Date)
            {
                _nowDay = DateTime.Now.Date;
                NewDayStarting.Invoke();
            }
        }

        private IEnumerator CheckTime()
        {
            while (true)
            {
                foreach (var time in _targetTimes)
                {
                    DateTime targetTime = time.Value;
                    TimeSpan countdown = targetTime - DateTime.Now;

                    _notifyRef.id = time.Key;
                    _notifyService.SetNotify(_notifyRef, countdown.Ticks < 0);
                }
                yield return null;
            }
        }
        
        private void Init(TimeServiceData data, LoadContext context)
        {
            _data = data;
            _targetTimes.Clear();
            foreach (var dataTime in _data.dates)
            {
                _targetTimes.Add(dataTime.id, new DateTime(dataTime.date));
            }

            StartCoroutine(CheckTime());
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new TimeServiceData()), context);
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}

