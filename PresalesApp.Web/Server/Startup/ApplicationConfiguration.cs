using PresalesApp.Web.Server.Extensions;
using PresalesApp.Web.Server.Services;
using PresalesApp.Web.Services;

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
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

        app.MapGrpcService<ApiService>().EnableGrpcWeb(); //.RequireCors("cors_policy");

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
