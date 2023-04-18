using Newtonsoft.Json;
using System.Runtime.Serialization;
using PresalesMonitor.Database.Helpers;

namespace PresalesMonitor.Database.Enums
{
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
}
