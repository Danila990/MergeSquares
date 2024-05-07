using System;
using System.Collections.Generic;
using Core.SaveLoad;
using LargeNumbers;
using UnityEngine;
using Utils;
using Zenject;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Composites;

namespace GameStats
{
    public class GameStatLargeService : AGameStatService<LargeNumber>
    {
        [SerializeField] private List<GameStatVariableLargeNumber> defaultStats;

        private GameStatDataLarge _data = new GameStatDataLarge();
        
        public override LargeNumber GetStatValue(EGameStatType type)
        {
            foreach (var stat in _data.statsLargeNumbers)
            {
                if (stat.type == type)
                {
                    var statInt = stat;
                    if (statInt != null)
                        return statInt.RealValue;
                }
            }
            Debug.LogError($"[GameStatLargeService][GetStatValue] Error: not find stat {type}. Returned '-1'");
            return LargeNumber.zero;
        }

        public override LargeNumber GetStatLimit(EGameStatType type)
        {
            foreach (var stat in _data.statsLargeNumbers)
            {
                if (stat.type == type)
                {
                    var statInt = stat;
                    if (statInt != null) 
                        return statInt.Limit;
                }
            }
            Debug.LogError($"[GameStatLargeService][GetStatLimit] Error: not find stat {type}. Returned '-1'");
            return LargeNumber.zero;
        }
        
        public void Init(IReadOnlyList<GameStatVariableLargeNumber> statsLargeNumbers)
        {
            _data.statsLargeNumbers = new List<GameStatVariableLargeNumber>(statsLargeNumbers);
            foreach (var stat in _data.statsLargeNumbers)
            {
                stat.TrySet(stat.GetValue(), false);
                stat.TryResetLocalDelta(false);
            }
            foreach (var stat in _data.statsLargeNumbers)
            {
                stat.StatChanged += ReportStatsChanged;
            }
        
            OnInited();
        }

        public override bool TryIncWithAnim(EGameStatType type, LargeNumber value, Action OnEnd = null, float durationInSec = 1f)
        {
            var stat = GetOrCreateStatLarge(type);
            return TrySetWithAnim(type, stat.RealValue + value, OnEnd);
        }
        

        public override bool TryDecWithAnim(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStat(type);
            return TrySetWithAnim(type, stat.RealValue - value);
        }

        public override bool TrySetWithAnim(EGameStatType type, LargeNumber value, Action OnEnd = null, float durationInSec = 1f)
        {
            var stat = GetOrCreateStatLarge(type);
            if (stat.TrySet(value, out var setValue))
            {
                stat.TryAddLocalDelta(-1 * setValue);

                var fx = new GameStatEffectLarge(type, setValue);
                fx.ValueChanged += OnFxValueChanged;
                if (OnEnd != null)
                {
                    fx.StatEnded += OnEnd;
                }
                _effects.Add(fx);
                return true;
            }

            return false;
        }

        public override void ResetLocalDeltaWithAnim(EGameStatType type, Action OnEnd = null)
        {
            var stat = GetOrCreateStat(type);
            // stat.TryAddLocalDelta(-1 * setValue);
            var fx = new GameStatEffectLarge(type, new LargeNumber(-stat.Delta));
            fx.ValueChanged += OnFxValueChanged;
            if (OnEnd != null)
            {
                fx.StatEnded += OnEnd;
            }
            _effects.Add(fx);
        }

        public override LargeNumber Get(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat.GetValue();
        }

        public override bool TryGet(EGameStatType type, ref LargeNumber value)
        {
            var result = _data.statsLargeNumbers.GetBy(stat => stat.type == type);
            if (result == null)
            {
                return false;
            }
            else
            {
                value = result.GetValue();
                return true;
            }
        }

        public GameStatVariableLargeNumber GetStat(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat;
        }
        
        public GameStatVariableLargeNumber GetStatLarge(EGameStatType type)
        {
            var stat = GetOrCreateStatLarge(type);
            return stat;
        }

        public override bool TrySet(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStatLarge(type);
            return stat.TrySet(value);
        }

        public override bool TryInc(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TrySet(stat.RealValue + value);
        }

        public override bool TryDec(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TrySet(stat.RealValue - value);
        }

        public override bool TryAddLocalDelta(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TryAddLocalDelta(value);
        }

        public override bool TryResetLocalDelta(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat.TryResetLocalDelta();
        }
        
        protected override void OnFxValueChanged(EGameStatType type, LargeNumber value)
        {
            var stat = GetOrCreateStatLarge(type);
            stat.TryAddLocalDelta(value);
        }

        protected override void RemoveEffect(AGameStatEffect<LargeNumber> effect)
        {
            effect.ValueChanged -= OnFxValueChanged;
        }

        private GameStatVariableLargeNumber GetOrCreateStat(EGameStatType type)
        {
            var result = _data.statsLargeNumbers.GetBy(stat => stat.type == type);
            if (result == null)
            {
                result = new GameStatVariableLargeNumber(type);
                _data.statsLargeNumbers.Add(result);
                result.StatChanged += ReportStatsChanged;
            }

            return result;
        }

        protected GameStatVariableLargeNumber GetOrCreateStatLarge(EGameStatType type)
        {
            var result = _data.statsLargeNumbers.GetBy(stat => stat.type == type);
            if (result == null)
            {
                result = new GameStatVariableLargeNumber(type);
                _data.statsLargeNumbers.Add(result);
                result.StatChanged += ReportStatsChanged;
            }

            return result;
        }

        protected override void ReportStatsChanged(AGameStatVariable<LargeNumber> statVariable)
        {
            OnStatChanged(statVariable.type, statVariable.GetValue());
            saver.SaveNeeded.Invoke(true);
        }

        protected override void OnDataLoaded(string data, LoadContext context)
        {
            var gameStatData = saver.Unmarshal(data, new GameStatDataLarge());
            Init(gameStatData.statsLargeNumbers);
        }
        
        protected override void OnLoadFinished(LoadContext loadContext)
        {
            foreach (var stat in _data.statsLargeNumbers)
            {
                 OnStatChanged(stat.type, stat.GetValue());
            }
        }

        protected override string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}