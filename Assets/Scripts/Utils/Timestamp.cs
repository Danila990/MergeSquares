using System;

namespace Utils
{
    public class Timestamp
    {
        public static TimeSpan CalculateTimeDiff(string timestamp)
        {
            return CalculateTimeDiff(Convert.ToInt64(timestamp));
        }

        public static TimeSpan CalculateTimeDiff(long timestamp) =>
            DateTime.UtcNow - ConvertToDateTime(timestamp);

        public static TimeSpan CalculateTimeDiff(long timestamp1, long timestamp2)
        {
            if (timestamp2 > timestamp1)
            {
                return ConvertToDateTime(timestamp2) - ConvertToDateTime(timestamp1);
            }

            return ConvertToDateTime(timestamp1) - ConvertToDateTime(timestamp2);
        }

        public static long GetTicks() => DateTime.UtcNow.Ticks;
        public static long GetTicks(TimeSpan ts) => DateTime.UtcNow.Subtract(ts).Ticks;

        public static string GetStringTicks() => DateTime.UtcNow.Ticks.ToString();

        private static DateTime ConvertToDateTime(long timestamp) =>
            timestamp == 0 ? DateTime.UtcNow : new DateTime(timestamp);
    }
}