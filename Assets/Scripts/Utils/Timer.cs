using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    [Serializable]
    public class TimerData
    {
        public bool autoReset = false;
        public bool isTimerFinished = true;
        public bool useRange = false;
        public float timerInitValue = -1f;
        public float timer;
        public float previousFillAmount = 0f;
        public FloatRange timerInitRange = null;
        public long startTimestamp = 0;
        public bool isInitialized = false;

        public float TimerFillAmount => timerInitValue == 0f ? 1f : 1f - timer / timerInitValue;
    }

    public class Timer
    {
        private TimerData _data = new TimerData();
        private readonly Timestamp _startTimestamp = new Timestamp();

        public event Action TimerFinished;

        public float TimerFillAmount => _data.TimerFillAmount;
        public bool IsTimerFinished => _data.isTimerFinished;
        public bool IsInitialized => _data.isInitialized;

        public bool IsReady()
        {
            if (_data.useRange)
            {
                return _data.timerInitRange != null;
            }
            else
            {
                return _data.timerInitValue > float.Epsilon;
            }
        }

        public void Init(TimerData data)
        {
            if (data == null)
            {
                return;
            }

            _data = data;
            _data.timer -= Timestamp.CalculateTimeDiff(data.startTimestamp).Seconds;
            _data.timer = Mathf.Max(_data.timer, 0);
        }

        public void Init(float initTime)
        {
            _data.timerInitValue = initTime;
            _data.useRange = false;
            Reset();
            _data.isTimerFinished = false;
            _data.startTimestamp = DateTime.UtcNow.Ticks;
            _data.isInitialized = true;
        }

        public void Init(FloatRange initTimeRange)
        {
            _data.timerInitRange = initTimeRange;
            _data.useRange = true;
            ResetRange();
            _data.isTimerFinished = false;

            _data.startTimestamp = DateTime.UtcNow.Ticks;

            _data.isInitialized = true;
        }

        public void DeInit()
        {
            _data.timerInitValue = -1f;
            TimerFinished = () => { };
            _data.timerInitRange = null;

            _data.isInitialized = false;
        }

        public void Reset(float newInitTime = -1f)
        {
            _data.startTimestamp = DateTime.UtcNow.Ticks;

            if (newInitTime > float.Epsilon)
            {
                Init(newInitTime);
            }
            else
            {
                _data.timer = _data.timerInitValue;
                _data.isTimerFinished = false;
            }
        }

        public void ResetRange()
        {
            if (_data.timerInitRange != null)
            {
                _data.timerInitValue = Random.Range(_data.timerInitRange.min, _data.timerInitRange.max);
                _data.timer = _data.timerInitValue;
            }

            _data.isTimerFinished = false;
        }

        public void ResetRange(FloatRange newRange)
        {
            Init(newRange);
        }

        public void SetFinished()
        {
            _data.timer = -1f;
            _data.isTimerFinished = true;
            TimerFinished?.Invoke();
        }

        public void Update(float deltaTime)
        {
            _data.timer -= deltaTime;
            if (_data.timer <= 0 && !IsTimerFinished)
            {
                _data.isTimerFinished = true;
                _data.timer = 0;
                TimerFinished?.Invoke();
                if (_data.autoReset)
                {
                    if (_data.useRange)
                    {
                        ResetRange();
                    }
                    else
                    {
                        Reset();
                    }
                }
            }
        }

        public float TimerFillAmountDelta()
        {
            var amount = TimerFillAmount;
            var res = amount - _data.previousFillAmount;
            _data.previousFillAmount = amount;
            return res;
        }

        public TimerData GetData => _data;
    }
}