using System;
using System.Collections.Generic;

namespace Firefly.Helpers
{
    public enum WeekMask
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64
    }

    public class Chronos
    {
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static IEnumerable<DateTime> EachWeek(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(7))
                yield return day;
        }

        public static IEnumerable<DateTime> EachMonth(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddMonths(1))
                yield return day;
        }

        public static IEnumerable<DateTime> ByDaysInWeek(DateTime from, DateTime thru, int weekMask)
        {
            for (var shift = 0; shift <= 6; shift++)
            {
                var weekDay = from.AddDays(shift).Date;
                if (!IsDayInMask(weekDay, weekMask))
                {
                    continue;
                }
                for (var day = weekDay.Date; day.Date <= thru.Date; day = day.AddDays(7))
                    yield return day;
            }
        }

        public static bool IsDayInMask(DateTime weekDay, int weekMask)
        {
            var dayBase = ((int) (weekDay.Date.DayOfWeek + 6) % 7);
            var bit = (int) Math.Pow(2, dayBase);
            return ((weekMask & bit) == bit);
        }
    }
}