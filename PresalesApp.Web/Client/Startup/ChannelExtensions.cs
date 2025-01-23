using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Startup;

public static class ChannelExtensions
{
    public static GrpcChannel GetAuthChannel(this NavigationManager navigationManager, string token)
        => new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()))
            .ToAuthChannel(navigationManager.BaseUri, token);

    public static GrpcChannel ToAuthChannel(this HttpClient httpClient, string baseUri, string token)
        => GrpcChannel.ForAddress(baseUri, new()
            {
                HttpClient = httpClient,
                Credentials = ChannelCredentials.Create(new SslCredentials(),
                    _GetJwtCredentials(token))
            });

    private static CallCredentials _GetJwtCredentials(string token)
        => CallCredentials.FromInterceptor((_, metadata) =>
        {
            if (!string.IsNullOrEmpty(token))
                metadata._AddJwt(token);

            return Task.CompletedTask;
        });

    private static void _AddJwt(this Metadata metadata, string token)
        => metadata.Add("Authorization", $"Bearer {token}");
}
