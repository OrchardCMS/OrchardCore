using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISettingsIndexEntityViewModel
{
    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
