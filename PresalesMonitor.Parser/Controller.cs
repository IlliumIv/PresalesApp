using Newtonsoft.Json;
using PresalesMonitor.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PresalesMonitor
{
    internal class Response<T>
    {
        public HttpResponseMessage ResponseMessage { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Response(HttpResponseMessage response, DateTime from, DateTime to)
        {
            ResponseMessage = response;
            From = from;
            To = to;
        }
    }
    public static class Controller
    {
        private static readonly Queue<Task<Response>> _tasks = new();
        private static readonly FileInfo _workLog = new("Parser.log");
        private static readonly FileInfo _errorLog = new("Error.log");

        public static void AddTask<T>(int delay, DateTime from, DateTime to) where T : notnull
        {
            _tasks.Enqueue(GetResponseFromServerAsync<T>(delay, from, to));
        }

        public static async void RunAsync()
        {
            while(_tasks.Count > 0)
            {
                var some = await _tasks.Dequeue();
                ProceedResponseAsync(await _tasks.Dequeue());
            }
        }
        private static HttpRequestMessage GenerateRequest<T>(DateTime from, DateTime to) where T : notnull
        {
            var request = $"trade/hs/API/Get{typeof(T).Name}s?" +
                $"begin={from:yyyy-MM-ddTHH:mm:ss}" +
                $"&end={to:yyyy-MM-ddTHH:mm:ss}";
            return new HttpRequestMessage(HttpMethod.Get, request);
        }
        private static async Task<Response<T>?> GetResponseFromServerAsync<T>(int delay, DateTime from, DateTime to) where T : notnull
        {
            await Task.Delay(delay);
            try
            {
                if (Settings.TryGetSection<Settings.Connection>(
                        out ConfigurationSection? r) && r != null)
                {
                    var connSettings = (Settings.Connection)r;
                    var _auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        $"{connSettings.Username}:{connSettings.Password}"));
                    var httpClient = new HttpClient { BaseAddress = new Uri($"http://{connSettings.Url}") };
                    var message = GenerateRequest<T>(from, to);
                    message.Headers.Add("Authorization", $"Basic {_auth}");
                    using var sw = File.AppendText(_workLog.FullName);
                    sw.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Request: {message.RequestUri}");
                    HttpResponseMessage response = await httpClient.SendAsync(message);
                    return new Response<T>(response, from, to);
                }
                else throw new ConfigurationErrorsException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                using var sw = File.AppendText(_errorLog.FullName);
                sw.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] {e}\n");
            }
            return null;
        }
        private static async void ProceedResponseAsync<T>(Response<T>? response)
        {
            if (response == null) return;

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                var dynObjects = JsonConvert.DeserializeObject<dynamic>(
                        await response.ResponseMessage.Content.ReadAsStringAsync(),
                        new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
                if (dynObjects == null) return;

                foreach (var obj in dynObjects)
                {
                            AddToDbProceedQueue(obj.ToObject<T>());
                }
            }
            // Some Work
        }
        public static void AddToDbProceedQueue<T>(T obj)
        {

        }
    }
}
