using Grpc.Core;
using Newtonsoft.Json;
using PresalesApp.ImageProvider;
using System.Security.Authentication;

namespace PresalesApp.Web.Server.Services;

public class ImageProviderService(ILogger<ImageProviderService> logger)
    : PresalesApp.ImageProvider.ImageProviderService.ImageProviderServiceBase
{
    private readonly ILogger<ImageProviderService> _Logger = logger;

    #region Private Static

    #region Members

    private static readonly bool _IsDevelopment =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    private static GetImageResponse _CashedImageGirl = new()
    {
        Image = new()
        {
            Raw = "https://images.unsplash.com/photo-1666932999928-f6029c081d77" +
                "?ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3",
            Full = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80",
            Regular = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=1080",
            Small = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=400",
            Thumb = "https://images.unsplash.com/photo-1666932999928-f6029c081d77?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2Njk3MjU4NTk&ixlib=rb-4.0.3&q=80&w=200",
            SmallS3 = "https://s3.us-west-2.amazonaws.com/images.unsplash.com/small/photo-1666932999928-f6029c081d77",
            AltDescription = "",
            AuthorName = "Dmitry Ganin",
            SourceName = "Unsplash",
            AuthorUrl = @"https://unsplash.com/@ganinph",
            SourceUrl = @"https://unsplash.com/",
            Liked = false,
            Id = "wGENt52EEnU"
        }
    };

    private static GetImageResponse _CashedImageNY = new()
    {
        Image = new()
        {
            Raw = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c" +
                "?ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3",
            Full = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80",
            Regular = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=1080",
            Small = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=400",
            Thumb = "https://images.unsplash.com/photo-1514803530614-3a2bef88f31c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg" +
                "&ixid=MnwzODQ4NjV8MHwxfHJhbmRvbXx8fHx8fHx8fDE2NzA1MDkwMTY&ixlib=rb-4.0.3&q=80&w=200",
            SmallS3 = "https://s3.us-west-2.amazonaws.com/images.unsplash.com/small/photo-1514803530614-3a2bef88f31c",
            AltDescription = "person holding sparkler",
            AuthorName = "Chinh Le Duc",
            SourceName = "Unsplash",
            AuthorUrl = @"https://unsplash.com/@mero_dnt",
            SourceUrl = @"https://unsplash.com/",
        }
    };

    #endregion

    #endregion

    #region Service Implementation

    public override async Task<GetImageResponse>
        GetImage (GetImageRequest request, ServerCallContext context)
    {
        if(_IsDevelopment)
        {
            return _CashedImageGirl;
        }

        try
        {
            var clientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                SslProtocols = SslProtocols.Tls12
            };
            var unsplashClient = new HttpClient(clientHandler) { BaseAddress = new Uri("https://api.unsplash.com") };
            var unsplashRequest = new HttpRequestMessage(HttpMethod.Get,
                $"photos/random?{request.KeywordType.ToString().ToLower()}={request.Keyword}" +
                $"&orientation={request.Orientation.ToString().ToLower()}");
            unsplashRequest.Headers.Add("Authorization", "Client-ID zoKHly26A5L5BCYWXdctm0hc9u5JGaqcsMv_znpsIR0");
            var unsplashResponse = await unsplashClient.SendAsync(unsplashRequest);
            if(!unsplashResponse.IsSuccessStatusCode)
            {
                return _CashedImageGirl;
            }

            var response = JsonConvert.DeserializeObject<dynamic>(await unsplashResponse.Content.ReadAsStringAsync());
            if(response == null)
            {
                return request.Keyword switch
                {
                    "happy new year" => _CashedImageNY,
                    _ => _CashedImageGirl
                };
            }

            var _img = new GetImageResponse()
            {
                Image = new()
                {
                    Raw = response.urls.raw,
                    Full = response.urls.full,
                    Regular = response.urls.regular,
                    Small = response.urls.small,
                    Thumb = response.urls.thumb,
                    SmallS3 = response.urls.small_s3,
                    AltDescription = $"{response.alt_description}",
                    AuthorName = $"{response.user.name}",
                    AuthorUrl = $"{response.user.links.html}",
                    SourceName = "Unsplash",
                    SourceUrl = @"https://unsplash.com/",
                    Liked = response.liked_by_user,
                    Id = response.id
                }
            };

            switch(request.Keyword)
            {
                case "happy new year":
                    _CashedImageNY = _img;
                    break;
                default:
                    _CashedImageGirl = _img;
                    break;
            }

            return _img;
        }
        catch
        {
            return _CashedImageGirl;
        }
    }

    #endregion
}
