using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Authorization;
using PresalesApp.Web.Shared;
using System.Security.Authentication;
using System.Security.Cryptography;
using ActionType = PresalesApp.Database.Enums.ActionType;
using Department = PresalesApp.Database.Enums.Department;
using Enum = System.Enum;
using Position = PresalesApp.Database.Enums.Position;
using ProjectStatus = PresalesApp.Database.Enums.ProjectStatus;
using Project = PresalesApp.Database.Entities.Project;
using static PresalesApp.Database.DbController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Google.Protobuf;

namespace PresalesApp.Web.Controllers
{
    [Authorize]
    public class PresalesAppApiController : PresalesAppApi.PresalesAppApiBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly TokenParameters _tokenParameters;
        private readonly ILogger<PresalesAppApiController> _logger;

        private readonly decimal plan = 10017608;
        private string _cachedOverview = @"{ ""Всего"": 0.0, ""Топ"": [ { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }, { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 } ] }";
        private ImageResponse _cashedImageGirl = new()
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
        private ImageResponse _cashedImageNY = new()
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

        public PresalesAppApiController(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            TokenParameters tokenParameters,
            ILogger<PresalesAppApiController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenParameters = tokenParameters;
            _logger = logger;
        }

        [AllowAnonymous]
        public override async Task<LoginResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.LoginRequest.Login))
            {
                return new LoginResponse
                {
                    Error = new Error
                    {
                        Message = "Login is not valid"
                    }
                };
            }

            var newUser = new User
            {
                ProfileName = request.Profile.Name,
                UserName = request.LoginRequest.Login
            };

            var user = await _userManager.CreateAsync(newUser, request.LoginRequest.Password);

            if (user.Succeeded)
            {
                return new LoginResponse
                {
                    UserInfo = new UserInfo
                    {
                        Token = await newUser.GenerateJwtToken(_tokenParameters, _roleManager, _userManager),
                        Profile = new UserProfile
                        {
                            Name = newUser.ProfileName
                        }
                    }
                };
            }

            return new LoginResponse
            {
                Error = new Error
                {
                    Message = user.Errors.FirstOrDefault()?.Description
                }
            };
        }

        [AllowAnonymous]
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _userManager.FindByNameAsync(request.Login);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new LoginResponse
                {
                    Error = new Error
                    {
                        Message = "User not found or password is incorrect"
                    }
                };
            }

            return new LoginResponse
            {
                UserInfo = new UserInfo
                {
                    Token = await user.GenerateJwtToken(_tokenParameters, _roleManager, _userManager),
                    Profile = new UserProfile
                    {
                        Name = user.ProfileName
                    }
                }
            };
        }

        public override async Task<UserInfoResponse> GetUserProfile(Empty request, ServerCallContext context)
        {
            var user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            if (user == null)
            {
                return new UserInfoResponse
                {
                    Error = new Error
                    {
                        Message = "No access"
                    }
                };
            }

            return new UserInfoResponse
            {
                Profile = new UserProfile
                {
                    Name = user.ProfileName
                },
            };
        }

        public override Task<KpiResponse> GetKpi(KpiRequest request, ServerCallContext context)
        {
            using var db = new ReadOnlyContext();

            var from = request.Period.From.ToDateTime();
            var to = request.Period.To.ToDateTime();

            var presale = db.Presales?
                .Where(p => p.Name == request.PresaleName)
                // .Include(p => p.Projects)
                .Include(p => p.Invoices.Where(i => i.Date >= from || i.LastPayAt >= from || i.LastShipmentAt >= from))?
                    .ThenInclude(i => i.Project)
                .Include(p => p.Invoices.Where(i => i.Date >= from || i.LastPayAt >= from || i.LastShipmentAt >= from))?
                    .ThenInclude(i => i.ProfitPeriods)
                .FirstOrDefault();

            if (presale == null)
                return Task.FromResult(new KpiResponse { Error = new Error { Message = "Пресейл не найден в базе данных." } });


            HashSet<Project> projects = new();
            RecursiveLoad(presale.Projects?.ToList(), db, ref projects);

            presale.SumProfit(from, to, out var invoices);

            if (invoices == null || !invoices.Any())
                return Task.FromResult(new KpiResponse());

            var reply = new Kpi();

            foreach (var invoice in invoices.OrderBy(i => (int)i.Counterpart[0]).ThenBy(i => i.Counterpart).ThenBy(i => i.Number))
            {
                HashSet<PresaleAction> actionsIgnored = new(), actionsTallied = new();
                HashSet<Database.Entities.Project> projectsIgnored = new(), projectsFound = new();

                var percent = invoice.Project?.Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound) switch
                {
                    int n when n >= 1 && n <= 3 => .004,
                    int n when n >= 4 && n <= 6 => .007,
                    int n when n >= 7 => .01,
                    _ => 0,
                };

                var profit = invoice.GetProfit(from, to);

                var invoiceReply = invoice.Translate();

                invoiceReply.Cost = DecimalValue.FromDecimal(invoice.Amount - profit);
                invoiceReply.SalesAmount = DecimalValue.FromDecimal(profit);
                invoiceReply.Percent = percent;
                invoiceReply.Profit = DecimalValue.FromDecimal(profit * (decimal)percent);

                foreach (var inv in projectsIgnored.OrderBy(p => p.Number))
                    invoiceReply.ProjectsIgnored.Add(inv.Translate());

                foreach (var inv in projectsFound.OrderBy(p => p.Number))
                    invoiceReply.ProjectsFound.Add(inv.Translate());

                foreach (var action in actionsIgnored.OrderBy(a => a.Project?.Number).ThenBy(a => a.Number))
                    invoiceReply.ActionsIgnored.Add(action.Translate());

                foreach (var action in actionsTallied.OrderBy(a => a.Project?.Number).ThenBy(a => a.Number))
                    invoiceReply.ActionsTallied.Add(action.Translate());

                reply.Invoices.Add(invoiceReply);
            }

            db.Dispose();
            return Task.FromResult(new KpiResponse { Kpi = reply });
        }

        public override Task<NamesResponse> GetNames(Empty request, ServerCallContext context)
        {
            using var db = new ReadOnlyContext();
            var presalesNames = db.Presales
                .Where(p => p.Department != Department.None)
                .Where(p => p.Position != Position.None && p.Position != Position.Director)
                .Where(p => p.IsActive)
                .Select(column => column.Name).ToList();

            var reply = new NamesResponse();
            foreach (var name in presalesNames) reply.Names.Add(name);

            db.Dispose();
            return Task.FromResult(reply);
        }

        public override Task<Overview> GetOverview(OverviewRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var position = request?.Position ?? Shared.Position.Any;
            var department = request?.Department ?? Shared.Department.Any;
            var onlyActive = request?.OnlyActive ?? false;

            using var db = new ReadOnlyContext();
            var presales = db.Presales
                .Where(p => ((position == Shared.Position.Any && p.Position != Position.None)
                    || (position != Shared.Position.Any && p.Position == position.Translate()))
                    && ((department == Shared.Department.Any && p.Department != Department.None)
                    || (department != Shared.Department.Any && p.Department == department.Translate())))
#pragma warning disable CS8604 // Possible null reference argument.
                .Include(p => p.Projects.Where(p => p != null)).ThenInclude(p => p.PresaleActions)
                .Include(p => p.Invoices.Where(i => (i.Date >= from && i.Date <= to)
                    || (i.LastPayAt >= from && i.LastPayAt <= to)
                    || (i.LastShipmentAt >= from && i.LastShipmentAt <= to))).ThenInclude(i => i.ProfitPeriods)
#pragma warning restore CS8604 // Possible null reference argument.
                .ToList();

            List<Database.Entities.Project> projects = new();
            foreach (var presale in presales)
                if (presale.Projects != null)
                    projects.AddRange(presale.Projects);

            var otherProjects = db.Projects.Where(p => p.Presale == null).ToList();
            if (otherProjects != null)
                projects.AddRange(otherProjects);

            decimal maxPotential = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)?
                .Sum(p => p.PotentialAmount)) ?? 0;
            int maxCount = presales.Max(p => p.Projects?
                .Where(p => p.Presale?.Department == Department.Russian)?
                .Where(p => p.Presale?.Position == Position.Engineer)?
                .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)?
                .Count()) ?? 0;

            int won, assign;
            var reply = new Overview();
            #region Метрики пресейла
            foreach (var presale in presales)
            {
                if (onlyActive && !presale.IsActive) continue;
                var isRuEngineer = presale.Department == Department.Russian
                                && presale.Position == Position.Engineer;
                won = presale.ClosedByStatus(ProjectStatus.Won, from, to);
                assign = presale.CountProjectsAssigned(from, to);

                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = new Statistic()
                    {
                        #region Показатели этого периода
                        #region В работе
                        InWork = presale.CountProjectsInWork(from, to),
                        #endregion
                        #region Назначено
                        Assign = assign,
                        #endregion
                        #region Выиграно
                        Won = won,
                        #endregion
                        #region Проиграно
                        Loss = presale.ClosedByStatus(ProjectStatus.Loss, from, to),
                        #endregion
                        #region Конверсия
                        Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                        #endregion
                        #region Среднее время реакции
                        AvgTimeToReaction = Duration.FromTimeSpan(presale.AverageTimeToReaction(from, to)),
                        #endregion
                        #region Суммарное потраченное время на проекты
                        SumSpend = Duration.FromTimeSpan(presale.SumTimeSpend(from, to)),
                        #endregion
                        #region Cреднее время потраченное на проект
                        AvgSpend = Duration.FromTimeSpan(presale.AverageTimeSpend(from, to)),
                        #endregion
                        #region Чистые
                        Profit = presale.SumProfit(from, to),
                        #endregion
                        #region Потенциал
                        Potential = presale.SumPotential(from, to),
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
                    },
                    #region Недостаток проектов
                    DeficitProjects = isRuEngineer ? maxCount - presale.CountProjectsAssigned(from, to) : 0,
                    #endregion
                    #region Недостаток потенциала
                    DeficitPotential = isRuEngineer ? maxPotential - presale.SumPotential(from, to) : 0,
                    #endregion
                    Department = Extensions.Translate(presale.Department),
                    Position = Extensions.Translate(presale.Position),
                    IsActive = presale.IsActive,
                });
            }
            #endregion
            #region Метрики службы

            won = presales.Where(p => p.Position == Position.Engineer
                                       || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Won, from, to));
            assign = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAssigned(from, to));

            reply.Statistics = new Statistic()
            {
                #region Показатели этого периода
                #region В работе
                InWork = presales.Where(p => p.Position == Position.Engineer
                                          || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsInWork(from, to)),
                #endregion
                #region Назначено
                Assign = assign,
                #endregion
                #region Выиграно
                Won = won,
                #endregion
                #region Проиграно
                Loss = presales.Where(p => p.Position == Position.Engineer
                                        || p.Position == Position.Account)
                    .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, from, to)),
                #endregion
                #region Конверсия
                Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
                #endregion
                #region Среднее время реакции
                AvgTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageTimeToReaction(from, to).TotalMinutes ?? 0) ?? 0)),
                #endregion
                #region Суммарное потраченное время на проекты
                SumSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                  || p.Position == Position.Account)
                        .Sum(p => p.SumTimeSpend(from, to).TotalMinutes) ?? 0)),
                #endregion
                #region Cреднее время потраченное на проект
                AvgSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?.Where(p => p.Position == Position.Engineer
                                                                     || p.Position == Position.Account)
                        .Sum(p => p.AverageTimeSpend(from, to).TotalMinutes) ?? 0)),
                #endregion
                #region Чистые
                Profit = presales?.Where(p => p.Department != Department.None)
                    .Sum(p => p.SumProfit(from, to)) ?? 0,
                #endregion
                #region Потенциал
                Potential = presales?.Where(p => p.Department != Department.None)
                    .Sum(p => p.SumPotential(from, to)) ?? 0,
                #endregion
                #endregion
                #region Среднее время жизни проекта до выигрыша
                AvgTimeToWin = Duration.FromTimeSpan(TimeSpan.FromDays(presales?
                    .Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageTimeToWin().TotalDays ?? 0) ?? 0)),
                #endregion
                #region Средний ранг проектов
                AvgRank = presales?.Where(p => p.Position == Position.Engineer
                             || p.Position == Position.Account)?
                    .DefaultIfEmpty()
                    .Average(p => p?.AverageRank() ?? 0) ?? 0,
                #endregion
                #region Количество "брошенных" проектов
                Abnd = presales?.Where(p => p.Position == Position.Engineer
                                             || p.Position == Position.Account)
                    .Sum(p => p.CountProjectsAbandoned(DateTime.UtcNow, 30)) ?? 0,
                #endregion
            };
            #region Просроченные проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)
                    .Where(p => p.IsOverdue()))
                reply.Escalations.Add(proj.Translate());
            #endregion
            #region Забытые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)
                    .Where(p => p.IsForgotten()))
                reply.Forgotten.Add(proj.Translate());
            #endregion
            #region Новые проекты
            foreach (var proj in projects
                    .Where(p => p.ApprovalBySalesDirectorAt >= from && p.ApprovalBySalesDirectorAt <= to)
                    .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue))
                reply.New.Add(proj.Translate());
            #endregion
            #region Среднее время реакции руководителя
            reply.AvgDirectorTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(projects?
                .Where(p => p.ApprovalBySalesDirectorAt >= from && p.ApprovalBySalesDirectorAt <= to)?
                .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue)?
                .DefaultIfEmpty()
                .Average(p => p?.TimeToDirectorReaction().TotalMinutes ?? 0) ?? 0));
            #endregion
            #endregion

            db.Dispose();
            return Task.FromResult(reply);
        }

        [AllowAnonymous]
        public override Task<ImageResponse> GetImage(ImageRequest request, ServerCallContext context)
        {
            try
            {
                var clientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    SslProtocols = SslProtocols.Tls12
                };
                var unsplashClient = new HttpClient(clientHandler) { BaseAddress = new Uri("https://api.unsplash.com") };
                var unsplashRequest = new HttpRequestMessage(HttpMethod.Get, $"photos/random?query={request.Keyword}&orientation={request.Orientation.ToString().ToLower()}");
                unsplashRequest.Headers.Add("Authorization", "Client-ID zoKHly26A5L5BCYWXdctm0hc9u5JGaqcsMv_znpsIR0");
                var unsplashResponse = unsplashClient.SendAsync(unsplashRequest).Result;
                if (!unsplashResponse.IsSuccessStatusCode) return Task.FromResult(_cashedImageGirl);
                var response = JsonConvert.DeserializeObject<dynamic>(unsplashResponse.Content.ReadAsStringAsync().Result);
                if (response == null) return request.Keyword switch
                {
                    "happy new year" => Task.FromResult(_cashedImageNY),
                    _ => Task.FromResult(_cashedImageGirl)
                };
                var _img = new ImageResponse
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

        public override Task<MonthProfitOverview> GetMonthProfitOverview(OverviewRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var position = request?.Position ?? Shared.Position.Any;
            var department = request?.Department ?? Shared.Department.Any;
            var onlyActive = request?.OnlyActive ?? false;

            using var db = new ReadOnlyContext();
            var presales = db.Presales?
                .Where(p => ((position == Shared.Position.Any && p.Position != Position.None)
                    || (position != Shared.Position.Any && p.Position == position.Translate()))
                    && ((department == Shared.Department.Any && p.Department != Department.None)
                    || (department != Shared.Department.Any && p.Department == department.Translate())))
                .ToList();

            _ = db.Invoices
                .Where(i => (i.Date >= from && i.Date <= to)
                    || (i.LastPayAt >= from && i.LastPayAt <= to)
                    || (i.LastShipmentAt >= from && i.LastShipmentAt <= to))
                .Include(i => i.Presale)
                .Include(i => i.ProfitPeriods).ToList();

            var reply = new MonthProfitOverview();
            if (presales == null) return Task.FromResult(reply);

            foreach (var presale in presales)
            {
                if (onlyActive && !presale.IsActive) continue;
                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = new Statistic()
                    {
                        Profit = DecimalValue.FromDecimal(presale.SumProfit(from, to))
                    }
                });
            }

            var profit = presales?.Sum(p => p.SumProfit(from, to)) ?? 0;
            var profitPrevDay = presales?.Sum(p => p.SumProfit(from, to.AddDays(-1))) ?? 0;

            reply.Profit = profit;
            reply.Plan = plan;
            reply.Left = plan - profit > 0 ? plan - profit : 0;
            reply.DeltaDay = profit - profitPrevDay > 0 ? profit - profitPrevDay : 0;

            db.Dispose();
            return Task.FromResult(reply);
        }

        public override Task<SalesOverview> GetSalesOverview(SalesOverviewRequest request, ServerCallContext context)
        {
            var prevStartTime = request?.Previous?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var prevEndTime = request?.Previous?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var startTime = request?.Current?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var endTime = request?.Current?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://127.0.0.1") };
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"trade/hs/API/GetTradeData?" +
                $"begin={prevStartTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&end={prevEndTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&begin2={startTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&end2={endTime:yyyy-MM-ddTHH:mm:ss}");
            httpRequest.Headers.Add("Authorization", "Basic 0J/QvtC70Y/QutC+0LLQmDpraDk5OFh0Rg==");
            var httpResponse = httpClient.SendAsync(httpRequest).Result;
            var result = httpResponse.Content.ReadAsStringAsync().Result;
            if (!httpResponse.IsSuccessStatusCode) result = _cachedOverview;
            _cachedOverview = result;

            var response = JsonConvert.DeserializeObject<dynamic>(_cachedOverview);
            var reply = new SalesOverview();

            if (response != null)
            {
                foreach (var manager in response.Топ)
                    reply.CurrentTopSalesManagers.Add(new Manager { Name = string.Join(" ", ((string)manager.Имя).Split().Take(2)), Profit = (decimal)manager.Сумма });

                reply.PreviousActualProfit = response.Факт1 is null ? 0 : (decimal)response.Факт1;
                reply.CurrentActualProfit = response.Факт2 is null ? 0 : (decimal)response.Факт2;
                reply.CurrentSalesTarget = response.План2 is null ? 0 : (decimal)response.План2;
            }

            return Task.FromResult(reply);
        }

        public override Task<UnpaidProjects> GetUnpaidProjects(UnpaidRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var is_main_project_include = request?.IsMainProjectInclude ?? false;
            var presale_name = request?.PresaleName ?? string.Empty;

            using var db = new ReadOnlyContext();

            List<Database.Entities.Presale>? presales;

            if (is_main_project_include)
#pragma warning disable CS8604 // Possible null reference argument.
                presales = db.Presales
                    .Where(p => presale_name != string.Empty ? p.Name == presale_name : p.IsActive == true)
                    .Include(p => p.Projects.Where(p => p.Status == ProjectStatus.Won
                                                        && p.ClosedAt >= from && p.ClosedAt <= to
                                                        && !p.Invoices.Any(i => i.ProfitPeriods.Any())
                                                        && p.MainProject != null && !p.MainProject.Invoices.Any(i => i.ProfitPeriods.Any())
                    )).ToList();
            else
                presales = db.Presales
                    .Where(p => presale_name != string.Empty ? p.Name == presale_name : p.IsActive == true)
                    .Include(p => p.Projects.Where(p => p.Status == ProjectStatus.Won
                                                        && p.ClosedAt >= from && p.ClosedAt <= to
                                                        && !p.Invoices.Any(i => i.ProfitPeriods.Any())
                    )).ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            var reply = new UnpaidProjects();

            foreach (var presale in presales)
#pragma warning disable CS8604 // Possible null reference argument.
                foreach (var project in presale.Projects.OrderBy(p => p.ClosedAt).ThenBy(p => p.Number))
#pragma warning restore CS8604 // Possible null reference argument.
                    reply.Projects.Add(project.Translate());

            db.Dispose();
            return Task.FromResult(reply);
        }

        private static void RecursiveLoad(List<Project>? projects, ReadOnlyContext db, ref HashSet<Project> projectsViewed)
        {
            if (projects == null || projects.Count == 0) return;
            foreach (var project in projects)
            {
                if (project == null) continue;
                if (projectsViewed.Contains(project)) continue;
                else projectsViewed.Add(project);

                var some = db.PresaleActions?.Where(a => a.Project.Number == project.Number)?.ToList();

                var prj = db.Projects?.Where(p => p.Number == project.Number)?.Include(p => p.MainProject).FirstOrDefault();
                if (prj?.MainProject?.Number != null)
                {
                    var mainPrjs = db.Projects?.Where(p => p.Number == prj.MainProject.Number)?.ToList();
                    RecursiveLoad(mainPrjs, db, ref projectsViewed);
                }
            }
        }
    }

    public static class Extensions
    {
        public static Shared.Project Translate(this Database.Entities.Project project)
        {
            var proj = new Shared.Project()
            {
                Number = project.Number,
                Name = project.Name ?? "",
                ApprovalByTechDirectorAt = Timestamp.FromDateTime(project.ApprovalByTechDirectorAt.ToUniversalTime()),
                ApprovalBySalesDirectorAt = Timestamp.FromDateTime(project.ApprovalBySalesDirectorAt.ToUniversalTime()),
                PresaleStartAt = Timestamp.FromDateTime(project.PresaleStartAt.ToUniversalTime()),
                ClosedAt = Timestamp.FromDateTime(project.ClosedAt.ToUniversalTime()),
                PresaleName = project.Presale?.Name ?? "",
                Status = project.Status.Translate(),
            };

            if (project.Invoices != null && project.Invoices.Any()) foreach (var invoice in project.Invoices) proj.Invoices.Add(invoice.Translate());
            return proj;
        }

        public static Shared.Invoice Translate(this Database.Entities.Invoice invoice)
        {
            var inv = new Shared.Invoice
            {
                Counterpart = invoice.Counterpart,
                Number = invoice.Number,
                Date = Timestamp.FromDateTime(invoice.Date.ToUniversalTime()),
                LastPayAt = Timestamp.FromDateTime(invoice.LastPayAt.ToUniversalTime()),
                LastShipmentAt = Timestamp.FromDateTime(invoice.LastShipmentAt.ToUniversalTime()),
                Amount = invoice.Amount,
                Profit = invoice.GetProfit(),
            };

            return inv;
        }

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

        public static Department Translate(this Shared.Department value)
        {
            if (value == Shared.Department.Any) return Department.None;
            return (Department)Enum.Parse(typeof(Department), value.ToString());
        }

        public static Shared.Department Translate(this Department value) =>
            (Shared.Department)Enum.Parse(typeof(Shared.Department), value.ToString());

        public static Position Translate(this Shared.Position value)
        {
            if (value == Shared.Position.Any) return Position.None;
            return (Position)Enum.Parse(typeof(Position), value.ToString());
        }

        public static Shared.Position Translate(this Position value) =>
            (Shared.Position)Enum.Parse(typeof(Shared.Position), value.ToString());

        public static Shared.ProjectStatus Translate(this ProjectStatus value) =>
            (Shared.ProjectStatus)Enum.Parse(typeof(Shared.ProjectStatus), value.ToString());

        public static Shared.ActionType Translate(this ActionType value) =>
            (Shared.ActionType)Enum.Parse(typeof(Shared.ActionType), value.ToString());
    }
}
