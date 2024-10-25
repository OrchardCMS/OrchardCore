using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISearchSettingsViewModel
{
    public string SearchFields { get; set; }

    public string SearchIndex { get; set; }

    [BindNever]
    public IList<SelectListItem> SearchIndexes { get; set; }
}
