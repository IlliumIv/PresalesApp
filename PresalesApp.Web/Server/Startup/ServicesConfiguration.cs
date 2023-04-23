using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Database.Authorization;
using PresalesApp.Database.Entities;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Web.Server.Startup
{
    public static class ServicesConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddGrpc();

            builder.Services.AddDbContext<TODOReadWriteContext>();

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<TODOReadWriteContext>()
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
