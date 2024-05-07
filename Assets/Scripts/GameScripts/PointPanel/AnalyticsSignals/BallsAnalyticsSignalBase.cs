using System.Collections.Generic;
using Core.Signals;

namespace GameScripts.PointPanel.AnalyticsSignals
{
    public class BallsAnalyticsSignalBase : AnalyticsSignal, IYandexAnalyticsSignal, IBallsSignal
    {
        public BallsAnalyticsSignalBase(Dictionary<string, object> signalParams) : base(signalParams) { }
    }
}
