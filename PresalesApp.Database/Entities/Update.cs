using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesApp.Database.Entities
{
    public abstract class Update : Entity
    {
        [JsonProperty("ДатаРасчета"), JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Timestamp { get; set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

        [NotMapped, JsonIgnore]
        protected DateTime _synchronizedTo = new(0, DateTimeKind.Utc);

        public virtual DateTime SynchronizedTo
        {
            get { return this.Timestamp < this._synchronizedTo ? this._synchronizedTo : this.Timestamp; }
            set { this._synchronizedTo = value; }
        }

        protected Update() { }

        public abstract Update GetPrevious();

        public override string ToString() =>
            $"{{\"ДатаРасчета\":\"{this.Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"СинхронизированоПо\":\"{this.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"}}";
    }
}