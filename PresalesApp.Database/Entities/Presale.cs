using Microsoft.EntityFrameworkCore;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities;

public class Presale(string name) : Entity
{
    public string Name { get; set; } = name;

    public Department Department { get; set; } = Department.None;

    public Position Position { get; set; } = Position.None;

    public bool IsActive { get; set; } = false;

    public virtual List<Project>? Projects { get; set; }

    public virtual List<Invoice>? Invoices { get; set; }

    internal override bool TryUpdateIfExist(ControllerContext dbContext)
    {
        var presale_in_db = dbContext.Presales.Where(p => p.Name == Name).SingleOrDefault();
        if (presale_in_db != null)
        {
            presale_in_db.Department = Department;
            presale_in_db.Position = Position;
            presale_in_db.IsActive = IsActive;
            presale_in_db.ToLog(false);
        }

        return presale_in_db != null;
    }

    internal override Presale GetOrAddIfNotExist(ControllerContext dbContext)
    {
        var presale_in_db = dbContext.Presales.Where(p => p.Name == Name).SingleOrDefault();
        if (presale_in_db == null)
        {
            dbContext.Add(this);
            ToLog(true);
            return this;
        }

        return presale_in_db;
    }

    public override string ToString() => $"{{\"Имя\":\"{Name}\"," +
        $"\"Направление\":\"{Department}\"," +
        $"\"Должность\":\"{Position}\"," +
        $"\"Действующий\":\"{IsActive}\"," +
        $"\"Проекты\":\"{Projects?.Count}\"," +
        $"\"Счета\":\"{Invoices?.Count}\"}}";

    public int CountProjectsAssigned() => Projects?.Count ?? 0;

    public int CountProjectsAssigned(DateTime from) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Count() ?? 0;

