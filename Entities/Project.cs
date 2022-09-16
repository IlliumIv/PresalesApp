using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static PresalesStatistic.Parser;

namespace PresalesStatistic.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        [JsonProperty("Код")]
        public string Number { get; private set; }
        [JsonProperty("Наименование")]
        public string? Name { get; set; }
        [JsonProperty("Потенциал")]
        public int? PotentialAmount { get; set; }
        [JsonProperty("Статус")]
        public ProjectStatus? Status { get; set; }
        [JsonProperty("ПричинаПроигрыша")]
        public string? LossReason { get; set; }
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
        [JsonIgnoreSerialization]
        public virtual List<Invoice>? Invoices { get; set; }

        public Project(string number) => Number = number;

        public void Update(Project project)
        {
            // Console.WriteLine($"Calling update to project {Number}");
            Name = project.Name;
            PotentialAmount = project.PotentialAmount;
            Status = project.Status;
            LossReason = project.LossReason;
            ApprovalByTechDirector = project.ApprovalByTechDirector;
            ApprovalBySalesDirector = project.ApprovalBySalesDirector;
            PresaleStart = project.PresaleStart;
            Actions = project.Actions;
            Presale = project.Presale;
            MainProject = project.MainProject;
        }

        public static Project? FindOrCreate(Project? project, Context db)
        {
            if (project == null) return null;
            var mainProjectsByNumber = db.Projects.Where(p => p.Number == project.Number);
            switch (mainProjectsByNumber.Count())
            {
                case 1:
                    project = mainProjectsByNumber.First();
                    break;
                case 0:
                    db.Projects.Add(project);
                    db.SaveChanges();
                    break;
                default:
                    throw new MultipleObjectsInDbException();
            }
            return project;
        }
    }
}
