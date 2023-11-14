using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PresalesApp.Web.Client.Services.Authorization;
using Radzen;
using AppApi = PresalesApp.Web.Shared.Api;
using BridgeApi = PresalesApp.Bridge1C.Api;

namespace PresalesApp.Web.Client.Startup;

public static class ServicesConfiguration
{
    public static WebAssemblyHostBuilder ConfigureServices(this WebAssemblyHostBuilder builder)
    {
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        _ = builder.Services.AddScoped<DialogService>();
        _ = builder.Services.AddScoped<NotificationService>();
        _ = builder.Services.AddScoped<TooltipService>();
        _ = builder.Services.AddScoped<ContextMenuService>();

        _ = builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        _ = builder.Services.AddSingleton(services =>
        {
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
            var channel = GrpcChannel.ForAddress("https://127.0.0.1:33443", new GrpcChannelOptions { HttpClient = httpClient });
            return new BridgeApi.ApiClient(channel);
        });

        _ = builder.Services.AddBlazorise();
        _ = builder.Services.AddBootstrap5Providers();
        _ = builder.Services.AddFontAwesomeIcons();

        _ = builder.Services.AddBlazoredLocalStorage();
        _ = builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

        builder.Services.AddAuthGrpcClientTransient<AppApi.ApiClient>();
        _ = builder.Services.AddScoped<AuthorizationService>();
        _ = builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
        _ = builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<IdentityAuthenticationStateProvider>());

        _ = builder.Services.AddAuthorizationCore();

        return builder;
    }
}
