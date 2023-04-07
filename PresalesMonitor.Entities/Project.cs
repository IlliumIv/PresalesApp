using Newtonsoft.Json;
using PresalesMonitor.Entities.Enums;
using PresalesMonitor.Entities.Helpers;

namespace PresalesMonitor.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        [JsonProperty("Код")]
        public string Number { get; private set; }
        [JsonProperty("Наименование")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("Потенциал")]
        public decimal PotentialAmount { get; set; }
        [JsonProperty("Статус")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
        [JsonProperty("ПричинаПроигрыша")]
        public string LossReason { get; set; } = string.Empty;
        [JsonProperty("ПлановаяДатаОкончанияТек")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PotentialWinAt { get; set; }
        [JsonProperty("ДатаОкончания")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ClosedAt { get; set; }
        [JsonProperty("ДатаСогласованияРТС")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ApprovalByTechDirectorAt { get; set; }
        [JsonProperty("ДатаСогласованияРОП")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime ApprovalBySalesDirectorAt { get; set; }
        [JsonProperty("ДатаНачалаРаботыПресейла")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PresaleStartAt { get; set; }
        [JsonProperty("ДействияПресейла")]
        public virtual List<PresaleAction>? Actions { get; set; }
        public int? PresaleId { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("ОсновнойПроект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? MainProject { get; set; }
        public virtual List<Invoice>? Invoices { get; set; }
        public Project(string number) => Number = number;
        public bool IsOverdue(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            var from = ApprovalByTechDirectorAt;
            var to = new List<DateTime?>() {
                        (Actions?.FirstOrDefault(a => a.Number == 1)?.Date ?? DateTime.Now),
                        PresaleStartAt }.Min(dt => dt);
            return Presale.CalculateWorkingMinutes(from, to) > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public bool IsForgotten(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            return PresaleStartAt == DateTime.MinValue && Presale.CalculateWorkingMinutes(ApprovalByTechDirectorAt, DateTime.Now)
                > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public TimeSpan TimeToDirectorReaction() => TimeSpan.FromMinutes(Presale.CalculateWorkingMinutes(ApprovalBySalesDirectorAt, ApprovalByTechDirectorAt) ?? 0);
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

            var ignored = Actions?.Where(a => a.Type == ActionType.Unknown)?.ToHashSet();
            if (ignored != null) actionsIgnored.UnionWith(ignored);

            var counted = Actions?.Where(a => a.Type != ActionType.Unknown)?.ToHashSet();
            if (counted == null || counted.Count == 0) projectsIgnored.Add(this);
            else actionsTallied.UnionWith(counted);

            int rank = CalcRankByTimeSpend(ActionType.Calculation, 5);
            rank += CalcRankByTimeSpend(ActionType.Consultation, 5);
            rank += CalcRankByTimeSpend(ActionType.Negotiations, 10);
            rank += CalcRankByTimeSpend(ActionType.ProblemDiagnostics, 15);
            rank += Actions?.Where(a => a.Type != ActionType.Calculation
                                    && a.Type != ActionType.Consultation
                                    && a.Type != ActionType.Negotiations
                                    && a.Type != ActionType.ProblemDiagnostics
                                    && a.Type != ActionType.Unknown)
                            .Sum(a => a.Rank) ?? 0;
            if (MainProject != null) rank += MainProject.Rank(ref actionsIgnored, ref actionsTallied, ref projectsIgnored, ref projectsFound);

            return rank;
        }
        private int CalcRankByTimeSpend(ActionType actionType, int minTimeToRank)
        {
            var ts = Actions?.Where(a => a.Type == actionType).Sum(a => a.TimeSpend) ?? 0;
            return ts % 60 >= minTimeToRank ? (int)Math.Ceiling(ts / 60d) : (int)Math.Round(ts / 60d);
        }
    }
}
