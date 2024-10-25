using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AdminIndexViewModel
{
    [BindNever]
    public IEnumerable<IndexViewModel> Indexes { get; set; }

    public AzureAIIndexOptions Options { get; set; } = new();

    [BindNever]
    public dynamic Pager { get; set; }
}
