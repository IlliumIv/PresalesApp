using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using PresalesMonitor;
using PresalesMonitor.Shared;
using Entities;
using Microsoft.EntityFrameworkCore;
using Entities.Enums;
using PresalesMonitor.Shared.CustomTypes;

namespace PresalesMonitor.Server.Services
{
    public class PresalesMonitorService : Presales.PresalesBase
    {
        public override Task<Data> GetStatistics(Empty request, ServerCallContext context)
        {
            return Task.FromResult(GetStatistics());
        }

        private static Data GetStatistics()
        {
            var reply = new Data();

            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var prevMonth = thisMonth.AddMonths(-1);

            using var db = new DbController.Context();
            var presales = db.Presales.Include(p => p.Projects).ToList();
            var actions = db.Actions.ToList();
            var projects = db.Projects.ToList();
            var invoices = db.Invoices.Include(i => i.Presale).ToList();

            decimal maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                .Sum(p => p.PotentialAmount)) ?? 0;
            int maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                .Count()) ?? 0;

            foreach (var presale in presales)
            {
                if (presale.Position == Position.None
                 || presale.Position == Position.Director) continue;

                var isRuEngineer = presale.Department == Department.Russian
                                && presale.Position == Position.Engineer;

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
                        Assign = presale.CountProjectsAssigned(thisMonth),
                        #endregion
                        #region Выиграно в этом месяце
                        Won = presale.ClosedByStatus(ProjectStatus.Won, thisMonth),
                        #endregion
                        #region Проиграно в этом месяце
                        Loss = presale.ClosedByStatus(ProjectStatus.Loss, thisMonth),
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
            reply.Statistics = new Statistic()
            {
                #region В работе
                InWork = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsByStatus(ProjectStatus.WorkInProgress)),
                #endregion
                #region Назначено в этом месяце
                Assign = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAssigned(thisMonth)),
                #endregion
                #region Выиграно в этом месяце
                Won = presales.Where(p => p.Position == Position.Engineer
                                       || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Won, thisMonth)),
                #endregion
                #region Проиграно в этом месяце
                Loss = presales.Where(p => p.Position == Position.Engineer
                                        || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, thisMonth)),
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
            foreach (var proj in projects?
                    .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                    .Where(p => p.IsOverdue()))
                reply.Escalations.Add(proj.Translate());
            #endregion
            #region Забытые проекты
            foreach (var proj in projects?
                    .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                    .Where(p => p.IsForgotten()))
                reply.Forgotten.Add(proj.Translate());
            #endregion
            #region Новые проекты
            foreach (var proj in projects?
                    .Where(p => p.ApprovalBySalesDirectorAt != DateTime.MinValue)
                    .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue)?
                    .DefaultIfEmpty())
                reply.New.Add(proj.Translate());
            #endregion
            #region Среднее время реакции руководителя
            reply.AvgDirectorTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(projects?
                .Where(p => p.ApprovalBySalesDirectorAt.ToLocalTime() > thisMonth)?
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
            PresaleName = project.Presale?.Name ?? ""
        };
    }
}
