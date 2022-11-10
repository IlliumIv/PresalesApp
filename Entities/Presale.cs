using Entities.Enums;
using Entities.Helpers;
using Newtonsoft.Json;

namespace Entities
{
    public class Presale
    {
        public int PresaleId { get; set; }
        public string Name { get; set; }
        public Department Department { get; set; } = Department.None;
        public Position Position { get; set; } = Position.None;
        public virtual List<Project>? Projects { get; set; }
        public virtual List<Invoice>? Invoices { get; set; }
        public Presale(string name) => Name = name;
        public int CountProjectsAssigned() => Projects?
            .Count ?? 0;
        public int CountProjectsAssigned(DateTime from) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Count() ?? 0;
        public int CountProjectsAssigned(DateTime from, DateTime to) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.ApprovalByTechDirector.ToLocal() <= to)?
            .Count() ?? 0;
        public IEnumerable<Project>? ProjectsOverdue() => Projects?
            .Where(p => p.IsOverdue());
        public IEnumerable<Project>? ProjectsOverdue(DateTime from) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.IsOverdue());
        public IEnumerable<Project>? ProjectsOverdue(DateTime from, DateTime to) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.ApprovalByTechDirector.ToLocal() <= to)?
            .Where(p => p.IsOverdue());
        public int CountProjectsAbandoned(TimeSpan within) => Projects?
            .Where(p => p.Actions != null
                && p.Actions.Max(a => a.Date).ToLocal() < DateTime.Now.Add(-within))?
            .Count() ?? 0;
        public int CountProjectsByStatus(ProjectStatus status) => Projects?
            .Where(p => p.Status == status)?
            .Count() ?? 0;
        public int CountProjectsByStatus(ProjectStatus status, DateTime from) => Projects?
            .Where(p => p.LastStatusChanged.ToLocal() >= from)?
            .Where(p => p.Status == status)?
            .Count() ?? 0;
        public int CountProjectsByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
            .Where(p => p.LastStatusChanged.ToLocal() >= from)?
            .Where(p => p.LastStatusChanged.ToLocal() <= to)?
            .Where(p => p.Status == status)?
            .Count() ?? 0;
        public decimal SumPotential() => Projects?
            .Sum(p => p.PotentialAmount) ?? 0;
        public decimal SumPotential(DateTime from) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Sum(p => p.PotentialAmount) ?? 0;
        public decimal SumPotential(DateTime from, DateTime to) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.ApprovalByTechDirector.ToLocal() <= to)?
            .Sum(p => p.PotentialAmount) ?? 0;
        public decimal SumPotentialByStatus(ProjectStatus status) => Projects?
            .Where(p => p.Status == status)?
            .Sum(p => p.PotentialAmount) ?? 0;
        public decimal SumPotentialByStatus(ProjectStatus status, DateTime from) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.Status == status)?
            .Sum(p => p.PotentialAmount) ?? 0;
        public decimal SumPotentialByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.ApprovalByTechDirector.ToLocal() <= to)?
            .Where(p => p.Status == status)
            .Sum(p => p.PotentialAmount) ?? 0;
        public double AverageRang() => Projects?
            .Where(p => p.Name != null)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : p.Rang()) ?? 0;
        public double AverageRangByStatus(ProjectStatus status) => Projects?
            .Where(p => p.Name != null)?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : p.Rang()) ?? 0;
        public decimal AveragePotentialByStatus(ProjectStatus status) => Projects?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : p.PotentialAmount) ?? 0;
        public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : p.PotentialAmount) ?? 0;
        public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
            .Where(p => p.ApprovalByTechDirector.ToLocal() >= from)?
            .Where(p => p.ApprovalByTechDirector.ToLocal() <= to)?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : p.PotentialAmount) ?? 0;
        public decimal SumProfit(ref List<Invoice>? invoices)
        {
            invoices = Invoices?
                .Where(i => i.LastShipment != null)?
                .Where(i => i.LastPay != null)?
                .ToList();
            return invoices?.Sum(i => i.Profit) ?? 0;
        }
        public decimal SumProfit(DateTime from, ref List<Invoice>? invoices)
        {
            invoices = Invoices?
                .Where(i => (i.LastShipment.ToLocal() >= from && i.LastPay != null)
                        || (i.LastPay.ToLocal() >= from && i.LastShipment != null))?
                .ToList();
            return invoices?.Sum(i => i.Profit) ?? 0;
        }
        public decimal SumProfit(DateTime from, DateTime to, ref List<Invoice>? invoices)
        {
            invoices = Invoices?
                .Where(i => (i.LastShipment.ToLocal() >= from && i.LastShipment.ToLocal() <= to && i.LastPay != null)
                         || (i.LastPay.ToLocal() >= from && i.LastPay.ToLocal() <= to && i.LastShipment.ToLocal() <= to))?
                .ToList();
            return invoices?.Sum(i => i.Profit) ?? 0;
        }
        public TimeSpan AverageTimeToWin() => AvgTTW(Projects);
        public TimeSpan AverageTimeToWin(DateTime from) => AvgTTW(Projects?.Where(p => p.LastStatusChanged.ToLocal() >= from));
        public TimeSpan AverageTimeToReaction() => AvgTTR(Projects);
        public TimeSpan AverageTimeToReaction(DateTime from) => AvgTTR(Projects?.Where(p => p.ApprovalByTechDirector.ToLocal() >= from));
        public TimeSpan SumTimeSpend() => SumTS(Projects, DateTime.MinValue, DateTime.MaxValue);
        public TimeSpan SumTimeSpend(DateTime from) => SumTS(Projects, from, DateTime.MaxValue);
        public TimeSpan SumTimeSpend(DateTime from, DateTime to) => SumTS(Projects, from, to);
        public TimeSpan AverageTimeSpend() => AvgTS(Projects, DateTime.MinValue, DateTime.MaxValue);
        public TimeSpan AverageTimeSpend(DateTime from) => AvgTS(Projects, from, DateTime.MaxValue);
        public TimeSpan AverageTimeSpend(DateTime from, DateTime to) => AvgTS(Projects, from, to);
        private static TimeSpan AvgTTW(IEnumerable<Project>? projects)
        {
            var days = projects?
                .Where(p => p.Status == ProjectStatus.Won)?
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : (p.LastStatusChanged - p.ApprovalByTechDirector).TotalDays());
            return days == null ? TimeSpan.Zero : TimeSpan.FromDays((double)days);
        }
        private static TimeSpan AvgTTR(IEnumerable<Project>? projects)
        {
            var minutes = projects?
                .Where(p => p.PresaleStart != null)
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : CalculateWorkingMinutes
                (p.ApprovalByTechDirector.ToLocal(),
                new List<DateTime?>() {
                    (p.Actions?.FirstOrDefault(a => a.Number == 1)?.Date.ToLocal() ?? DateTime.Now)
                        .AddMinutes(p.Actions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
                    p.PresaleStart.ToLocal()
                }.Min(dt => dt)));

            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((double)minutes);
        }
        private static TimeSpan SumTS(IEnumerable<Project>? projects, DateTime from, DateTime to)
        {
            var minutes = projects?
                .Sum(p => p.Actions?
                    .Where(a => a.Date.ToLocal() >= from)?
                    .Where(a => a.Date.ToLocal() <= to)?
                    .Sum(a => a.TimeSpend));
            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
        }
        private static TimeSpan AvgTS(IEnumerable<Project>? projects, DateTime from, DateTime to)
        {
            var minutes = projects?
                .Where(p => p.Actions != null
                    && p.Actions.Any(a => a.Date.ToLocal() >= from)
                    && p.Actions.Any(a => a.Date.ToLocal() <= to))
                .Average(p => p.Actions?
                    .Where(a => a.Date.ToLocal() >= from)?
                    .Where(a => a.Date.ToLocal() <= to)?
                    .Sum(a => a.TimeSpend));
            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
        }

        // https://www.codeproject.com/Articles/19559/Calculating-Business-Hours
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
    }
}
