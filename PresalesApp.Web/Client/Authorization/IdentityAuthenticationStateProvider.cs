using System.Security.Claims;
using Blazored.LocalStorage;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components.Authorization;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Authorization
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly PresalesAppApi.PresalesAppApiClient _apiClient;

        public static UserProfile Profile { get; private set; } = new UserProfile();

        public IdentityAuthenticationStateProvider(
            ILocalStorageService localStorage,
            PresalesAppApi.PresalesAppApiClient apiClient)
        {
            _localStorage = localStorage;
            _apiClient = apiClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("token");

            return await IsTokenValid(token)
                ? JwtTokenExtensions.GetStateFromJwt(token) : new(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void MarkUserAsAuthenticated(UserInfo userInfo)
        {
            var authState = Task.FromResult(JwtTokenExtensions.GetStateFromJwt(userInfo.Token));
            Profile = userInfo.Profile;
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkLogouted() => NotifyAuthenticationStateChanged(Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));

        private async Task<bool> IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var authUser =
                    await _apiClient.GetUserProfileAsync(new Empty());

                if (authUser.ResultCase == UserInfoResponse.ResultOneofCase.Profile)
                {
                    Profile = authUser.Profile;
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }
    }
}
