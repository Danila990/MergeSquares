using System;

namespace Utils
{
    public class TimeUtils
    {
        public const long TicksInSecond = 10_000_000;

        public static string SecondsToMS(long seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.TotalMinutes >= 1)
                return timeSpan.ToString("mm\\:ss");
            return timeSpan.ToString("ss");
        }

        public static string SecondsToHMS(long seconds)
        {
            return SecondsToHMS(TimeSpan.FromSeconds(seconds));
        }

        public static string SecondsToHMS(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString("hh\\:mm\\:ss");
            if (timeSpan.TotalMinutes >= 1)
                return timeSpan.ToString("mm\\:ss");
            return timeSpan.ToString("ss");
        }

        public static string SecondsToHMSFormat(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString("hh\\:mm\\:ss");
        }

        public static string SecondsToHMSFormat(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString("hh\\:mm\\:ss");
            return timeSpan.ToString("mm\\:ss");
        }

        public static string TicksToHMS(long ticks)
        {
            var timeSpan = TimeSpan.FromTicks(ticks);
            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}