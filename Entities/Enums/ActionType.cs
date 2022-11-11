using Newtonsoft.Json;
using Entities.Helpers;
using System.Runtime.Serialization;

namespace Entities.Enums
{
    [JsonConverter(typeof(SafeStringEnumConverter), Unknown)]
    public enum ActionType
    {
        [EnumMember(Value = "Прочее")]
        Unknown,
        [EnumMember(Value = "Полная настройка системы у заказчика")]
        FullSetup,
        [EnumMember(Value = "Корректировка настроек")]
        SettingsCheckup,
        [EnumMember(Value = "Диагностика проблемы")]
        ProblemDiagnostics,
        [EnumMember(Value = "Разработка технического решения")]
        Calculation,
        [EnumMember(Value = "Корректировка ТЗ или проработка ТЗ на возможность использования ПО Макроскоп")]
        SpecificationCheckup,
        [EnumMember(Value = "Разработка ТЗ на основе шаблонов")]
        SpecificationCreateFromTemplate,
        [EnumMember(Value = "Разработка ТЗ «с нуля»")]
        SpecificationCreate,
        [EnumMember(Value = "Составление требований к ПО в тендерную документацию")]
        RequirementsCreate,
        [EnumMember(Value = "Консультация")]
        Consultation,
        [EnumMember(Value = "Переговоры с клиентом")]
        Negotiations,
        [EnumMember(Value = "Презентация продукта/демо")]
        Presentation,
        [EnumMember(Value = "Полная настройка системы у заказчика с выездом")]
        DepartureFullSetup,
        [EnumMember(Value = "Корректировка настроек с выездом")]
        DepartureSettingsCheckup,
        [EnumMember(Value = "Диагностика проблемы с выездом")]
        DepartureProblemDiagnosis,
        [EnumMember(Value = "Презентация/демо с выездом")]
        DeparturePresentation,
        [EnumMember(Value = "Консультирование/обучение с выездом")]
        DepartureConsultation
    }
}
