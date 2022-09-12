using Newtonsoft.Json;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Entities
{
    public class PresaleAction
    {
        public int PresaleActionId { get; set; }
        public int НомерСтроки;
        public DateTime Дата;
        public ActionType ТипЗадачи;
        public int ВремяВыполнения;
        public string? Описание;
    }
}
