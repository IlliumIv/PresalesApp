using System.Text;

namespace PresalesApp.Web.Server.Extensions;

// https://devblogs.microsoft.com/odata/tutorial-build-grpc-odata-in-asp-net-core/
// https://github.com/xuzhg/MyAspNetCore/blob/master/src/gRPC.OData/gRPC.OData.Server/Extensions/EndpointDebugMiddleware.cs

public static class ApplicationBuilderEndpointDebugExtensions
{
    public static IApplicationBuilder UseEndpointDebug(this IApplicationBuilder app)
        => app.UseMiddleware<EndpointDebugMiddleware>();
}

public class EndpointDebugMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _Next = next;

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        if(string.Equals(request.Path.Value, "/$endpoint", StringComparison.OrdinalIgnoreCase))
        {
            await WriteRoutesAsHtml(context).ConfigureAwait(false);
        }
        else
        {
            await _Next(context).ConfigureAwait(false);
        }
    }

    private static readonly IReadOnlyList<string> _EmptyHeaders = [];

    internal static async Task WriteRoutesAsHtml(HttpContext context)
    {
        var stdRouteTable = new StringBuilder();

        var dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
        foreach(var routeEndpoint in dataSource.Endpoints.OfType<RouteEndpoint>())
        {
            stdRouteTable.Append("<tr>");
            stdRouteTable.Append($"<td>{routeEndpoint.DisplayName}</td>");
            stdRouteTable.Append($"<td>{routeEndpoint.RoutePattern.RawText}</td>");

            var httpMethods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? _EmptyHeaders;

            stdRouteTable.Append($"<td>{string.Join(",", httpMethods)}</td>");

            stdRouteTable.Append($"<td>{_GetRequestDelegateString(routeEndpoint.RequestDelegate)}</td>");

            stdRouteTable.AppendLine("</tr>");
        }

        var output = _RouteMappingHtmlTemplate;
        output = output.Replace("STD_ROUTE_CONTENT", stdRouteTable.ToString(), StringComparison.Ordinal);

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(output).ConfigureAwait(false);
    }
    private static string _GetRequestDelegateString(RequestDelegate? rd)
    {
        if(rd == null) return "";

        var sb = new StringBuilder();

        var methodInfo = rd.Method;
        sb.Append(methodInfo.ReturnType?.Name);
        sb.Append(' ');
        sb.Append(methodInfo.DeclaringType?.Namespace + "." + methodInfo.DeclaringType?.Name);
        sb.Append('.');
        sb.Append(methodInfo.Name);
        sb.Append('(');
        var index = 0;
        foreach(var p in methodInfo.GetParameters())
        {
            if(index == 0)
            {
                index = 1;
            }
            else
            {
                sb.Append(',');
            }

            sb.Append(p.ParameterType.Name);

        }

        sb.Append(')');
        return sb.ToString();
    }

    private static readonly string _RouteMappingHtmlTemplate = @"<html>
<head>
    <title>Endpoint Routing Debugger</title>
    <style>
        table {
            font-family: arial, sans-serif;
            border-collapse: collapse;
            width: 100%;
        }
        td,
        th {
            border: 1px solid #dddddd;
            text-align: left;
            padding: 8px;
        }
        tr:nth-child(even) {
            background-color: #dddddd;
        }
    </style>
</head>
<body>
    <h1 id=""odata"">Endpoint Mappings</h1>
    <table>
        <tr>
            <th> Display Name </th>
            <th> RoutePattern </th>
            <th> HttpMethods </th>
            <th> RequestDelegate </th>
        </tr>
        STD_ROUTE_CONTENT
    </table>
</body>
</html>";
}