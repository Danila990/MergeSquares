using System.Collections.Generic;

namespace Core.Signals
{
    public interface IYandexAnalyticsSignal { }

    public abstract class AnalyticsSignal : ISignal
    {
        public readonly Dictionary<string, object> Params;
        public string Name { get; protected set; }
        public virtual bool IsFlush { get; } = false;

        public AnalyticsSignal() { }

        public AnalyticsSignal(Dictionary<string, object> signalParams)
        {
            Params = signalParams;
        }
    }
}