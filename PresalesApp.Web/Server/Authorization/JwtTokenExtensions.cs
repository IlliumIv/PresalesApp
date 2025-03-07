using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PresalesApp.Database.Entities;
using PresalesApp.Web.Server;
using PresalesApp.Web.Server.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PresalesApp.Web.Authorization
{
    public static class JwtTokenExtensions
    {
        public static async Task<string> GenerateJwtToken(this User user,
            TokenParameters tokenParameters,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(ClaimsIdentity.DefaultNameClaimType, user.UserName ?? string.Empty),
                // new(ClaimTypes.Name, user.ProfileName),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenParameters.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: tokenParameters.Issuer,
                audience: tokenParameters.Audience,
                claims: claims,
                expires: tokenParameters.Expiry,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
