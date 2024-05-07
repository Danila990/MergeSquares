using System;
using Core.Localization;
using GameTime;
using TMPro;
using UnityEngine;
using Zenject;

namespace Shop
{
    public class LabelTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private LocalizeUi localization;

        public Action ReloadPanel = () => {};
        
        private string _localH;
        private string _localMin;
        private string _localSec;
        private bool _isInited;
        private DateTime _timeOfRecharge;

        private TimeService _timeService;

        [Inject]
        public void Construct(TimeService timeService)
        {
            _timeService = timeService;
        }

        private void OnEnable()
        {
            _timeService.Tick += OnTick;

            _localH = localization.GetLocalizedText("HText");
            _localMin = localization.GetLocalizedText("MinText");
            _localSec = localization.GetLocalizedText("SecText");
        }

        private void OnDisable()
        {
            _timeService.Tick -= OnTick;
        }

        public static DateTime CalculateTimeOfReload(bool reloadAtMidnight, DateTime lastReloadTimeStamp, int reloadEverySeconds)
        {
            if (reloadAtMidnight)
            {
                var nowDay = DateTime.Now;
                var dayEnd = new DateTime(nowDay.Year, nowDay.Month, nowDay.Day, 23, 59, 59);
                return dayEnd;
            }
            
            var timeSinceLastReload = DateTime.Now - lastReloadTimeStamp;
            var timeToReload = reloadEverySeconds - timeSinceLastReload.TotalSeconds;
            var nextReloadTime = DateTime.Now.AddSeconds(timeToReload);

            return nextReloadTime;
        }
        
        public void Init(DateTime timeOfRecharge)
        {
            _timeOfRecharge = timeOfRecharge;
            _isInited = true;
            OnTick();
        }

        private void OnTick()
        {
            if (!_isInited) return;
            
            var timeSpan = _timeOfRecharge - DateTime.Now;
            if (timeSpan.TotalSeconds <= 0)
            {
                ReloadPanel.Invoke();
                return;
            }
            
            countdownText.text = timeSpan.Hours > 0 ? $"{timeSpan.Hours:d2}{_localH} : {timeSpan.Minutes:d2}{_localMin}" 
                : $"{timeSpan.Minutes:d2}{_localMin} : {timeSpan.Seconds:d2}{_localSec}";
        }
    }
}

