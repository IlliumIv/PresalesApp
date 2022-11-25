using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using PresalesMonitor;
using PresalesMonitor.Shared;
using Entities;
using Microsoft.EntityFrameworkCore;
using Entities.Enums;
using PresalesMonitor.Shared.CustomTypes;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;

namespace PresalesMonitor.Server.Services
{
    public class PresalesMonitorService : Presales.PresalesBase
    {
        public override Task<KpiResponse> GetKpi(KpiRequest request, ServerCallContext context)
        {
            using var db = new DbController.Context();
            db.ChangeTracker.LazyLoadingEnabled = false;
            var presale = db.Presales.Single(p => p.Name == request.PresaleName);

            if (presale == null)
                return Task.FromResult(new KpiResponse { Error = new Error{ Message = "Пресейл не найден в базе данных."}});

            presale = db.Presales
                .Include(p => p.Invoices
                    .Where(i => i.Date >= request.Period.From.ToDateTime()
                             || i.LastPayAt >= request.Period.From.ToDateTime()
                             || i.LastShipmentAt >= request.Period.From.ToDateTime()))
                    .ThenInclude(i => i.Project).ThenInclude(p => p.Actions)
                .Include(p => p.Invoices
                    .Where(i => i.Date >= request.Period.From.ToDateTime()
                             || i.LastPayAt >= request.Period.From.ToDateTime()
                             || i.LastShipmentAt >= request.Period.From.ToDateTime()))
                    .ThenInclude(i => i.Project).ThenInclude(p => p.MainProject)
                .Single(p => p.Name == request.PresaleName);

            HashSet<Entities.Project> projects = new();
            RecurseLoad(presale.Projects, db, ref projects);

            HashSet<Entities.Invoice>? invoices = new();
            presale.SumProfit(request.Period.From.ToDateTime(), request.Period.To.ToDateTime(), ref invoices);

            if (invoices == null)
                return Task.FromResult(new KpiResponse());

            var reply = new Kpi();

            foreach(var invoice in invoices)
            {
                HashSet<PresaleAction> actionsIgnored = new(), actionsTallied = new();
                HashSet<Entities.Project> projectsIgnored = new(), projectsFound = new();

                var percent = invoice.Project?.Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound) switch
                {
                    int n when n >= 1 && n <= 3 => .004,
                    int n when n >= 4 && n <= 6 => .007,
                    int n when n >= 7 => .01,
                    _ => 0,
                };

                // Console.WriteLine("------------------------------------");
                // Console.WriteLine($"{invoice.Number} -- {invoice.Amount} -- {percent}");

                var invoiceReply = new Shared.Invoice
                {
                    Counterpart = invoice.Counterpart,
                    Nubmer = invoice.Number,
                    Date = Timestamp.FromDateTime(invoice.Date),
                    Amount = DecimalValue.FromDecimal(invoice.Amount),
                    Cost = DecimalValue.FromDecimal(invoice.Amount - invoice.Profit),
                    SalesAmount = DecimalValue.FromDecimal(invoice.Profit),
                    Percent = percent,
                    Profit = DecimalValue.FromDecimal(invoice.Profit * (decimal)percent),
                };

                foreach (var inv in projectsIgnored)
                    invoiceReply.ProjectsIgnored.Add(inv.Translate());

                foreach (var inv in projectsFound)
                    invoiceReply.ProjectsFound.Add(inv.Translate());

                foreach (var action in actionsIgnored)
                    invoiceReply.ActionsIgnored.Add(action.Translate());

                foreach (var action in actionsTallied)
                    invoiceReply.ActionsTallied.Add(action.Translate());

                reply.Invoices.Add(invoiceReply);
            }

