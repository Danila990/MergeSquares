using System;
using UnityEngine;
using Utils.Attributes;
using Zenject;

namespace CloudServices
{
    public enum ECloudActivatorType
    {
        SocialShare = 0,
        SocialInvite = 1,
        SocialJoinCommunity = 2,
        InApp = 3,
        FullScreenAd = 4,
        RewardedAd = 5,
        PlatformType = 6,
    }
    
    public enum EActivatorActionType
    {
        None = 0,
        Deactivate = 1,
    }
    public class CloudActivator : MonoBehaviour
    {
        [SerializeField] private ECloudActivatorType type;

        [EnumConditionalHide(nameof(type), ECloudActivatorType.PlatformType, true)]
        [SerializeField] private EPlatformType platformType;
        [SerializeField] private EActivatorActionType action;
        [SerializeField] private bool applyOnStart;

        public Action Activated = () => {};

        private CloudService _cloudService;
        
        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
        }
        
        private void Start()
        {
            if(applyOnStart)
            {
                ApplyAction();
            }
        }

        public bool CheckActive()
        {
            var active = false;
            switch (type)
            {
                case ECloudActivatorType.SocialShare:
                    if (_cloudService.CloudProvider.IsSupportsShare ||
                        _cloudService.CloudProvider.IsSupportsNativeShare)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.SocialInvite:
                    if (_cloudService.CloudProvider.IsSupportsNativeInvite)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.SocialJoinCommunity:
                    if (_cloudService.CloudProvider.IsSupportsNativeCommunityJoin && _cloudService.CloudProvider.CanJoinCommunity)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.RewardedAd:
                    if (_cloudService.CloudProvider.IsRewardedAvailable)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.FullScreenAd:
                    if (_cloudService.CloudProvider.IsFullscreenAvailable)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.InApp:
                    if (_cloudService.CloudProvider.IsPaymentsAvailable)
                    {
                        active = true;
                    }
                    break;
                case ECloudActivatorType.PlatformType:
                    if (_cloudService.CloudProvider.GetPlatformType() != platformType)
                    {
                        active = true;
                    }
                    break;
            }

            return active;
        }
        
        private void ApplyAction()
        {
            switch (action)
            {
                case EActivatorActionType.Deactivate:
                    if(!CheckActive())
                    {
                        gameObject.SetActive(false);
                    }
                    break;
                default:
                    if(CheckActive())
                    {
                        Activated.Invoke();
                    }
                    break;
            }
        }
    }
}