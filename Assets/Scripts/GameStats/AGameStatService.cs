using System;
using System.Collections.Generic;
using Core.SaveLoad;
using UnityEngine;
using Utils;
using Zenject;

namespace GameStats
{
    [Serializable]
    public class GameStatDataInt
    {
        public List<GameStatVariable> statsInt = new();
    }
    
    public class GameStatDataLarge
    {
        public List<GameStatVariableLargeNumber> statsLargeNumbers = new();
    }

    [Serializable]
    public class GameStatMap
    {
        public EGameStatType type;
        public string id;
    }
    [Serializable]
    public abstract class AGameStatService<T> : MonoBehaviour
    {
        [SerializeField] protected List<GameStatMap> maps = new();
        [Space] [SerializeField] protected Saver saver;
        
        public event Action<EGameStatType, T> StatChanged = (type, i) => { };
        public event Action Inited = () => { };
        
        public EGameStatType Map(string id) => maps.GetBy(map => map.id == id).type;
        public bool IsInited => _isInited;

        protected readonly List<AGameStatEffect<T>> _effects = new();
        protected SaveService _saveService;
        protected bool _isInited = false;
        
        [Inject]
        public void Construct(SaveService saveService)
        {
            _saveService = saveService;
            _saveService.LoadFinished += OnLoadFinished;
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }

        protected virtual void OnDestroy()
        {
            _saveService.LoadFinished -= OnLoadFinished;
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
        
        
        private void Update()
        {
            for (var i = 0; i < _effects.Count; ++i)
            {
                var fx = _effects[i];
                fx.Tick(Time.deltaTime);
                if (fx.Complete())
                {
                    RemoveEffect(fx);
                    // fx.ValueChanged -= OnFxValueChanged;
                    fx.StatEnded.Invoke();
                    _effects.SwapEraseAt(i);
                    i -= 1;
                }
            }
        }

        public void OnStatChanged(EGameStatType type, T value)
        {
            StatChanged.Invoke(type, value);
        }

        public abstract bool TryIncWithAnim(EGameStatType type, T value, Action OnEnd = null, float duration = 1f);

        public abstract bool TryDecWithAnim(EGameStatType type, T value);

        public abstract bool TrySetWithAnim(EGameStatType type, T value, Action OnEnd = null, float duration = 1f);

        public abstract void ResetLocalDeltaWithAnim(EGameStatType type, Action OnEnd = null);

        public abstract T Get(EGameStatType type);
        
        public abstract bool TryGet(EGameStatType type, ref T value);

        public abstract bool TrySet(EGameStatType type, T value);

        public abstract bool TryInc(EGameStatType type, T value);

        public abstract bool TryDec(EGameStatType type, T value);

        public abstract bool TryAddLocalDelta(EGameStatType type, T value);

        public abstract bool TryResetLocalDelta(EGameStatType type);
        protected abstract void OnFxValueChanged(EGameStatType type, T value);

        protected abstract void RemoveEffect(AGameStatEffect<T> effect);
        
        protected abstract void ReportStatsChanged(AGameStatVariable<T> statVariable);
        public abstract T GetStatValue(EGameStatType type);
        
        public abstract T GetStatLimit(EGameStatType type);

        protected void OnInited()
        {
            _isInited = true;
            Inited?.Invoke();
        }

        protected abstract void OnDataLoaded(string data, LoadContext context);

        protected abstract void OnLoadFinished(LoadContext loadContext);

        protected abstract string OnDataSaved();
    }
}
