using System;
using LargeNumbers;
using UnityEngine;
using Utils;
using Zenject;

namespace GameStats
{
    public class GameStatView : MonoBehaviour
    {
        [SerializeField] private EGameStatType type;
        [SerializeField] private UnityEventString onValueChanged;
        [SerializeField] private UnityEventBool onNonZeroChanged;

        private int _currentValue;
        private LargeNumber _currentValueLarge;
        
        private GameStatService _gameStatService;
        private GameStatLargeService _gameStatLargeService;
        
        [Inject]
        public void Construct(GameStatService gameStatService, GameStatLargeService gameStatLargeService)
        {
            _gameStatService = gameStatService;
            _gameStatLargeService = gameStatLargeService;
        }
        
        public void Start()
        {
            UpdateValue();
        }

        public void OnEnable()
        {
            _gameStatService.StatChanged += OnStatChanged;
            if (_gameStatLargeService != null)
            {
                _gameStatLargeService.StatChanged += OnStatChanged;
            }
            UpdateValue();
        }
        
        public void OnDisable()
        {
            if (_gameStatLargeService != null)
            {
                _gameStatLargeService.StatChanged -= OnStatChanged;
            }
            _gameStatService.StatChanged -= OnStatChanged;
        }
        
        private void UpdateValue()
        {
            if (type != EGameStatType.None)
            {
                var valueLarge = LargeNumber.zero;
                if (_gameStatLargeService.TryGet(type, ref valueLarge))
                {
                    ReportValueChanged(valueLarge, true);
                    return;
                }
                ReportValueChanged(_gameStatService.Get(type), true);
            }
        }
        
        private void OnStatChanged(EGameStatType type, int value)
        {
            if (this.type == type)
            {
                ReportValueChanged(value, false);
            }
        }
        
        private void OnStatChanged(EGameStatType type, LargeNumber value)
        {
            if (this.type == type)
            {
                ReportValueChanged(value, false);
            }
        }

        private void ReportValueChanged(int value, bool forceReport)
        {
            onValueChanged.Invoke(value.ToString());
            var curFlag = _currentValue != 0;
            var newFlag = value != 0;
            if (curFlag != newFlag || forceReport)
            {
                onNonZeroChanged.Invoke(newFlag);
            }
        
            _currentValue = value;
        }

        private void ReportValueChanged(LargeNumber value, bool forceReport)
        {
            onValueChanged.Invoke(new AlphabeticNotation(value).ToString());
            // if(value.magnitude < 2)
            // {
            //     onValueChanged.Invoke(Math.Round(value.Standard()).ToString());
            // }
            // else
            //     onValueChanged.Invoke($"{value.coefficient}{LargeNumber.GetLargeNumberName(value.magnitude)}");
            //
            var curFlag = !_currentValueLarge.isZero;
            var newFlag = !value.isZero;
            if (curFlag != newFlag || forceReport)
            {
                onNonZeroChanged.Invoke(newFlag);
            }

            _currentValueLarge = value;
        }
    }
}