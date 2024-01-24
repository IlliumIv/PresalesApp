using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities;

public class Project(string number) : Entity
{
    [JsonProperty("Код")]
    public string Number { get; private set; } = number;

    [JsonProperty("Наименование")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("Потенциал")]
    public decimal PotentialAmount { get; set; } = decimal.Zero;

    [JsonProperty("Статус")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;

    [JsonProperty("ПричинаПроигрыша")]
    public string LossReason { get; set; } = string.Empty;

    [JsonProperty("ПлановаяДатаОкончанияТек"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime PotentialWinAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДатаОкончания"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime ClosedAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДатаСогласованияРТС"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime ApprovalByTechDirectorAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДатаСогласованияРОП"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime ApprovalBySalesDirectorAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДатаНачалаРаботыПресейла"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime PresaleStartAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДействияПресейла")]
    public virtual List<PresaleAction>? PresaleActions { get; set; }

    [JsonProperty("Пресейл"), JsonConverter(typeof(CreateByStringConverter))]
    public virtual Presale? Presale { get; set; }

    [JsonProperty("ОсновнойПроект"), JsonConverter(typeof(CreateByStringConverter))]
    public virtual Project? MainProject { get; set; }

    public virtual List<Invoice>? Invoices { get; set; }

    public FunnelStage FunnelStage { get; set; } = FunnelStage.None;

    internal override Project GetOrAddIfNotExist(ControllerContext dbContext)
    {
        var project_in_db = dbContext.Projects.Where(p => p.Number == Number)
            .Include(p => p.PresaleActions).SingleOrDefault();

        if (project_in_db == null)
        {
            dbContext.Add(this);
            ToLog(true);
            return this;
        }
        
        return project_in_db;
    }

    internal override bool TryUpdateIfExist(ControllerContext dbContext)
    {
        Presale = Presale?.GetOrAddIfNotExist(dbContext);
        MainProject = MainProject?.GetOrAddIfNotExist(dbContext);

        var project_in_db = dbContext.Projects.Where(p => p.Number == Number)
            .Include(p => p.Presale)
            .Include(p => p.MainProject)
            .Include(p => p.PresaleActions).SingleOrDefault();

        if (project_in_db != null)
        {
            project_in_db.Name = Name;
            project_in_db.PotentialAmount = PotentialAmount;
            project_in_db.Status = Status;
            project_in_db.LossReason = LossReason;
            project_in_db.ApprovalByTechDirectorAt = ApprovalByTechDirectorAt;
            project_in_db.ApprovalBySalesDirectorAt = ApprovalBySalesDirectorAt;
            project_in_db.PresaleStartAt = PresaleStartAt;
            project_in_db.Presale = Presale;
            project_in_db.MainProject = MainProject;
            project_in_db.ClosedAt = ClosedAt;
            project_in_db.PotentialWinAt = PotentialWinAt;
            project_in_db.PresaleActions = project_in_db.PresaleActions.Update(PresaleActions, dbContext);
            project_in_db.ToLog(false);
        }

        return project_in_db != null;
    }

    public override string ToString() => $"{{\"Код\":\"{Number}\"," +
        $"\"Наименование\":\"{Name}\"," +
        $"\"Потенциал\":\"{PotentialAmount}\"," +
        $"\"Статус\":\"{Status}\"," +
        $"\"ПричинаПроигрыша\":\"{LossReason}\"," +
        $"\"ПлановаяДатаОкончанияТек\":\"{PotentialWinAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДатаОкончания\":\"{ClosedAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДатаСогласованияРТС\":\"{ApprovalByTechDirectorAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДатаСогласованияРОП\":\"{ApprovalBySalesDirectorAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДатаНачалаРаботыПресейла\":\"{PresaleStartAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДействияПресейла\":\"{PresaleActions?.Count ?? 0}\"," +
        $"\"Пресейл\":\"{Presale?.Name}\"," +
        $"\"ОсновнойПроект\":\"{MainProject?.Number}\"}}";

    public bool IsClosed() => ClosedAt != DateTime.MinValue;

    public bool IsClosedAfter(DateTime dt) => ClosedAt != DateTime.MinValue && ClosedAt < dt;

    public bool IsOverdue(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
    {
        var from = ApprovalByTechDirectorAt;
        var to = new List<DateTime>()
        {
            PresaleActions?.FirstOrDefault(a => a.Number == 1)?.Date ?? DateTime.Now,
            PresaleStartAt
        } .Min(dt => dt);

        return BusinessTimeCalculator.CalculateBusinessMinutesLOCAL(start: from, end: to) >
            (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
    }

    public bool IsForgotten(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180) =>
        PresaleStartAt == DateTime.MinValue && BusinessTimeCalculator
            .CalculateBusinessMinutesLOCAL(start: ApprovalByTechDirectorAt, end: DateTime.Now)
            > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);

    public TimeSpan TimeToDirectorReaction() => TimeSpan.FromMinutes(
        BusinessTimeCalculator.CalculateBusinessMinutesLOCAL(start: ApprovalBySalesDirectorAt, end: ApprovalByTechDirectorAt));

    public int Rank()
    {
        HashSet<PresaleAction> actionsIgnored = [], actionsTallied = [];
        HashSet<Project> projectsIgnored = [], projectsFound = [];
        return Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound);
    }

    public int Rank(ref HashSet<PresaleAction> actionsIgnored, ref HashSet<PresaleAction> actionsTallied,
        ref HashSet<Project> projectsIgnored, ref HashSet<Project> projectsFound, DateTime ignoreProjectsClosedAfter = new DateTime())
    {
        if (projectsFound.Contains(this)) return 0;
        else projectsFound.Add(this);

        var rank = 0;
        var counted = PresaleActions?.ToHashSet();

        if (counted == null || counted.Count == 0 || IsClosedAfter(ignoreProjectsClosedAfter))
        {
            projectsIgnored.Add(this);
            if (counted != null) actionsIgnored.UnionWith(counted);
        } 
        else
        {
            foreach(var actionType in PresaleAction.ActionTypes)
            {
                rank += actionType.Value.CalculationType switch
                {
                    ActionCalculationType.TimeSpend => _CalcRankByTimeSpend(actionType.Key, actionType.Value.MinRankTime),
                    ActionCalculationType.Unique => _GetRankByActionType(actionType.Key, actionType.Value.Rank),
                    ActionCalculationType.Sum => _GetRankByActionsSum(actionType.Key, actionType.Value.Rank),
                    _ => 0,
                };
            }

            actionsTallied.UnionWith(counted);
        }

        if (MainProject != null)
        {
            rank += MainProject.Rank(
                ref actionsIgnored,
                ref actionsTallied,
                ref projectsIgnored,
                ref projectsFound,
                ignoreProjectsClosedAfter);
        }

        return rank;
    }

    private int _CalcRankByTimeSpend(ActionType actionType, int minTimeToRank)
    {
        var time_spend = PresaleActions?.Where(a => a.Type == actionType).Sum(a => a.TimeSpend) ?? 0;
        return time_spend % 60 >= minTimeToRank ?
            (int)Math.Ceiling(time_spend / 60d) : (int)Math.Round(time_spend / 60d);
    }

    private int _GetRankByActionType(ActionType actionType, int r) =>
        (PresaleActions?.Any(a => a.Type == actionType) ?? false) ? r : 0;

    private int _GetRankByActionsSum(ActionType actionType, int rank) =>
        PresaleActions?.Where(a => a.Type == actionType)
        .Sum(a => actionType is ActionType.Unknown ? a.RegulationsRank : rank) ?? 0;

    public static async Task<(bool IsSuccess, string ErrorMessage)> SetFunnelStageAsync(FunnelStage newStage, string projectNumber)
    {
        var query = new Task<(bool, string)>(() =>
        {
            using var db_context = new ControllerContext();
            try
            {
                var project_in_db = db_context.Projects.Where(p => p.Number == projectNumber).SingleOrDefault();
                if (project_in_db == null) return (false, "No project found.");
                project_in_db.FunnelStage = newStage;
                db_context.SaveChanges();
                db_context.Dispose();
                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        });

        Queries.Enqueue(query);
        return await query;
    }
}
