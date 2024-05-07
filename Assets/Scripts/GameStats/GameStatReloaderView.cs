using System;
using UnityEngine;
using Utils;
using Zenject;

namespace GameStats
{
    public class GameStatReloaderView : MonoBehaviour
    {
        [SerializeField] private UnityEventString onValueChanged;

        public Action<bool> ValueChanged = maxValue => {};
        public bool InReload => _gameStatReloader.ActiveReload();
        
        private GameStatReloader _gameStatReloader;

        [Inject]
        public void Construct(GameStatReloader gameStatReloader)
        {
            _gameStatReloader = gameStatReloader;
            _gameStatReloader.ReloadTimerChanged += OnReloadTimerChanged;
            _gameStatReloader.ReloadActiveChanged += OnActiveChanged;
        }

        private void OnDestroy()
        {
            _gameStatReloader.ReloadTimerChanged -= OnReloadTimerChanged;
            _gameStatReloader.ReloadActiveChanged -= OnActiveChanged;
        }

        public void Start()
        {
            UpdateValue();
        }

        public void OnEnable()
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            if (_gameStatReloader.ActiveReload())
            {
                ValueChanged.Invoke(false);
                gameObject.SetActive(true);
                ReportValueChanged(_gameStatReloader.GetReloadAmount());
            }
            else
            {
                ValueChanged.Invoke(true);
                gameObject.SetActive(false);
            }
        }

        private void OnActiveChanged(bool value)
        {
            UpdateValue();
        }

        private void OnReloadTimerChanged(int value)
        {
            ReportValueChanged(value);
        }

        private void ReportValueChanged(int value)
        {
            var timeSpan = TimeSpan.FromSeconds(value);
            onValueChanged.Invoke($"{timeSpan.Minutes}:{timeSpan.Seconds:D2}");
        }
    }
}