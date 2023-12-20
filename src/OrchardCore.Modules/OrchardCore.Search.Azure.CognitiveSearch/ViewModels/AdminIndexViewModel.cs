using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Azure.CognitiveSearch.ViewModels;

public class AdminIndexViewModel
{
    public IEnumerable<IndexViewModel> Indexes { get; set; }

    public ContentOptions Options { get; set; } = new();

    [BindNever]
    public dynamic Pager { get; set; }
}