    public int CountProjectsAssigned(DateTime from, DateTime to) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.ApprovalByTechDirectorAt <= to)?
        .Count() ?? 0;

    public IEnumerable<Project>? ProjectsOverdue() => Projects?.Where(p => p.IsOverdue());

    public IEnumerable<Project>? ProjectsOverdue(DateTime from) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.IsOverdue());

    public IEnumerable<Project>? ProjectsOverdue(DateTime from, DateTime to) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.ApprovalByTechDirectorAt <= to)?
        .Where(p => p.IsOverdue());

    public int CountProjectsAbandoned(DateTime from, int since) => Projects?
        .Where(p => p.Status == ProjectStatus.WorkInProgress)?
        .Where(p => p.PresaleActions != null && p.PresaleActions.Count != 0)?
        .Where(p => p.PresaleActions?.Max(a => a.Date)
            < (from == DateTime.MinValue ? from : from.AddDays(-since)))?
        .Count() ?? 0;

    public int CountProjectsInWork(DateTime from, DateTime to) => Projects?
        .Where(p => p.PresaleActions != null && p.PresaleActions.Any(a => a.Date >= from && a.Date <= to))?
        .Count() ?? 0;

    public IEnumerable<Project>? ProjectsAbandoned(TimeSpan within) => Projects?
        .Where(p => p.Status == ProjectStatus.WorkInProgress)
        .Where(p => p.PresaleActions != null && p.PresaleActions.Max(a => a.Date) < DateTime.Now.Add(-within));

    public int CountProjectsByStatus(ProjectStatus status) => Projects?
        .Where(p => p.Status == status)?
        .Count() ?? 0;

    public int ClosedByStatus(ProjectStatus status, DateTime from) => Projects?
        .Where(p => p.ClosedAt >= from)?
        .Where(p => p.Status == status)?
        .Count() ?? 0;

    public int ClosedByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
        .Where(p => p.ClosedAt >= from)?
        .Where(p => p.ClosedAt <= to)?
        .Where(p => p.Status == status)?
        .Count() ?? 0;

    public decimal SumPotential() => Projects?.Sum(p => p.PotentialAmount) ?? 0;

    public decimal SumPotential(DateTime from) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Sum(p => p.PotentialAmount) ?? 0;

    public decimal SumPotential(DateTime from, DateTime to) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.ApprovalByTechDirectorAt <= to)?
        .Sum(p => p.PotentialAmount) ?? 0;

    public decimal SumPotentialByStatus(ProjectStatus status) => Projects?
        .Where(p => p.Status == status)?
        .Sum(p => p.PotentialAmount) ?? 0;

    public decimal SumPotentialByStatus(ProjectStatus status, DateTime from) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.Status == status)?
        .Sum(p => p.PotentialAmount) ?? 0;

    public decimal SumPotentialByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.ApprovalByTechDirectorAt <= to)?
        .Where(p => p.Status == status)
        .Sum(p => p.PotentialAmount) ?? 0;

    public double AverageRank() => Projects?
        .DefaultIfEmpty()
        .Average(p => p?.Rank() ?? 0) ?? 0;

    public double AverageRankByStatus(ProjectStatus status) => Projects?
        .Where(p => p.Status == status)?
        .DefaultIfEmpty()
        .Average(p => p?.Rank() ?? 0) ?? 0;

    public decimal AveragePotentialByStatus(ProjectStatus status) => Projects?
        .Where(p => p.Status == status)?
        .DefaultIfEmpty()
        .Average(p => p?.PotentialAmount ?? 0) ?? 0;

    public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.Status == status)?
        .DefaultIfEmpty()
        .Average(p => p?.PotentialAmount ?? 0) ?? 0;

    public decimal AveragePotentialByStatus(ProjectStatus status, DateTime from, DateTime to) => Projects?
        .Where(p => p.ApprovalByTechDirectorAt >= from)?
        .Where(p => p.ApprovalByTechDirectorAt <= to)?
        .Where(p => p.Status == status)?
        .DefaultIfEmpty()
        .Average(p => p?.PotentialAmount ?? 0) ?? 0;

    public decimal SumProfit() => SumProfit(out _);

    public decimal SumProfit(DateTime from) => SumProfit(from, out _);

    public decimal SumProfit(DateTime from, DateTime to) => SumProfit(from, to, out _);

    public decimal SumProfit(out HashSet<Invoice>? invoices)
        => SumProfit(DateTime.MinValue, DateTime.MaxValue, out invoices);

    public decimal SumProfit(DateTime from, out HashSet<Invoice>? invoices)
        => SumProfit(from, DateTime.MaxValue, out invoices);

    public decimal SumProfit(DateTime from, DateTime to, out HashSet<Invoice>? invoices)
    {
        invoices = Invoices?
            .Where(i => (i.LastShipmentAt >= from && i.LastShipmentAt <= to && i.LastPayAt != DateTime.MinValue && i.LastPayAt <= to)
                || (i.LastPayAt >= from && i.LastPayAt <= to && i.LastShipmentAt != DateTime.MinValue && i.LastShipmentAt <= to))?
            .ToHashSet();

        return invoices?.Sum(i => i.GetProfit(from, to)) ?? 0;
    }

    public TimeSpan AverageTimeToWin() => _AverageTimeToWin(Projects);

    public TimeSpan AverageTimeToWin(DateTime from) => _AverageTimeToWin(Projects?.Where(p => p.ClosedAt >= from));

    public TimeSpan AverageTimeToReaction() => _AverageTimeToReaction(Projects?
        .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue));

    public TimeSpan AverageTimeToReaction(DateTime from) => _AverageTimeToReaction(Projects?
        .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue && p.PresaleStartAt >= from));

    public TimeSpan AverageTimeToReaction(DateTime from, DateTime to) => _AverageTimeToReaction(Projects?
        .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue && p.PresaleStartAt >= from && p.PresaleStartAt <= to));

    public TimeSpan SumTimeSpend() => _SumTimeSpend(Projects, DateTime.MinValue, DateTime.MaxValue);

    public TimeSpan SumTimeSpend(DateTime from) => _SumTimeSpend(Projects, from, DateTime.MaxValue);

    public TimeSpan SumTimeSpend(DateTime from, DateTime to) => _SumTimeSpend(Projects, from, to);

    public TimeSpan AverageTimeSpend() => _AverageTimeSpend(Projects, DateTime.MinValue, DateTime.MaxValue);

    public TimeSpan AverageTimeSpend(DateTime from) => _AverageTimeSpend(Projects, from, DateTime.MaxValue);

    public TimeSpan AverageTimeSpend(DateTime from, DateTime to) => _AverageTimeSpend(Projects, from, to);

    private static TimeSpan _AverageTimeToWin(IEnumerable<Project>? projects)
    {
        var days = projects?
            .Where(p => p.Status == ProjectStatus.Won)?
            .Where(p => p.ApprovalByTechDirectorAt > DateTime.MinValue)?
            .Where(p => p.ClosedAt > DateTime.MinValue)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : (p.ClosedAt - p.ApprovalByTechDirectorAt).TotalDays);

        return days == null ? TimeSpan.Zero : TimeSpan.FromDays((double)days);
    }

    private static TimeSpan _AverageTimeToReaction(IEnumerable<Project>? projects)
    {
        var minutes = projects?
            .Where(p => p.PresaleStartAt != DateTime.MinValue)?
            .DefaultIfEmpty()
            .Average(p => p is null ? 0 : BusinessTimeCalculator.CalculateBusinessMinutesLOCAL(
                p.ApprovalByTechDirectorAt, _SelectTiming(p.PresaleActions?.FirstOrDefault(a => a.Number == 1)?.Date
                    .AddMinutes(p.PresaleActions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0), p.PresaleStartAt)));

        return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((double)minutes);
    }

    private static DateTime _SelectTiming(DateTime? actionDate, DateTime presaleStartAt)
        => actionDate != null
            && actionDate != DateTime.MinValue
            && actionDate < presaleStartAt
                ? (DateTime)actionDate
                : presaleStartAt;

    private static int _GetTimeSpend(PresaleAction a) => a.TimeSpend;

    private static TimeSpan _SumTimeSpend(IEnumerable<Project>? projects, DateTime from, DateTime to)
    {
        var minutes = projects?
            .Sum(p => p.PresaleActions?
                       .Where(a => a.Date >= from)?
                       .Where(a => a.Date <= to)?
                       .Sum(_GetTimeSpend));

        return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
    }

    private static TimeSpan _AverageTimeSpend(IEnumerable<Project>? projects, DateTime from, DateTime to)
    {
        var minutes = projects?
            .Where(p => p.PresaleActions != null
                && p.PresaleActions.Any(a => a.Date >= from)
                && p.PresaleActions.Any(a => a.Date <= to))?
            .DefaultIfEmpty()
            .Average(p => p?.PresaleActions?
                            .Where(a => a.Date >= from)?
                            .Where(a => a.Date <= to)?
                            .Sum(_GetTimeSpend) ?? 0);

        return minutes == null ? TimeSpan.Zero : TimeSpan.FromMinutes((int)minutes);
    }
}
