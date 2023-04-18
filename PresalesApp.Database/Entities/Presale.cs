using Microsoft.EntityFrameworkCore;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities
{
    public class Presale : Entity
    {
        public string Name { get; set; } = string.Empty;

        public Department Department { get; set; } = Department.None;

        public Position Position { get; set; } = Position.None;

        public bool IsActive { get; set; } = false;

        public virtual List<Project>? Projects { get; set; }

        public virtual List<Invoice>? Invoices { get; set; }

        public Presale(string name) => this.Name = name;

        internal override bool TryUpdateIfExist(ReadWriteContext dbContext)
        {
            var presale_in_db = dbContext.Presales.Where(p => p.Name == this.Name).SingleOrDefault();
            if (presale_in_db != null)
            {
                presale_in_db.Department = this.Department;
                presale_in_db.Position = this.Position;
                presale_in_db.IsActive = this.IsActive;
                presale_in_db.ToLog(false);
            }
            return presale_in_db != null;
        }

        internal override Presale GetOrAddIfNotExist(ReadWriteContext dbContext)
        {
            var presale_in_db = dbContext.Presales.Where(p => p.Name == this.Name).SingleOrDefault();
            if (presale_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }

            return presale_in_db;
        }

        public override string ToString() => $"{{\"Имя\":\"{this.Name}\"," +
            $"\"Направление\":\"{this.Department}\"," +
            $"\"Должность\":\"{this.Position}\"," +
            $"\"Действующий\":\"{this.IsActive}\"," +
            $"\"Проекты\":\"{this.Projects?.Count}\"," +
            $"\"Счета\":\"{this.Invoices?.Count}\"}}";

        public int CountProjectsAssigned() => this.Projects?.Count ?? 0;

        public int CountProjectsAssigned(DateTime from) => this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Count() ?? 0;

        public int CountProjectsAssigned(DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.ApprovalByTechDirectorAt <= to)?
            .Count() ?? 0;

        public IEnumerable<Project>? ProjectsOverdue() => this.Projects?.Where(p => p.IsOverdue());

        public IEnumerable<Project>? ProjectsOverdue(DateTime from) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.IsOverdue());

        public IEnumerable<Project>? ProjectsOverdue(DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.ApprovalByTechDirectorAt <= to)?
            .Where(p => p.IsOverdue());

        public int CountProjectsAbandoned(DateTime from, int since) =>
            this.Projects?.Where(p => p.Status == ProjectStatus.WorkInProgress)?
            .Where(p => p.PresaleActions != null && p.PresaleActions.Any())?
            .Where(p => p.PresaleActions?.Max(a => a.Date) <
                (from == DateTime.MinValue ? from : from.AddDays(-since)))?
            .Count() ?? 0;

        public int CountProjectsInWork(DateTime from, DateTime to) => this.Projects?.Where(p => p.PresaleActions != null &&
                p.PresaleActions.Any(a => a.Date >= from && a.Date <= to))?
            .Count() ?? 0;

        public IEnumerable<Project>? ProjectsAbandoned(TimeSpan within) =>
            this.Projects?.Where(p => p.Status == ProjectStatus.WorkInProgress)
            .Where(p => p.PresaleActions != null &&
                p.PresaleActions.Max(a => a.Date) < DateTime.Now.Add(-within));

        public int CountProjectsByStatus(ProjectStatus status) => this.Projects?.Where(p => p.Status == status)?
            .Count() ?? 0;

        public int ClosedByStatus(ProjectStatus status, DateTime from) => this.Projects?.Where(p => p.ClosedAt >= from)?
            .Where(p => p.Status == status)?
            .Count() ?? 0;

        public int ClosedByStatus(ProjectStatus status, DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ClosedAt >= from)?
            .Where(p => p.ClosedAt <= to)?
            .Where(p => p.Status == status)?
            .Count() ?? 0;

        public decimal SumPotential() => this.Projects?.Sum(p => p.PotentialAmount) ?? 0;

        public decimal SumPotential(DateTime from) => this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Sum(p => p.PotentialAmount) ?? 0;

        public decimal SumPotential(DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.ApprovalByTechDirectorAt <= to)?
            .Sum(p => p.PotentialAmount) ?? 0;

        public decimal SumPotentialByStatus(ProjectStatus status) => this.Projects?.Where(p => p.Status == status)?
            .Sum(p => p.PotentialAmount) ?? 0;

        public decimal SumPotentialByStatus(ProjectStatus status, DateTime from) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.Status == status)?
            .Sum(p => p.PotentialAmount) ?? 0;

        public decimal SumPotentialByStatus(ProjectStatus status, DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.ApprovalByTechDirectorAt <= to)?
            .Where(p => p.Status == status)
            .Sum(p => p.PotentialAmount) ?? 0;

        public double AverageRank() => this.Projects?.DefaultIfEmpty().Average(p => p?.Rank() ?? 0) ?? 0;

        public double AverageRankByStatus(ProjectStatus status) => this.Projects?.Where(p => p.Status == status)?
            .DefaultIfEmpty().Average(p => p?.Rank() ?? 0) ?? 0;

        public decimal AveragePotentialByStatus(ProjectStatus status) => this.Projects?.Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p?.PotentialAmount ?? 0) ?? 0;

        public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p?.PotentialAmount ?? 0) ?? 0;

        public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from, DateTime to) =>
            this.Projects?.Where(p => p.ApprovalByTechDirectorAt >= from)?
            .Where(p => p.ApprovalByTechDirectorAt <= to)?
            .Where(p => p.Status == status)?
            .DefaultIfEmpty()
            .Average(p => p?.PotentialAmount ?? 0) ?? 0;

        public decimal SumProfit() => SumProfit(out _);

        public decimal SumProfit(DateTime from) => SumProfit(from, out _);

        public decimal SumProfit(DateTime from, DateTime to) => SumProfit(from, to, out _);

        public decimal SumProfit(out HashSet<Invoice>? invoices) =>
            SumProfit(DateTime.MinValue, DateTime.MaxValue, out invoices);

        public decimal SumProfit(DateTime from, out HashSet<Invoice>? invoices) =>
            SumProfit(from, DateTime.MaxValue, out invoices);

        public decimal SumProfit(DateTime from, DateTime to, out HashSet<Invoice>? invoices)
        {
            invoices = this.Invoices?.Where(i => i.LastShipmentAt >= from && i.LastShipmentAt <= to &&
                i.LastPayAt != DateTime.MinValue && i.LastPayAt <= to ||
                i.LastPayAt >= from && i.LastPayAt <= to &&
                i.LastShipmentAt != DateTime.MinValue && i.LastShipmentAt <= to)?
                .ToHashSet();

            return invoices?.Sum(i => i.GetProfit(from, to)) ?? 0;
        }

        public TimeSpan AverageTimeToWin() => AvgTTW(this.Projects);

        public TimeSpan AverageTimeToWin(DateTime from) => AvgTTW(this.Projects?.Where(p => p.ClosedAt >= from));

        public TimeSpan AverageTimeToReaction() =>
            AvgTTR(this.Projects?.Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue));

        public TimeSpan AverageTimeToReaction(DateTime from) =>
            AvgTTR(this.Projects?.Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue &&
                p.PresaleStartAt >= from));

        public TimeSpan AverageTimeToReaction(DateTime from, DateTime to) =>
            AvgTTR(this.Projects?.Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue &&
                p.PresaleStartAt >= from && p.PresaleStartAt <= to));

        public TimeSpan SumTimeSpend() => SumTS(this.Projects, DateTime.MinValue, DateTime.MaxValue);

        public TimeSpan SumTimeSpend(DateTime from) => SumTS(this.Projects, from, DateTime.MaxValue);

        public TimeSpan SumTimeSpend(DateTime from, DateTime to) => SumTS(this.Projects, from, to);

        public TimeSpan AverageTimeSpend() => AvgTS(this.Projects, DateTime.MinValue, DateTime.MaxValue);

        public TimeSpan AverageTimeSpend(DateTime from) => AvgTS(this.Projects, from, DateTime.MaxValue);

        public TimeSpan AverageTimeSpend(DateTime from, DateTime to) => AvgTS(this.Projects, from, to);

        private static TimeSpan AvgTTW(IEnumerable<Project>? projects)
        {
            var days = projects?.Where(p => p.Status == ProjectStatus.Won)?
                .Where(p => p.ApprovalByTechDirectorAt > DateTime.MinValue)?
                .Where(p => p.ClosedAt > DateTime.MinValue)?
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : (p.ClosedAt - p.ApprovalByTechDirectorAt).TotalDays);

            return days == null ? TimeSpan.Zero : TimeSpan.FromDays((double)days);
        }

        private static TimeSpan AvgTTR(IEnumerable<Project>? projects)
        {
            var minutes = projects?.Where(p => p.PresaleStartAt != DateTime.MinValue)?
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : WorkTimeCalculator.CalculateWorkingMinutes(
                    p.ApprovalByTechDirectorAt, SelectTiming(p.PresaleActions?.FirstOrDefault(a => a.Number == 1)?.Date
                        .AddMinutes(p.PresaleActions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0), p.PresaleStartAt)
                    ));

            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((double)minutes);
        }

        private static DateTime SelectTiming(DateTime? actionDate, DateTime presaleStartAt) =>
            actionDate != null && actionDate != DateTime.MinValue &&
                actionDate < presaleStartAt ? (DateTime)actionDate : presaleStartAt;

        private static TimeSpan SumTS(IEnumerable<Project>? projects, DateTime from, DateTime to)
        {
            var minutes = projects?
                .Sum(p => p.PresaleActions?.Where(a => a.Date >= from)?
                    .Where(a => a.Date <= to)?
                    .Sum(a => a.TimeSpend));

            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
        }

        private static TimeSpan AvgTS(IEnumerable<Project>? projects, DateTime from, DateTime to)
        {
            var minutes = projects?.Where(p => p.PresaleActions != null
                    && p.PresaleActions.Any(a => a.Date >= from)
                    && p.PresaleActions.Any(a => a.Date <= to))?
                .DefaultIfEmpty()
                .Average(p => p?.PresaleActions?.Where(a => a.Date >= from)?
                    .Where(a => a.Date <= to)?
                    .Sum(a => a.TimeSpend) ?? 0);

            return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
        }
    }
}
