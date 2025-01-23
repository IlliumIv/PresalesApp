using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PresalesApp.Web.Client.Startup;

namespace PresalesApp.Web.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var app = builder.ConfigureServices().Build();
        await app.ConfigureApplication();
        await app.RunAsync();
    }
}