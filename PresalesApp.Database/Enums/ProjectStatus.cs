using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using System.Runtime.Serialization;

namespace PresalesApp.Database.Enums;

[JsonConverter(typeof(SafeStringEnumConverter), None)]
public enum ProjectStatus
{
    [EnumMember(Value = "Неопределён")]
    None,

    [EnumMember(Value = "В работе")]
    WorkInProgress,

    [EnumMember(Value = "Выиграна")]
    Won,

    [EnumMember(Value = "Проиграна")]
    Loss
}
