﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesMonitor.Database.Helpers;
using static PresalesMonitor.Database.Controller;

namespace PresalesMonitor.Database.Entities
{
    public class Invoice : Entity
    {
        [JsonProperty("Номер")]
        public string Number { get; set; } = string.Empty;
        
        [JsonProperty("Дата")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Date { get; set; } = new(0, DateTimeKind.Utc);
        
        [JsonProperty("Контрагент")]
        public string Counterpart { get; set; } = string.Empty;
        
        [JsonProperty("Проект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? Project { get; set; }
        
        [JsonProperty("СуммаРуб")]
        public decimal Amount { get; set; } = decimal.Zero;
        
        [JsonProperty("ДатаПоследнейОплаты")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastPayAt { get; set; } = new(0, DateTimeKind.Utc);
        
        [JsonProperty("ДатаПоследнейОтгрузки")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastShipmentAt { get; set; } = new(0, DateTimeKind.Utc);
        
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        
        [JsonProperty("ПрибыльПериоды")]
        public virtual List<ProfitPeriod>? ProfitPeriods { get; set; }
        
        public Invoice(string number) => this.Number = number;
        
        public decimal GetProfit() => this.ProfitPeriods?.Sum(p => p.Amount) ?? 0;
        
        public decimal GetProfit(DateTime from, DateTime to)
        {
            var first_day = new DateTime(from.Year, from.Month, 1);
            var last_day = to == DateTime.MaxValue ? to : new DateTime(to.Year, to.Month, 1).AddMonths(1).AddDays(-1);

            return this.ProfitPeriods?.Where(pd => pd.StartTime >= first_day && pd.StartTime <= last_day)?
                .Sum(pd => pd.Amount) ?? 0;
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

        internal override Invoice Add(ReadWriteContext dbContext)
        {
            var invoice_in_db = dbContext.Invoices.Where(
                i => i.Number == this.Number && i.Date.Date == this.Date.Date ||
                i.Number == this.Number && i.Counterpart == this.Counterpart).Include(i => i.ProfitPeriods)
                .SingleOrDefault();

            if (invoice_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }

            return invoice_in_db;
        }

        internal override bool TryUpdate(ReadWriteContext dbContext)
        {
            this.Presale = this.Presale?.Add(dbContext);
            this.Project = this.Project?.Add(dbContext);

            var invoice_in_db = dbContext.Invoices.Where(
                i => i.Number == this.Number && i.Date.Date == this.Date.Date ||
                i.Number == this.Number && i.Counterpart == this.Counterpart).Include(i => i.ProfitPeriods)
                .SingleOrDefault();
            if (invoice_in_db != null)
            {
                invoice_in_db.Date = this.Date;
                invoice_in_db.Counterpart = this.Counterpart;
                invoice_in_db.Project = this.Project;
                invoice_in_db.Amount = this.Amount;
                invoice_in_db.LastPayAt = this.LastPayAt;
                invoice_in_db.LastShipmentAt = this.LastShipmentAt;
                invoice_in_db.Presale = this.Presale;
                invoice_in_db.ProfitPeriods = invoice_in_db.ProfitPeriods.Update(this.ProfitPeriods, dbContext);
                invoice_in_db.ToLog(false);
            }
            return invoice_in_db != null;
        }

        public override string ToString()
        {
            var profit_periods = string.Empty;

            if (this.ProfitPeriods != null)
            {
                foreach (var p in this.ProfitPeriods) profit_periods += $",{p}";
            }

            return "{" +
                $"\"Номер\":\"{this.Number}\"," +
                $"\"Дата\":\"{this.Date.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
                $"\"Контрагент\":\"{this.Counterpart}\"," +
                $"\"Проект\":\"{this.Project?.Number}\"," +
                $"\"СуммаРуб\":\"{this.Amount}\"," +
                $"\"ДатаПоследнейОплаты\":\"{this.LastPayAt.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
                $"\"ДатаПоследнейОтгрузки\":\"{this.LastShipmentAt.ToLocalTime():dd.MM.yyyy HH: mm:ss.fff zzz}\"," +
                $"\"Пресейл\":\"{this.Presale?.Name}\"," +
                $"\"Суммарная прибыль за периоды\":[{profit_periods[1..]}]" +
            "}";
        }
    }
}
