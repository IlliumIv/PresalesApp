using PresalesApp.CustomTypes;
using System.Web;

namespace PresalesApp.Extensions;

public static class QueryExtension
{
    public static Uri GetODataQueryString(this Query query, Uri uri)
    {
        var uriBuilder = new UriBuilder(uri);
        var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);

        if(query.Filter.HasValue)
        {
            queryString["$filter"] = $"{query.Filter.Value.Replace("\"", "'")}";
        }

        if(query.Top.HasValue)
        {
            queryString["$top"] = $"{query.Top.Value}";
        }

        if(query.Skip.HasValue)
        {
            queryString["$skip"] = $"{query.Skip.Value}";
        }

        if(query.Orderby.HasValue)
        {
            queryString["$orderby"] = $"{query.Orderby.Value}";
        }

        if(query.Expand.HasValue)
        {
            queryString["$expand"] = $"{query.Expand.Value}";
        }

        if(query.Select.HasValue)
        {
            queryString["$select"] = $"{query.Select.Value}";
        }

        if(query.Apply.HasValue)
        {
            queryString["$apply"] = $"{query.Apply.Value}";
        }

        if(query.Count.HasValue)
        {
            queryString["$count"] = $"{query.Count.Value}".ToLower();
        }

        uriBuilder.Query = queryString.ToString();

        return uriBuilder.Uri;
    }
}
