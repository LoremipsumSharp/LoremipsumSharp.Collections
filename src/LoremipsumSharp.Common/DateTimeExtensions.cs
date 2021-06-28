using System;

namespace LoremipsumSharp.Common
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// UnixTime转DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="isMillisecond"></param>
        /// <returns></returns>
        public static DateTime ToDateTimeByTimeStamp(this long timestamp, bool isMillisecond = false)
        {
            DateTime sTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            var resultTime = isMillisecond ? sTime.AddMilliseconds(timestamp) : sTime.AddSeconds(timestamp);
            return resultTime;
        }

        public static DateTime ToDateTimeByTimeStamp(this long timestamp, int timeZoneOffset, bool isMillisecond = false)
        {
            return timestamp.ToDateTimeByTimeStamp(isMillisecond: isMillisecond).AddHours(timeZoneOffset);
        }

        /// <summary>
        /// DateTime转UnixTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimeStampByDateTimeNull(this DateTime? dateTime, UnixTimeType type = UnixTimeType.Seconds)
        {
            if (!dateTime.HasValue) return 0;

            DateTime sTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            var ts = dateTime.Value - sTime;
            return type == UnixTimeType.Milliseconds ? (long)ts.TotalMilliseconds : (long)ts.TotalSeconds;
        }

        /// <summary>
        /// DateTime转UnixTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimeStampByDateTime(this DateTime dateTime, UnixTimeType type = UnixTimeType.Seconds)
        {
            if (dateTime == default) return 0;

            DateTime sTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            var ts = dateTime - sTime;
            return type == UnixTimeType.Milliseconds ? (long)ts.TotalMilliseconds : (long)ts.TotalSeconds;
        }

        public static bool IsCurrentMonth(this DateTime dateTime)
        {
            var today = DateTime.Today;

            var monthStart = dateTime.BeginOfMonth();
            var monthEnd = dateTime.EndOfMonth();

            return (dateTime >= monthStart && dateTime <= monthEnd);
        }

        public static DateTime BeginOfDay(this DateTime dateTime)
        {
            if (dateTime == default) return default;
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }
        public static DateTime EndOfDay(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
        }

        public static DateTime BeginOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.BeginOfMonth().AddMonths(1).AddMilliseconds(-1);
        }

    }

    /// <summary>
    /// UnixTimeType
    /// </summary>
    public enum UnixTimeType
    {
        Milliseconds = 0,
        Seconds = 1
    }
}