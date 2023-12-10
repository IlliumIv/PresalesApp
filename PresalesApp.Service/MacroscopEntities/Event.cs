using Google.Protobuf.Reflection;
using Newtonsoft.Json;
using System.Reflection;
using static PresalesApp.Service.MacroscopEntities.Enums;

namespace PresalesApp.Service.MacroscopEntities;

public class Event
{
    private readonly dynamic _Event;
    public DateTime Timestamp => DateTime.SpecifyKind((string)_Event.Timestamp != string.Empty ?
        DateTime.Parse((string)_Event.Timestamp) : DateTime.MinValue, DateTimeKind.Utc);
    // public string ExternalId => _event.ExternalId;
    // public string ChannelId => _event.ChannelId;
    // public string ChannelName => _event.ChannelName;
    public bool IsIdentified => _Event.IsIdentified != null && (bool)_Event.IsIdentified;
    public string LastName => _Event.lastName;
    public string FirstName => _Event.firstName;
    // public string Patronymic => _event.patronymic;
    // public string[] Groups => (string)_event.groups != string.Empty ? ((string)_event.groups).Replace(" ", "").Split(',') : Array.Empty<string>();
    // public string AdditionalInfo => _event.additionalInfo;
    // public double Similarity => (string)_event.Similarity != string.Empty ? double.Parse((string)_event.Similarity) : double.MinValue;
    // public int Age => _event.Age;
    // public Gender Gender => _event.Gender;
    public string ImageBytes => _Event.ImageBytes;
    public Emotion Emotion => Enum.Parse<Emotion>(string.Join("\n", (_Event.Emotion).ToObject<string[]>()));
    public double EmotionConfidence => (string)_Event.EmotionConfidence != string.Empty ? double.Parse((string)_Event.EmotionConfidence) : double.MinValue;

    public Event(string jsonBody)
    {
        var parsedBody = JsonConvert.DeserializeObject<dynamic>(jsonBody);
        _Event = parsedBody ?? "";
    }
}
