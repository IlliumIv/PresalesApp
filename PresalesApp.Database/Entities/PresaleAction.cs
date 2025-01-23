using Newtonsoft.Json;
using PresalesApp.Database.Enums;
using PresalesApp.Database.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesApp.Database.Entities;

public class PresaleAction : Entity
{
    public static readonly Dictionary<ActionType, (ActionCalculationType CalculationType, int Rank, int MinRankTime)> ActionTypes =
        new()
    {
            { ActionType.Unknown, (ActionCalculationType.Sum, 0, 0) },
            { ActionType.FullSetup, (ActionCalculationType.Unique, 4, 0) },
            { ActionType.SettingsCheckup, (ActionCalculationType.TimeSpend, 1, 15) },
            { ActionType.ProblemDiagnostics, (ActionCalculationType.TimeSpend, 1, 15) },
            { ActionType.Calculation, (ActionCalculationType.TimeSpend, 1, 10) },
            { ActionType.SpecificationCheckup, (ActionCalculationType.Unique, 1, 0) },
            { ActionType.SpecificationCreateFromTemplate, (ActionCalculationType.Unique, 3, 0) },
            { ActionType.SpecificationCreate, (ActionCalculationType.Unique, 5, 0) },
            { ActionType.RequirementsCreate, (ActionCalculationType.Unique, 2, 0) },
            { ActionType.Consultation, (ActionCalculationType.TimeSpend, 1, 5) },
            { ActionType.Negotiations, (ActionCalculationType.TimeSpend, 1, 10) },
            { ActionType.Presentation, (ActionCalculationType.Sum, 3, 0) },
            { ActionType.DepartureFullSetup, (ActionCalculationType.Unique, 5, 0) },
            { ActionType.DepartureSettingsCheckup, (ActionCalculationType.Unique, 2, 0) },
            { ActionType.DepartureProblemDiagnosis, (ActionCalculationType.Unique, 2, 0) },
            { ActionType.DeparturePresentation, (ActionCalculationType.Unique, 5, 0) },
            { ActionType.DepartureConsultation, (ActionCalculationType.Unique, 2, 0) }
    };

    [JsonProperty("НомерСтроки")]
    public int Number { get; set; } = 0;

    [JsonProperty("Дата"), JsonConverter(typeof(DateTimeDeserializationConverter))]
    public DateTime Date { get; set; } = new DateTime(0, DateTimeKind.Utc);

    [JsonProperty("ТипЗадачи")]
    public ActionType Type { get; set; } = ActionType.Unknown;

    [JsonProperty("ВремяВыполнения")]
    public int TimeSpend { get; set; } = 0;

    [JsonProperty("Воронка")]
    public bool SalesFunnel { get; set; } = false;

    [JsonProperty("Описание")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("Ранг")]
    public int Rank { get; set; }

    public virtual Project? Project { get; set; }

    internal PresaleAction() { }

    public PresaleAction(Project project) => Project = project;

    [NotMapped]
    public int RegulationsRank => Type switch
    {
        ActionType.Unknown => Rank,
        _ => ActionTypes[Type].Rank
    };

    public override string ToString() => $"{{\"НомерСтроки\":\"{Number}\"," +
        $"\"Дата\":\"{Date.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
        $"\"ТипЗадачи\":\"{Type}\"," +
        $"\"ВремяВыполнения\":\"{TimeSpend}\"," +
        $"\"Воронка\":\"{SalesFunnel}\"," +
        $"\"Описание\":\"{Description}\"," +
        $"\"Проект\":\"{Project?.Number}\"}}";

    internal override bool TryUpdateIfExist(DbController.ControllerContext dbContext)
        => throw new NotImplementedException();

    internal override PresaleAction GetOrAddIfNotExist(DbController.ControllerContext dbContext)
        => throw new NotImplementedException();
}
