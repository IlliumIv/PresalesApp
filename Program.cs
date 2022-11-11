using Entities;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Configuration;

namespace PresalesStatistic
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

            // using var db = new Context();
            // db.Delete();
            // db.Create();

            while (true)
            {
                // Parser.Run();
                Settings.TryGetSection<Settings.Application>(out ConfigurationSection? r);
                if (r == null) return;
                var appSettings = (Settings.Application)r;
                ShowData(appSettings.PreviosUpdate);
                Task.Delay(600000).Wait();
            };
        }
        public static void ShowData(DateTime prevUpdate)
        {
            Console.Clear();

            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var prevMonth = thisMonth.AddMonths(-1);

            using var db = new DbController.Context();
            var presales = db.Presales.Include(p => p.Projects).ToList();
            var actions = db.Actions.ToList();
            var projects = db.Projects.ToList();
            var invoices = db.Invoices.Include(i => i.Presale).ToList();

            var maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)
                .Where(p => p.Presale?.Position == Position.Engineer)
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)
                .Sum(p => p.PotentialAmount));
            var maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)
                .Where(p => p.Presale?.Position == Position.Engineer)
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)
                .Count());

            #region Отображение заголовков таблицы
            Console.WriteLine($"{"",5}{"Пресейл",-12}|InWork|Assign|Won|Loss|Convr|Abnd|AvgTTW|AvgTTR|AvgR|{"",1}{"Spend",-6}| {"Чистые за месяц", -17}" +
                // $"|{"",12}{"Потенциал, руб.",-25}" +
                $"|Требуется распределить|");
            Console.WriteLine($"{"",17}|{"",6}|{"",7}{thisMonth,-14:MMMM}|{"",4}|{"",6}|{"",6}|{"",4}|Sum|Avg|{"",7}{"руб.",-11}" +
                // $"|{"",7}{"общий",-11}|{"",6}{"средний",-12}" +
                $"|шт.|{"",7}{"руб.",-11}|");
            #endregion

            int inWork, assing, won, loss, abandoned;
            decimal sumP, avgP, profit;
            TimeSpan avgTTW, avgTTR, timeSpend, avgTimeSpend;
            double avgRang;
            var invs = new List<Invoice>();

            foreach (var presale in presales)
            {
                if (presale.Position == Position.None
                 || presale.Position == Position.Director) continue;

                var isRuEngineer = presale.Department == Department.Russian
                                && presale.Position == Position.Engineer;
                #region Метрики пресейла
                #region В работе
                inWork = presale.CountProjectsByStatus(ProjectStatus.WorkInProgress);
                #endregion
                #region Назначено в этом месяце
                assing = presale.CountProjectsAssigned(thisMonth);
                #endregion
                #region Выиграно в этом месяце
                won = presale.ClosedByStatus(ProjectStatus.Won, thisMonth);
                #endregion
                #region Проиграно в этом месяце
                loss = presale.ClosedByStatus(ProjectStatus.Loss, thisMonth);
                #endregion
                #region Суммарный потенциал открытых проектов
                // sumP = presale.SumPotentialByStatus(ProjectStatus.WorkInProgress);
                #endregion
                #region Средний потенциал открытых проектов
                // avgP = presale.AveragePotentialByStatus(ProjectStatus.WorkInProgress);
                #endregion
                #region Недостаток проектов
                var oddsProjects = isRuEngineer ? maxCount - presale.CountProjectsAssigned(thisMonth) : 0;
                #endregion
                #region Недостаток потенциала
                var oddsPotential = isRuEngineer ? maxPotential - presale.SumPotential(thisMonth) : 0;
                #endregion
                #region Среднее время жизни проекта до выигрыша
                avgTTW = presale.AverageTimeToWin(prevMonth);
                #endregion
                #region Среднее время реакции
                avgTTR = presale.AverageTimeToReaction(thisMonth);
                #endregion
                #region Суммарное потраченное время на проекты в этом месяце
                timeSpend = presale.SumTimeSpend(thisMonth);
                #endregion
                #region Cреднее время потраченное на проект в этом месяце
                avgTimeSpend = presale.AverageTimeSpend(thisMonth);
                #endregion
                #region Средний ранг проектов
                avgRang = presale.AverageRang();
                /*
                foreach (var proj in presale.Projects)
                {
                    List<PresaleAction> ignoredActions = new();
                    List<PresaleAction> countedActions = new();
                    if (proj.Name == null) continue;
                    Console.WriteLine($"{proj.Number} -- {proj.Rank(ref ignoredActions, ref countedActions)}");
                    if (ignoredActions.Any())
                    {
                        Console.WriteLine("Ignored:");
                        foreach (var action in ignoredActions)
                            Console.WriteLine($"\t{action.Project.Number}:{action.Number}.{action.Type} ({action.TimeSpend}) \"{action.Description}\"");
                    }
                    if (countedActions.Any())
                    {
                        Console.WriteLine("Counted:");
                        foreach (var action in countedActions)
                            Console.WriteLine($"\t{action.Project.Number}:{action.Number}.{action.Type} ({action.TimeSpend}) \"{action.Description}\"");
                    }
                }
                //*/
                #endregion
                #region Количество "брошенных" проектов
                abandoned = presale.CountProjectsAbandoned(TimeSpan.FromDays(30));
                #endregion
                #region Чистые за месяц
                invs = new List<Invoice>();
                profit = presale.SumProfit(thisMonth, ref invs);
                /*
                if (invs != null)
                    foreach (var i in invs)
                    {
                        List<PresaleAction> ignoredActions = new();
                        List<PresaleAction> countedActions = new();
                        List<Project> countedUpProjects = new();
                        double p = i.Project?.Rank(ref ignoredActions, ref countedActions, ref countedUpProjects) switch
                        {
                            int n when n >= 1 && n <= 3 => .004,
                            int n when n >= 4 && n <= 6 => .007,
                            int n when n >= 7 => .01,
                            _ => 0,
                        };
                        Console.WriteLine($"{i.Counterpart};{i.Number};{i.Data.ToLocalTime()};{i.Amount};{i.Amount - i.Profit};{i.Profit};{(decimal)p*100:f1};{Math.Round(i.Profit * (decimal)p, 2)};{string.Join(", ", countedUpProjects.Select(p => p.Number))}");
                        if (ignoredActions.Any())
                        {
                            Console.WriteLine("\tIgnored actions:");
                            foreach (var action in ignoredActions)
                                Console.WriteLine($"\t\t{action.Project?.Number}:{action.Number}.{action.Type} ({action.TimeSpend}) \"{action.Description}\"");
                        }
                        if (countedUpProjects.Where(p => p.Name == null).Any())
                        {
                            Console.WriteLine("\tUnknown projects:");
                            foreach (var project in countedUpProjects.Where(p => p.Name == null))
                            Console.WriteLine($"\t\t{project.Number}");
                        }
                    }
                //*/
                #endregion
                #endregion
                #region Отображение данных по пресейлу
                Console.WriteLine(
                    $"{presale.Name,17}" +
                    // В работе
                    $"|{(inWork == 0 ? "" : inWork),6}" +
                    // Назначено
                    $"|{(assing == 0 ? "" : assing),6}" +
                    // Выиграно
                    $"|{(won == 0 ? "" : won),3}" +
                    // Проиграно
                    $"|{(loss == 0 ? "" : loss),4}" +
                    // Конверсия
                    $"|{(won == 0 || assing == 0 ? "" : won / (assing == 0 ? 0f : assing)),5:P0}" +
                    // Заброшено проектов
                    $"|{(abandoned == 0 ? "" : abandoned),4}" +
                    // Среднее время жизни
                    $"|{(avgTTW == TimeSpan.Zero ? "" : avgTTW.TotalDays),6:f0}" +
                    // Среднее время реакции
                    $"|{(avgTTR == TimeSpan.Zero ? "" : avgTTR.TotalMinutes),6:f0}" +
                    // Средний ранг
                    $"|{(avgRang == 0 ? "" : avgRang),4:f1}" +
                    // Потрачено времени суммарно
                    $"|{(timeSpend == TimeSpan.Zero ? "" : timeSpend.TotalMinutes / 60 < 1 ? $"{timeSpend.TotalMinutes / 60:f1}" : $"{timeSpend.TotalMinutes / 60:f0}"),3}" +
                    // Потрачено времени в среднем
                    $"|{(avgTimeSpend == TimeSpan.Zero ? "" : avgTimeSpend.TotalMinutes / 60 < 1 ? $"{avgTimeSpend.TotalMinutes / 60:f1}" : $"{avgTimeSpend.TotalMinutes / 60:f0}"),3}" +
                    // Чистые за месяц
                    $"|{(profit == 0 ? "" : profit),18:C}" +
                    // Потенциал общий
                    // $"|{(sumP == 0 ? "" : sumP),18:C}" +
                    // Потнециал средний
                    // $"|{(avgP == 0 ? "" : avgP),18:C}" +
                    // Требуется распределить в штуках
                    $"|{((oddsProjects ?? 0) == 0 ? "" : oddsProjects),3}" +
                    // Требуется распределить в потенциале
                    $"|{((oddsPotential ?? 0) == 0 ? "" : oddsPotential),18:C}|");
                #endregion
            }
            #region Метрики службы
            #region В работе
            inWork = presales.Where(p => p.Position == Position.Engineer
                                      || p.Position == Position.Account)
                .Sum(p => p.CountProjectsByStatus(ProjectStatus.WorkInProgress));
            #endregion
            #region Назначено в этом месяце
            assing = presales.Where(p => p.Position == Position.Engineer
                                      || p.Position == Position.Account)
                .Sum(p => p.CountProjectsAssigned(thisMonth));
            #endregion
            #region Выиграно в этом месяце
            won = presales.Where(p => p.Position == Position.Engineer
                                   || p.Position == Position.Account)
                .Sum(p => p.ClosedByStatus(ProjectStatus.Won, thisMonth));
            #endregion
            #region Проиграно в этом месяце
            loss = presales.Where(p => p.Position == Position.Engineer
                                    || p.Position == Position.Account)
                .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, thisMonth));
            #endregion
            #region Суммарный потенциал открытых проектов
            /*
            sumP = presales.Where(p => p.Position == Position.Engineer
                                    || p.Position == Position.Account)
                .Sum(p => p.SumPotentialByStatus(ProjectStatus.WorkInProgress));
            //*/
            #endregion
            #region Средний потенциал открытых проектов
            /*
            avgP = presales
                .Where(p => p.Position == Position.Engineer
                         || p.Position == Position.Account)
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : p.AveragePotentialByStatus(ProjectStatus.WorkInProgress));
            //*/
            #endregion
            #region Среднее время жизни проекта до выигрыша
            avgTTW = TimeSpan.FromDays(presales
                .Where(p => p.Position == Position.Engineer
                         || p.Position == Position.Account)
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : p.AverageTimeToWin(thisMonth).TotalDays));
            #endregion
            #region Среднее время реакции
            avgTTR = TimeSpan.FromMinutes(presales
                .Where(p => p.Position == Position.Engineer
                         || p.Position == Position.Account)
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : p.AverageTimeToReaction(thisMonth).TotalMinutes));
            #endregion
            #region Суммарное потраченное время на проекты в этом месяце
            timeSpend = TimeSpan.FromMinutes(presales.Where(p => p.Position == Position.Engineer
                                                              || p.Position == Position.Account)
                    .Sum(p => p.SumTimeSpend(thisMonth).TotalMinutes));
            #endregion
            #region Cреднее время потраченное на проект в этом месяце
            avgTimeSpend = TimeSpan.FromMinutes(presales.Where(p => p.Position == Position.Engineer
                                                                 || p.Position == Position.Account)
                    .Sum(p => p.AverageTimeSpend(thisMonth).TotalMinutes));
            #endregion
            #region Средний ранг проектов
            avgRang = presales
                .Where(p => p.Position == Position.Engineer
                         || p.Position == Position.Account)
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : p.AverageRang());
            #endregion
            #region Количество "брошенных" проектов
            abandoned = presales.Where(p => p.Position == Position.Engineer
                                         || p.Position == Position.Account)
                .Sum(p => p.CountProjectsAbandoned(TimeSpan.FromDays(30)));
            #endregion
            #region Чистые за месяц
            profit = presales.Where(p => p.Department != Department.None)
                .Sum(p => p.SumProfit(thisMonth, ref invs));
            #endregion
            #region Просроченные проекты
            var overdue = projects?
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                .Where(p => p.IsOverdue());
            #endregion
            #region Забытые проекты
            var forgotten = projects?
                .Where(p => p.ApprovalByTechDirectorAt.ToLocalTime() > thisMonth)?
                .Where(p => p.IsForgotten());
            #endregion
            #region Новые проекты
            var newProjects = projects?.
                Where(p => p.ApprovalBySalesDirectorAt != DateTime.MinValue)
               .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue);
            #endregion
            #region Среднее время реакции руководителя
            var avgTTDR = projects?
                .Where(p => p.ApprovalBySalesDirectorAt.ToLocalTime() > thisMonth)?
                .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue)?
                .DefaultIfEmpty()
                .Average(p => p is null ? 0 : p.TimeToDirectorReaction().TotalMinutes);
            #endregion
            #endregion
            #region Отображение данных по службе
            Console.WriteLine(
                $"{"Всего",17}" +
                // В работе
                $"|{(inWork == 0 ? "" : inWork),6}" +
                // Назначено
                $"|{(assing == 0 ? "" : assing),6}" +
                // Выиграно
                $"|{(won == 0 ? "" : won),3}" +
                // Проиграно
                $"|{(loss == 0 ? "" : loss),4}" +
                // Конверсия
                $"|{(won == 0 || assing == 0 ? "" : won / (assing == 0 ? 0f : assing)),5:P0}" +
                // Заброшено проектов
                $"|{(abandoned == 0 ? "" : abandoned),4}" +
                // Среднее время жизни
                $"|{(avgTTW == TimeSpan.Zero ? "" : avgTTW.TotalDays),6:f0}" +
                // Среднее время реакции
                $"|{(avgTTR == TimeSpan.Zero ? "" : avgTTR.TotalMinutes),6:f0}" +
                // Средний ранг
                $"|{(avgRang == 0 ? "" : avgRang),4:f1}" +
                // Потрачено времени суммарно
                $"|{(timeSpend == TimeSpan.Zero ? "" : timeSpend.TotalMinutes / 60 < 1 ? $"{timeSpend.TotalMinutes / 60:f1}" : $"{timeSpend.TotalMinutes / 60:f0}"),3}" +
                // Потрачено времени в среднем
                $"|{(avgTimeSpend == TimeSpan.Zero ? "" : avgTimeSpend.TotalMinutes / 60 < 1 ? $"{avgTimeSpend.TotalMinutes / 60:f1}" : $"{avgTimeSpend.TotalMinutes / 60:f0}"),3}" +
                /// Чистые за месяц
                $"|{(profit == 0 ? "" : profit),18:C}"// +
                // Потенциал общий
                // $"|{(sumP == 0 ? "" : sumP),18:C}" +
                // Потнециал средний
                // $"|{(avgP == 0 ? "" : avgP),18:C}"
                );

            Console.WriteLine($"\n\tAbnd - количество заброшенных проектов (не было действий больше месяца).");
            Console.WriteLine($"\tAvgTTW - среднее время жизни проекта до выигрыша в днях.");
            Console.WriteLine($"\tAvgTTR - среднее время реакции пресейла в минутах.");
            Console.WriteLine($"\tAvgR - средний ранг проектов.");
            Console.WriteLine($"\tSpend - потраченное на проекты время в часах, {thisMonth:MMMM}.\n");
            Console.WriteLine($"\tСреднее время реакции руководителя (среднее время до назначения) в минутах: {avgTTDR:f0}");
            Console.WriteLine($"\tПроекты с нарушением пунктов 3.1 и 3.2 Регламента (просроченные): {overdue?.Count()}");
            foreach (var p in overdue) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalByTechDirectorAt.ToLocalTime()} - {p.PresaleStartAt.ToLocalTime()}, {p.Presale?.Name}");
            Console.WriteLine($"\tПроекты без отметки начала работы пресейлом (забытые): {forgotten?.Count()}");
            foreach (var p in forgotten) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalByTechDirectorAt.ToLocalTime()}, {p.Presale?.Name}");
            Console.WriteLine($"\tНовые проекты (ожидают распределения): {newProjects?.Count()}");
            foreach (var p in newProjects) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalBySalesDirectorAt.ToLocalTime()}");
            Console.WriteLine($"\n\tДоступны данные за период: 20.09.2022 00:00:00 - {prevUpdate:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine($"\tПоследнее обновление: {prevUpdate:dd.MM.yyyy HH:mm:ss.fff}");
            #endregion
        }
    }
}