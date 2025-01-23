using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PresalesApp.Database.Entities;
using PresalesApp.Shared;
using PresalesApp.Database.Helpers;
using PresalesApp.CustomTypes;
using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using OdataToEntity.EfCore;
using OdataToEntity;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using PresalesApp.Extensions;

using Department = PresalesApp.Database.Enums.Department;
using Position = PresalesApp.Database.Enums.Position;
using Project = PresalesApp.Database.Entities.Project;
using Presale = PresalesApp.Database.Entities.Presale;
using ProjectStatus = PresalesApp.Database.Enums.ProjectStatus;

using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Services;

[Authorize]
public class PresalesAppService(ILogger<PresalesAppService> logger)
    : PresalesApp.Shared.PresalesAppService.PresalesAppServiceBase
{
    private readonly ILogger<PresalesAppService> _Logger = logger;

    #region Private Static

    #region Members

    private const decimal _Handicap = (decimal)1.3;

    private static readonly Dictionary<(DateTime Start, DateTime End),
                                       (decimal Actual, decimal Target, DateTime CalculationTime)>
        _SalesTargetCache = new()
        {
            {
                (new(2023, 10, 1, 0, 0, 0), new(2023, 12, 31, 23, 59, 59)),
                (Actual: 120_000_000, Target: 150_000_000, CalculationTime: DateTime.Now)
            }
        };

    private static string _CachedOverview =
        @"{ ""Всего"": 0.0,
            ""Топ"": [
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
                { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 }
        ] }";

    #endregion

    #region Methods

    private static void _RecursiveLoad(List<Project>? projects,
        ReadOnlyContext db, ref HashSet<Project> projectsViewed)
    {
        if(projects == null || projects.Count == 0)
        {
            return;
        }

        foreach(var project in projects)
        {
            if(project == null)
            {
                continue;
            }

            if(!projectsViewed.Add(project))
            {
                continue;
            }

            db.PresaleActions?.Where(a => a.Project!.Number == project.Number)?.ToList();

            var prj = db.Projects?.Where(p => p.Number == project.Number)?
                        .Include(p => p.MainProject).FirstOrDefault();

            if(prj?.MainProject?.Number != null)
            {
                var mainPrjs = db.Projects?.Where(p => p.Number == prj.MainProject.Number)?.ToList();
                _RecursiveLoad(mainPrjs, db, ref projectsViewed);
            }
        }
    }

    private static double _GetPercent(int rank, DateTime from, Presale? presale, KpiCalculation calculationType)
        => _ShouldFitOrder26(from, presale, calculationType)
        ? rank switch
        {
            int n when n is >= 1 and <= 3 => .003,
            int n when n is >= 4 and <= 6 => .005,
            int n when n >= 7 => .007,
            _ => 0,
        }
        : rank switch
        {
            int n when n is >= 1 and <= 3 => .004,
            int n when n is >= 4 and <= 6 => .007,
            int n when n >= 7 => .01,
            _ => 0,
        };

    private static bool _ShouldFitOrder26(DateTime from, Presale? presale, KpiCalculation calculationType)
        => calculationType switch
        {
            KpiCalculation.Default =>
                (presale is not null
                    && presale.Name.Contains("Латышев")
                    && presale.Name.Contains("Никита"))
                || from >= new DateTime(2024, 11, 1).ToUniversalTime(),
            KpiCalculation.PreOrder26 => false,
            KpiCalculation.PostOrder26 => true,
            _ => throw new NotImplementedException()
        };

    private static async Task<(Dictionary<DateTime, decimal> Profit,
                               PresalesApp.Shared.Presale[] Presales)>
        _GetProfitStatistic(DateTime from, DateTime to,
                            PresalesApp.Shared.Position position,
                            PresalesApp.Shared.Department department,
                            bool onlyActive)
    {
        using var db = new ReadOnlyContext();

        var invQuery = db.Invoices
            .Where(i => !i.MarkedAsDeleted && ((i.Date >= from && i.Date <= to)
                || (i.LastPayAt >= from && i.LastPayAt <= to)
                || (i.LastShipmentAt >= from && i.LastShipmentAt <= to)))
            .Include(i => i.Presale)
            .Include(i => i.ProfitPeriods);

        await invQuery.LoadAsync();

        var presalesFromDb = db.Presales?
            .Where(p => ((position == PresalesApp.Shared.Position.Any && p.Position != Position.None)
            || (position != PresalesApp.Shared.Position.Any && p.Position == position.Translate()))
                && ((department == PresalesApp.Shared.Department.Any && p.Department != Department.None)
                || (department != PresalesApp.Shared.Department.Any && p.Department == department.Translate())))
            .ToList();

        var filteredPresales = new HashSet<PresalesApp.Shared.Presale>();

        if(presalesFromDb != null)
        {
            foreach(var presale in presalesFromDb)
            {
                var presaleProfit = presale.SumProfit(from, to);
                if((onlyActive && !presale.IsActive) || presaleProfit == 0)
                {
                    continue;
                }

                filteredPresales.Add(new()
                {
                    Name = presale.Name,
                    Metrics = presale.GetMetrics(from, to)
                });
            }
        }

        Dictionary<DateTime, decimal> profit = [];

        foreach(var day in _EachDay(from, to))
        {
            decimal p = presalesFromDb?.Sum(p => p.SumProfit(day, day.AddDays(1))) ?? 0;
            if(p != 0 || day.AddHours(5).IsBusinessDayUTC())
            {
                profit.Add(day, presalesFromDb?.Sum(p => p.SumProfit(day, day.AddDays(1))) ?? 0);
            }
        }

        db.Dispose();

        return (profit, Presales: filteredPresales.ToArray());
    }

    private static IEnumerable<DateTime> _EachDay(DateTime from, DateTime to)
    {
        for(var day = from; day <= to; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    #endregion

    #endregion

    #region Service Implementation

    public override async Task<GetKpiResponse>
        GetKpi (GetKpiRequest request, ServerCallContext context)
    {
        using var db = new ReadOnlyContext();

        var from = request.Period.From.ToDateTime();
        var to = request.Period.To.ToDateTime();

        var invQuery = db.Invoices
            .Where(i => i.Date >= from || i.LastPayAt >= from || i.LastShipmentAt >= from)
            .Where(i => i.Presale!.Name == request.PresaleName)
            .Include(i => i.Project)
            .Include(i => i.ProfitPeriods);
        await invQuery.LoadAsync();

        var presale = db.Presales
            .Where(p => p.Name == request.PresaleName).SingleOrDefault();

        if(presale == null)
        {
            return new()
            {
                Error = new()
                {
                    Message = "Пресейл не найден в базе данных."
                }
            };
        }

        HashSet<Project> projects = [];
        _RecursiveLoad(presale.Projects?.ToList(), db, ref projects);

        presale.SumProfit(from, to, out var invoices);

        if(invoices == null || invoices.Count == 0)
        {
            return new();
        }

        var response = new Kpi();

        Dictionary<Project, decimal> ограничитель_дохуя_большой_зарплаты = [];

        foreach(var invoice in invoices.OrderBy(i => (int)i.Counterpart[0])
                                       .ThenBy(i => i.Counterpart).ThenBy(i => i.Number))
        {
            HashSet<PresaleAction> actionsIgnored = [], actionsTallied = [];
            HashSet<Project> projectsIgnored = [], projectsFound = [];

            var startAt = invoice.Project?.PresaleStartAt is null
                ? DateTime.MinValue
                : invoice.Project.PresaleStartAt;

            var dtToCompare = startAt > DateTime.MinValue.AddDays(180)
                ? startAt.AddDays(-180)
                : DateTime.MinValue;

            var rank = invoice.Project is null
                ? 0
                : invoice.Project.Rank(ref actionsIgnored, ref actionsTallied,
                                       ref projectsIgnored, ref projectsFound, dtToCompare);
            
            var percent = _GetPercent(rank, from, invoice.Presale, request.KpiCalculation);
            var profit = invoice.GetProfit(from, to);
            var invoiceReply = invoice.Translate();

            invoiceReply.Cost = DecimalValue.FromDecimal(invoice.Amount - profit);
            invoiceReply.SalesAmount = DecimalValue.FromDecimal(profit);
            invoiceReply.Percent = percent;
            var invoiceSalary = profit * (decimal)percent;

            if(invoice.Project is not null)
            {
                if(!ограничитель_дохуя_большой_зарплаты.ContainsKey(invoice.Project))
                    ограничитель_дохуя_большой_зарплаты.Add(invoice.Project, 0);
            }

            if(invoice.Project is not null
                && ограничитель_дохуя_большой_зарплаты
                    .TryGetValue(invoice.Project, out var projSalary))
            {
                ограничитель_дохуя_большой_зарплаты[invoice.Project]
                    = ограничитель_дохуя_большой_зарплаты[invoice.Project] + invoiceSalary;

                var слишком_дохуя = ограничитель_дохуя_большой_зарплаты[invoice.Project] > 100_000;

                if(слишком_дохуя && _ShouldFitOrder26(from, invoice.Presale, request.KpiCalculation))
                {
                    invoiceSalary -= ограничитель_дохуя_большой_зарплаты[invoice.Project] - 100_000;
                    invoiceSalary = invoiceSalary < 0 ? 0 : invoiceSalary;
                }
            }

            invoiceReply.Profit = DecimalValue.FromDecimal(invoiceSalary);
            invoiceReply.Rank = rank;

            foreach(var inv in projectsIgnored.OrderBy(p => p.Number))
            {
                invoiceReply.ProjectsIgnored.Add(inv.Translate());
            }

            foreach(var inv in projectsFound.OrderBy(p => p.Number))
            {
                invoiceReply.ProjectsFound.Add(inv.Translate());
            }

            foreach(var action in actionsIgnored.OrderBy(a => a.Project?.Number).ThenBy(a => a.Number))
            {
                invoiceReply.ActionsIgnored.Add(action.Translate());
            }

            foreach(var action in actionsTallied.OrderBy(a => a.Project?.Number).ThenBy(a => a.Number))
            {
                invoiceReply.ActionsTallied.Add(action.Translate());
            }

            response.Invoices.Add(invoiceReply);
        }

        db.Dispose();

        return new()
        {
            Kpi = response
        };
    }

    public override async Task<GetProfitOverviewResponse>
        GetProfitOverview (GetProfitOverviewRequest request, ServerCallContext context)
    {
        var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
        var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
        var position = request?.Position ?? PresalesApp.Shared.Position.Any;
        var department = request?.Department ?? PresalesApp.Shared.Department.Any;
        var onlyActive = request?.OnlyActive ?? false;

        var (profit, presales) = await _GetProfitStatistic(from, to, position, department, onlyActive);

        if(!_SalesTargetCache.TryGetValue((from, to), out var value)
            || value.CalculationTime.Month < DateTime.Now.Month
            || value.CalculationTime.Year < DateTime.Now.Year)
        {
            var calendar = new GregorianCalendar();

            var actual = _GetProfitStatistic(
                calendar.IsLeapDay(from.Year, from.Month, from.Day)
                    ? from.AddYears(-1).AddDays(1)
                    : from.AddYears(-1),
                calendar.IsLeapDay(to.Year, to.Month, to.Day)
                    ? to.AddDays(-1).AddYears(-1)
                    : to.AddYears(-1),
                position, department, onlyActive).Result.Profit.Values.Sum();

            _SalesTargetCache[(from, to)] = (Actual: actual, Target: actual * _Handicap,
                CalculationTime: DateTime.Now);
        }

        var response = new GetProfitOverviewResponse();

        foreach(var presale in presales)
        {
            response.Presales.Add(presale);
        }

        response.Plan = _SalesTargetCache[(from, to)].Target;
        response.Actual = _SalesTargetCache[(from, to)].Actual;
        response.Left = _SalesTargetCache[(from, to)].Target - profit.Values.Sum() > 0
            ? _SalesTargetCache[(from, to)].Target - profit.Values.Sum()
            : 0;

        foreach((var date, var amount) in profit)
        {
            response.Profit.Add(date.ToString(CultureInfo.InvariantCulture), amount);
        }

        return response;
    }

    public override async Task<GetOverviewResponse>
        GetOverview (GetProfitOverviewRequest request, ServerCallContext context)
    {
        var from = request?.Period?.From?.ToDateTime() ?? DateTime.MinValue;
        var to = request?.Period?.To?.ToDateTime() ?? DateTime.MaxValue;
        var position = request?.Position ?? PresalesApp.Shared.Position.Any;
        var department = request?.Department ?? PresalesApp.Shared.Department.Any;
        var onlyActive = request?.OnlyActive ?? false;

        using var db = new ReadOnlyContext();

        var projQuery = db.Projects // .Where(p => p != null)
            .Where(p => ((position == PresalesApp.Shared.Position.Any
                                      && p.Presale!.Position != Position.None)
                || (position != PresalesApp.Shared.Position.Any
                                && p.Presale!.Position == position.Translate()))
                && ((department == PresalesApp.Shared.Department.Any
                                   && p.Presale.Department != Department.None)
                || (department != PresalesApp.Shared.Department.Any
                                  && p.Presale.Department == department.Translate())))
            .Include(p => p.PresaleActions);

        var invQuery = db.Invoices.Where(i => (i.Date >= from && i.Date <= to)
                || (i.LastPayAt >= from && i.LastPayAt <= to)
                || (i.LastShipmentAt >= from && i.LastShipmentAt <= to))
            .Where(i => ((position == PresalesApp.Shared.Position.Any
                                      && i.Presale!.Position != Position.None)
                || (position != PresalesApp.Shared.Position.Any
                                && i.Presale!.Position == position.Translate()))
                && ((department == PresalesApp.Shared.Department.Any
                                   && i.Presale.Department != Department.None)
                || (department != PresalesApp.Shared.Department.Any
                                  && i.Presale.Department == department.Translate())))
            .Include(i => i.ProfitPeriods);

        await projQuery.LoadAsync();
        await invQuery.LoadAsync();

        var presales = db.Presales
            .Where(p => ((position == PresalesApp.Shared.Position.Any
                                      && p.Position != Position.None)
                || (position != PresalesApp.Shared.Position.Any
                                && p.Position == position.Translate()))
                && ((department == PresalesApp.Shared.Department.Any
                                   && p.Department != Department.None)
                || (department != PresalesApp.Shared.Department.Any
                                  && p.Department == department.Translate())))
            .ToList();

        List<Project> projects = [];
        foreach(var presale in presales)
        {
            if(presale.Projects != null)
            {
                projects.AddRange(presale.Projects);
            }
        }

        var otherProjects = db.Projects.Where(p => p.Presale == null).ToList();
        if(otherProjects != null)
        {
            projects.AddRange(otherProjects);
        }

        int won, assign;
        var response = new GetOverviewResponse();
        #region Метрики пресейла
        foreach(var presale in presales)
        {
            if(onlyActive && !presale.IsActive)
            {
                continue;
            }

            response.Presales.Add(new PresalesApp.Shared.Presale()
            {
                Name = presale.Name,
                Metrics = presale.GetMetrics(from, to),
                Department = presale.Department.Translate(),
                Position = presale.Position.Translate(),
                IsActive = presale.IsActive,
            });
        }
        #endregion
        #region Метрики службы

        won = presales.Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.ClosedByStatus(ProjectStatus.Won, from, to));
        assign = presales.Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.CountProjectsAssigned(from, to));

        response.Metrics = new()
        {
            #region Показатели этого периода
            #region В работе
            InWork = presales.Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.CountProjectsInWork(from, to)),
            #endregion
            #region Назначено
            Assign = assign,
            #endregion
            #region Выиграно
            Won = won,
            #endregion
            #region Проиграно
            Loss = presales.Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.ClosedByStatus(ProjectStatus.Loss, from, to)),
            #endregion
            #region Конверсия
            Conversion = won == 0 || assign == 0 ? 0 : won / (assign == 0 ? 0d : assign),
            #endregion
            #region Среднее время реакции
            AvgTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                .Where(p => p.Position is Position.Engineer or Position.Account)?
                .DefaultIfEmpty()
                .Average(p => p?.AverageTimeToReaction(from, to).TotalMinutes ?? 0) ?? 0)),
            #endregion
            #region Суммарное потраченное время на проекты
            SumSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                .Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.SumTimeSpend(from, to).TotalMinutes) ?? 0)),
            #endregion
            #region Cреднее время потраченное на проект
            AvgSpend = Duration.FromTimeSpan(TimeSpan.FromMinutes(presales?
                .Where(p => p.Position is Position.Engineer or Position.Account)
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
                .Where(p => p.Position is Position.Engineer or Position.Account)?
                .DefaultIfEmpty()
                .Average(p => p?.AverageTimeToWin().TotalDays ?? 0) ?? 0)),
            #endregion
            #region Средний ранг проектов
            AvgRank = presales?.Where(p => p.Position is Position.Engineer or Position.Account)?
                .DefaultIfEmpty()
                .Average(p => p?.AverageRank() ?? 0) ?? 0,
            #endregion
            #region Количество "брошенных" проектов
            Abnd = presales?.Where(p => p.Position is Position.Engineer or Position.Account)
                .Sum(p => p.CountProjectsAbandoned(DateTime.UtcNow, 30)) ?? 0,
            #endregion
        };
        #region Просроченные проекты
        foreach(var proj in projects
                .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)
                .Where(p => p.IsOverdue()))
        {
            response.Escalations.Add(proj.Translate());
        }
        #endregion
        #region Забытые проекты
        foreach(var proj in projects
                .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)
                .Where(p => p.IsForgotten()))
        {
            response.Forgotten.Add(proj.Translate());
        }
        #endregion
        #region Новые проекты
        foreach(var proj in projects
                .Where(p => p.ApprovalBySalesDirectorAt >= from && p.ApprovalBySalesDirectorAt <= to)
                .Where(p => p.ApprovalByTechDirectorAt == DateTime.MinValue)
                .Where(p => p.Status != ProjectStatus.Loss))
        {
            response.New.Add(proj.Translate());
        }
        #endregion
        #region Проекты, ожидающие реакции пресейла
        foreach(var proj in projects
                .Where(p => p.ApprovalByTechDirectorAt >= from && p.ApprovalByTechDirectorAt <= to)
                .Where(p => p.PresaleStartAt == DateTime.MinValue))
        {
            response.Waiting.Add(proj.Translate());
        }
        #endregion
        #region Среднее время реакции руководителя
        response.AvgDirectorTimeToReaction = Duration.FromTimeSpan(TimeSpan.FromMinutes(projects?
            .Where(p => p.ApprovalBySalesDirectorAt >= from && p.ApprovalBySalesDirectorAt <= to)?
            .Where(p => p.ApprovalByTechDirectorAt != DateTime.MinValue)?
            .DefaultIfEmpty()
            .Average(p => p?.TimeToDirectorReaction().TotalMinutes ?? 0) ?? 0));
        #endregion
        #endregion

        db.Dispose();
        return response;
    }

    public override async Task<GetSalesOverviewResponse>
        GetSalesOverview(GetSalesOverviewRequest request, ServerCallContext context)
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
        var httpResponse = await httpClient.SendAsync(httpRequest);
        var result = await httpResponse.Content.ReadAsStringAsync();
        if(!httpResponse.IsSuccessStatusCode)
        {
            result = _CachedOverview;
        }

        _CachedOverview = result;

        var dynamicHttpResponse = JsonConvert.DeserializeObject<dynamic>(_CachedOverview);
        var response = new GetSalesOverviewResponse();

        if(dynamicHttpResponse != null)
        {
            foreach(var manager in dynamicHttpResponse.Топ)
            {
                response.CurrentTopSalesManagers.Add(
                    new Manager
                    {
                        Name = string.Join(" ", ((string)manager.Имя).Split().Take(2)),
                        Profit = (decimal)manager.Сумма
                    });
            }

            response.PreviousActualProfit = dynamicHttpResponse.Факт1 is null
                ? 0
                : (decimal)dynamicHttpResponse.Факт1;
            response.CurrentActualProfit = dynamicHttpResponse.Факт2 is null
                ? 0
                : (decimal)dynamicHttpResponse.Факт2;
            response.CurrentSalesTarget = dynamicHttpResponse.План2 is null
                ? 0
                : (decimal)dynamicHttpResponse.План2;
        }

        return response;
    }

    public override Task<GetPresalesResponse>
        GetPresales(QueryableRequest request, ServerCallContext context)
    {
        var response = new GetPresalesResponse();

        var uri = new Uri(context.GetHttpContext().Request.GetEncodedUrl());
        var httpRequest = context.GetHttpContext().Request;
        httpRequest.QueryString = new(request.Query.GetODataQueryString(uri).Query);

        var adapter = new OeEfCoreDataAdapter<ReadOnlyContext>();

        var queryContext = new ODataQueryContext(adapter.BuildEdmModel(), typeof(Presale), new());

        var options = new ODataQueryOptions<Presale>(queryContext, httpRequest);

        using var db = new ReadOnlyContext();

        IQueryable<Presale> presales = db.Presales;
        var queried = options.ApplyTo(presales);
        var queriedPresales = queried.Cast<Presale>();

        foreach(var presale in queriedPresales)
        {
            response.Presales.Add(presale.Translate());
        }

        return Task.FromResult(response);
    }

    public override Task<GetProjectsResponse>
        GetProjects(QueryableRequest request, ServerCallContext context)
    {
        var response = new GetProjectsResponse();

        var uri = new Uri(context.GetHttpContext().Request.GetEncodedUrl());
        var httpRequest = context.GetHttpContext().Request;
        httpRequest.QueryString = new(request.Query.GetODataQueryString(uri).Query);
        var adapter = new OeEfCoreDataAdapter<ReadOnlyContext>();
        var queryContext = new ODataQueryContext(adapter.BuildEdmModel(), typeof(Project), new());
        var options = new ODataQueryOptions<Presale>(queryContext, httpRequest);

        using var db = new ReadOnlyContext();

        IQueryable<Project> projects = db.Projects;

        var queried = options.ApplyTo(projects);

        var queriedProjects = queried.Cast<Project>();

        foreach(var project in queriedProjects)
        {
            response.Projects.Add(project.Translate());
        }

        return Task.FromResult(response);
    }

    public override async Task<GetNamesResponse>
        GetNames(Empty request, ServerCallContext context)
    {
        using var db = new ReadOnlyContext();
        var presalesNames = await db.Presales
            .Where(p => p.Department != Department.None)
            .Where(p => p.Position != Position.None && p.Position != Position.Director)
            .Where(p => p.IsActive)
            .Select(column => column.Name).ToListAsync();

        var response = new GetNamesResponse();

        foreach (var name in presalesNames)
        {
            response.Names.Add(name);
        }

        db.Dispose();
        return response;
    }

    public override async Task<GetProjectsResponse>
        GetUnpaidProjects(GetUnpaidProjectsRequest request, ServerCallContext context)
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
                    !p.Invoices!.Any(i => i.ProfitPeriods!.Count != 0) && p.MainProject != null &&
                    !p.MainProject!.Invoices!.Any(i => i.ProfitPeriods!.Count != 0))
                .Where(p => presale_name != string.Empty
                            ? p.Presale!.Name == presale_name
                            : p.Presale!.IsActive == true)
                .LoadAsync();
        }
        else
        {
            await db.Projects
                .Where(p => p.Status == ProjectStatus.Won && p.ClosedAt >= from && p.ClosedAt <= to &&
                    !p.Invoices!.Any(i => i.ProfitPeriods!.Count != 0))
                .Where(p => presale_name != string.Empty
                            ? p.Presale!.Name == presale_name
                            : p.Presale!.IsActive == true)
                .LoadAsync();
        }

        var presales = await db.Presales
            .Where(p => presale_name != string.Empty
                        ? p.Name == presale_name
                        : p.IsActive == true)
            .ToListAsync();

        var response = new GetProjectsResponse();

        foreach (var presale in presales)
        {
            if ((presale.Projects?.Count ?? 0) == 0)
            {
                continue;
            }

            foreach (var project in presale.Projects!.OrderBy(p => p.ClosedAt).ThenBy(p => p.Number))
            {
                response.Projects.Add(project.Translate());
            }
        }

        db.Dispose();
        return response;
    }

    public override async Task<GetProjectsResponse>
        GetFunnelProjects(Empty request, ServerCallContext context)
    {
        using var db = new ReadOnlyContext();

        var projects = await db.Projects
            .Where(p => p.Status == ProjectStatus.WorkInProgress)
            .Where(p => p.Presale!.Department == Department.Russian)
            .Where(p => p.PresaleActions!.Any(a => a.SalesFunnel) ||
                (p.PotentialAmount > 2000000
                 && p.ApprovalBySalesDirectorAt > new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
            .Include(p => p.PresaleActions!.Where(a => a.SalesFunnel))
            .Include(p => p.Presale)
            .ToListAsync();

        var response = new GetProjectsResponse();

        foreach (var project in projects)
        {
            response.Projects.Add(project.Translate());
        }

        db.Dispose();
        return response;
    }

    #endregion
}
