using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Authorization;
using PresalesApp.Web.Server.Authorization;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Startup
{
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
            builder.Services.AddGrpc();

            builder.Services.AddDbContext<UsersContext>();

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UsersContext>()
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
                options.UseSecurityTokenValidators = true;
                options.SecurityTokenValidators.Add(new JwtTokenValidator(tokenParams));
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
}
