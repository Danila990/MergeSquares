using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Signals
{
    public interface ISignal { }

    public abstract class SignalBase<T, TOwner> : ISignal where T : struct
    {
        private readonly T _value;
        private readonly TOwner _owner;

        public T Value => _value;
        public TOwner Owner => _owner;

        public SignalBase(T value, TOwner owner)
        {
            _value = value;
            _owner = owner;
        }

        public static Action<SignalBase<T, TOwner>> CreateRunDelegate(IReadOnlyList<TOwner> collection, Action<T> onRun)
        {
            return signal =>
            {
                if (collection.Contains(signal._owner))
                {
                    onRun(signal._value);
                }
            };
        }

        public static Action<SignalBase<T, TOwner>> CreateRunDelegate<TValue>(
            IReadOnlyDictionary<TOwner, TValue> collection, Action<T> onRun)
        {
            return signal =>
            {
                if (collection.ContainsKey(signal._owner))
                {
                    onRun(signal._value);
                }
            };
        }
    }
}