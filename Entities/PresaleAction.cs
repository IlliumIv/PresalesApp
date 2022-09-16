using Newtonsoft.Json;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    }
}
