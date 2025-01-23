using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PresalesApp.Web.Client.Services.Authorization;

public static class JwtTokenExtensions
{
    public static AuthenticationState GetStateFromJwt(string? token)
        => string.IsNullOrEmpty(token)
            ? new(new())
            : new(new(_GetIdentityFromJwtToken(token)));

    private static ClaimsIdentity _GetIdentityFromJwtToken(string token)
    {
        var some = new ClaimsIdentity(_ParseClaimsFromJwt(token), "jwt");
        return some;
    }

    private static IEnumerable<Claim> _ParseClaimsFromJwt(string token)
    {
        var se = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var some = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        return some;
    }
}
