using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using pax.BlazorChartJs;
using PresalesApp.Web.Client.Services.Authorization;
using Radzen;
using AppApi = PresalesApp.Web.Shared.Api;
using BridgeApi = PresalesApp.Service.Api;

namespace PresalesApp.Web.Client.Startup;

public static class ServicesConfiguration
{
    public static WebAssemblyHostBuilder ConfigureServices(this WebAssemblyHostBuilder builder)
    {
        var settings = new Settings(builder.Configuration);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<ContextMenuService>();

        builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddSingleton(services =>
        {
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
            var channel = GrpcChannel.ForAddress(
                $"http{(settings.UseSSL ? "s" : "")}://{settings.ServiceHost}:{settings.ServicePort}",
                new GrpcChannelOptions { HttpClient = httpClient });
            return new BridgeApi.ApiClient(channel);
        });

        builder.Services.AddBlazorise();
        builder.Services.AddBootstrap5Providers();
        builder.Services.AddFontAwesomeIcons();

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

        builder.Services.AddChartJs(options =>
        {
            options.ChartJsLocation = "https://cdn.jsdelivr.net/npm/chart.js";
            options.ChartJsPluginDatalabelsLocation = "https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2";
        });

        builder.Services.AddAuthGrpcClientTransient<AppApi.ApiClient>();
        builder.Services.AddScoped<AuthorizationService>();
        builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<IdentityAuthenticationStateProvider>());

        builder.Services.AddAuthorizationCore();

        return builder;
    }
}
