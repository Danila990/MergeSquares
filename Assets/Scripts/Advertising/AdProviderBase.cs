using System;
using UnityEngine;

namespace Advertising
{
    public enum EAdType
    {
        FullScreen = 1,
        Rewarded = 2,
        Sticky = 3,
    }
    
    public enum EAdLevelType
    {
        Before = 1,
        After = 2,
    }
    
    public abstract class AdProviderBase : MonoBehaviour
    {
        public bool IsAvailable => _available;
        public abstract void Show(EAdType type, string placementName);
        public abstract bool TryShow(EAdType type, string placementName);
        
        public abstract void StopShow(EAdType type, string placementName);

        public abstract bool CanShowForLevel(EAdLevelType type);
        public abstract bool CanShow(EAdType type);

        public Action<EAdType> Loaded = type => {};
        public Action<EAdType> Clicked = type => {};
        public Action<EAdType, string> Failed = (type, placementName) => {};
        public Action<EAdType, string> Rewarded = (type, placementName) => {};

        protected bool _available = false;
    }
}
