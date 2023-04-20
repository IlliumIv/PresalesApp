using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities.Updates
{
    public class CacheLog : Update
    {
        [JsonProperty("НачалоПериода"), JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PeriodBegin { get; private set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

        [JsonProperty("КонецПериода"), JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PeriodEnd { get; private set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

        public override DateTime SynchronizedTo
        {
            get { return PeriodBegin < _synchronizedTo ? _synchronizedTo : PeriodBegin; }
            set { _synchronizedTo = value; }
        }

        public CacheLog(DateTime synchronizedTo)
        {
            this._synchronizedTo = synchronizedTo.ToUniversalTime();
            this.SynchronizedTo = synchronizedTo.ToUniversalTime();
            this.Timestamp = synchronizedTo.ToUniversalTime();
            this.PeriodBegin = synchronizedTo.ToUniversalTime();
            this.PeriodEnd = synchronizedTo.ToUniversalTime();
        }

        internal override bool TryUpdateIfExist(ReadWriteContext dbContext)
        {
            var cache_log_in_db = dbContext.CacheLogsHistory
                .Where(i => i.PeriodBegin == this.PeriodBegin && i.PeriodEnd == this.PeriodEnd).SingleOrDefault();
            if (cache_log_in_db != null)
            {
                cache_log_in_db.Timestamp = Timestamp;
                cache_log_in_db.SynchronizedTo = SynchronizedTo;
                cache_log_in_db.ToLog(false);
            }

            return cache_log_in_db != null;
        }

        internal override CacheLog GetOrAddIfNotExist(ReadWriteContext dbContext)
        {
            var cache_log_in_db = dbContext.CacheLogsHistory
                .Where(i => i.PeriodBegin == this.PeriodBegin && i.PeriodEnd == this.PeriodEnd).SingleOrDefault();

            if (cache_log_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }

            return cache_log_in_db;
        }

        public override string ToString() =>
            $"{{\"НачалоПериода\":\"{this.PeriodBegin.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"КонецПериода\":\"{this.PeriodEnd.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаРасчета\":\"{this.Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"СинхронизированоПо\":\"{this.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"}}";

        public override CacheLog GetPrevious()
        {
            using var _dbContext = new ReadOnlyContext();
            var cache_log = _dbContext.CacheLogsHistory.Where(l => l.SynchronizedTo < l.PeriodEnd)
                .OrderBy(l => l.Timestamp).FirstOrDefault() ?? new CacheLog(DateTime.UtcNow);
            _dbContext.Dispose();
            return cache_log;
        }
    }
}
