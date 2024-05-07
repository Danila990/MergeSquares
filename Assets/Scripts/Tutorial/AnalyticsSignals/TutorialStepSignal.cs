using Core.Signals;

namespace Tutorial.AnalyticsSignals
{
    public class TutorialStepSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public int Index { get; }
        public string Id { get; }

        public TutorialStepSignal(int index, string id)
        {
            Index = index;
            Id = id;
        }
    }
}
