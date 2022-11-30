using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using PresalesMonitor;
using PresalesMonitor.Shared;
using PresalesMonitor.Entities;
using Microsoft.EntityFrameworkCore;
using PresalesMonitor.Entities.Enums;
using PresalesMonitor.Shared.CustomTypes;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using System.Security.Authentication;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace PresalesMonitor.Server.Services
{
    public class PresalesMonitorService : Presales.PresalesBase
    {
        private Image _cashedImage = new()
        {
            Raw = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3",
            Full = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80",
            Regular = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=1080",
            Small = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=400",
            Thumb = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=200",
            SmallS3 = "https://s3.us-west-2.amazonaws.com/images.unsplash.com/small/photo-1666932999928-f6029c081d77",
            AltDescription = "",
            AuthorName = "Dmitry Ganin",
            SourceName = "Unsplash",
            AuthorUrl = @"https://api.unsplash.com/users/ganinph?utm_source=presales_monitor&utm_medium=referral",
            SourceUrl = @"https://unsplash.com/?utm_source=presales_monitor&utm_medium=referral",
        };

        public override Task<KpiResponse> GetKpi(KpiRequest request, ServerCallContext context)
        {
            using var db = new DbController.Context();
            db.ChangeTracker.LazyLoadingEnabled = false;
            var presale = db.Presales.Single(p => p.Name == request.PresaleName);

            if (presale == null)
                return Task.FromResult(new KpiResponse { Error = new Error{ Message = "Пресейл не найден в базе данных."}});

            var from = request.Period.From.ToDateTime();
            var to = request.Period.To.ToDateTime();


            presale = db.Presales
                .Include(p => p.Invoices
                    .Where(i => i.Date >= from
                             || i.LastPayAt >= from
                             || i.LastShipmentAt >= from))
                    .ThenInclude(i => i.Project).ThenInclude(p => p.Actions)
                .Include(p => p.Invoices
                    .Where(i => i.Date >= from
                             || i.LastPayAt >= from
                             || i.LastShipmentAt >= from))
                    .ThenInclude(i => i.Project).ThenInclude(p => p.MainProject)
                .Include(p => p.Invoices
                    .Where(i => i.Date >= from
                             || i.LastPayAt >= from
                             || i.LastShipmentAt >= from))
                    .ThenInclude(i => i.ProfitPeriods)
                .Single(p => p.Name == request.PresaleName);


            if (presale.Projects == null)
                return Task.FromResult(new KpiResponse());

            HashSet<Entities.Project> projects = new();
            RecurseLoad(presale.Projects, db, ref projects);

            HashSet<Entities.Invoice>? invoices = new();
            presale.SumProfit(from, to, ref invoices);

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

                var profit = invoice.GetProfit(from, to);

                var invoiceReply = new Shared.Invoice
                {
                    Counterpart = invoice.Counterpart,
                    Nubmer = invoice.Number,
                    Date = Timestamp.FromDateTime(invoice.Date),
                    Amount = DecimalValue.FromDecimal(invoice.Amount),
                    Cost = DecimalValue.FromDecimal(invoice.Amount - profit),
                    SalesAmount = DecimalValue.FromDecimal(profit),
                    Percent = percent,
                    Profit = DecimalValue.FromDecimal(profit * (decimal)percent),
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
            using var db = new DbController.Context();
            var presales = db.Presales
                .Include(p => p.Projects
                /*
                    .Where(p => 
                        (p.Actions != null && p.Actions.Any(a => a.Date >= from.AddYears(-1)))
                        || p.PresaleStartAt >= from.AddYears(-1)
                        || p.ApprovalBySalesDirectorAt >= from.AddYears(-1)
                        || p.ApprovalByTechDirectorAt >= from.AddYears(-1))
                    //*/
                    )
                .ThenInclude(p => p.Actions)
                .ToList();

            _ = db.Invoices
                .Where(i => i.Date >= from
                    || i.LastPayAt >= from
                    || i.LastShipmentAt >= from)
                .Include(i => i.Presale)
                .Include(i => i.ProfitPeriods).ToList();

            List<Entities.Project> projects = new();

            foreach (var presale in presales)
                if (presale.Projects != null)
                    projects.AddRange(presale.Projects);

            decimal maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt > from)?
                .Sum(p => p.PotentialAmount)) ?? 0;
            int maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt > from)?
                .Count()) ?? 0;

            int won, assign;
            var reply = new Overview();
            foreach (var presale in presales)
            {
                if (presale.Position == Position.None
                 || presale.Position == Position.Director) continue;

                var isRuEngineer = presale.Department == Department.Russian
                                && presale.Position == Position.Engineer;
                won = presale.ClosedByStatus(ProjectStatus.Won, from);
                assign = presale.CountProjectsAssigned(from);

                #region Метрики пресейла
                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = new Statistic()
                    {
                        #region В работе
                        InWork = presale.CountProjectsInWork(TimeSpan.FromDays(30)),
                        #endregion
                        #region Назначено в этом месяце
                        Assign = assign,
                        #endregion
                        #region Выиграно в этом месяце
                        Won = won,
                        #endregion
                        #region Проиграно в этом месяце
                        Loss = presale.ClosedByStatus(ProjectStatus.Loss, from),
                        #endregion
                        #region Конверсия
                        Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                        #endregion
                        #region Среднее время жизни проекта до выигрыша
                        AvgTimeToWin = Duration.FromTimeSpan(presale.AverageTimeToWin()),
                        #endregion
                        #region Среднее время реакции
                        AvgTimeToReaction = Duration.FromTimeSpan(presale.AverageTimeToReaction(from)),
                        #endregion
                        #region Суммарное потраченное время на проекты в этом месяце
                        SumSpend = Duration.FromTimeSpan(presale.SumTimeSpend(from)),
                        #endregion
                        #region Cреднее время потраченное на проект в этом месяце
                        AvgSpend = Duration.FromTimeSpan(presale.AverageTimeSpend(from)),
                        #endregion
                        #region Средний ранг проектов
                        AvgRank = presale.AverageRang(),
                        #endregion
                        #region Количество "брошенных" проектов
                        Abnd = presale.CountProjectsAbandoned(TimeSpan.FromDays(30)),
                        #endregion
                        #region Чистые за месяц
                        Profit = DecimalValue.FromDecimal(presale.SumProfit(from)),
                        #endregion
                    },
                    #region Недостаток проектов
                    DeficitProjects = isRuEngineer ? maxCount - presale.CountProjectsAssigned(from) : 0,
                    #endregion
                    #region Недостаток потенциала
                    DeficitPotential = isRuEngineer ? maxPotential - presale.SumPotential(from) : 0,
                    #endregion
                });
                #endregion
            }
            #region Метрики службы

            won = presales.Where(p => p.Position == Position.Engineer
                                       || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Won, from));
            assign = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAssigned(from));

            reply.Statistics = new Statistic()
            {
                #region В работе
                InWork = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsInWork(TimeSpan.FromDays(30))),
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
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, from)),
                #endregion
                #region Конверсия
                Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                #endregion
                #region Среднее время жизни проекта до выигрыша
                AvgTimeToWin = Duration.FromTimeSpan(TimeSpan.FromDays(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .Average(p => p.AverageTimeToWin(from).TotalDays) ?? 0)),
                #endregion
                #region Среднее время реакции
                AvgTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .Average(p => p.AverageTimeToReaction(from).TotalMinutes) ?? 0)),
                #endregion
                #region Суммарное потраченное время на проекты в этом месяце
                SumSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                  || p.Position == Position.Account)
                        .Sum(p => p.SumTimeSpend(from).TotalMinutes) ?? 0)),
                #endregion
                #region Cреднее время потраченное на проект в этом месяце
                AvgSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                     || p.Position == Position.Account)
                        .Sum(p => p.AverageTimeSpend(from).TotalMinutes) ?? 0)),
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
                    .Sum(p => p.SumProfit(from)) ?? 0,
                #endregion
            };
            #region Просроченные проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt > from)
                    .Where(p => p.IsOverdue()))
                reply.Escalations.Add(proj.Translate());
            #endregion
            #region Забытые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt > from)
                    .Where(p => p.IsForgotten()))
                reply.Forgotten.Add(proj.Translate());
            #endregion
            #region Новые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalBySalesDirectorAt >= from)
                    .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue))
                reply.New.Add(proj.Translate());
            #endregion
            #region Среднее время реакции руководителя
            reply.AvgDirectorTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(projects?
                .Where(p => p.ApprovalBySalesDirectorAt > from)?
                .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue)?
                .Average(p => p.TimeToDirectorReaction().TotalMinutes) ?? 0 ));
            #endregion
            #endregion

            return reply;
        }

        public override Task<Image> GetImageUrl(ImageRequest request, ServerCallContext context)
        {
            var macroscopClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                SslProtocols = SslProtocols.Tls12
            };
            var unsplashClient = new HttpClient(macroscopClientHandler)
            {
                BaseAddress = new Uri("https://api.unsplash.com"),
            };
            var unsplashRequest = new HttpRequestMessage(HttpMethod.Get, $"photos/random/?query={request.Keyword}&orientation=portrait");
            unsplashRequest.Headers.Add("Authorization", "Client-ID zoKHly26A5L5BCYWXdctm0hc9u5JGaqcsMv_znpsIR0");

            var unsplashResponse = unsplashClient.SendAsync(unsplashRequest).Result;
            if (!unsplashResponse.IsSuccessStatusCode) return Task.FromResult(_cashedImage);

            var response = JsonConvert.DeserializeObject<dynamic>(unsplashResponse.Content.ReadAsStringAsync().Result);

            _cashedImage.Raw = response.urls.raw;
            _cashedImage.Full = response.urls.full;
            _cashedImage.Regular = response.urls.regular;
            _cashedImage.Small = response.urls.small;
            _cashedImage.Thumb = response.urls.thumb;
            _cashedImage.SmallS3 = response.urls.small_s3;
            _cashedImage.AltDescription = $"{response.alt_description}";
            _cashedImage.AuthorName = $"{response.user.name}";
            _cashedImage.SourceName = "Unsplash";
            _cashedImage.AuthorUrl = $"{response.user.links.self}?utm_source=presales_monitor&utm_medium=referral";
            _cashedImage.SourceUrl = @"https://unsplash.com/?utm_source=presales_monitor&utm_medium=referral";

            return Task.FromResult(_cashedImage);
        }
    }

    public static class Extensions
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
