using Core.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Advertising.AnalyticsSignals
{
    public enum AdStatus
    {
        Completed, 
        Failed
    }

    public enum AdType
    {
        Rewarded,
        Interstitial,
        Banner
    }

    public class AdSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly AdType Type;
        public readonly AdStatus Status;
        public readonly string PlacementName;

        public AdSignal(AdType type, AdStatus status, string placementName)
        {
            Type = type;
            Status = status;
            PlacementName = placementName;
        }
    }
}
