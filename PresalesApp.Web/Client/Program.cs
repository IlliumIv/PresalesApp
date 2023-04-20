using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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

            builder.Services.AddBlazorise();
            builder.Services.AddBootstrap5Providers();
            builder.Services.AddFontAwesomeIcons();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddLocalization(options => options.ResourcesPath = Settings.Default.LocalizationPath);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthorizeApi>();

            var app = builder.Build();

            var storage = app.Services.GetService<ILocalStorageService>();
            var culture = storage is null ? null : await storage.GetItemAsStringAsync(Settings.Default.StorageCultureKey);
            var default_culture = new CultureInfo(culture == null || culture == string.Empty ? "ru-RU" : culture);
            await storage.SetItemAsStringAsync(Settings.Default.StorageCultureKey, default_culture.Name);

            CultureInfo.DefaultThreadCurrentCulture = default_culture;
            CultureInfo.DefaultThreadCurrentUICulture = default_culture;

            await app.RunAsync();
        }
    }
}