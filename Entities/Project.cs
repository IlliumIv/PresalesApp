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
        public string? Name { get; set; }
        [JsonProperty("Потенциал")]
        public decimal PotentialAmount { get; set; }
        [JsonProperty("Статус")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
        public DateTime? LastStatusChanged { get; set; }
        [JsonProperty("ПричинаПроигрыша")]
        public string LossReason { get; set; } = string.Empty;
        [JsonProperty("ДатаСогласованияРТС")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? ApprovalByTechDirector { get; set; }
        [JsonProperty("ДатаСогласованияРОП")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? ApprovalBySalesDirector { get; set; }
        [JsonProperty("ДатаНачалаРаботыПресейла")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? PresaleStart { get; set; }
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
            return Presale.CalculateWorkingMinutes(ApprovalByTechDirector.ToLocal(),
                new List<DateTime?>() {
                    (Actions?.FirstOrDefault(a => a.Number == 1)?.Date.ToLocal() ?? DateTime.Now)
                        .AddMinutes(-Actions?.FirstOrDefault(a => a.Number == 1)?.TimeSpend ?? 0),
                    PresaleStart.ToLocal()
                }.Min(dt => dt)) > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public bool IsForgotten(int majorProjectMinAmount = 2000000, int majorProjectMaxTTR = 120, int maxTTR = 180)
        {
            return PresaleStart == null && Presale.CalculateWorkingMinutes(ApprovalByTechDirector.ToLocal(), DateTime.Now)
                    > (PotentialAmount > majorProjectMinAmount ? majorProjectMaxTTR : maxTTR);
        }
        public TimeSpan TimeToDirectorReaction() => TimeSpan.FromMinutes(Presale.CalculateWorkingMinutes(ApprovalBySalesDirector, ApprovalByTechDirector) ?? 0);
        public int Rang()
        {
            List<PresaleAction> ignoredActions = new();
            List<PresaleAction> countedActions = new();
            List<Project> countedUpProjects = new();
            return Rang(ref ignoredActions, ref countedActions, ref countedUpProjects);
        }
        public int Rang(ref List<PresaleAction> ignoredActions, ref List<PresaleAction> countedActions, ref List<Project> countedUpProjects)
        {
            if (countedUpProjects.Contains(this)) return 0;
            else countedUpProjects.Add(this);

            var ignored = Actions?.Where(a => a.Type == ActionType.Unknown)?.ToList();
            if (ignored != null) ignoredActions.AddRange(ignored);

            var counted = Actions?.Where(a => a.Type != ActionType.Unknown)?.ToList();
            if (counted != null) countedActions.AddRange(counted);

            int rang = CalcRangByTimeSpend(ActionType.Calculation, 5);
            rang += CalcRangByTimeSpend(ActionType.Consultation, 5);
            rang += CalcRangByTimeSpend(ActionType.Negotiations, 10);
            rang += Actions?.Where(a => a.Type != ActionType.Calculation
                                    && a.Type != ActionType.Consultation
                                    && a.Type != ActionType.Negotiations
                                    && a.Type != ActionType.Unknown)?
                            .Sum(a => a.Rang) ?? 0;
            if (MainProject != null) rang += MainProject.Rang(ref ignoredActions, ref countedActions, ref countedUpProjects);

            return rang;
        }
        private int CalcRangByTimeSpend(ActionType actionType, int minTimeToRang)
        {
            var ts = Actions?.Where(a => a.Type == actionType)?.Sum(a => a.TimeSpend) ?? 0;
            return ts % 60 > minTimeToRang ? (int)Math.Ceiling(ts / 60d) : (int)Math.Round(ts / 60d);
        }
    }
}
