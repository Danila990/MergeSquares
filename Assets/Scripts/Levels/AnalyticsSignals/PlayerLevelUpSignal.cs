using Core.Signals;

namespace Levels.AnalyticsSignals
{
    public class PlayerLevelUpSignal : AnalyticsSignal, IYandexAnalyticsSignal
    {
        public int Level { get; }

        public PlayerLevelUpSignal(int level)
        {
            Level = level;
        }
    }
}
