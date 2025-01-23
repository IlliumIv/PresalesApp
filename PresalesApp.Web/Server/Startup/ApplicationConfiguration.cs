using PresalesApp.Web.Server.Extensions;
using PresalesApp.Web.Server.Services;

namespace PresalesApp.Web.Server.Startup;

public static class ApplicationConfiguration
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
            app.UseEndpointDebug(); // send "/$endpoint" for route debug
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios,
            // see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseGrpcWeb();
        // app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGrpcService<AuthorizationService>().EnableGrpcWeb(); //.RequireCors("cors_policy");
        app.MapGrpcService<ImageProviderService>().EnableGrpcWeb();
        app.MapGrpcService<PresalesAppService>().EnableGrpcWeb();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
