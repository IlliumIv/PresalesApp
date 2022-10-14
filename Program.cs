using Entities;
using Entities.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesStatistic.Helpers;
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
                    ContractResolver = new JsonPropertiesResolver(),
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
            };

            if (!Settings.ConfigurationFileIsExists()) Settings.CreateConfigurationFile();

            Settings.TryGetSection<Settings.Database>(out ConfigurationSection? r);
            if (r == null) return;
            var dbSettings = (Settings.Database)r;
            var optionsBuilder = new DbContextOptionsBuilder<DbController.Context>();
            var dbOptions = optionsBuilder.UseNpgsql($"host={dbSettings.Url};" +
                $"port={dbSettings.Port};" +
                $"database={dbSettings.DatabaseName};" +
                $"username={dbSettings.Username};" +
                $"password={dbSettings.Password}")
                // .EnableSensitiveDataLogging().LogTo(message => Debug.WriteLine(message))
                .Options;

            // using var db = new Context();
            // db.Delete();
            // db.Create();

            while (true)
            {
                // Parser.Run(dbOptions);
                Settings.TryGetSection<Settings.Application>(out r);
                if (r == null) return;
                var appSettings = (Settings.Application)r;
                ShowData(dbOptions, appSettings.PreviosUpdate);
                Task.Delay(600000).Wait();
            };
        }

        public static void ShowData(DbContextOptions<DbController.Context> dbOptions, DateTime prevUpdate)
        {
            Console.Clear();

            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var prevMonth = thisMonth.AddMonths(-1);

            using var db = new DbController.Context(dbOptions);
            var presales = db.Presales.Include(p => p.Projects).ToList();
            var actions = db.Actions.ToList();
            var projects = db.Projects.ToList();

            var maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Entities.Enums.Department.Russian)
                .Where(p => p.Presale?.Position == Entities.Enums.Position.Engineer)
                .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                .Sum(p => p.PotentialAmount));
            var maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Entities.Enums.Department.Russian)
                .Where(p => p.Presale?.Position == Entities.Enums.Position.Engineer)
                .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                .Count());

            #region Отображение заголовков таблицы
            Console.WriteLine($"{"",5}{"Пресейл",-12}|InWork|Assign|Won|Loss|Convr|Abnd|AvgTTW|AvgTTR|AvgR|{"",1}{"Spend",-6}|{"",12}{"Потенциал, руб.",-25}|Требуется распределить|");
            Console.WriteLine($"{"",17}|{"",6}|{"",7}{thisMonth,-14:MMMM}|{"",4}|{"",6}|{"",6}|{"",4}|Sum|Avg|{"",7}{"общий",-11}|{"",6}{"средний",-12}|шт.|{"",7}{"руб.",-11}|");
            #endregion

            int? inWork, assing, won, loss, abandoned;
            decimal? sumP, avgP;
            TimeSpan avgTTW;
            double? avgTTR, avgRang;
            double timeSpend, avgTimeSpend;

            foreach (var presale in presales)
            {
                if (presale.Position == Entities.Enums.Position.None
                    || presale.Position == Entities.Enums.Position.Director) continue;

                var isRuEngineer = presale.Department == Entities.Enums.Department.Russian
                                && presale.Position == Entities.Enums.Position.Engineer;
                #region Метрики пресейла
                #region В работе
                inWork = presale.Projects?
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                    .Count();
                #endregion
                #region Назначено в этом месяце
                assing = presale.Projects?
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                    .Count();
                #endregion
                #region Выиграно в этом месяце
                won = presale.Projects?
                    .Where(p => p.LastStatusChanged.ToLocal() > thisMonth)
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.Won)
                    .Count();
                #endregion
                #region Проиграно в этом месяце
                loss = presale.Projects?
                    .Where(p => p.LastStatusChanged.ToLocal() > thisMonth)
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.Loss)
                    .Count();
                #endregion
                #region Суммарный потенциал открытых проектов
                sumP = presale.Projects?
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > prevMonth)
                    .Sum(p => p.PotentialAmount);
                #endregion
                #region Средний потенциал открытых проектов
                avgP = presale.Projects?
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > prevMonth)
                    .Average(p => p.PotentialAmount);
                #endregion
                #region Недостаток проектов
                var oddsProjects = isRuEngineer ? maxCount - presale.Projects?
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                    .Count() : 0;
                #endregion
                #region Недостаток потенциала
                var oddsPotential = isRuEngineer ? maxPotential - presale.Projects?
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                    .Sum(p => p.PotentialAmount) : 0;
                #endregion
                #region Среднее время жизни проекта до выигрыша
                avgTTW = TimeSpan.FromDays((presale.Projects?.Where(p => p.Status == Entities.Enums.ProjectStatus.Won).Count()
                    > 0 ? presale.Projects?
                    .Where(p => p.Status == Entities.Enums.ProjectStatus.Won)
                    .Where(p => p.LastStatusChanged.ToLocal() > prevMonth)
                    .DefaultIfEmpty()
                    .Average(p => p == null ? 0 : (p.LastStatusChanged - p.ApprovalByTechDirector ?? new TimeSpan()).TotalDays)
                    : 0) ?? 0);
                #endregion
                #region Среднее время реакции
                avgTTR = presale.Projects?
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                    .Where(p => p.PresaleStart != null)
                    .DefaultIfEmpty()
                    .Average(p => p == null ? 0 :
                        CalculateWorkingMinutes(
#pragma warning disable CS8629 // Nullable value type may be null.
                            (DateTime)p.ApprovalByTechDirector.ToLocal(),
#pragma warning restore CS8629 // Nullable value type may be null.
                            new List<DateTime>()
                            {
                                (actions
                                    .Where(a => a.Project == p)
                                    .FirstOrDefault(a => a.Number == 1)?.Date.ToLocal() ?? DateTime.Now)
                                .AddMinutes(-actions
                                    .Where(a => a.Project == p)
                                    .FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
#pragma warning disable CS8629 // Nullable value type may be null.
                                (DateTime)p.PresaleStart.ToLocal()
#pragma warning restore CS8629 // Nullable value type may be null.
                            }.Min(dt => dt)));
                #endregion
                #region Суммарное потраченное время на проекты в этом месяце
                timeSpend = actions
                    .Where(a => a.Date.ToLocal() > thisMonth)
                    .Where(a => a.Project != null && a.Project.Presale == presale)
                    .Sum(a => a.TimeSpend);
                #endregion
                #region Cреднее время потраченное на проект в этом месяце
                avgTimeSpend = actions
                    .Where(a => a.Date.ToLocal() > thisMonth)
                    .Where(a => a.Project != null && a.Project.Presale == presale)
                    .DefaultIfEmpty()
                    .Average(a => a == null ? 0 : a.TimeSpend);
                #endregion
                #region Средний ранг проектов
                avgRang = presale.Projects?
                    .Average(p => p.Actions?
                        .Where(a => a.Project == p)
                        .Where(a => a.Rang > 0)
                        .Sum(a => a.Rang));
                #endregion
                #region Количество "брошенных" проектов
                abandoned = presale.Projects?
                    .Where(p => p.Actions != null
                        && p.Actions.Max(a => a.Date).ToLocal() < DateTime.Now.AddDays(-30))
                    .Count();
                #endregion
                #endregion
                #region Отображение данных по пресейлу
                Console.WriteLine(
                    $"{presale.Name,17}" +
                    // В работе
                    $"|{((inWork ?? 0) == 0 ? "" : inWork),6}" +
                    // Назначено
                    $"|{((assing ?? 0) == 0 ? "" : assing),6}" +
                    // Выиграно
                    $"|{((won ?? 0) == 0 ? "" : won),3}" +
                    // Проиграно
                    $"|{((loss ?? 0) == 0 ? "" : loss),4}" +
                    // Конверсия
                    $"|{((won ?? 0) == 0 || (assing ?? 0) == 0 ? "" : (won ?? 0) / (assing ?? 0f)),5:P0}" +
                    // Заброшено проектов
                    $"|{((abandoned ?? 0) == 0 ? "" : abandoned),4}" +
                    // Среднее время жизни
                    $"|{(avgTTW == new TimeSpan() ? "" : avgTTW),6:%d}" +
                    // Среднее время реакции
                    $"|{((avgTTR ?? 0) == 0 ? "" : avgTTR),6:f0}" +
                    // Средний ранг
                    $"|{((avgRang ?? 0) == 0 ? "" : avgRang),4:f1}" +
                    // Потрачено времени суммарно
                    $"|{(timeSpend == 0 ? "" : timeSpend / 60 < 1 ? $"{timeSpend / 60:f1}" : $"{timeSpend / 60:f0}"),3}" +
                    // Потрачено времени в среднем
                    $"|{(avgTimeSpend == 0 ? "" : avgTimeSpend / 60 < 1 ? $"{avgTimeSpend / 60:f1}" : $"{avgTimeSpend / 60:f0}"),3}" +
                    // Потенциал общий
                    $"|{((sumP ?? 0) == 0 ? "" : sumP),18:C}" +
                    // Потнециал средний
                    $"|{((avgP ?? 0) == 0 ? "" : avgP),18:C}" +
                    // Требуется распределить в штуках
                    $"|{((oddsProjects ?? 0) == 0 ? "" : oddsProjects),3}" +
                    // Требуется распределить в потенциале
                    $"|{((oddsPotential ?? 0) == 0 ? "" : oddsPotential),18:C}|");                                                                                    // 15
                #endregion
            }
            #region Метрики службы
            #region В работе
            inWork = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p =>p.Projects?
                .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                .Count());
            #endregion
            #region Назначено в этом месяце
            assing = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p => p.Projects?
                    .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                    .Count());
            #endregion
            #region Выиграно в этом месяце
            won = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p => p.Projects?
                .Where(p => p.LastStatusChanged.ToLocal() > thisMonth)
                .Where(p => p.Status == Entities.Enums.ProjectStatus.Won)
                .Count());
            #endregion
            #region Проиграно в этом месяце
            loss = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p => p.Projects?
                .Where(p => p.LastStatusChanged.ToLocal() > thisMonth)
                .Where(p => p.Status == Entities.Enums.ProjectStatus.Loss)
                .Count());
            #endregion
            #region Суммарный потенциал открытых проектов
            sumP = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p => p.Projects?
                .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                .Where(p => p.ApprovalByTechDirector.ToLocal() > prevMonth)
                .Sum(p => p.PotentialAmount));
            #endregion
            #region Средний потенциал открытых проектов
            avgP = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Average(p => p.Projects?
                .Where(p => p.Status == Entities.Enums.ProjectStatus.WorkInProgress)
                .Where(p => p.ApprovalByTechDirector.ToLocal() > prevMonth)
                .Average(p => p.PotentialAmount));
            #endregion
            #region Среднее время жизни проекта до выигрыша
            avgTTW = TimeSpan.FromDays(presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Average(p => p.Projects?.Where(p => p.Status == Entities.Enums.ProjectStatus.Won).Count()
                > 0 ? p.Projects?
                .Where(p => p.Status == Entities.Enums.ProjectStatus.Won)
                .Where(p => p.LastStatusChanged.ToLocal() > prevMonth)
                .DefaultIfEmpty()
                .Average(p => p == null ? 0 : (p.LastStatusChanged - p.ApprovalByTechDirector ?? new TimeSpan()).TotalDays)
                : 0) ?? 0);
            #endregion
            #region Среднее время реакции
            avgTTR = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Average(p => p.Projects?
                .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                .Where(p => p.PresaleStart != null)
                .DefaultIfEmpty()
                .Average(p => p == null ? 0 :
                    CalculateWorkingMinutes(
#pragma warning disable CS8629 // Nullable value type may be null.
                        (DateTime)p.ApprovalByTechDirector.ToLocal(),
#pragma warning restore CS8629 // Nullable value type may be null.
                            new List<DateTime>()
                            {
                                (actions
                                    .Where(a => a.Project == p)
                                    .FirstOrDefault(a => a.Number == 1)?.Date.ToLocal() ?? DateTime.Now)
                                .AddMinutes(-actions.Where(a => a.Project == p)
                                    .FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
#pragma warning disable CS8629 // Nullable value type may be null.
                                (DateTime)p.PresaleStart.ToLocal()
#pragma warning restore CS8629 // Nullable value type may be null.
                            }.Min(dt => dt))));
            #endregion
            #region Суммарное потраченное время на проекты в этом месяце
            timeSpend = actions
                    .Where(a => a.Date.ToLocal() > thisMonth)
                    .Sum(a => a.TimeSpend);
            #endregion
            #region Cреднее время потраченное на проект в этом месяце
            avgTimeSpend = actions
                    .Where(a => a.Date.ToLocal() > thisMonth)
                    .DefaultIfEmpty()
                    .Average(a => a == null ? 0 : a.TimeSpend);
            #endregion
            #region Средний ранг проектов
            avgRang = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Average(p => p.Projects?
                    .Average(p => p.Actions?
                        .Where(a => a.Project == p)
                        .Where(a => a.Rang > 0)
                        .Sum(a => a.Rang)));
            #endregion
            #region Количество "брошенных" проектов
            abandoned = presales.Where(p => p.Position == Entities.Enums.Position.Engineer
                                       || p.Position == Entities.Enums.Position.Account)
                .Sum(p => p.Projects?
                    .Where(p => p.Actions != null
                        && p.Actions.Max(a => a.Date).ToLocal() < DateTime.Now.AddDays(-30))
                    .Count());
            #endregion
            #region Просроченные проекты
            var majorProjectMinAmount = 2000000;
            var majorProjectMaxTTR = 120; // Минут
            var maxTTR = 180; // Минут
            var overdue = projects
                .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                .Where(p => p.PresaleStart != null)
                .Where(p => CalculateWorkingMinutes(
#pragma warning disable CS8629 // Nullable value type may be null.
                    (DateTime)p.ApprovalByTechDirector.ToLocal(),
#pragma warning restore CS8629 // Nullable value type may be null.
                    new List<DateTime>() { (actions
                        .Where(a => a.Project == p)
                        .FirstOrDefault(a => a.Number == 1)?.Date.ToLocal() ?? DateTime.Now)
                        .AddMinutes(- actions
                        .Where(a => a.Project == p)
                        .FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
#pragma warning disable CS8629 // Nullable value type may be null.
                        (DateTime)p.PresaleStart.ToLocal()
#pragma warning restore CS8629 // Nullable value type may be null.
                    }.Min(dt => dt)) > (p.PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR));
            #endregion
            #region Забытые проекты
            var forgotten = projects
                .Where(p => p.ApprovalByTechDirector.ToLocal() > thisMonth)
                .Where(p => p.PresaleStart == null)
#pragma warning disable CS8629 // Nullable value type may be null.
                .Where(p => CalculateWorkingMinutes((DateTime)p.ApprovalByTechDirector.ToLocal(), DateTime.Now)
#pragma warning restore CS8629 // Nullable value type may be null.
                    > (p.PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR));
            #endregion
            #region Новые проекты
            var newProjects = projects.
                Where(p => p.ApprovalBySalesDirector != null)
               .Where(p => p.ApprovalByTechDirector == null);
            #endregion
            #region Среднее время реакции руководителя
            var avgTTDR = projects
                .Where(p => p.ApprovalBySalesDirector.ToLocal() > thisMonth)
                .Where(p => p.ApprovalByTechDirector != null)
#pragma warning disable CS8629 // Nullable value type may be null.
                .Average(p => CalculateWorkingMinutes((DateTime)p.ApprovalBySalesDirector, (DateTime)p.ApprovalByTechDirector));
#pragma warning restore CS8629 // Nullable value type may be null.
            #endregion
            #endregion
            #region Отображение данных по службе
            Console.WriteLine(
                $"{"Всего",17}" +
                // В работе
                $"|{((inWork ?? 0) == 0 ? "" : inWork),6}" +
                // Назначено
                $"|{((assing ?? 0) == 0 ? "" : assing),6}" +
                // Выиграно
                $"|{((won ?? 0) == 0 ? "" : won),3}" +
                // Проиграно
                $"|{((loss ?? 0) == 0 ? "" : loss),4}" +
                // Конверсия
                $"|{((won ?? 0) == 0 || (assing ?? 0) == 0 ? "" : (won ?? 0) / (assing ?? 0f)),5:P0}" +
                // Заброшено проектов
                $"|{((abandoned ?? 0) == 0 ? "" : abandoned),4}" +
                // Среднее время жизни
                $"|{(avgTTW == new TimeSpan() ? "" : avgTTW),6:%d}" +
                // Среднее время реакции
                $"|{((avgTTR ?? 0) == 0 ? "" : avgTTR),6:f0}" +
                // Средний ранг
                $"|{((avgRang ?? 0) == 0 ? "" : avgRang),4:f1}" +
                // Потрачено времени суммарно
                $"|{(timeSpend == 0 ? "" : timeSpend / 60 < 1 ? $"{timeSpend / 60:f1}" : $"{timeSpend / 60:f0}"),3}" +
                // Потрачено времени в среднем
                $"|{(avgTimeSpend == 0 ? "" : avgTimeSpend / 60 < 1 ? $"{avgTimeSpend / 60:f1}" : $"{avgTimeSpend / 60:f0}"),3}" +
                // Потенциал общий
                $"|{((sumP ?? 0) == 0 ? "" : sumP),18:C}" +
                // Потнециал средний
                $"|{((avgP ?? 0) == 0 ? "" : avgP),18:C}");

            Console.WriteLine($"\n\tAbnd - количество заброшенных проектов (не было действий больше месяца).");
            Console.WriteLine($"\tAvgTTW - среднее время жизни проекта до выигрыша в днях.");
            Console.WriteLine($"\tAvgTTR - среднее время реакции пресейла в минутах.");
            Console.WriteLine($"\tAvgR - средний ранг проектов.");
            Console.WriteLine($"\tSpend - потраченное на проекты время в часах, {thisMonth:MMMM}.\n");
            Console.WriteLine($"\tСреднее время реакции руководителя (среднее время до назначения) в минутах: {avgTTDR:f0}");
            if (overdue.Any())
            {
                Console.WriteLine($"\tПроекты с нарушением пунктов 3.1 и 3.2 Регламента (просроченные): {overdue.Count()}");
                // foreach (var p in overdue) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalByTechDirector.ToLocal()} - {p.PresaleStart.ToLocal()}, {p.Presale?.Name}");
            }
            if (forgotten.Any())
            {
                Console.WriteLine($"\tПроекты без отметки начала работы пресейлом (забытые): {forgotten.Count()}");
                // foreach (var p in forgotten) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalByTechDirector.ToLocal()}, {p.Presale?.Name}");
            }
            if (newProjects.Any())
            {
                Console.WriteLine($"\tНовые проекты (ожидают распределения):");
                foreach (var p in newProjects) Console.WriteLine($"\t\t{p.Number}, {p.ApprovalBySalesDirector.ToLocal()}");
            }
            Console.WriteLine($"\n\tДоступны данные за период: 20.09.2022 00:00:00 - {prevUpdate:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine($"\tПоследнее обновление: {prevUpdate:dd.MM.yyyy HH:mm:ss.fff}");
            #endregion
        }

        // https://www.codeproject.com/Articles/19559/Calculating-Business-Hours
        public static int GetWorkingDays(DateTime start, DateTime end)
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
        public static int CorrectFirstDayTime(DateTime start, DateTime maxTime, DateTime minTime)
        {
            if (maxTime < start) return 0;
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday) return 0;
            if (start < minTime) start = minTime;

            var diff = maxTime - start;
            return diff.Days * 24 * 60 * 60 + diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
        }
        public static int CorrectLastDayTime(DateTime end, DateTime maxTime, DateTime minTime)
        {
            if (minTime > end) return 0;
            if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday) return 0;
            if (end > maxTime) end = maxTime;

            var diff = end - minTime;
            return diff.Days * 24 * 60 * 60 + diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
        }
        public static double CalculateWorkingMinutes(DateTime start, DateTime end)
        {
            if (start > end) return 0;
            start = start.TimeOfDay == new DateTime().TimeOfDay ? start : start.ToLocalTime();
            end = end.TimeOfDay == new DateTime().TimeOfDay ? end : end.ToLocalTime();

            var tempStart = new DateTime(start.Year, start.Month, start.Day);
            var tempEnd = new DateTime(end.Year, end.Month, end.Day);

            var isSameDay = start.Date == end.Date;

            var iBDays = GetWorkingDays(tempStart, tempEnd);
            tempStart += new TimeSpan(start.Hour, start.Minute, start.Second);
            tempEnd += new TimeSpan(end.Hour, end.Minute, end.Second);

            var maxTime = new DateTime(start.Year, start.Month, start.Day, 18, 0, 0);
            var minTime = new DateTime(start.Year, start.Month, start.Day, 9, 0, 0);
            var firstDaySec = CorrectFirstDayTime(tempStart, maxTime, minTime);

            maxTime = new DateTime(end.Year, end.Month, end.Day, 18, 0, 0);
            minTime = new DateTime(end.Year, end.Month, end.Day, 9, 0, 0);
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
    }
}