using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class ElasticsearchIndexEntityViewModel
{
    public bool StoreSourceData { get; set; }

    public string AnalyzerName { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
