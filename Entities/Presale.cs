using Entities.Enums;
using Entities.Helpers;

namespace Entities
{
    public class Presale
    {
        public int PresaleId { get; set; }
        public string Name { get; set; }
        public Department Department { get; set; } = Department.None;
        public Position Position { get; set; } = Position.None;
        [JsonIgnoreSerialization]
        public virtual List<Project>? Projects { get; set; }
        [JsonIgnoreSerialization]
        public virtual List<Invoice>? Invoices { get; set; }

        public Presale(string name) => Name = name;
    }
}
