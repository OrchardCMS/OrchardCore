using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISearchDefaultQueryViewModel
{
    public SelectListItem[] DefaultSearchFields { get; set; }
}
