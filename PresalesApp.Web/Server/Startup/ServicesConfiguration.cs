using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Database.Authorization;
using PresalesApp.Database.Entities;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System.Reflection;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Startup
{
    public static class ServicesConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            #region Logger configuration

            var log_template = "[{@l:u3}] [{@t:dd.MM.yyyy HH:mm:ss.fff} ThreadId={ThreadId}, ProcessId={ProcessId}]\n" +
                "      {#if @m <> ''}{@m}\n{#end}{@x}\n";
            var log_directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                Directory.GetCurrentDirectory();

            var logger = new LoggerConfiguration()
                // https://github.com/serilog/serilog/wiki/Enrichment
                .Enrich.FromLogContext()
                .Enrich.WithThreadId() // Serilog.Enrichers.Thread
                .Enrich.WithProcessId() // Serilog.Enrichers.Process
                                        // .Enrich.WithMachineName() // Serilog.Enrichers.Environment
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumStringLength(100)
                .Destructure.ToMaximumCollectionCount(10)
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console(
                    formatter: new ExpressionTemplate(log_template, theme: TemplateTheme.Code),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(log_template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    path: $"{log_directory}/Logs/Server.log"))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(log_template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    path: $"{log_directory}/Logs/Error.log"))
                .CreateLogger()
                // .ForContext<Program>()
                ;

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            #endregion

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddGrpc();

            builder.Services.AddDbContext<UsersContext>();

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();

            var tokenParams = new Database.TokenParameters();
            builder.Services.AddSingleton(tokenParams);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SecurityTokenValidators.Add(new JwtTokenValidator(tokenParams));
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "cors_policy",
                    policy =>
                    {
                        policy
                        .WithOrigins("https://localhost:45443")
                        // .AllowAnyMethod()
                        .AllowAnyHeader() // (Причина: заголовок «access-control-allow-origin» не разрешён согласно заголовку «Access-Control-Allow-Headers» из ответа CORS preflight).
                        ;
                    });
            });

            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
