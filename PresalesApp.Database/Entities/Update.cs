﻿using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesApp.Database.Entities;

public abstract class Update : Entity
{
    [JsonProperty("ДатаРасчета"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime Timestamp { get; set; } = new(2023, 3, 31, 19, 0, 0, DateTimeKind.Utc);

    [NotMapped, JsonIgnore]
    protected DateTime SynchronizedTo = new(0, DateTimeKind.Utc);

    public virtual DateTime GetSynchronizedTo
    {
        get => Timestamp < SynchronizedTo ? SynchronizedTo : Timestamp;
        set => SynchronizedTo = value;
    }

    protected Update() { }

    public abstract Update GetPrevious();

    public override string ToString() =>
        $"{{\"ДатаРасчета\":\"{Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"СинхронизированоПо\":\"{GetSynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"}}";
}