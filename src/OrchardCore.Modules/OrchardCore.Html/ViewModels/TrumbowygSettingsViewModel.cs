using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Html.Models;

namespace OrchardCore.Html.ViewModels
{
    public class TrumbowygSettingsViewModel
    {
        public string Options { get; set; }
        public bool InsertMediaWithUrl { get; set; }
    }
}
