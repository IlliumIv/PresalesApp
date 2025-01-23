using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace PresalesApp.Web.Server.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(ILogger<ErrorModel> logger) : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
        "IDE0052:Remove unread private members", Justification = "<Pending>")]
    private readonly ILogger<ErrorModel> _Logger = logger;

    public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}