namespace PresalesApp.Database.Helpers
{
    public static class WorkTimeCalculator
    {
        // https://www.codeproject.com/Articles/19559/Calculating-Business-Hours
        public static double? CalculateWorkingMinutes(DateTime? start, DateTime? end)
        {
            if (start == null || end == null) return null;
            var s = (DateTime)start;
            var e = (DateTime)end;

            if (s > e) return 0;
            var timeZoneOffset = TimeSpan.FromHours(5);
            s = s.TimeOfDay == new DateTime().TimeOfDay ? s : s + timeZoneOffset;
            e = e.TimeOfDay == new DateTime().TimeOfDay ? e : e + timeZoneOffset;

            var tempStart = new DateTime(s.Year, s.Month, s.Day);
            var tempEnd = new DateTime(e.Year, e.Month, e.Day);

            var isSameDay = s.Date == e.Date;

            var iBDays = GetWorkingDays(tempStart, tempEnd);
            tempStart += new TimeSpan(s.Hour, s.Minute, s.Second);
            tempEnd += new TimeSpan(e.Hour, e.Minute, e.Second);

            var maxTime = new DateTime(s.Year, s.Month, s.Day, 18, 0, 0);
            var minTime = new DateTime(s.Year, s.Month, s.Day, 9, 0, 0);
            var firstDaySec = CorrectFirstDayTime(tempStart, maxTime, minTime);

            maxTime = new DateTime(e.Year, e.Month, e.Day, 18, 0, 0);
            minTime = new DateTime(e.Year, e.Month, e.Day, 9, 0, 0);
            var lastDaySec = CorrectLastDayTime(tempEnd, maxTime, minTime);

            var overAllSec = 0;
            if (isSameDay)
            {
                if (iBDays != 0)
                {
                    var diff = maxTime - minTime;
                    var bDaySec = diff.Days * 24 * 60 * 60 + diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
                    overAllSec = firstDaySec + lastDaySec - bDaySec;
                }
            }
            else if (iBDays > 1)
                overAllSec = (iBDays - 2) * 9 * 60 * 60 + firstDaySec + lastDaySec;

            return overAllSec / 60d;
        }

        private static int GetWorkingDays(DateTime start, DateTime end)
        {
            var diff = end - start;
            var iDays = diff.Days + 1;
            var iWeeks = iDays / 7;
            var iBDays = iWeeks * 5;
            var iRem = iDays % 7;
            while (iRem > 0)
            {
                if (start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday)
                {
                    iBDays++;
                    start += TimeSpan.FromDays(1);
                }
                iRem--;
            }
            return iBDays;
        }

        private static int CorrectFirstDayTime(DateTime start, DateTime maxTime, DateTime minTime)
        {
            if (maxTime < start) return 0;
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday) return 0;
            if (start < minTime) start = minTime;

            var diff = maxTime - start;
            return diff.Days * 24 * 60 * 60 + diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
        }

        private static int CorrectLastDayTime(DateTime end, DateTime maxTime, DateTime minTime)
        {
            if (minTime > end) return 0;
            if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday) return 0;
            if (end > maxTime) end = maxTime;

            var diff = end - minTime;
            return diff.Days * 24 * 60 * 60 + diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
        }
    }
}
