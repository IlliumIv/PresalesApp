using Newtonsoft.Json;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;
using System.Net.NetworkInformation;
using System.Xml.Linq;

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
        [JsonIgnoreSerialization]
        public virtual List<Invoice>? Invoices { get; set; }

        public Project(string number) => Number = number;

        public static void AddOrUpdate(Project project, Context db)
        {
            var pr = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
            if (pr != null)
            {
                pr.Name = project.Name;
                pr.PotentialAmount = project.PotentialAmount;
                pr.LossReason = project.LossReason;
                pr.ApprovalByTechDirector = project.ApprovalByTechDirector;
                pr.ApprovalBySalesDirector = project.ApprovalBySalesDirector;
                pr.PresaleStart = project.PresaleStart;
                pr.DeleteActions(db);
                pr.Actions = project.Actions;
                pr.Presale = project.Presale;
                pr.MainProject = project.MainProject;
                if (pr.Status != project.Status)
                {
                    pr.Status = project.Status;
                    pr.LastStatusChanged = DateTime.UtcNow;
                }
            }
            else
            {
                db.Projects.Add(project);
            }
            db.SaveChanges();
        }

        public static Project? GetOrAdd(Project? project, Context db)
        {
            if (project != null)
            {
                var pr = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
                if (pr != null) return pr;
                else
                {
                    db.Projects.Add(project);
                    db.SaveChanges();
                }
            }
            return project;
        }

        public void DeleteActions(Context db)
        {
            var proj = db.Projects.Where(p => p.Number == Number).FirstOrDefault();
            if (proj != null) 
            {
                var actions = db.Actions.Where(a => a.Project == proj).ToList();
                foreach (var action in actions) db.Actions.Remove(action);
            };
        }
    }
}
