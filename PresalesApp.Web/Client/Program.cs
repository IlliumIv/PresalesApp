using Blazored.LocalStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PresalesApp.Bridge1C;
using PresalesApp.Web.Shared;
using System.Globalization;

namespace PresalesApp.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSingleton(services =>
            {
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
                return new PresalesAppApi.PresalesAppApiClient(channel);
            });

            builder.Services.AddSingleton(services =>
            {
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(Settings.Default.Bridge1CAddress, new GrpcChannelOptions { HttpClient = httpClient });
                return new PresalesAppBridge1CApi.PresalesAppBridge1CApiClient(channel);
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddLocalization(options => options.ResourcesPath = Settings.Default.LocalizationPath);

            var app = builder.Build();

            var culture = await app.Services.GetService<ILocalStorageService>().GetItemAsStringAsync(Settings.Default.StorageCultureKey);
            var default_culture = new CultureInfo(culture ?? "ru-RU");
            CultureInfo.CurrentCulture = default_culture;
            CultureInfo.CurrentUICulture = default_culture;

            await app.RunAsync();
        }
    }
}