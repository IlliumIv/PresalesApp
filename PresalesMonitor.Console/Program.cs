using PresalesMonitor.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Configuration;
using PresalesMonitor.Entities;

namespace PresalesMonitor.Console
{
    public class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
            };

            // using var db = new DbController.Context();
            // db.Delete();
            // db.Create();
            /*
            if (!Settings.ConfigurationFileIsExists()) Settings.CreateConfigurationFile();

            while (true)
            {
                Parser.Run();
                Settings.TryGetSection<Settings.Application>(out ConfigurationSection r);
                var prevUpdate = ((Settings.Application)r).PreviosUpdate;
                ShowData(prevUpdate);
                int delay = 600000;
                if (DateTime.Now - prevUpdate > TimeSpan.FromMilliseconds(delay)) delay = 0;
                Task.Delay(delay).Wait();
            };
            //*/

            var some = TimeZoneInfo.GetSystemTimeZones();
            foreach (var s in some)
                System.Console.WriteLine(s.Id);

        }
        public static bool IsOverdue(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            var ApprovalByTechDirectorAt = new DateTime(2022, 12, 07, 18, 03, 48).ToUniversalTime();
            var Actions = new List<PresaleAction> { new PresaleAction { 
                Number = 1,
                Date = new DateTime(2022, 12, 08, 09, 09, 16).ToUniversalTime(),
                TimeSpend = 40
            }};
            var PresaleStartAt = new DateTime(2022, 12, 08, 09, 06, 20).ToUniversalTime();

            System.Console.WriteLine($"{ApprovalByTechDirectorAt} -- {Actions.First().Date.AddMinutes(Actions.First().TimeSpend)}");

            decimal PotentialAmount = 480000;

            return CalculateWorkingMinutes
                (
                    ApprovalByTechDirectorAt,
                    new List<DateTime?>() {
                        (Actions?.FirstOrDefault(a => a.Number == 1)?.Date ?? DateTime.Now)
                            .AddMinutes(-Actions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
                        PresaleStartAt }.Min(dt => dt)
                ) > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }


        public static double? CalculateWorkingMinutes(DateTime? start, DateTime? end)
        {
            if (start == null || end == null) return null;
            var s = (DateTime)start;
            var e = (DateTime)end;

            if (s > e) return 0;
            s = s.TimeOfDay == new DateTime().TimeOfDay ? s : s.ToLocalTime();
            e = e.TimeOfDay == new DateTime().TimeOfDay ? e : e.ToLocalTime();

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

        public static void ShowData(DateTime prevUpdate)
        {
            System.Console.WriteLine($"\nДоступны данные за период: 20.09.2022 00:00:00 - {prevUpdate:dd.MM.yyyy HH:mm:ss}");
            System.Console.WriteLine($"Последнее обновление: {prevUpdate:dd.MM.yyyy HH:mm:ss.fff}");
        }
    }
}