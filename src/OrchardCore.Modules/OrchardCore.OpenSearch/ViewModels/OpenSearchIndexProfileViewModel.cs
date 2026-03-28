using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.OpenSearch.ViewModels;

public class OpenSearchIndexProfileViewModel
{
    public bool StoreSourceData { get; set; }

    public string AnalyzerName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
