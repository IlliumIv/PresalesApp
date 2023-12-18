using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Shared;
using ActionType = PresalesApp.Database.Enums.ActionType;
using Department = PresalesApp.Database.Enums.Department;
using Enum = System.Enum;
using FunnelStage = PresalesApp.Database.Enums.FunnelStage;
using Invoice = PresalesApp.Database.Entities.Invoice;
using Position = PresalesApp.Database.Enums.Position;
using Presale = PresalesApp.Database.Entities.Presale;
using Project = PresalesApp.Database.Entities.Project;
using ProjectStatus = PresalesApp.Database.Enums.ProjectStatus;
using Shared = PresalesApp.Web.Shared;

namespace PresalesApp.Database.Helpers;

public static class Extensions
{
    public static Statistic GetStatistic(this Presale presale, DateTime? from = null, DateTime? to = null)
    {
        if (from is null || to is null)
        {
            return new Statistic();
        }

        var _from = (DateTime)from;
        var _to = (DateTime)to;

        var won = presale.ClosedByStatus(ProjectStatus.Won, _from, _to);
        var assign = presale.CountProjectsAssigned(_from, _to);

        return new Statistic()
        {
            #region Показатели этого периода
            #region В работе
            InWork = presale.CountProjectsInWork(_from, _to),
            #endregion
            #region Назначено
            Assign = assign,
            #endregion
            #region Выиграно
            Won = won,
            #endregion
            #region Проиграно
            Loss = presale.ClosedByStatus(ProjectStatus.Loss, _from, _to),
            #endregion
            #region Конверсия
            Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
            #endregion
            #region Среднее время реакции
            AvgTimeToReaction = Duration.FromTimeSpan(presale.AverageTimeToReaction(_from, _to)),
            #endregion
            #region Суммарное потраченное время на проекты
            SumSpend = Duration.FromTimeSpan(presale.SumTimeSpend(_from, _to)),
            #endregion
            #region Cреднее время потраченное на проект
            AvgSpend = Duration.FromTimeSpan(presale.AverageTimeSpend(_from, _to)),
            #endregion
            #region Чистые
            Profit = presale.SumProfit(_from, _to),
            #endregion
            #region Потенциал
            Potential = presale.SumPotential(_from, _to),
            #endregion
            #region Счетов
            Invoices = presale.Invoices?.Count ?? 0,
            #endregion
            #region Счетов отгружено
            InvoicesShipped = presale.Invoices?.Where(i => from <= i.LastShipmentAt && i.LastShipmentAt <= to)?.Count() ?? 0,
            #endregion
            #endregion
            #region Среднее время жизни проекта до выигрыша
            AvgTimeToWin = Duration.FromTimeSpan(presale.AverageTimeToWin()),
            #endregion
            #region Средний ранг проектов
            AvgRank = presale.AverageRank(),
            #endregion
            #region Количество "брошенных" проектов
            Abnd = presale.CountProjectsAbandoned(DateTime.UtcNow, 30),
            #endregion
        };
    }

    public static Shared.Project Translate(this Project project)
    {
        var proj = new Shared.Project()
        {
            Number = project.Number,
            Name = project.Name ?? "",
            ApprovalByTechDirectorAt = Timestamp.FromDateTime(project.ApprovalByTechDirectorAt.ToUniversalTime()),
            ApprovalBySalesDirectorAt = Timestamp.FromDateTime(project.ApprovalBySalesDirectorAt.ToUniversalTime()),
            PresaleStartAt = Timestamp.FromDateTime(project.PresaleStartAt.ToUniversalTime()),
            ClosedAt = Timestamp.FromDateTime(project.ClosedAt.ToUniversalTime()),
            Presale = project.Presale?.Translate(),
            Status = project.Status.Translate(),
            FunnelStage = project.FunnelStage.Translate(),
            Potential = project.PotentialAmount
        };

        if (project.Invoices != null && project.Invoices.Count != 0)
        {
            foreach(var invoice in project.Invoices)
                proj.Invoices.Add(invoice.Translate());
        }

        if (project.PresaleActions != null && project.PresaleActions.Count != 0)
        {
            foreach(var action in project.PresaleActions.OrderBy(a => a.Number))
                proj.Actions.Add(action.Translate());
        }

        return proj;
    }

    public static Shared.Invoice Translate(this Invoice invoice) => new()
    {
        Counterpart = invoice.Counterpart,
        Number = invoice.Number,
        Date = Timestamp.FromDateTime(invoice.Date.ToUniversalTime()),
        LastPayAt = Timestamp.FromDateTime(invoice.LastPayAt.ToUniversalTime()),
        LastShipmentAt = Timestamp.FromDateTime(invoice.LastShipmentAt.ToUniversalTime()),
        Amount = invoice.Amount,
        Profit = invoice.GetProfit(),
    };

    public static Shared.Presale Translate(this Presale presale) => new()
    {
        Name = presale.Name,
        Statistics = presale.GetStatistic(),
        Department = presale.Department.Translate(),
        Position = presale.Position.Translate(),
        IsActive = presale.IsActive
    };

    public static Shared.Action Translate(this PresaleAction action) => new()
    {
        ProjectNumber = action.Project?.Number ?? "",
        Number = action.Number,
        Date = Timestamp.FromDateTime(action.Date.ToUniversalTime()),
        Type = action.Type.Translate(),
        Timespend = Duration.FromTimeSpan(TimeSpan.FromMinutes(action.TimeSpend)),
        Description = action.Description,
        SalesFunnel = action.SalesFunnel
    };

    public static Shared.Department Translate(this Department value) =>
        (Shared.Department)Enum.Parse(typeof(Shared.Department), value.ToString());

    public static Department Translate(this Shared.Department value) =>
        value == Shared.Department.Any ? Department.None : (Department)Enum.Parse(typeof(Department), value.ToString());

    public static Shared.Position Translate(this Position value) =>
        (Shared.Position)Enum.Parse(typeof(Shared.Position), value.ToString());

    public static Position Translate(this Shared.Position value) =>
        value == Shared.Position.Any ? Position.None : (Position)Enum.Parse(typeof(Position), value.ToString());

    public static Shared.FunnelStage Translate(this FunnelStage value) =>
        (Shared.FunnelStage)Enum.Parse(typeof(Shared.FunnelStage), value.ToString());

    public static FunnelStage Translate(this Shared.FunnelStage value) =>
        value == Shared.FunnelStage.Any ? FunnelStage.None : (FunnelStage)Enum.Parse(typeof(FunnelStage), value.ToString());

    public static Shared.ProjectStatus Translate(this ProjectStatus value) =>
        (Shared.ProjectStatus)Enum.Parse(typeof(Shared.ProjectStatus), value.ToString());

    public static Shared.ActionType Translate(this ActionType value) =>
        (Shared.ActionType)Enum.Parse(typeof(Shared.ActionType), value.ToString());

    public static Role Translate(this IdentityRole role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        NormalizedName = role.NormalizedName,
        ConcurrencyStamp = role.ConcurrencyStamp,
    };
}
