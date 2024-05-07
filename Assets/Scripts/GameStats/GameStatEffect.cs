using System;
using LargeNumbers;
using UnityEngine;

namespace GameStats
{
    public abstract class AGameStatEffect<T>
    {
        public event Action<EGameStatType, T> ValueChanged = (type, i) => { };
        
        public Action StatEnded = () => { };
        
        protected readonly EGameStatType _type;
        protected readonly T _localDelta;
        protected readonly float _durationInSec;
        protected readonly EasingFunction.Ease _ease;
        protected float _elapsedTime = 0.0f;
        protected T _currentAppliedDelta;
        
        public AGameStatEffect(EGameStatType type, T localDelta, float durationInSec = 1f)
        {
            _type = type;
            _localDelta = localDelta;
            _durationInSec = durationInSec;
            _ease = EasingFunction.Ease.Linear;
        }
        
        public AGameStatEffect(EGameStatType type, T localDelta, float durationInSec, EasingFunction.Ease ease)
        {
            _type = type;
            _localDelta = localDelta;
            _durationInSec = 1f;
            _ease = EasingFunction.Ease.Linear;
        }

        public abstract bool Complete();

        public void OnValueChanged(EGameStatType type, T value)
        {
            ValueChanged.Invoke(type, value);
        }

        public abstract void Tick(float deltaTime);
    }
    
    public class GameStatEffectInt : AGameStatEffect<int>
    {
        public override bool Complete() => _currentAppliedDelta == _localDelta;

        public GameStatEffectInt(EGameStatType type, int localDelta) : base(type, localDelta)
        {
            
        }

        public GameStatEffectInt(EGameStatType type, int localDelta, float durationInSec, EasingFunction.Ease ease) : base(type, localDelta, durationInSec, ease)
        {

        }

        public override void Tick(float deltaTime)
        {
            if (!Complete())
            {
                _elapsedTime += Time.deltaTime;
                var progress = Mathf.Clamp(_elapsedTime / _durationInSec, 0.0f, 1.0f);
                var easeProgress = EasingFunction.GetEasingFunction(_ease)(0.0f, 1.0f, progress);
                var newAppliedDelta = (int) Math.Round(Mathf.Lerp(0, _localDelta, easeProgress));
                if (newAppliedDelta != _currentAppliedDelta)
                {
                    var d = newAppliedDelta - _currentAppliedDelta;
                    OnValueChanged(_type, d);
                    _currentAppliedDelta = newAppliedDelta;
                }
            }
        }
    }

    public class GameStatEffectLarge : AGameStatEffect<LargeNumber>
    {
        public override bool Complete() => _currentAppliedDelta == _localDelta;

        public GameStatEffectLarge(EGameStatType type, LargeNumber localDelta) : base(type, localDelta)
        {
            
        }
        
        public override void Tick(float deltaTime)
        {
            if (!Complete())
            {
                _elapsedTime += Time.deltaTime;
                var progress = Mathf.Clamp(_elapsedTime / _durationInSec, 0.0f, 1.0f);
                var easeProgress = EasingFunction.GetEasingFunction(_ease)(0.0f, 1.0f, progress);
                // var newAppliedDelta = (int) Math.Round(Mathf.Lerp(0, _localDelta, easeProgress));

                var newAppliedDelta = (_localDelta * Mathf.Clamp01(easeProgress));

                if (newAppliedDelta != _currentAppliedDelta)
                {
                    var d = newAppliedDelta - _currentAppliedDelta;
                    OnValueChanged(_type, d);
                    _currentAppliedDelta = newAppliedDelta;
                }
            }
        }
    }
}