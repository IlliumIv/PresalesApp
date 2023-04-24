using Newtonsoft.Json;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesApp.Database.Entities
{
    public class PresaleAction : Entity
    {
        [JsonProperty("НомерСтроки")]
        public int Number { get; set; } = 0;

        [JsonProperty("Дата"), JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Date { get; set; } = new DateTime(0, DateTimeKind.Utc);

        [JsonProperty("ТипЗадачи")]
        public ActionType Type { get; set; } = ActionType.Unknown;

        [JsonProperty("ВремяВыполнения")]
        public int TimeSpend { get; set; } = 0;

        [JsonProperty("Воронка")]
        public bool SalesFunnel { get; set; } = false;

        [JsonProperty("Описание")]
        public string Description { get; set; } = string.Empty;

        public virtual Project? Project { get; set; }

        internal PresaleAction() { }

        public PresaleAction(Project project) => this.Project = project;

        [NotMapped]
        public int Rank
        {
            get => Type switch
            {
                ActionType.FullSetup => 4,
                ActionType.SettingsCheckup => 1,
                ActionType.SpecificationCheckup => 1,
                ActionType.SpecificationCreateFromTemplate => 3,
                ActionType.SpecificationCreate => 5,
                ActionType.RequirementsCreate => 2,
                ActionType.Presentation => 3,
                ActionType.DepartureFullSetup => 5,
                ActionType.DepartureSettingsCheckup => 2,
                ActionType.DepartureProblemDiagnosis => 2,
                ActionType.DeparturePresentation => 5,
                ActionType.DepartureConsultation => 2,
                _ => 0,
            };
        }

        public override string ToString() => $"{{\"НомерСтроки\":\"{this.Number}\"," +
            $"\"Дата\":\"{this.Date.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ТипЗадачи\":\"{this.Type}\"," +
            $"\"ВремяВыполнения\":\"{this.TimeSpend}\"," +
            $"\"Воронка\":\"{this.SalesFunnel}\"," +
            $"\"Описание\":\"{this.Description}\"," +
            $"\"Проект\":\"{this.Project?.Number}\"}}";

        internal override bool TryUpdateIfExist(DbController.ControllerContext dbContext)
        {
            throw new NotImplementedException();
        }

        internal override PresaleAction GetOrAddIfNotExist(DbController.ControllerContext dbContext)
        {
            throw new NotImplementedException();
        }
    }
}
