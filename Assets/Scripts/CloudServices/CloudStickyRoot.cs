using System;
using System.Collections.Generic;
using System.Linq;
using CrazyGames;
using UnityEngine;
using Zenject;

namespace CloudServices
{
    [Serializable]
    public class CloudSticky
    {
        public ECloudDeviceType DeviceType;
        public GameObject GameObject;
    }
    public class CloudStickyRoot : MonoBehaviour
    {
        [SerializeField] private List<CloudSticky> stickies = new();

        private CloudService _cloudService;
        
        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
            _cloudService.CloudProvider.StickyChanged += OnStickyChanged;
        }

        private void Start()
        {
            OnStickyChanged(false);
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.StickyChanged -= OnStickyChanged;
        }

        private void OnStickyChanged(bool active)
        {
            var deviceType = _cloudService.CloudProvider.GetDeviceType();
            var stickies = this.stickies.Where(s => s.DeviceType == deviceType);
            foreach (var sticky in stickies)
            {
                sticky.GameObject.SetActive(active);
                if (_cloudService.CloudProvider.GetPlatformType() == EPlatformType.CRAZY_GAMES_NAO)
                {
                    var crazySticky = sticky.GameObject.GetComponent<CrazyBanner>();
                    if (crazySticky != null)
                    {
                        crazySticky.MarkVisible(active);
                    }
                }
            }

            if (_cloudService.CloudProvider.GetPlatformType() == EPlatformType.CRAZY_GAMES_NAO)
            {
                CrazyAds.Instance.updateBannersDisplay();
            }
        }
    }
}