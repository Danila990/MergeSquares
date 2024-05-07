using System.Collections.Generic;
using Core.Signals;

namespace GameScripts.MergeSquares.AnalyticsSignals
{
    public class SquaresAnalyticsSignalBase : AnalyticsSignal, IYandexAnalyticsSignal, ISquaresSignal
    {
        public SquaresAnalyticsSignalBase(Dictionary<string, object> signalParams) : base(signalParams) { }
    }
}
