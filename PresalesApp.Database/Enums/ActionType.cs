using Newtonsoft.Json;
using PresalesApp.Database.Helpers;
using System.Runtime.Serialization;

namespace PresalesApp.Database.Enums;

///<summary>Регламентные типы действий</summary>
[JsonConverter(typeof(SafeStringEnumConverter), Unknown)]
public enum ActionType
{
    ///<summary>Прочее</summary>
    [EnumMember(Value = "Прочее")]
    Unknown,

    ///<summary>Полная настройка системы у заказчика</summary>
    [EnumMember(Value = "Полная настройка системы у заказчика")]
    FullSetup,

    ///<summary>Корректировка настроек</summary>
    [EnumMember(Value = "Корректировка настроек")]
    SettingsCheckup,

    ///<summary>Диагностика проблемы</summary>
    [EnumMember(Value = "Диагностика проблемы")]
    ProblemDiagnostics,

    ///<summary>Разработка технического решения</summary>
    [EnumMember(Value = "Разработка технического решения")]
    Calculation,

    ///<summary>Корректировка ТЗ или проработка ТЗ на возможность использования ПО Макроскоп</summary>
    [EnumMember(Value = "Корректировка ТЗ или проработка ТЗ на возможность использования ПО Макроскоп")]
    SpecificationCheckup,

    ///<summary>Разработка ТЗ на основе шаблонов</summary>
    [EnumMember(Value = "Разработка ТЗ на основе шаблонов")]
    SpecificationCreateFromTemplate,

    ///<summary>Разработка ТЗ «с нуля»</summary>
    [EnumMember(Value = "Разработка ТЗ «с нуля»")]
    SpecificationCreate,

    ///<summary>Составление требований к ПО в тендерную документацию</summary>
    [EnumMember(Value = "Составление требований к ПО в тендерную документацию")]
    RequirementsCreate,

    ///<summary>Консультация</summary>
    [EnumMember(Value = "Консультация")]
    Consultation,

    ///<summary>Переговоры с клиентом</summary>
    [EnumMember(Value = "Переговоры с клиентом")]
    Negotiations,

    ///<summary>Презентация продукта/демо</summary>
    [EnumMember(Value = "Презентация продукта/демо")]
    Presentation,

    ///<summary>Полная настройка системы у заказчика с выездом</summary>
    [EnumMember(Value = "Полная настройка системы у заказчика с выездом")]
    DepartureFullSetup,

    ///<summary>Корректировка настроек с выездом</summary>
    [EnumMember(Value = "Корректировка настроек с выездом")]
    DepartureSettingsCheckup,

    ///<summary>Диагностика проблемы с выездом</summary>
    [EnumMember(Value = "Диагностика проблемы с выездом")]
    DepartureProblemDiagnosis,

    ///<summary>Презентация/демо с выездом</summary>
    [EnumMember(Value = "Презентация/демо с выездом")]
    DeparturePresentation,

    ///<summary>Консультирование/обучение с выездом</summary>
    [EnumMember(Value = "Консультирование/обучение с выездом")]
    DepartureConsultation
}
