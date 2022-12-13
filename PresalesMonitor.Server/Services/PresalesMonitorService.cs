using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using PresalesMonitor.Shared;
using PresalesMonitor.Entities;
using Microsoft.EntityFrameworkCore;
using PresalesMonitor.Entities.Enums;
using PresalesMonitor.Shared.CustomTypes;
using System.Security.Authentication;
using Newtonsoft.Json;
using Department = PresalesMonitor.Entities.Enums.Department;
using Position = PresalesMonitor.Entities.Enums.Position;

namespace PresalesMonitor.Server.Services
{
    public class PresalesMonitorService : Presales.PresalesBase
    {
        private string cachedTop = @"{ ""Всего"": 0.0, ""Топ"": [ { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 } ] }";
        private Image _cashedImageGirl = new()
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
            AuthorUrl = @"https://unsplash.com/@ganinph",
            SourceUrl = @"https://unsplash.com/",
        };
        private Image _cashedImageNY = new()
        {
            Raw = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3",
            Full = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80",
            Regular = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=1080",
            Small = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=400",
            Thumb = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=200",
            SmallS3 = "https://s3.us-west-2.amazonaws.com/images.unsplash.com/small/photo-1514803530614-3a2bef88f31c",
            AltDescription = "person holding sparkler",
            AuthorName = "Chinh Le Duc",
            SourceName = "Unsplash",
            AuthorUrl = @"https://unsplash.com/@mero_dnt",
            SourceUrl = @"https://unsplash.com/",
        };
        public override Task<KpiResponse> GetKpi(KpiRequest request, ServerCallContext context)
        {
            using var db = new DbController.Context();
            var presale = db.Presales.Single(p => p.Name == request.PresaleName);

            if (presale == null)
                return Task.FromResult(new KpiResponse { Error = new Error{ Message = "Пресейл не найден в базе данных."}});

            var from = request.Period.From.ToDateTime();
            var to = request.Period.To.ToDateTime();

#pragma warning disable CS8604 // Possible null reference argument.
            presale = db.Presales?
                .Where(p => p.Name == request.PresaleName)?
                .Where(p => p != null && p.Invoices != null)?
                .Include(p => p.Invoices
                    .Where(i => i.Date >= from
                                || i.LastPayAt >= from
                                || i.LastShipmentAt >= from))?.ThenInclude(i => i.Project)
                .Include(p => p.Invoices
                    .Where(i => i.Date >= from
                                || i.LastPayAt >= from
                                || i.LastShipmentAt >= from))?.ThenInclude(i => i.ProfitPeriods)
                .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.

            if (presale?.Projects == null)
                return Task.FromResult(new KpiResponse());

            HashSet<Entities.Project> projects = new();
            RecurseLoad(presale.Projects.ToList(), db, ref projects);

            HashSet<Entities.Invoice>? invoices = new();
            presale.SumProfit(from, to, ref invoices);

            if (invoices == null)
                return Task.FromResult(new KpiResponse());

            var reply = new Kpi();

            foreach(var invoice in invoices.OrderBy(i => (int)i.Counterpart[0]).ThenBy(i => i.Counterpart).ThenBy(i => i.Number))
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

                var profit = invoice.GetProfit(from, to);

                var invoiceReply = new Shared.Invoice
                {
                    Counterpart = invoice.Counterpart,
                    Number = invoice.Number,
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
        public override Task<Overview> GetOverview(OverviewRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;

            return Task.FromResult(GetOverview(from, to, request.Department, request.Position));
        }
        public override Task<Image> GetImageUrl(ImageRequest request, ServerCallContext context)
        {
            try
            {
                var clientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    SslProtocols = SslProtocols.Tls12
                };
                var unsplashClient = new HttpClient(clientHandler) { BaseAddress = new Uri("https://api.unsplash.com") };
                var unsplashRequest = new HttpRequestMessage(HttpMethod.Get, $"photos/random?query={request.Keyword}&orientation={request.Orientation}");
                unsplashRequest.Headers.Add("Authorization", "Client-ID zoKHly26A5L5BCYWXdctm0hc9u5JGaqcsMv_znpsIR0");
                var unsplashResponse = unsplashClient.SendAsync(unsplashRequest).Result;
                if (!unsplashResponse.IsSuccessStatusCode) return Task.FromResult(_cashedImageGirl);
                var response = JsonConvert.DeserializeObject<dynamic>(unsplashResponse.Content.ReadAsStringAsync().Result);
                if (response == null) return request.Keyword switch
                {
                    "happy new year" => Task.FromResult(_cashedImageNY),
                    _ => Task.FromResult(_cashedImageGirl)
                };
                var _img = new Image
                {
                    Raw = response.urls.raw,
                    Full = response.urls.full,
                    Regular = response.urls.regular,
                    Small = response.urls.small,
                    Thumb = response.urls.thumb,
                    SmallS3 = response.urls.small_s3,
                    AltDescription = $"{response.alt_description}",
                    AuthorName = $"{response.user.name}",
                    AuthorUrl = $"{response.user.links.html}",
                    SourceName = "Unsplash",
                    SourceUrl = @"https://unsplash.com/",
                };
                switch (request.Keyword)
                {
                    case "happy new year":
                        _cashedImageNY = _img;
                        break;
                    default:
                        _cashedImageGirl = _img;
                        break;
                }
                return Task.FromResult(_img);
            }
            catch
            {
                return Task.FromResult(_cashedImageGirl);
            }
        }
        public override Task<MonthProfit> GetMonthProfit(Period request, ServerCallContext context)
        {
            var from = request?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.To?.ToDateTime() ?? DateTime.MaxValue;
            decimal plan = 75000000;

            using var db = new DbController.Context();
            var presales = db.Presales.ToList();
            _ = db.Invoices
                .Where(i => i.Date >= from
                    || i.LastPayAt >= from
                    || i.LastShipmentAt >= from)
                .Include(i => i.Presale)
                .Include(i => i.ProfitPeriods).ToList();

            var reply = new MonthProfit();
            foreach (var presale in presales)
            {
                if (presale.Position == Position.None
                 || presale.Position == Position.Director) continue;

                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = new Statistic()
                    {
                        Profit = DecimalValue.FromDecimal(presale.SumProfit(from, to))
                    }
                });
            }

