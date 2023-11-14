using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Authorization;
using PresalesApp.Web.Server.Authorization;
using Radzen;
using Serilog;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Startup;

public static class ServicesConfiguration
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings(builder.Configuration);
        _ = builder.Services.AddScoped(s => appSettings);

        DbController.Configure(appSettings);

        _ = builder.Logging.ClearProviders();
        _ = builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        _ = builder.Services.AddControllersWithViews();
        _ = builder.Services.AddRazorPages();
        _ = builder.Services.AddScoped<DialogService>();
        _ = builder.Services.AddScoped<NotificationService>();
        _ = builder.Services.AddScoped<TooltipService>();
        _ = builder.Services.AddScoped<ContextMenuService>();
        _ = builder.Services.AddSingleton(sp =>
        {
            // Get the address that the app is currently running at
            var server = sp.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            string baseAddress = addressFeature.Addresses.First();
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        });

        _ = builder.Services.AddGrpc();

        _ = builder.Services.AddDbContext<UsersContext>();

        _ = builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<UsersContext>()
            .AddDefaultTokenProviders();

        var tokenParams = new TokenParameters(appSettings);
        _ = builder.Services.AddSingleton(tokenParams);

        _ = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.UseSecurityTokenValidators = true;
            options.SecurityTokenValidators.Add(new JwtTokenValidator(tokenParams));
        });

        _ = builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
        });

        /*
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: "cors_policy",
                policy =>
                {
                    policy
                    // .WithOrigins("http://localhost", "https://localhost", "https://127.0.0.1")
                    // .AllowAnyMethod()
                    // .AllowAnyOrigin()
                    .AllowAnyHeader() // (Причина: заголовок «access-control-allow-origin» не разрешён согласно заголовку «Access-Control-Allow-Headers» из ответа CORS preflight).
                    ;
                });
        });
        //*/

        _ = builder.Services.AddAuthorization();

        return builder;
    }
}
