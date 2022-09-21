using Newtonsoft.Json;
using PresalesStatistic.Entities.Enums;
using PresalesStatistic.Helpers;

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

        public static void UpdateActions(Project project, Context db)
        {
            if (project.Actions == null) return;
            var proj = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
            if (proj != null) 
            {
                var actions = db.Actions.Where(a => a.Project == proj).ToList();
                foreach (var action in actions)
                {
                    var newAction = project.Actions.FirstOrDefault(a => a.Equals(action));
                    if (newAction == null) db.Actions.Remove(action);
                    else
                    {
                        project.Actions.Remove(newAction);
                        project.Actions.Add(action);
                    };
                };
                proj.Actions = project.Actions;
                db.SaveChanges();
            };
        }
    }
}
