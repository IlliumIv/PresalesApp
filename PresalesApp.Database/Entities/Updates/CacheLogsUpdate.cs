using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities.Updates
{
    public class CacheLogsUpdate : Update
    {
        internal override bool TryUpdateIfExist(ReadWriteContext dbContext)
        {
            var update_in_db = dbContext.CacheLogsUpdates.SingleOrDefault();
            if (update_in_db != null)
            {
                update_in_db.Timestamp = Timestamp;
                update_in_db.SynchronizedTo = SynchronizedTo;
                update_in_db.ToLog(false);
            }

            return update_in_db != null;
        }

        internal override CacheLogsUpdate GetOrAddIfNotExist(ReadWriteContext dbContext)
        {
            var update_in_db = dbContext.CacheLogsUpdates.SingleOrDefault();
            if (update_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }

            return update_in_db;
        }

        public override CacheLogsUpdate GetPrevious()
        {
            using var _dbContext = new ReadOnlyContext();
            var cache_logs_update = _dbContext.CacheLogsUpdates.SingleOrDefault() ?? new CacheLogsUpdate();
            _dbContext.Dispose();
            return cache_logs_update;
        }
    }

}
