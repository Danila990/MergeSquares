using GameStats;
using UnityEngine;

namespace Core.Conditions
{
    public enum EGameStatOperation
    {
        None = 0,
        Eq = 1,
        Ne = 2,
        Lt = 3,
        Le = 4,
        Ge = 5,
        Gt = 6,
    }

    public class ConditionGameStat : ConditionBase
    {
        private readonly EGameStatType _type;
        private readonly EGameStatOperation _operation;
        private readonly int _targetValue;
        private readonly GameStatService _gameStatService;

        public ConditionGameStat(
            EGameStatType type,
            EGameStatOperation operation,
            int targetValue,
            GameStatService gameStatService
        )
        {
            _type = type;
            _operation = operation;
            _targetValue = targetValue;
            _gameStatService = gameStatService;
            _gameStatService.StatChanged += OnGameStatChanged;
        }

        public override void Dispose()
        {
            _gameStatService.StatChanged -= OnGameStatChanged;
            base.Dispose();
        }

        public override bool IsTrue
        {
            get
            {
                var currentValue = _gameStatService.Get(_type);
                switch (_operation)
                {
                    case EGameStatOperation.None:
                        Debug.LogError("[ConditionGameStatIsZero][IsTrue] None is unsupported operation");
                        return false;
                    case EGameStatOperation.Eq:
                        return currentValue == _targetValue;
                    case EGameStatOperation.Ne:
                        return currentValue != _targetValue;
                    case EGameStatOperation.Lt:
                        return currentValue < _targetValue;
                    case EGameStatOperation.Le:
                        return currentValue <= _targetValue;
                    case EGameStatOperation.Ge:
                        return currentValue >= _targetValue;
                    case EGameStatOperation.Gt:
                        return currentValue > _targetValue;
                }

                return false;
            }
        }

        private void OnGameStatChanged(EGameStatType type, int value)
        {
            if (type == _type)
            {
                MarkChanged();
            }
        }
    }
}