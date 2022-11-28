using Newtonsoft.Json;
using PresalesMonitor.Entities.Enums;
using PresalesMonitor.Entities.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesMonitor.Entities
{
    public class PresaleAction
    {
        public int PresaleActionId { get; set; }
        [JsonProperty("НомерСтроки")]
        public int Number { get; set; }
        [JsonProperty("Дата")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Date { get; set; }
        [JsonProperty("ТипЗадачи")]
        public ActionType Type { get; set; }
        [JsonProperty("ВремяВыполнения")]
        public int TimeSpend { get; set; }
        [JsonProperty("Описание")]
        public string Description { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        [NotMapped]
        public int Rank
        {
            get
            {
                return Type switch
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
        }
        public bool Equals(PresaleAction? action)
        {
            if (action is null) return false;
            if (Number != action.Number
                || Date != action.Date
                || Type != action.Type
                || TimeSpend != action.TimeSpend
                || Description != action.Description) return false;
            return true;
        }
    }
}
