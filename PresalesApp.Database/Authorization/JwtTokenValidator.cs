using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PresalesApp.Database.Authorization
{
    public class JwtTokenValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; } = int.MaxValue;

        public string Issuer { get; }

        public string Audience { get; }

        public string SecretKey { get; }

        public JwtTokenValidator(TokenParameters tokenParameters)
        {
            Issuer = tokenParameters.Issuer;
            Audience = tokenParameters.Audience;
            SecretKey = tokenParameters.SecretKey;
        }

        public bool CanReadToken(string securityToken) => true;

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = Issuer,
                ValidAudience = Audience,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
            };

            try
            {
                return handler.ValidateToken(securityToken, validationParameters, out validatedToken);
            }
            catch (Exception e)
            {
                validatedToken = new JwtSecurityToken();
                return new ClaimsPrincipal();
            }
        }
    }
}
