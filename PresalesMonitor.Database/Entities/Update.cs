using Newtonsoft.Json;
using PresalesMonitor.Database.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesMonitor.Database.Entities
{
    public abstract class Update : Entity
    {
        [JsonProperty("ДатаРасчета")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Timestamp { get; set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

        public virtual DateTime SynchronizedTo
        {
            get { return this.Timestamp < this._synchronizedTo ? this._synchronizedTo : this.Timestamp; }
            set { this._synchronizedTo = value; }
        }

        [NotMapped]
        [JsonIgnore]
        protected DateTime _synchronizedTo = new(0, DateTimeKind.Utc);

        protected Update() { }

        public abstract Update GetPrevious();
    }
}