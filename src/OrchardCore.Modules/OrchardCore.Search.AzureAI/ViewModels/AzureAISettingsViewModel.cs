using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISettingsViewModel
{
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    [BindNever]
    public bool IsNew { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
