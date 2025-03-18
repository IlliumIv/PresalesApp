using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Helpers;
using PresalesApp.Service.Extensions;
using PresalesApp.Service.MacroscopEntities;
using Serilog;
using System.Security.Authentication;
using System.Text;

namespace PresalesApp.Service.Controllers;

public class ApiController(AppSettings appSettings) : Api.ApiBase
{
    private readonly AppSettings _AppSettings = appSettings;

    private static string _CachedOverview = @"{ ""Всего"": 0.0, ""Топ"": [
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 },
        { ""Имя"": ""Doe John Jr"", ""Сумма"": 0.0 } ] }";

    private readonly HashSet<(string LastName, string FirstName)> _Presales =
    [
        new ("Бурнышев", "Роман"),
        new ("Поляков", "Иван"),
        new ("Молоков", "Антон"),
        new ("Кладов", "Артем"),
        new ("Бузмаков", "Андрей"),
        new ("Гущин", "Антон"),
        new ("Попов", "Александр"),
        new ("Мочалов", "Александр"),
        new ("Латышев", "Никита"),
    ];

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        Console.WriteLine("SayHello called.");
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task<IsActionSuccess> SetProjectFunnelStage(NewProjectFunnelStage request, ServerCallContext context)
    {
        var newStage = request.NewStage;
        var projectNumber = request.ProjectNumber;

        var (IsSuccess, ErrorMessage) = await Project.SetFunnelStageAsync(newStage.Translate(), projectNumber);

        return new IsActionSuccess
        {
            IsSuccess = IsSuccess,
            ErrorMessage = ErrorMessage
        };
    }

    public override async Task GetPresalesArrival(Empty request, IServerStreamWriter<Arrival> responseStream, ServerCallContext context)
    {
        if(string.IsNullOrEmpty(_AppSettings.Macroscop.Host) ||
            string.IsNullOrEmpty(_AppSettings.Macroscop.Username))
        {
            return;
        }

        foreach(var e in _GetArrived()) _Send(responseStream, e);

        using var client = _GetClient(out var auth);
        var r = $"event?responsetype=json";
        r += $"&filter={_AppSettings.Macroscop.EventId}";
        r += string.IsNullOrEmpty(_AppSettings.Macroscop.EntranceChannelId) ? "" : $"&channelid={_AppSettings.Macroscop.EntranceChannelId}";

        var m = new HttpRequestMessage(HttpMethod.Get, r);
        m.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(auth))}");

        client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

        try
        {
            using var resp = client.Send(m, HttpCompletionOption.ResponseHeadersRead);
            using var content = await resp.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(content);

            var eventStrings = string.Empty;

            while(reader.ReadLine() is string line)
            {
                if(line == "}")
                {
                    var e = new Event(eventStrings + "}");
                    if(_IsPresale(e))
                    {
                        _Send(responseStream, e);
                    }

                    eventStrings = "";
                }
                else
                {
                    eventStrings += line;
                }
            }
        }
        catch(Exception e) { Log.Information(e.Message); }
    }

    public override async Task<SalesOverview> GetSalesOverview(SalesOverviewRequest request, ServerCallContext context)
    {
        var prevStartTime = request?.Previous?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
        var prevEndTime = request?.Previous?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
        var startTime = request?.Current?.From?.ToDateTime().AddHours(5) ?? DateTime.MinValue;
        var endTime = request?.Current?.To?.ToDateTime().AddHours(5) ?? DateTime.MinValue;

        var httpClient = new HttpClient() { BaseAddress = new Uri($"http://{_AppSettings._1C.Host}") };
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"trade/hs/API/GetTradeData?" +
            $"begin={prevStartTime:yyyy-MM-ddTHH:mm:ss}" +
            $"&end={prevEndTime:yyyy-MM-ddTHH:mm:ss}" +
            $"&begin2={startTime:yyyy-MM-ddTHH:mm:ss}" +
            $"&end2={endTime:yyyy-MM-ddTHH:mm:ss}");
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{_AppSettings._1C.Username}:{_AppSettings._1C.Password}"));

        httpRequest.Headers.Add("Authorization", $"Basic {auth}");

        var httpResponse = await httpClient.SendAsync(httpRequest);
        var result = await httpResponse.Content.ReadAsStringAsync();
        if(!httpResponse.IsSuccessStatusCode)
        {
            result = _CachedOverview;
        }

        _CachedOverview = result;

        var response = JsonConvert.DeserializeObject<dynamic>(_CachedOverview);
        var reply = new SalesOverview();

        if(response != null)
        {
#pragma warning disable IDE0008 // Use explicit type
            foreach(var manager in response.Топ)
            {
                reply.CurrentTopSalesManagers.Add(new Manager { Name = string.Join(" ", ((string)manager.Имя).Split().Take(2)), Profit = (decimal)manager.Сумма });
            }
#pragma warning restore IDE0008 // Use explicit type

            reply.PreviousActualProfit = response.Факт1 is null ? 0 : (decimal)response.Факт1;
            reply.CurrentActualProfit = response.Факт2 is null ? 0 : (decimal)response.Факт2;
            reply.CurrentSalesTarget = response.План2 is null ? 0 : (decimal)response.План2;
        }

        return reply;
    }

    private static void _Send(IServerStreamWriter<Arrival> responseStream, Event @event) =>
        responseStream.WriteAsync(new Arrival()
        {
            Name = $"{@event.LastName} {@event.FirstName}",
            Timestamp = Timestamp.FromDateTime(@event.Timestamp),
            ImageBytes = @event.ImageBytes
        }).Wait();

    private HttpClient _GetClient(out string auth)
    {
        auth = $"{_AppSettings.Macroscop.Username}:{(_AppSettings.Macroscop.IsActiveDirectoryUser ? _AppSettings.Macroscop.Password : _AppSettings.Macroscop.Password.CreateMD5())}";

        var client = _AppSettings.Macroscop.UseSSL switch
        {
            true => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                SslProtocols = SslProtocols.Tls12
            }),
            _ => new HttpClient()
        };

        client.BaseAddress = new Uri($"http{(_AppSettings.Macroscop.UseSSL ? "s" : "")}://{_AppSettings.Macroscop.Host}:{_AppSettings.Macroscop.Port}");

        return client;
    }

    private HashSet<Event> _GetArrived()
    {
        var client = _GetClient(out var auth);
        var events = new HashSet<Event>();
        var startTime = DateTime.Now.Date;

        while(events.Count % 1000 == 0)
        {
            var r = $"specialarchiveevents?" +
                $"startTime={startTime.ToUniversalTime():dd.MM.yyyy HH:mm:ss}" +
                $"&endTime={startTime.AddDays(1).ToUniversalTime():dd.MM.yyyy HH:mm:ss}" +
                $"&eventId={_AppSettings.Macroscop.EventId}";
            r += string.IsNullOrEmpty(_AppSettings.Macroscop.EntranceChannelId) ? "" : $"&channelid={_AppSettings.Macroscop.EntranceChannelId}";
            var m = new HttpRequestMessage(HttpMethod.Get, r);
            m.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(auth))}");

            try
            {
                var resp = client.Send(m);

                if(resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var answer = resp.Content.ReadAsStringAsync().Result;

                    if(string.IsNullOrEmpty(answer))
                    {
                        Log.Information($"Empty answer for request:\n{resp.RequestMessage}");
                        break;
                    }

                    var new_events = _ParseAnswer(answer);
                    events.UnionWith(new_events);
                    startTime = events.Last().Timestamp;
                }
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
        }

        var arrived = new HashSet<Event>();

        foreach(var (LastName, FirstName) in _Presales)
        {
            var e = events.FirstOrDefault(e => e.FirstName == FirstName && e.LastName == LastName);

            if(e != null)
            {
                arrived.Add(e);
            }
        }

        return arrived;
    }

    private bool _IsPresale(Event? @event) =>
        @event != null && _Presales.Contains((@event.LastName, @event.FirstName));

    private static HashSet<Event> _ParseAnswer(string answer)
    {
        using var reader = new StringReader(answer);
        var eventStrings = string.Empty;
        var line = string.Empty;

        var events = new HashSet<Event>();

        while((line = reader.ReadLine()) != null)
        {
            if(line == "}{")
            {
                events.Add(new Event(eventStrings + "}"));
                eventStrings = "{";
            }
            else
            {
                eventStrings += line;
            }
        }

        events.Add(new Event(eventStrings));
        return events;
    }
}