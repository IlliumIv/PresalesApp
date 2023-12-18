using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities.Updates;

public class CacheLog : Update
{
    [JsonProperty("НачалоПериода"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime PeriodBegin { get; private set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

    [JsonProperty("КонецПериода"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime PeriodEnd { get; private set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

    public override DateTime SynchronizedTo
    {
        get => PeriodBegin < Synchronized ? Synchronized : PeriodBegin;
        set => Synchronized = value;
    }

    public CacheLog(DateTime synchronizedTo)
    {
        Synchronized = synchronizedTo.ToUniversalTime();
        SynchronizedTo = synchronizedTo.ToUniversalTime();
        Timestamp = synchronizedTo.ToUniversalTime();
        PeriodBegin = synchronizedTo.ToUniversalTime();
        PeriodEnd = synchronizedTo.ToUniversalTime();
    }

    internal override bool TryUpdateIfExist(ControllerContext dbContext)
    {
        var cache_log_in_db = dbContext.CacheLogsHistory
            .Where(i => i.PeriodBegin == PeriodBegin && i.PeriodEnd == PeriodEnd)
            .SingleOrDefault();

        if (cache_log_in_db != null)
        {
            cache_log_in_db.Timestamp = Timestamp;
            cache_log_in_db.SynchronizedTo = SynchronizedTo;
            cache_log_in_db.ToLog(false);
        }

        return cache_log_in_db != null;
    }

    internal override CacheLog GetOrAddIfNotExist(ControllerContext dbContext)
    {
        var cache_log_in_db = dbContext.CacheLogsHistory
            .Where(i => i.PeriodBegin == PeriodBegin && i.PeriodEnd == PeriodEnd)
            .SingleOrDefault();

        if (cache_log_in_db == null)
        {
            dbContext.Add(this);
            ToLog(true);
            return this;
        }

        return cache_log_in_db;
    }

    public override string ToString() =>
        $"{{\"НачалоПериода\":\"{PeriodBegin.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"КонецПериода\":\"{PeriodEnd.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ДатаРасчета\":\"{Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"СинхронизированоПо\":\"{SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"}}";

    public override CacheLog GetPrevious()
    {
        using var db_context = new ReadOnlyContext();
        var cache_log = db_context.CacheLogsHistory
            .Where(l => l.SynchronizedTo < l.PeriodEnd)
            .OrderBy(l => l.Timestamp)
            .FirstOrDefault()
            ?? new CacheLog(DateTime.UtcNow);

        db_context.Dispose();
        return cache_log;
    }
}
