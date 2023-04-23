using Blazored.LocalStorage;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PresalesApp.Bridge1C;
using PresalesApp.Web.Client.Authorization;
using PresalesApp.Web.Client.Startup;
using PresalesApp.Web.Shared;
using System.Globalization;

namespace PresalesApp.Web.Client
{
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
}