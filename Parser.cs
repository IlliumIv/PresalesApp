using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresalesStatistic.Entities;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic
{
    public class Parser
    {
        private DateTime _lastUpdated = DateTime.MinValue;
        public void GetUpdate(Context db)
        {
            _lastUpdated = DateTime.Now;

            var deserializeSettings = new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            var objects = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"C:\Projects_TestData.txt"), deserializeSettings);
            if (objects != null)
                foreach (var obj in objects)
                {
                    Project proj = obj.ToObject<Project>();
                    proj.MainProject = Project.FindOrCreate(proj.MainProject, db);
                    proj.Presale = Presale.FindOrCreate(proj.Presale, db);
                    var projectsByNumber = db.Projects.Where(p => p.Number == proj.Number);
                    switch (projectsByNumber.Count())
                    {
                        case 1:
                            var p = projectsByNumber.First();
                            var newActions = proj.Actions;
                            if (newActions != null) {
                                var actions = db.Actions.Where(a => a.Project == p).ToListAsync().Result;
                                foreach (var action in actions)
                                {
                                    var newAction = newActions.FirstOrDefault(a => a.Equals(action));
                                    if (newAction == null) db.Actions.Remove(action);
                                    else
                                    {
                                        newActions.Remove(newAction);
                                        newActions.Add(action);
                                    };
                                }
                            }
                            proj.Actions = newActions;
                            p.Update(proj);
                            break;
                        case 0:
                            db.Projects.Add(proj);
                            break;
                        default:
                            throw new MultipleObjectsInDbException();
                    }
                    db.SaveChanges();
                }

            objects = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"C:\Invoices_TestData.txt"), deserializeSettings);
            if (objects != null)
                foreach (var obj in objects)
                {
                    Invoice inv = obj.ToObject<Invoice>();
                    inv.Project = Project.FindOrCreate(inv.Project, db);
                    inv.Presale = Presale.FindOrCreate(inv.Presale, db);
                    var invoices = db.Invoices.Where(i => i.Number == inv.Number && i.Data == inv.Data && i.Counterpart == inv.Counterpart);
                    switch (invoices.Count())
                    {
                        case 1:
                            var i = invoices.First();
                            i.Update(inv);
                            break;
                        case 0:
                            db.Invoices.Add(inv);
                            break;
                        default:
                            throw new MultipleObjectsInDbException();
                    }
                }
            // if (invoices != null) foreach (var invoice in invoices) Console.WriteLine(JsonConvert.SerializeObject(invoice, serializeSettings));
        }

        public class MultipleObjectsInDbException : Exception
        {
            public override string Message => "Found duplicated data in database";
        }
    }
}