            return Task.FromResult(new KpiResponse { Kpi = reply });
        }
        private void RecurseLoad(IEnumerable<Entities.Project?> projects, DbController.Context db, ref HashSet<Entities.Project> projectsViewed)
        {
            if (projects == null || projects.All(p => p?.MainProject == null)) return;
            var mainProjects = projects.Select(p => p?.MainProject);
            if (mainProjects == null || mainProjects?.Count() == 0) return;

            foreach (var project in mainProjects)
            {
                if (project == null) continue;
                if (projectsViewed.Contains(project)) continue;
                else projectsViewed.Add(project);

                var prjs = db.Projects.Where(p => p == project)
                    .Include(p => p.Actions)
                    .Include(p => p.MainProject);

                RecurseLoad(prjs, db, ref projectsViewed);
            }
        }
        public override Task<Names> GetNames(Empty request, ServerCallContext context)
        {
            using var db = new DbController.Context();
            var presalesNames = db.Presales
                .Where(p => p.Department != Department.None)
                .Where(p => p.Position != Position.None && p.Position != Position.Director)
                .Select(column => column.Name).ToList();

            var reply = new Names();
            foreach (var name in presalesNames) reply.Names_.Add(name);

            return Task.FromResult(reply);
        }
        public override Task<Overview> GetOverview(Period request, ServerCallContext context)
        {
            var from = request?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.To?.ToDateTime() ?? DateTime.MaxValue;

            return Task.FromResult(GetOverview(from, to));
        }
        private static Overview GetOverview(DateTime from, DateTime to)
        {
            var reply = new Overview();

            var thisMonth = from;
            var prevMonth = thisMonth.AddMonths(-1);

            using var db = new DbController.Context();
            var presales = db.Presales.Include(p => p.Projects).ToList();
            var actions = db.Actions.ToList();
            var projects = db.Projects.ToList();
            var invoices = db.Invoices.Include(i => i.Presale).ToList();

            decimal maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt > thisMonth)?
                .Sum(p => p.PotentialAmount)) ?? 0;
            int maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt > thisMonth)?
                .Count()) ?? 0;

            int won, assign;

            foreach (var presale in presales)
            {
                if (presale.Position == Position.None
                 || presale.Position == Position.Director) continue;

                var isRuEngineer = presale.Department == Department.Russian
                                && presale.Position == Position.Engineer;
                won = presale.ClosedByStatus(ProjectStatus.Won, thisMonth);
                assign = presale.CountProjectsAssigned(thisMonth);

                #region Метрики пресейла
                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = new Statistic()
                    {
                        #region В работе
                        InWork = presale.CountProjectsByStatus(ProjectStatus.WorkInProgress),
                        #endregion
                        #region Назначено в этом месяце
                        Assign = assign,
                        #endregion
                        #region Выиграно в этом месяце
                        Won = won,
                        #endregion
                        #region Проиграно в этом месяце
                        Loss = presale.ClosedByStatus(ProjectStatus.Loss, thisMonth),
                        #endregion
                        #region Конверсия
                        Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                        #endregion
                        #region Среднее время жизни проекта до выигрыша
                        AvgTimeToWin = Duration.FromTimeSpan(presale.AverageTimeToWin(prevMonth)),
                        #endregion
                        #region Среднее время реакции
                        AvgTimeToReaction = Duration.FromTimeSpan(presale.AverageTimeToReaction(thisMonth)),
                        #endregion
                        #region Суммарное потраченное время на проекты в этом месяце
                        SumSpend = Duration.FromTimeSpan(presale.SumTimeSpend(thisMonth)),
                        #endregion
                        #region Cреднее время потраченное на проект в этом месяце
                        AvgSpend = Duration.FromTimeSpan(presale.AverageTimeSpend(thisMonth)),
                        #endregion
                        #region Средний ранг проектов
                        AvgRank = presale.AverageRang(),
                        #endregion
                        #region Количество "брошенных" проектов
                        Abnd = presale.CountProjectsAbandoned(TimeSpan.FromDays(30)),
                        #endregion
                        #region Чистые за месяц
                        Profit = DecimalValue.FromDecimal(presale.SumProfit(thisMonth)),
                        #endregion
                    },
                    #region Недостаток проектов
                    DeficitProjects = isRuEngineer ? maxCount - presale.CountProjectsAssigned(thisMonth) : 0,
                    #endregion
                    #region Недостаток потенциала
                    DeficitPotential = isRuEngineer ? maxPotential - presale.SumPotential(thisMonth) : 0,
                    #endregion
                });
                #endregion
            }
            #region Метрики службы

            won = presales.Where(p => p.Position == Position.Engineer
                                       || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Won, thisMonth));
            assign = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAssigned(thisMonth));

            reply.Statistics = new Statistic()
            {
                #region В работе
                InWork = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsByStatus(ProjectStatus.WorkInProgress)),
                #endregion
                #region Назначено в этом месяце
                Assign = assign,
                #endregion
                #region Выиграно в этом месяце
                Won = won,
                #endregion
                #region Проиграно в этом месяце
                Loss = presales.Where(p => p.Position == Position.Engineer
                                        || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, thisMonth)),
                #endregion
                #region Конверсия
                Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                #endregion
                #region Среднее время жизни проекта до выигрыша
                AvgTimeToWin = Duration.FromTimeSpan(TimeSpan.FromDays(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .Average(p => p.AverageTimeToWin(thisMonth).TotalDays) ?? 0)),
                #endregion
                #region Среднее время реакции
                AvgTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .Average(p => p.AverageTimeToReaction(thisMonth).TotalMinutes) ?? 0)),
                #endregion
                #region Суммарное потраченное время на проекты в этом месяце
                SumSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                  || p.Position == Position.Account)
                        .Sum(p => p.SumTimeSpend(thisMonth).TotalMinutes) ?? 0)),
                #endregion
                #region Cреднее время потраченное на проект в этом месяце
                AvgSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                     || p.Position == Position.Account)
                        .Sum(p => p.AverageTimeSpend(thisMonth).TotalMinutes) ?? 0)),
                #endregion
                #region Средний ранг проектов
                AvgRank = presales?.Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .Average(p => p.AverageRang()) ?? 0,
                #endregion
                #region Количество "брошенных" проектов
                Abnd = presales?.Where(p => p.Position == Position.Engineer
                                             || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAbandoned(TimeSpan.FromDays(30))) ?? 0,
                #endregion
                #region Чистые за месяц
                Profit = presales?.Where(p => p.Department != Department.None)
                    .Sum(p => p.SumProfit(thisMonth)) ?? 0,
                #endregion
            };
            #region Просроченные проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt > thisMonth)
                    .Where(p => p.IsOverdue()))
                reply.Escalations.Add(proj.Translate());
            #endregion
            #region Забытые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt > thisMonth)
                    .Where(p => p.IsForgotten()))
                reply.Forgotten.Add(proj.Translate());
            #endregion
            #region Новые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalBySalesDirectorAt >= thisMonth)
                    .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue))
                reply.New.Add(proj.Translate());
            #endregion
            #region Среднее время реакции руководителя
            reply.AvgDirectorTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(projects?
                .Where(p => p.ApprovalBySalesDirectorAt > thisMonth)?
                .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue)?
                .Average(p => p.TimeToDirectorReaction().TotalMinutes) ?? 0 ));
            #endregion
            #endregion

            return reply;
        }
    }

    public static class ProjectExtensions
    {
        public static Shared.Project Translate(this Entities.Project project) => new()
        {
            Number = project.Number,
            Name = project.Name ?? "",
            ApprovalByTechDirectorAt = Timestamp.FromDateTime(
                project.ApprovalByTechDirectorAt == DateTime.MinValue ? DateTime.MinValue.ToUniversalTime() : project.ApprovalByTechDirectorAt),
            ApprovalBySalesDirectorAt = Timestamp.FromDateTime(
                project.ApprovalBySalesDirectorAt == DateTime.MinValue ? DateTime.MinValue.ToUniversalTime() : project.ApprovalBySalesDirectorAt),
            PresaleStartAt = Timestamp.FromDateTime(
                project.PresaleStartAt == DateTime.MinValue ? DateTime.MinValue.ToUniversalTime() : project.PresaleStartAt),
            PresaleName = project.Presale?.Name ?? "",
            Status = (int)project.Status
        };
        public static Shared.Action Translate(this PresaleAction action) => new()
        {
            ProjectNumber = action.Project?.Number ?? "",
            Number = action.Number,
            Date = Timestamp.FromDateTime(
                action.Date == DateTime.MinValue ? DateTime.MinValue.ToUniversalTime() : action.Date),
            Type = (int)action.Type,
            Timespend = Duration.FromTimeSpan(TimeSpan.FromMinutes(action.TimeSpend)),
            Description = action.Description
        };
    }
}
