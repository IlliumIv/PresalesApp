﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Shared;
using System.Security.Authentication;
using System.Security.Cryptography;
using Enum = System.Enum;
using Position = PresalesApp.Database.Enums.Position;
using Department = PresalesApp.Database.Enums.Department;
using ActionType = PresalesApp.Database.Enums.ActionType;
using FunnelStage = PresalesApp.Database.Enums.FunnelStage;
using ProjectStatus = PresalesApp.Database.Enums.ProjectStatus;
using Project = PresalesApp.Database.Entities.Project;
using Presale = PresalesApp.Database.Entities.Presale;
using Invoice = PresalesApp.Database.Entities.Invoice;
using static PresalesApp.Database.DbController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Google.Protobuf;
using PresalesApp.Database.Enums;
using PresalesApp.Web.Server.Authorization;
using PresalesApp.Web.Authorization;
using PresalesApp.Web.Server;
using AppApi = PresalesApp.Web.Shared.Api;

namespace PresalesApp.Web.Controllers
{
    [Authorize]
    public class ApiController : AppApi.ApiBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly TokenParameters _tokenParameters;
        private readonly ILogger<ApiController> _logger;

        private readonly decimal plan = 8163862;
        private string _cachedOverview = @"{ ""Всего"": 0.0, ""Топ"": [ { ""Имя"": ""Захаров Юрий"", ""Сумма"": 0.0 }, { ""Имя"": ""Вради Константин Вячеславович"", ""Сумма"": 0.0 }, { ""Имя"": ""Юрова Марина Александровна"", ""Сумма"": 0.0 }, { ""Имя"": ""Ширинкин Андрей"", ""Сумма"": 0.0 }, { ""Имя"": ""Шишкина Анна Сергеевна"", ""Сумма"": 0.0 }, { ""Имя"": ""Насыров Марат"", ""Сумма"": 0.0 }, { ""Имя"": ""Копьев Олег"", ""Сумма"": 0.0 }, { ""Имя"": ""Окулов Дмитрий"", ""Сумма"": 0.0 } ] }";
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

        public ApiController(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            TokenParameters tokenParameters,
            ILogger<ApiController> logger)
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

        public override async Task<KpiResponse> GetKpi(KpiRequest request, ServerCallContext context)
        {
            using var db = new ReadOnlyContext();

            var from = request.Period.From.ToDateTime();
            var to = request.Period.To.ToDateTime();

            var invQuery = db.Invoices
                .Where(i => i.Date >= from || i.LastPayAt >= from || i.LastShipmentAt >= from)
                .Where(i => i.Presale.Name == request.PresaleName)
                .Include(i => i.Project)
                .Include(i => i.ProfitPeriods);
            await invQuery.LoadAsync();

            var presale = db.Presales
                .Where(p => p.Name == request.PresaleName).SingleOrDefault();

            if (presale == null)
                return new KpiResponse { Error = new Error { Message = "Пресейл не найден в базе данных." } };


            HashSet<Project> projects = new();
            RecursiveLoad(presale.Projects?.ToList(), db, ref projects);

            presale.SumProfit(from, to, out var invoices);

            if (invoices == null || !invoices.Any())
                return new KpiResponse();

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
            return new KpiResponse { Kpi = reply };
        }

        public override async Task<NamesResponse> GetNames(Empty request, ServerCallContext context)
        {
            using var db = new ReadOnlyContext();
            var presalesNames = await db.Presales
                .Where(p => p.Department != Department.None)
                .Where(p => p.Position != Position.None && p.Position != Position.Director)
                .Where(p => p.IsActive)
                .Select(column => column.Name).ToListAsync();

            var reply = new NamesResponse();
            foreach (var name in presalesNames) reply.Names.Add(name);

            db.Dispose();
            return reply;
        }

