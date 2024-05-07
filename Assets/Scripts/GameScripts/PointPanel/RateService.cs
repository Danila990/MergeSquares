using System;
using CloudServices;
using Core.Windows;
using UnityEngine;
using Zenject;

namespace GameScripts.PointPanel
{
    public class RateService : MonoBehaviour
    {
        [SerializeField] private int minRateLevel = 3;
        [SerializeField] private int rateLevelInterval = 3;
        [Space][SerializeField] private int ratingEnabledEditor = 1;

        private WindowManager _windowManager;
        private CloudService _cloudService;

        [Inject]
        public void Construct(WindowManager windowManager, CloudService cloudService)
        {
            _windowManager = windowManager;
            _cloudService = cloudService;
            _cloudService.CloudProvider.ReviewChecked += ReviewChecked;
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.ReviewChecked -= ReviewChecked;
        }

        public bool CanShowOnLevel(int level)
        {
            return level == minRateLevel || level > minRateLevel && level % rateLevelInterval == 0;
        }

        public void StartRating()
        {
            _cloudService.CloudProvider.StartRate();
        }

        public void ShowRateWindow(int ratingEnabled)
        {
            if (ratingEnabled == 0)
            {
                return;
            }

            _windowManager.ShowWindow(EPopupType.RatePopup.ToString());
        }
        
        private void ReviewChecked(bool canReview)
        {
            if(canReview)
            {
                ShowRateWindow(ratingEnabledEditor);
            }
        }
    }
}