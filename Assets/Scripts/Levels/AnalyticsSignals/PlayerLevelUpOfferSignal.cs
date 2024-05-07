using Core.Signals;

namespace Levels.AnalyticsSignals
{
    class PlayerLevelUpOfferSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public int Level { get; }

        public PlayerLevelUpOfferSignal(int level)
        {
            Level = level;
        }
    }
}
