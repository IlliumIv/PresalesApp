using Blazored.LocalStorage;
using Grpc.Core;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace PresalesApp.Web.Client.Startup
{
    public static class StartupExtensions
    {
        public static void AddAuthGrpcClient<T>(this IServiceCollection services) where T : ClientBase
        {
            services.AddScoped(provider =>
            {
                var nav = provider.GetService<NavigationManager>();
                var storage = provider.GetService<ISyncLocalStorageService>();

                if (nav != null && storage != null)
                {
                    string token = storage.GetItem<string>(Settings.Default.StorageTokenKey);
                    var client = (T?)Activator.CreateInstance(typeof(T), nav.GetAuthChannel(token));

                    if (client != null)
                    {
                        return client;
                    }
                }

                return Activator.CreateInstance<T>();
            });
        }
    }
}
