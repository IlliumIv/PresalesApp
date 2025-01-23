using Newtonsoft.Json;
using System.Globalization;
using static PresalesApp.Service.MacroscopEntities.Enums;

namespace PresalesApp.Service.MacroscopEntities;

public class Event
{
    private readonly dynamic _Event;

    public DateTime Timestamp => DateTime.SpecifyKind((string)_Event.Timestamp != string.Empty
        ? DateTime.ParseExact((string)_Event.Timestamp, "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture)
        : DateTime.MinValue, DateTimeKind.Utc);

    public bool IsIdentified => _Event.IsIdentified != null && (bool)_Event.IsIdentified;

    public string LastName => _Event.lastName;

    public string FirstName => _Event.firstName;

    public string ImageBytes => _Event.ImageBytes;

    public Emotion Emotion => Enum.Parse<Emotion>(string.Join("\n", _Event.Emotion.ToObject<string[]>()));

    public double EmotionConfidence => (string)_Event.EmotionConfidence != string.Empty
        ? double.Parse((string)_Event.EmotionConfidence)
        : double.MinValue;

    public Event(string jsonBody)
    {
        var parsedBody = JsonConvert.DeserializeObject<dynamic>(jsonBody);
        _Event = parsedBody ?? "";
    }
}
