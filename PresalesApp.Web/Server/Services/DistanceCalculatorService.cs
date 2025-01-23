using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Query;
using PresalesApp.Web.Server.Models;
using PresalesApp.DistanceCalculator;
using PresalesApp.Extensions;

namespace PresalesApp.Web.Server.Services;

[Authorize]
public class DistanceCalculatorService(ILogger<DistanceCalculatorService> logger,
                                       IModuleRepository moduleRepository)
    : PresalesApp.DistanceCalculator.DistanceCalculatorService.DistanceCalculatorServiceBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
        "IDE0052:Remove unread private members", Justification = "<Pending>")]
    private readonly ILogger<DistanceCalculatorService> _Logger = logger;

    private readonly IModuleRepository _ModuleRepository = moduleRepository;

    #region Service Implementation

    public override Task<GetModulesResponse>
        GetModules (GetModulesRequest request, ServerCallContext context)
    {
        var response = new GetModulesResponse();

        var uri = new Uri(context.GetHttpContext().Request.GetEncodedUrl());
        var httpRequest = context.GetHttpContext().Request;
        httpRequest.QueryString = new(request.Query.GetODataQueryString(uri).Query);
        var queryContext = new ODataQueryContext(EdmModelBuilder.GetEdmModel(), typeof(Module), new());
        var options = new ODataQueryOptions<Module>(queryContext, httpRequest);

        IQueryable queryable = _ModuleRepository.GetModules().AsQueryable();
        var queried = options.ApplyTo(queryable);
        var modules = queried.Cast<Module>();

        foreach(var module in modules)
        {
            response.Modules.Add(module);
        }

        return Task.FromResult(response);
    }

    #endregion
}