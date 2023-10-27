using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Views
{
    partial class SurveyPrompt
    {
        // Demonstrates how a parent component can supply parameters
        [Parameter]
        public string? Title { get; set; }
    }
}
