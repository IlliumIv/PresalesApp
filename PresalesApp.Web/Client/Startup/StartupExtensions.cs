using Blazored.LocalStorage;
using Grpc.Core;
using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Startup;

public static class StartupExtensions
{
    public static void AddAuthGrpcClientTransient<T>(this IServiceCollection services)
        where T : ClientBase => services.AddTransient(provider =>
        {
            var navigationManager = provider.GetService<NavigationManager>();
            var storage = provider.GetService<ISyncLocalStorageService>();

            if (navigationManager != null && storage != null)
            {
                var token = storage.GetItem<string>("token");
                var client = (T?)Activator.CreateInstance(typeof(T),
                    navigationManager.GetAuthChannel(token ?? string.Empty));

                if (client != null)
                {
                    return client;
                }
            }

            return Activator.CreateInstance<T>();
        });
}
