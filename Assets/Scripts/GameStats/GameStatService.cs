using System;
using System.Collections.Generic;
using Core.SaveLoad;
using UnityEngine;
using Utils;

namespace GameStats
{
    public class GameStatService : AGameStatService<int>
    {
        [SerializeField] private List<GameStatVariable> defaultStats;

        private GameStatDataInt _data = new();
        
        public void Init(IReadOnlyList<GameStatVariable> stats)
        {
            _data.statsInt = new List<GameStatVariable>(stats);
            foreach (var stat in _data.statsInt)
            {
                stat.StatChanged += ReportStatsChanged;
            }

            OnInited();
        }
        
        public override int GetStatValue(EGameStatType type)
        {
            foreach (var stat in _data.statsInt)
            {
                if (stat.type == type)
                {
                    var statInt = stat;
                    if (statInt != null)
                        return statInt.RealValue;
                }
            }

            Debug.LogError($"[GameStatService][GetStatValue] Error: not find stat {type}. Returned '-1'");
            return -1;
        }

        public override int GetStatLimit(EGameStatType type)
        {
            foreach (var stat in _data.statsInt)
            {
                if (stat.type == type)
                {
                    var statInt = stat;
                    if (statInt != null)
                        return statInt.Limit;
                }
            }

            Debug.LogError($"[GameStatService][GetStatLimit] Error: not find stat {type}. Returned '-1'");
            return -1;
        }

        public bool TryIncWithAnim(GameStatContainer statContainer)
        {
            return TryIncWithAnim(statContainer.type, statContainer.value);
        }
        
        public override bool TryIncWithAnim(EGameStatType type, int value, Action OnEnd = null, float durationInSec = 1f)
        {
            var stat = GetOrCreateStat(type);
            return TrySetWithAnim(type, stat.RealValue + value, OnEnd);
        }

        public bool TryDecWithAnim(GameStatContainer statContainer)
        {
            return TryDecWithAnim(statContainer.type, statContainer.value);
        }

        public override bool TryDecWithAnim(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            return TrySetWithAnim(type, stat.RealValue - value);
        }

        public bool TrySetWithAnim(GameStatContainer statContainer)
        {
            return TrySetWithAnim(statContainer.type, statContainer.value);
        }
        
        public override bool TrySetWithAnim(EGameStatType type, int value, Action OnEnd = null, float durationInSec = 1f)
        {
            var stat = GetOrCreateStat(type);
            if (stat.TrySet(value, out var setValue))
            {
                stat.TryAddLocalDelta(-1 * setValue);
                var fx = new GameStatEffectInt(type, setValue);
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
            var fx = new GameStatEffectInt(type, -stat.Delta);
            fx.ValueChanged += OnFxValueChanged;
            if (OnEnd != null)
            {
                fx.StatEnded += OnEnd;
            }

            _effects.Add(fx);
        }

        public override int Get(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat.GetValue();
        }
        
        public override bool TryGet(EGameStatType type, ref int value)
        {
            var result = _data.statsInt.GetBy(stat => stat.type == type);
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
        
        public GameStatVariable GetStat(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat;
        }

        public bool TrySet(GameStatContainer statContainer)
        {
            return TrySet(statContainer.type, statContainer.value);
        }

        public override bool TrySet(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TrySet(value);
        }

        public bool TryInc(GameStatContainer statContainer)
        {
            return TrySet(statContainer.type, statContainer.value);
        }

        public override bool TryInc(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TrySet(stat.RealValue + value);
        }

        public bool TryDec(GameStatContainer statContainer)
        {
            return TryDec(statContainer.type, statContainer.value);
        }

        public override bool TryDec(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TrySet(stat.RealValue - value);
        }

        public bool TryAddLocalDelta(GameStatContainer statContainer)
        {
            return TryAddLocalDelta(statContainer.type, statContainer.value);
        }

        public override bool TryAddLocalDelta(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            return stat.TryAddLocalDelta(value);
        }

        public override bool TryResetLocalDelta(EGameStatType type)
        {
            var stat = GetOrCreateStat(type);
            return stat.TryResetLocalDelta();
        }

        protected override void OnFxValueChanged(EGameStatType type, int value)
        {
            var stat = GetOrCreateStat(type);
            stat.TryAddLocalDelta(value);
        }
        
        protected override void RemoveEffect(AGameStatEffect<int> effect)
        {
            effect.ValueChanged -= OnFxValueChanged;
        }

        protected GameStatVariable GetOrCreateStat(EGameStatType type)
        {
            var result = _data.statsInt.GetBy(stat => stat.type == type);
            if (result == null)
            {
                result = new GameStatVariable(type);
                _data.statsInt.Add(result);
                result.StatChanged += ReportStatsChanged;
            }

            return result;
        }

        protected override void ReportStatsChanged(AGameStatVariable<int> statVariable)
        {
            OnStatChanged(statVariable.type, statVariable.GetValue());
            saver.SaveNeeded.Invoke(false);
        }

        protected override void OnDataLoaded(string data, LoadContext context)
        {
            var gameStatData = saver.Unmarshal(data, new GameStatDataInt
            {
                statsInt = defaultStats
            });
            Init(gameStatData.statsInt);
        }

        protected override void OnLoadFinished(LoadContext loadContext)
        {
            foreach (var stat in _data.statsInt)
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