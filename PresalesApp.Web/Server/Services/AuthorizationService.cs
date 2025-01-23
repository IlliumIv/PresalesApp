using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using PresalesApp.Authorization;
using PresalesApp.Web.Server.Authorization;
using PresalesApp.Web.Server.Extensions;

namespace PresalesApp.Web.Server.Services;

public class AuthorizationService(ILogger<AuthorizationService> logger,
                                  UserManager<Database.Entities.User> userManager,
                                  RoleManager<IdentityRole> roleManager,
                                  TokenParameters tokenParameters)
    : PresalesApp.Authorization.AuthorizationService.AuthorizationServiceBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
        "IDE0052:Remove unread private members", Justification = "<Pending>")]
    private readonly ILogger<AuthorizationService> _Logger = logger;

    private readonly UserManager<Database.Entities.User> _UserManager = userManager;

    private readonly RoleManager<IdentityRole> _RoleManager = roleManager;

    private readonly TokenParameters _TokenParameters = tokenParameters;

    #region Service Implementation

    public override async Task<LoginResponse>
        Register(RegisterRequest request, ServerCallContext context)
    {
        if(string.IsNullOrWhiteSpace(request.LoginRequest.Login))
        {
            return new()
            {
                Error = new()
                {
                    Message = "Login is not valid."
                }
            };
        }

        var newUser = new Database.Entities.User
        {
            ProfileName = request.User.Name,
            UserName = request.LoginRequest.Login
        };

        var user = await _UserManager.CreateAsync(newUser, request.LoginRequest.Password);

        return !user.Succeeded
            ? new()
            {
                Error = new()
                {
                    Message = user.Errors.FirstOrDefault()?.Description
                }
            }
            : new()
            {
                User = new()
                {
                    Token = await newUser.GenerateJwtToken(_TokenParameters, _RoleManager, _UserManager),
                    Name = newUser.ProfileName
                }
            };
    }

    public override async Task<LoginResponse>
        Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _UserManager.FindByNameAsync(request.Login);

        return user == null || !await _UserManager.CheckPasswordAsync(user, request.Password)
            ? new()
            {
                Error = new()
                {
                    Message = "User not found or password is incorrect."
                }
            }
            : new()
            {
                User = new()
                {
                    Token = await user.GenerateJwtToken(_TokenParameters, _RoleManager, _UserManager),
                    Name = user.ProfileName
                }
            };
    }

    public override async Task<LoginResponse>
        GetState(Empty request, ServerCallContext context)
    {
        var dbUser = await _UserManager.GetUserAsync(context.GetHttpContext().User);

        return dbUser == null
            ? new()
            {
                Error = new()
                {
                    Message = "No access."
                }
            }
            : new()
            {
                User = new()
                {
                    Name = dbUser.ProfileName,
                }
            };
    }

    #endregion
}