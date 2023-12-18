using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using System.Runtime.Serialization;

namespace PresalesApp.Database.Enums;

[JsonConverter(typeof(SafeStringEnumConverter), Unknown)]
public enum ProjectStatus
{
    [EnumMember(Value = "Неопределён")]
    Unknown,

    [EnumMember(Value = "В работе")]
    WorkInProgress,

    [EnumMember(Value = "Выиграна")]
    Won,

    [EnumMember(Value = "Проиграна")]
    Loss
}
