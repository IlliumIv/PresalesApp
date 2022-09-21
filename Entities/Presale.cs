using PresalesStatistic.Helpers;

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

        public static Presale? GetOrAdd(Presale? presale, Context db)
        {
            if (presale != null)
            {
                var pr = db.Presales.Where(p => p.Name == presale.Name).FirstOrDefault();
                if (pr != null) return pr;
                else
                {
                    db.Presales.Add(presale);
                    db.SaveChanges();
                }
            }
            return presale;
        }
    }
}
