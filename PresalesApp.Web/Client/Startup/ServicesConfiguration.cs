using Blazored.LocalStorage;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BridgeApi = PresalesApp.Bridge1C.Api;
using AppApi = PresalesApp.Web.Shared.Api;
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
                var channel = GrpcChannel.ForAddress("https://127.0.0.1:33443", new GrpcChannelOptions { HttpClient = httpClient });
                return new BridgeApi.ApiClient(channel);
            });

            builder.Services.AddAuthGrpcClient<AppApi.ApiClient>();

            builder.Services.AddBlazorise();
            builder.Services.AddBootstrap5Providers();
            builder.Services.AddFontAwesomeIcons();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthorizeApi>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            return builder;
        }
    }
}
