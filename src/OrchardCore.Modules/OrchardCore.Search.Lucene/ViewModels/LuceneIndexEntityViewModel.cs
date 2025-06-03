using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Lucene.ViewModels;

public class LuceneIndexEntityViewModel
{
    public string AnalyzerName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
    public bool StoreSourceData { get; internal set; }
}
