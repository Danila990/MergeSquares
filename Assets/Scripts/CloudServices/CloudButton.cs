using System;
using Core.Localization;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace CloudServices
{
    public class CloudButton : MonoBehaviour
    {
        [SerializeField] private string shareTextKey;
        [SerializeField] private string postTextKey;
        
        private CloudService _cloudService;
        private LocalizationRepository _localizationRepository;
        
        [Inject]
        public void Construct(CloudService cloudService, LocalizationRepository localizationRepository)
        {
            _cloudService = cloudService;
            _localizationRepository = localizationRepository;
        }

        [UsedImplicitly]
        public void Share()
        {
            if(!String.IsNullOrEmpty(shareTextKey))
            {
                _cloudService.CloudProvider.Share(_localizationRepository.GetTextInCurrentLocale(shareTextKey));
            }
        }
        
        [UsedImplicitly]
        public void Post()
        {
            if(!String.IsNullOrEmpty(postTextKey))
            {
                _cloudService.CloudProvider.Post(_localizationRepository.GetTextInCurrentLocale(postTextKey));
            }
        }
        
        [UsedImplicitly]
        public void Invite()
        {
            _cloudService.CloudProvider.Invite();
        }
        
        [UsedImplicitly]
        public void JoinCommunity()
        {
            _cloudService.CloudProvider.JoinCommunity();
        }
    }
}