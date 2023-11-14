using System.Text.Encodings.Web;

namespace PresalesApp.Web.Shared;

public partial class Query
{
    //
    // Summary:
    //     Converts the query to OData query format.
    //
    // Parameters:
    //   url:
    //     The URL.
    //
    // Returns:
    //     System.String.
    public string ToUrl(string url)
    {
        var dictionary = new Dictionary<string, object> { };

        if(Skip.HasValue)
        {
            dictionary.Add("$skip", Skip.Value);
        }

        if(Top.HasValue)
        {
            dictionary.Add("$top", Top.Value);
        }

        if(!string.IsNullOrEmpty(OrderBy))
        {
            dictionary.Add("$orderBy", OrderBy);
        }

        if(!string.IsNullOrEmpty(Filter))
        {
            dictionary.Add("$filter", UrlEncoder.Default.Encode(Filter));
        }

        if(!string.IsNullOrEmpty(Expand))
        {
            dictionary.Add("$expand", Expand);
        }

        if(!string.IsNullOrEmpty(Select))
        {
            dictionary.Add("$select", Select);
        }

        return string.Format("{0}{1}", url, dictionary.Count != 0 ? ("?" + string.Join("&", dictionary.Select((KeyValuePair<string, object> a) => $"{a.Key}={a.Value}"))) : "");
    }
}
