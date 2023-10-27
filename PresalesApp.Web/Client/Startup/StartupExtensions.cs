using Blazored.LocalStorage;
using Grpc.Core;
using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Startup
{
    public static class StartupExtensions
    {
        public static void AddAuthGrpcClientTransient<T>(this IServiceCollection services) where T : ClientBase
        {
            services.AddTransient(provider =>
            {
                var nav = provider.GetService<NavigationManager>();
                var storage = provider.GetService<ISyncLocalStorageService>();

                if (nav != null && storage != null)
                {
                    string token = storage.GetItem<string>("token");
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
