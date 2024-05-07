using System;
using Core.Localization;
using GameTime;
using TMPro;
using UnityEngine;
using Zenject;

namespace Shop
{
    public class UnitCategoryHeaderLabelTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private long updateEverySeconds;
        [SerializeField] private LocalizeUi localization;
        
        public Action UpdateCategory = () => {};

        public long UpdateInterval => updateEverySeconds;

        private DateTime _lastCategoryUpdate;
        private string _localH;
        private string _localMin;
        private string _localSec;
        
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

        public void SetLastCategoryUpdateTime(long ticksTimeStamp)
        {
            _lastCategoryUpdate = new DateTime(ticksTimeStamp).ToLocalTime();
            OnTick();
        }

        private void OnTick()
        {
            if (!CategoryTimeExpired(_lastCategoryUpdate, out var secFromLastUpdate))
            {
                var remainingTime = TimeSpan.FromSeconds(updateEverySeconds - secFromLastUpdate);
                
                countdownText.text = remainingTime.Hours > 0 ? $"{remainingTime.Hours:d2}{_localH} : {remainingTime.Minutes:d2}{_localMin}" 
                    : $"{remainingTime.Minutes:d2}{_localMin} : {remainingTime.Seconds:d2}{_localSec}";
            }
            else
            {
                UpdateCategory.Invoke();
            }
        }

        public bool CategoryTimeExpired(DateTime lastCategoryUpdate, out double secFromLastUpdate)
        {
            var timeFromLastUpdate = DateTime.Now - lastCategoryUpdate;
            secFromLastUpdate = timeFromLastUpdate.TotalSeconds;
            
            return !(secFromLastUpdate < updateEverySeconds);
        }
    }
}

