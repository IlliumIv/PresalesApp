using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities;

public class Invoice(string number) : Entity
{
    [JsonProperty("Номер")]
    public string Number { get; set; } = number;

    [JsonProperty("Дата"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime Date { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ПометкаУдаления")]
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("Контрагент")]
    public string Counterpart { get; set; } = string.Empty;

    [JsonProperty("Проект"), JsonConverter(typeof(CreateByStringConverter))]
    public virtual Project? Project { get; set; }

    [JsonProperty("СуммаРуб")]
    public decimal Amount { get; set; } = decimal.Zero;

    [JsonProperty("ДатаПоследнейОплаты"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime LastPayAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("ДатаПоследнейОтгрузки"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime LastShipmentAt { get; set; } = new(0, DateTimeKind.Utc);

    [JsonProperty("Пресейл"), JsonConverter(typeof(CreateByStringConverter))]
    public virtual Presale? Presale { get; set; }

    [JsonProperty("ПрибыльПериоды")]
    public virtual List<ProfitPeriod>? ProfitPeriods { get; set; }

    public decimal GetProfit() => ProfitPeriods?.Sum(p => p.Amount) ?? 0;

    public decimal GetProfit(DateTime from, DateTime to)
    {
        var fixed_data = from == DateTime.MinValue || from == DateTime.MaxValue
            ? from : from.Add(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));
        var first_day = new DateTime(fixed_data.Year, fixed_data.Month, 1, 0, 0, 0, DateTimeKind.Local)
            .ToUniversalTime();

        fixed_data = to == DateTime.MinValue || to == DateTime.MaxValue
            ? to : to.Add(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));
        var last_day = new DateTime(fixed_data.Year, fixed_data.Month, 1, 0, 0, 0, DateTimeKind.Local)
            .AddMonths(1).AddSeconds(-1).ToUniversalTime();

        return ProfitPeriods?
            .Where(pd => pd.StartTime >= first_day && pd.StartTime <= last_day)?
            .Sum(pd => pd.Amount) ?? 0;
    }

    internal override Invoice GetOrAddIfNotExist(ControllerContext dbContext)
    {
        var invoice_in_db = dbContext.Invoices
            .Where(i => (i.Number == Number && i.Date.Date == Date.Date)
                || (i.Number == Number && i.Counterpart == Counterpart))
            .Include(i => i.ProfitPeriods)
            .SingleOrDefault();

        if (invoice_in_db == null)
        {
            dbContext.Add(this);
            ToLog(true);
            return this;
        }

        return invoice_in_db;
    }

    internal override bool TryUpdateIfExist(ControllerContext dbContext)
    {
        Presale = Presale?.GetOrAddIfNotExist(dbContext);
        Project = Project?.GetOrAddIfNotExist(dbContext);

        var invoice_in_db = dbContext.Invoices
            .Where(i => (i.Number == Number && i.Date.Date == Date.Date)
                || (i.Number == Number && i.Counterpart == Counterpart))
            .Include(i => i.ProfitPeriods)
            .SingleOrDefault();

        if (invoice_in_db != null)
        {
            invoice_in_db.Date = Date;
            invoice_in_db.Counterpart = Counterpart;
            invoice_in_db.Project = Project;
            invoice_in_db.Amount = Amount;
            invoice_in_db.LastPayAt = LastPayAt;
            invoice_in_db.LastShipmentAt = LastShipmentAt;
            invoice_in_db.Presale = Presale;
            invoice_in_db.ProfitPeriods = invoice_in_db.ProfitPeriods.Update(ProfitPeriods, dbContext);
            invoice_in_db.ToLog(false);
        }

        return invoice_in_db != null;
    }

    public override string ToString()
    {
        var profit_periods = string.Empty;

        if (ProfitPeriods != null)
            foreach (var p in ProfitPeriods) profit_periods += $",{p}";

        return $"{{\"Номер\":\"{Number}\"," +
            $"\"Дата\":\"{Date.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"Контрагент\":\"{Counterpart}\"," +
            $"\"Проект\":\"{Project?.Number}\"," +
            $"\"СуммаРуб\":\"{Amount}\"," +
            $"\"ДатаПоследнейОплаты\":\"{LastPayAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"ДатаПоследнейОтгрузки\":\"{LastShipmentAt.ToLocalTime():dd.MM.yyyy HH: mm:ss.fff zzz}\"," +
            $"\"Пресейл\":\"{Presale?.Name}\"," +
            $"\"Суммарная прибыль за периоды\":[{(profit_periods == string.Empty ? "" : profit_periods?[1..])}]}}";
    }
}