        public override async Task<Overview> GetOverview(OverviewRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var position = request?.Position ?? Shared.Position.Any;
            var department = request?.Department ?? Shared.Department.Any;
            var onlyActive = request?.OnlyActive ?? false;

            using var db = new ReadOnlyContext();

            var projQuery = db.Projects.Where(p => p != null)
                .Where(p => ((position == Shared.Position.Any && p.Presale.Position != Position.None)
                    || (position != Shared.Position.Any && p.Presale.Position == position.Translate()))
                    && ((department == Shared.Department.Any && p.Presale.Department != Department.None)
                    || (department != Shared.Department.Any && p.Presale.Department == department.Translate())))
                .Include(p => p.PresaleActions);

            var invQuery = db.Invoices.Where(i => (i.Date >= from && i.Date <= to)
                    || (i.LastPayAt >= from && i.LastPayAt <= to)
                    || (i.LastShipmentAt >= from && i.LastShipmentAt <= to))
                .Where(i => ((position == Shared.Position.Any && i.Presale.Position != Position.None)
                    || (position != Shared.Position.Any && i.Presale.Position == position.Translate()))
                    && ((department == Shared.Department.Any && i.Presale.Department != Department.None)
                    || (department != Shared.Department.Any && i.Presale.Department == department.Translate())))
                .Include(i => i.ProfitPeriods);

            await projQuery.LoadAsync();
            await invQuery.LoadAsync();

            var presales = db.Presales
                .Where(p => ((position == Shared.Position.Any && p.Position != Position.None)
                    || (position != Shared.Position.Any && p.Position == position.Translate()))
                    && ((department == Shared.Department.Any && p.Department != Department.None)
                    || (department != Shared.Department.Any && p.Department == department.Translate())))
                .ToList();

            List<Project> projects = new();
            foreach (var presale in presales)
                if (presale.Projects != null)
                    projects.AddRange(presale.Projects);

            var otherProjects = db.Projects.Where(p => p.Presale == null).ToList();
            if (otherProjects != null)
                projects.AddRange(otherProjects);

            int won, assign;
            var reply = new Overview();
            #region Метрики пресейла
            foreach (var presale in presales)
            {
                if (onlyActive && !presale.IsActive) continue;

                reply.Presales.Add(new Shared.Presale()
                {
                    Name = presale.Name,
                    Statistics = presale.GetStatistic(from, to),
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
            return reply;
        }

        [AllowAnonymous]
        public override async Task<ImageResponse> GetImage(ImageRequest request, ServerCallContext context)
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
                var unsplashResponse = await unsplashClient.SendAsync(unsplashRequest);
                if (!unsplashResponse.IsSuccessStatusCode) return _cashedImageGirl;
                var response = JsonConvert.DeserializeObject<dynamic>(await unsplashResponse.Content.ReadAsStringAsync());
                if (response == null) return request.Keyword switch
                {
                    "happy new year" => _cashedImageNY,
                    _ => _cashedImageGirl
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
                return _img;
            }
            catch
            {
                return _cashedImageGirl;
            }
        }

        public override async Task<MonthProfitOverview> GetMonthProfitOverview(OverviewRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var position = request?.Position ?? Shared.Position.Any;
            var department = request?.Department ?? Shared.Department.Any;
            var onlyActive = request?.OnlyActive ?? false;

            using var db = new ReadOnlyContext();

            var invQuery = db.Invoices
                .Where(i => (i.Date >= from && i.Date <= to)
                    || (i.LastPayAt >= from && i.LastPayAt <= to)
                    || (i.LastShipmentAt >= from && i.LastShipmentAt <= to))
                .Include(i => i.Presale)
                .Include(i => i.ProfitPeriods);

            await invQuery.LoadAsync();

            var presales = db.Presales?
                .Where(p => ((position == Shared.Position.Any && p.Position != Position.None)
                    || (position != Shared.Position.Any && p.Position == position.Translate()))
                    && ((department == Shared.Department.Any && p.Department != Department.None)
                    || (department != Shared.Department.Any && p.Department == department.Translate())))
                .ToList();

            var reply = new MonthProfitOverview();
            if (presales == null) return reply;

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
            return reply;
        }

        public override async Task<SalesOverview> GetSalesOverview(SalesOverviewRequest request, ServerCallContext context)
        {
            var prevStartTime = request?.Previous?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var prevEndTime = request?.Previous?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var startTime = request?.Current?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
            var endTime = request?.Current?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://server0") };
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"trade/hs/API/GetTradeData?" +
                $"begin={prevStartTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&end={prevEndTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&begin2={startTime:yyyy-MM-ddTHH:mm:ss}" +
                $"&end2={endTime:yyyy-MM-ddTHH:mm:ss}");
            httpRequest.Headers.Add("Authorization", "Basic 0J/QvtC70Y/QutC+0LLQmDpraDk5OFh0Rg==");
            var httpResponse = await httpClient.SendAsync(httpRequest);
            var result = await httpResponse.Content.ReadAsStringAsync();
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

            return reply;
        }

        public override async Task<UnpaidProjects> GetUnpaidProjects(UnpaidRequest request, ServerCallContext context)
        {
            var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
            var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
            var is_main_project_include = request?.IsMainProjectInclude ?? false;
            var presale_name = request?.PresaleName ?? string.Empty;

            using var db = new ReadOnlyContext();

            if (is_main_project_include)
            {
                await db.Projects
                    .Where(p => p.Status == ProjectStatus.Won && p.ClosedAt >= from && p.ClosedAt <= to &&
                        !p.Invoices.Any(i => i.ProfitPeriods.Any()) && p.MainProject != null &&
                        !p.MainProject.Invoices.Any(i => i.ProfitPeriods.Any()))
                    .Where(p => presale_name != string.Empty ? p.Presale.Name == presale_name : p.Presale.IsActive == true).LoadAsync();
            }
            else
            {
                await db.Projects
                    .Where(p => p.Status == ProjectStatus.Won && p.ClosedAt >= from && p.ClosedAt <= to &&
                        !p.Invoices.Any(i => i.ProfitPeriods.Any()))
                    .Where(p => presale_name != string.Empty ? p.Presale.Name == presale_name : p.Presale.IsActive == true).LoadAsync();
            }

            var presales = await db.Presales.Where(p => presale_name != string.Empty ? p.Name == presale_name : p.IsActive == true).ToListAsync();
            var reply = new UnpaidProjects();

            foreach (var presale in presales)
            {
                if (!presale.Projects?.Any() ?? true) continue;
                foreach (var project in presale.Projects.OrderBy(p => p.ClosedAt).ThenBy(p => p.Number))
                    reply.Projects.Add(project.Translate());
            }

            db.Dispose();
            return reply;
        }

        public override async Task<FunnelProjects> GetFunnelProjects(Empty request, ServerCallContext context)
        {
            using var db = new ReadOnlyContext();

            var projects = await db.Projects
                .Where(p => p.Status == ProjectStatus.WorkInProgress)
                .Where(p => p.Presale.Department == Department.Russian)
                .Where(p => p.PresaleActions.Any(a => a.SalesFunnel) ||
                    (p.PotentialAmount > 2000000 && p.ApprovalBySalesDirectorAt > new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
                .Include(p => p.PresaleActions)
                .Include(p => p.Presale)
                .ToListAsync();

            var reply = new FunnelProjects();

            foreach(var project in projects)
            {
                reply.Projects.Add(project.Translate());
            }

            db.Dispose();
            return reply;
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
}