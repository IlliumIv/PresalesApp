using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PresalesApp.Web.Client.Services.Authorization;

public static class JwtTokenExtensions
{
    public static AuthenticationState GetStateFromJwt(string token) =>
        new(new ClaimsPrincipal(GetIdentityFromJwtToken(token)));

    private static ClaimsIdentity GetIdentityFromJwtToken(string token) =>
        new(ParseClaimsFromJwt(token), "jwt");

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
}
