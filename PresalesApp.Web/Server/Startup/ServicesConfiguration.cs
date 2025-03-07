using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Server.Authorization;
using Radzen;
using Serilog;
using System.Text;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Startup;

public static class ServicesConfiguration
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings(builder.Configuration);
        builder.Services.AddScoped(s => appSettings);

        DbController.Configure(appSettings);

        builder.Logging.ClearProviders();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

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

        builder.Services.AddSingleton(sp =>
        {
            // Get the address that the app is currently running at
            var server = sp.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            var baseAddress = addressFeature?.Addresses.First();
            return new HttpClient { BaseAddress = new Uri(baseAddress ?? string.Empty) };
        });

        builder.Services.AddGrpc();

        builder.Services.AddDbContext<AuthDbContext>();

        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        var tokenParams = new TokenParameters(appSettings);

        builder.Services.AddSingleton(tokenParams);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = tokenParams.Issuer,
                ValidAudience = tokenParams.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(tokenParams.SecretKey))
            };
        });

        builder.Services.Configure<IdentityOptions>(options =>
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

        builder.Services.AddAuthorization();

        return builder;
    }
}
