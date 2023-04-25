using Blazored.LocalStorage;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PresalesApp.Bridge1C;
using PresalesApp.Web.Shared;
using PresalesApp.Web.Client.Authorization;

namespace PresalesApp.Web.Client.Startup
{
    public static class ServicesConfiguration
    {
        public static WebAssemblyHostBuilder ConfigureServices(this WebAssemblyHostBuilder builder)
        {
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSingleton(services =>
            {
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(Settings.Default.Bridge1CAddress, new GrpcChannelOptions { HttpClient = httpClient });
                return new PresalesAppBridge1CApi.PresalesAppBridge1CApiClient(channel);
            });

            builder.Services.AddAuthGrpcClient<PresalesAppApi.PresalesAppApiClient>();

            builder.Services.AddBlazorise();
            builder.Services.AddBootstrap5Providers();
            builder.Services.AddFontAwesomeIcons();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddLocalization(options => options.ResourcesPath = Settings.Default.LocalizationPath);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthorizeApi>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            return builder;
        }
    }
}
