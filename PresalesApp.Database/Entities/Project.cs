using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities
{
    public class Project : Entity
    {
        [JsonProperty("Код")]
        public string Number { get; private set; } = string.Empty;

        [JsonProperty("Наименование")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Потенциал")]
        public decimal PotentialAmount { get; set; } = decimal.Zero;

        [JsonProperty("Статус")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;

        [JsonProperty("ПричинаПроигрыша")]
        public string LossReason { get; set; } = string.Empty;

        [JsonProperty("ПлановаяДатаОкончанияТек")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PotentialWinAt { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("ДатаОкончания")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ClosedAt { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("ДатаСогласованияРТС")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ApprovalByTechDirectorAt { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("ДатаСогласованияРОП")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ApprovalBySalesDirectorAt { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("ДатаНачалаРаботыПресейла")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PresaleStartAt { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("ДействияПресейла")]
        public virtual List<PresaleAction>? PresaleActions { get; set; }

        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }

        [JsonProperty("ОсновнойПроект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? MainProject { get; set; }

        public virtual List<Invoice>? Invoices { get; set; }

        public Project(string number) => this.Number = number;

        internal override Project GetOrAddIfNotExist(ReadWriteContext dbContext)
        {
            var project_in_db = dbContext.Projects.Where(p => p.Number == this.Number)
                .Include(p => p.PresaleActions).SingleOrDefault();

            if (project_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }
            else return project_in_db;
        }

        internal override bool TryUpdateIfExist(ReadWriteContext dbContext)
        {
            this.Presale = this.Presale?.GetOrAddIfNotExist(dbContext);
            this.MainProject = this.MainProject?.GetOrAddIfNotExist(dbContext);

            var project_in_db = dbContext.Projects.Where(p => p.Number == this.Number)
                .Include(p => p.PresaleActions).SingleOrDefault();
            if (project_in_db != null)
            {
                project_in_db.Name = this.Name;
                project_in_db.PotentialAmount = this.PotentialAmount;
                project_in_db.Status = this.Status;
                project_in_db.LossReason = this.LossReason;
                project_in_db.ApprovalByTechDirectorAt = this.ApprovalByTechDirectorAt;
                project_in_db.ApprovalBySalesDirectorAt = this.ApprovalBySalesDirectorAt;
                project_in_db.PresaleStartAt = this.PresaleStartAt;
                project_in_db.Presale = this.Presale;
                project_in_db.MainProject = this.MainProject;
                project_in_db.ClosedAt = this.ClosedAt;
                project_in_db.PotentialWinAt = this.PotentialWinAt;
                project_in_db.PresaleActions = project_in_db.PresaleActions.Update(this.PresaleActions, dbContext);
                project_in_db.ToLog(false);
            }
            return project_in_db != null;
        }

        public override string ToString() => $"{{\"Код\":\"{this.Number}\"," +
            $"\"Наименование\":\"{this.Name}\"," +
            $"\"Потенциал\":\"{this.PotentialAmount}\"," +
            $"\"Статус\":\"{this.Status}\"," +
            $"\"ПричинаПроигрыша\":\"{this.LossReason}\"," +
            $"\"ПлановаяДатаОкончанияТек\":\"{this.PotentialWinAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаОкончания\":\"{this.ClosedAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаСогласованияРТС\":\"{this.ApprovalByTechDirectorAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаСогласованияРОП\":\"{this.ApprovalBySalesDirectorAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаНачалаРаботыПресейла\":\"{this.PresaleStartAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДействияПресейла\":\"{this.PresaleActions?.Count ?? 0}\"," +
            $"\"Пресейл\":\"{this.Presale?.Name}\"," +
            $"\"ОсновнойПроект\":\"{this.MainProject?.Number}\"}}";

        public bool IsOverdue(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            var from = this.ApprovalByTechDirectorAt;
            var to = new List<DateTime?>() {
                this.PresaleActions?.FirstOrDefault(a => a.Number == 1)?.Date ?? DateTime.Now,
                this.PresaleStartAt
            }.Min(dt => dt);

            return WorkTimeCalculator.CalculateWorkingMinutes(start: from, end: to) >
                (this.PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }

        public bool IsForgotten(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180) =>
            this.PresaleStartAt == DateTime.MinValue &&
            WorkTimeCalculator.CalculateWorkingMinutes(start: this.ApprovalByTechDirectorAt, end: DateTime.Now) >
                (this.PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);

        public TimeSpan TimeToDirectorReaction() =>
            TimeSpan.FromMinutes(WorkTimeCalculator.CalculateWorkingMinutes(
                start: this.ApprovalBySalesDirectorAt, end: this.ApprovalByTechDirectorAt) ?? 0);

        public int Rank()
        {
            HashSet<PresaleAction> actionsIgnored = new(), actionsTallied = new();
            HashSet<Project> projectsIgnored = new(), projectsFound = new();
            return Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound);
        }

        public int Rank(ref HashSet<PresaleAction> actionsIgnored, ref HashSet<PresaleAction> actionsTallied,
            ref HashSet<Project> projectsIgnored, ref HashSet<Project> projectsFound)
        {
            if (projectsFound.Contains(this)) return 0;
            else projectsFound.Add(this);

            var ignored = this.PresaleActions?.Where(a => a.Type == ActionType.Unknown)?.ToHashSet();
            if (ignored != null) actionsIgnored.UnionWith(ignored);

            var counted = this.PresaleActions?.Where(a => a.Type != ActionType.Unknown)?.ToHashSet();
            if (counted == null || counted.Count == 0) projectsIgnored.Add(this);
            else actionsTallied.UnionWith(counted);

            int rank = CalcRankByTimeSpend(ActionType.Calculation, 5);
            rank += CalcRankByTimeSpend(ActionType.Consultation, 5);
            rank += CalcRankByTimeSpend(ActionType.Negotiations, 10);
            rank += CalcRankByTimeSpend(ActionType.ProblemDiagnostics, 15);
            rank += this.PresaleActions?.Where(a => a.Type != ActionType.Calculation
                                    && a.Type != ActionType.Consultation
                                    && a.Type != ActionType.Negotiations
                                    && a.Type != ActionType.ProblemDiagnostics
                                    && a.Type != ActionType.Unknown)
                            .Sum(a => a.Rank) ?? 0;

            if (this.MainProject != null)
                rank += this.MainProject
                    .Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound);

            return rank;
        }

        private int CalcRankByTimeSpend(ActionType actionType, int minTimeToRank)
        {
            var time_spend = this.PresaleActions?.Where(a => a.Type == actionType).Sum(a => a.TimeSpend) ?? 0;
            return time_spend % 60 >= minTimeToRank ?
                (int)Math.Ceiling(time_spend / 60d) : (int)Math.Round(time_spend / 60d);
        }
    }
}
