using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class ElasticsearchDefaultQueryViewModel
{
    public string QueryAnalyzerName { get; set; }

    public string DefaultQuery { get; set; }

    public string SearchType { get; set; }

    public SelectListItem[] DefaultSearchFields { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> SearchTypes { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
