using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities.Updates;

public class CacheLogsUpdate : Update
{
    internal override bool TryUpdateIfExist(ControllerContext dbContext)
    {
        var update_in_db = dbContext.CacheLogsUpdates.SingleOrDefault();
        if (update_in_db != null)
        {
            update_in_db.Timestamp = Timestamp;
            update_in_db.GetSynchronizedTo = GetSynchronizedTo;
            update_in_db.ToLog(false);
        }

        return update_in_db != null;
    }

    internal override CacheLogsUpdate GetOrAddIfNotExist(ControllerContext dbContext)
    {
        var update_in_db = dbContext.CacheLogsUpdates.SingleOrDefault();
        if (update_in_db == null)
        {
            dbContext.Add(this);
            ToLog(true);
            return this;
        }

        return update_in_db;
    }

    public override CacheLogsUpdate GetPrevious()
    {
        using var db_context = new ReadOnlyContext();
        var cache_logs_update = db_context.CacheLogsUpdates.SingleOrDefault() ?? new CacheLogsUpdate();
        db_context.Dispose();
        return cache_logs_update;
    }
}
