using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISearchDefaultQueryViewModel
{
    public SelectListItem[] DefaultSearchFields { get; set; }

    public string QueryAnalyzerName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
