using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PresalesStatistic.Parser;

namespace PresalesStatistic.Entities
{
    public class Presale
    {
        public int PresaleId { get; set; }
        public string Name { get; set; }
        [JsonIgnoreSerialization]
        public virtual List<Project>? Projects { get; set; }
        [JsonIgnoreSerialization]
        public virtual List<Invoice>? Invoices { get; set; }

        public Presale(string name) => Name = name;

        public static Presale? FindOrCreate(Presale? presale, Context db)
        {
            if (presale == null) return null;
            var presalesByName = db.Presales.Where(p => p.Name == presale.Name);
            switch (presalesByName.Count())
            {
                case 1:
                    presale = presalesByName.First();
                    break;
                case 0:
                    db.Presales.Add(presale);
                    db.SaveChanges();
                    break;
                default:
                    throw new MultipleObjectsInDbException();
            }
            return presale;
        }
    }
}
