﻿using Newtonsoft.Json;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesStatistic.Entities
{
    public class PresaleAction
    {
        public int PresaleActionId { get; set; }
        [JsonProperty("НомерСтроки")]
        public int? Number { get; set; }
        [JsonProperty("Дата")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? Date { get; set; }
        [JsonProperty("ТипЗадачи")]
        public ActionType Type { get; set; }
        [NotMapped]
        public int Rang { get => _getRang; }
        [JsonProperty("ВремяВыполнения")]
        public int TimeSpend { get; set; }
        [JsonProperty("Описание")]
        public string? Description { get; set; }
        public int ProjectId { get; set; }
        [JsonIgnoreSerialization]
        public virtual Project? Project { get; set; }

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

        private int _getRang
        {
            get
            {
                return Type switch
                {
                    ActionType.FullSetup => 4,
                    ActionType.SettingsCheckup => 1,
                    ActionType.ProblemDiagnosis => 1,
                    ActionType.Сalculation => TimeSpend % 60 > 5 ? (int)Math.Ceiling(TimeSpend / 60d) : (int)Math.Round(TimeSpend / 60d),
                    ActionType.SpecificationCheckup => 1,
                    ActionType.SpecificationCreateFromTemplate => 3,
                    ActionType.SpecificationCreate => 5,
                    ActionType.RequirementsCreate => 2,
                    ActionType.Consultation => TimeSpend % 60 > 5 ? (int)Math.Ceiling(TimeSpend / 60d) : (int)Math.Round(TimeSpend / 60d),
                    ActionType.Negotiations => TimeSpend % 60 > 10 ? (int)Math.Ceiling(TimeSpend / 60d) : (int)Math.Round(TimeSpend / 60d),
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
    }
}
