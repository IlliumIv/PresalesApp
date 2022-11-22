using Newtonsoft.Json;
using Entities.Enums;
using Entities.Helpers;

namespace Entities
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
            return Presale.CalculateWorkingMinutes(ApprovalByTechDirectorAt.ToLocalTime(),
                new List<DateTime?>() {
                    (Actions?.FirstOrDefault(a => a.Number == 1)?.Date.ToLocalTime() ?? DateTime.Now)
                        .AddMinutes(-Actions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
                    PresaleStartAt.ToLocalTime()
                }.Min(dt => dt)) > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public bool IsForgotten(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            return PresaleStartAt == DateTime.MinValue && Presale.CalculateWorkingMinutes(ApprovalByTechDirectorAt.ToLocalTime(), DateTime.Now)
                    > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public TimeSpan TimeToDirectorReaction() => TimeSpan.FromMinutes(Presale.CalculateWorkingMinutes(ApprovalBySalesDirectorAt, ApprovalByTechDirectorAt) ?? 0);
        public int Rank()
        {
            List<PresaleAction> ignoredActions = new();
            List<PresaleAction> countedActions = new();
            List<Project> countedUpProjects = new();
            return Rank(ref ignoredActions, ref countedActions, ref countedUpProjects);
        }
        public int Rank(ref List<PresaleAction> ignoredActions, ref List<PresaleAction> countedActions, ref List<Project> countedUpProjects)
        {
            if (countedUpProjects.Contains(this)) return 0;
            else countedUpProjects.Add(this);

            var ignored = Actions?.Where(a => a.Type == ActionType.Unknown)?.ToList();
            if (ignored != null) ignoredActions.AddRange(ignored);

            var counted = Actions?.Where(a => a.Type != ActionType.Unknown)?.ToList();
            if (counted != null) countedActions.AddRange(counted);

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
            if (MainProject != null) rank += MainProject.Rank(ref ignoredActions, ref countedActions, ref countedUpProjects);

            return rank;
        }
        private int CalcRankByTimeSpend(ActionType actionType, int minTimeToRank)
        {
            var ts = Actions?.Where(a => a.Type == actionType).Sum(a => a.TimeSpend) ?? 0;
            return ts % 60 > minTimeToRank ? (int)Math.Ceiling(ts / 60d) : (int)Math.Round(ts / 60d);
        }
    }
}
