using Blazored.LocalStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using pax.BlazorChartJs;
using PresalesApp.Web.Client.Services.Authorization;
using Radzen;

namespace PresalesApp.Web.Client.Startup;

public static class ServicesConfiguration
{
    public static WebAssemblyHostBuilder ConfigureServices(this WebAssemblyHostBuilder builder)
    {
        var settings = new Settings(builder.Configuration);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped<Radzen.DialogService>();
        builder.Services.AddScoped<Radzen.NotificationService>();
        builder.Services.AddScoped<Radzen.TooltipService>();
        builder.Services.AddScoped<Radzen.ContextMenuService>();
        builder.Services.AddScoped<Radzen.ThemeService>();
        builder.Services.AddRadzenCookieThemeService(options =>
        {
            options.Name = "PresalesAppTheme";
            options.Duration = TimeSpan.FromDays(365);
        });

        builder.Services.AddTransient(provider =>
        {
            var channel = GrpcChannel.ForAddress(
                $"http{(settings.UseSSL ? "s" : "")}:" +
                $"//{settings.ServiceHost}:{settings.ServicePort}",
                new()
                {
                    HttpClient = new(new GrpcWebHandler(GrpcWebMode.GrpcWeb,
                    new HttpClientHandler()))
                });
            return new Service.Api.ApiClient(channel);
        });

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

        builder.Services.AddChartJs(options =>
        {
            options.ChartJsLocation = "https://cdn.jsdelivr.net/npm/chart.js";
            options.ChartJsPluginDatalabelsLocation = "https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2";
        });

        builder.Services.AddAuthGrpcClientTransient<PresalesApp.Shared.
            PresalesAppService.PresalesAppServiceClient>();
        builder.Services.AddAuthGrpcClientTransient<Authorization
            .AuthorizationService.AuthorizationServiceClient>();
        builder.Services.AddAuthGrpcClientTransient<DistanceCalculator
            .DistanceCalculatorService.DistanceCalculatorServiceClient>();
        builder.Services.AddAuthGrpcClientTransient<ImageProvider.
            ImageProviderService.ImageProviderServiceClient>();

        builder.Services.AddScoped<AuthorizationService>();
        builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider
            => provider.GetRequiredService<IdentityAuthenticationStateProvider>());

        builder.Services.AddAuthorizationCore();

        return builder;
    }
}
