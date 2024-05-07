using Core.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.AnalyticsSignals
{
    public enum LevelStatus
    {
        Started,
        Passed,
        Failed
    }

    public class LevelStatusSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public readonly LevelStatus Status;
        public readonly int Id;

        public LevelStatusSignal(LevelStatus status, int id)
        {
            Status = status;
            Id = id;
        }
    }
}
