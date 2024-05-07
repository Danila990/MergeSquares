using CloudServices;
using Core.Windows;
using GameScripts.AnalyticsSignals;
using Popups.AnalyticsSignals;
using UnityEngine;
using Zenject;

namespace Popups
{
    public class RatePopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;

        private bool _wasRated;
        
        private CloudService _cloudService;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(CloudService cloudService, SignalBus signalBus)
        {
            _cloudService = cloudService;
            _signalBus = signalBus;
            popupBase.Disposed += OnDisposed;
        }

        private void OnDisposed(PopupBaseCloseType type)
        {
            _signalBus.Fire(new RateSignal(_wasRated));
        }

        public void Rate()
        {
            _wasRated = true;
            _cloudService.CloudProvider.Rate();
        }
    }
}