            var profit = presales?.Where(p => p.Department != Department.None)
                .Sum(p => p.SumProfit(from, to)) ?? 0;

            var profitPrevDay = presales?.Where(p => p.Department != Department.None)
                .Sum(p => p.SumProfit(from, to.AddDays(-1))) ?? 0;

            reply.Profit = profit;
            reply.Plan = plan;
            reply.Left = plan - profit > 0 ? plan - profit : 0;
            reply.DeltaDay = profit - profitPrevDay > 0 ? profit - profitPrevDay : 0;

            return Task.FromResult(reply);
        }
        public override Task<SalesOverview> GetSalesOverview(Empty request, ServerCallContext context)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://127.0.0.1") };
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"trade/hs/API/GetTradeData");
            httpRequest.Headers.Add("Authorization", "Basic 0J/QvtC70Y/QutC+0LLQmDpraDk5OFh0Rg==");
            var httpResponse = httpClient.SendAsync(httpRequest).Result;
            var result = httpResponse.Content.ReadAsStringAsync().Result;

            if (!httpResponse.IsSuccessStatusCode) result = cachedTop;
            cachedTop = result;

            var response = JsonConvert.DeserializeObject<dynamic>(result);
            var reply = new SalesOverview();

            foreach (var manager in response.Топ)
                reply.Top.Add(new Manager { Name = string.Join(" ", ((string)manager.Имя).Split().Take(2)), Profit = (decimal)manager.Сумма });

            reply.Profit = (decimal)response.Всего;
            reply.PlanFull = 440000000;
            reply.PlanSmall = 420000000;

            return Task.FromResult(reply);
        }
        private static Overview GetOverview(DateTime from, DateTime to, Shared.Department department, Shared.Position position)
        {
            using var db = new DbController.Context();
#pragma warning disable CS8604 // Possible null reference argument.
            var presales = db.Presales
                .Include(p => p.Projects
                    .Where(p => p.Actions != null)
                )
                .ThenInclude(p => p.Actions)
                .ToList();
#pragma warning restore CS8604 // Possible null reference argument.

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

            var otherProjects = db.Projects.Where(p => p.Presale == null).ToList();
            if (otherProjects != null)
                projects.AddRange(otherProjects);

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
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageTimeToWin().TotalDays ?? 0) ?? 0)),
                #endregion
                #region Среднее время реакции
                AvgTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageTimeToReaction(from).TotalMinutes ?? 0) ?? 0)),
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
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageRang() ?? 0) ?? 0,
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
                .DefaultIfEmpty()
                .Average(p => p?.TimeToDirectorReaction().TotalMinutes ?? 0) ?? 0));
            #endregion
            #endregion

            return reply;
        }
        private void RecurseLoad(List<Entities.Project>? projects, DbController.Context db, ref HashSet<Entities.Project> projectsViewed)
        {
            if (projects == null || projects.Count == 0) return;
            foreach (var project in projects)
            {
                if (project == null) continue;
                if (projectsViewed.Contains(project)) continue;
                else projectsViewed.Add(project);
                _ = db.Actions?.Where(a => a.Project.Number == project.Number)?.ToList();
                var prj = db.Projects?.Where(p => p.Number == project.Number)?.Include(p => p.MainProject).FirstOrDefault();
                if (prj?.MainProject?.Number != null)
                {
                    var mainPrjs = db.Projects?.Where(p => p.Number == prj.MainProject.Number)?.ToList();
                    RecurseLoad(mainPrjs, db, ref projectsViewed);
                }
            }
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
