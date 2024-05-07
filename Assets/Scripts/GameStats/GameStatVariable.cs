using System;
using LargeNumbers;
using UnityEngine;

namespace GameStats
{
    [Serializable]
    public abstract class AGameStatVariable<T>
    {
        public EGameStatType type;
        public bool limited;
        public event Action<AGameStatVariable<T>> StatChanged = stat => { };
        
        // public T Delta => _localDelta;        
        // public T RealValue => realValue;
        // public T Limit => limit;
        
        public abstract bool MaxReached();

        public abstract T GetValue();

        // public T _localDelta;
        // public T realValue;
        // public T limit;
        
        public AGameStatVariable(EGameStatType type)
        {
            this.type = type;   
        }
        
        public void OnStatChanged(AGameStatVariable<T> variable)
        {
            StatChanged.Invoke(variable);
        }
        
        public virtual bool TrySet(T value, bool needsAction = true)
        {
            return TrySet(value, out var setValue, needsAction);
        }

        public abstract bool TrySet(T value, out T setValue, bool needsAction = true);

        public abstract bool TryResetLocalDelta(bool needsAction = true);

        public abstract bool TryAddLocalDelta(T value);
    }
    
    [Serializable]
    public class GameStatVariable : AGameStatVariable<int>
    {
        public int Delta => _localDelta;        
        public int RealValue => realValue;
        public int Limit => limit;
        
        public int _localDelta;
        public int realValue;
        public int limit;
        public GameStatVariable(EGameStatType type) : base(type)
        {
            _localDelta = 0;
        }

        public override bool MaxReached() => realValue >= limit;
        public override int GetValue() => Mathf.Max(realValue + _localDelta, 0);

        public override bool TrySet(int value, out int setValue, bool needsAction = true)
        {
            setValue = 0;
            if (realValue != value && value >= 0)
            {
                var prev = realValue;
                realValue = limited ? Mathf.Min(value, limit) : value;
                setValue = realValue - prev;
                if (needsAction)
                    OnStatChanged(this);
                return true;
            }

            return false;
        }

        public override bool TryResetLocalDelta(bool needsAction = true)
        {
            if (_localDelta != 0)
            {
                _localDelta = 0;
                if (needsAction)
                    OnStatChanged(this);
                return true;
            }

            return false;
        }
        
        public bool TrySetLocalDelta(int value)
        {
            if (value != 0)
            {
                _localDelta = value;
                OnStatChanged(this);
                return true;
            }
            
            return false;
        }

        public override bool TryAddLocalDelta(int value)
        {
            if (value != 0)
            {
                _localDelta += value;
                OnStatChanged(this);
                return true;
            }

            return false;
        }
    }
    
    [Serializable]
    public class GameStatVariableLargeNumber : AGameStatVariable<LargeNumber>
    {
        public LargeNumber Delta => _localDelta;        
        public LargeNumber RealValue => realValue;
        public LargeNumber Limit => limit;
        
        public LargeNumber _localDelta;
        public LargeNumber realValue;
        public LargeNumber limit;
        
        public GameStatVariableLargeNumber(EGameStatType type) : base(type)
        {
            _localDelta = LargeNumber.zero;
        }
        
        public override bool MaxReached() => realValue >= limit;

        public override LargeNumber GetValue()
        {
            var value = realValue + _localDelta;
            return value > LargeNumber.zero ? value : LargeNumber.zero;
        }

        public override bool TrySet(LargeNumber value, out LargeNumber setValue, bool needsAction = true)
        {
            setValue = LargeNumber.zero;
            if (realValue != value && value >= LargeNumber.zero)
            {
                var prev = realValue;

                if (limited)
                {
                    if (limit > value)
                        realValue = value;
                    else
                        realValue = limit;
                }
                else
                {
                    realValue = value;
                }
                setValue = realValue - prev;
                if(needsAction)
                    OnStatChanged(this);
                return true;
            }

            return false;
        }

        public override bool TryResetLocalDelta(bool needsAction = true)
        {
            if (_localDelta != LargeNumber.zero)
            {
                _localDelta = LargeNumber.zero;
                if (needsAction)
                    OnStatChanged(this);
                return true;
            }

            return false;
        }
        
        public override bool TryAddLocalDelta(LargeNumber value)
        {
            if (value != 0)
            {
                _localDelta += value;
                OnStatChanged(this);
                return true;
            }
        
            return false;
        }
    }
}