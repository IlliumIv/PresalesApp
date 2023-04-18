﻿using Newtonsoft.Json;
using PresalesMonitor.Database.Helpers;
using static PresalesMonitor.Database.Controller;

namespace PresalesMonitor.Database.Entities.Updates
{
    public class CacheLog : Update
    {
        [JsonProperty("НачалоПериода")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime PeriodBegin { get; private set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

        [JsonProperty("КонецПериода")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
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

        public override void Save()
        {
            var query = new Task(() =>
            {
                using var _dbContext = new ReadWriteContext();
                if (!this.TryUpdate(_dbContext)) this.Add(_dbContext);
                _dbContext.SaveChanges();
                _dbContext.Dispose();
            });

            Queries.Enqueue(query);
            query.Wait();
        }

        internal override bool TryUpdate(ReadWriteContext dbContext)
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

        internal override CacheLog Add(ReadWriteContext dbContext)
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

        public override string ToString() => "{" +
            $"\"НачалоПериода\":\"{this.PeriodBegin.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"КонецПериода\":\"{this.PeriodEnd.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаРасчета\":\"{this.Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"СинхронизированоПо\":\"{this.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"" +
            "}";

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
