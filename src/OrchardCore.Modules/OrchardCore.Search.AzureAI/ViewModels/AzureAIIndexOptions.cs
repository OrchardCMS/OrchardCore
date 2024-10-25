using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAIIndexOptions
{
    public AzureAISearchIndexBulkAction BulkAction { get; set; }

    public string Search { get; set; }

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }
}
