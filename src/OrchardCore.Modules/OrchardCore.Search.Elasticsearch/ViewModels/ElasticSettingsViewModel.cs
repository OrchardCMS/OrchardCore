using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class ElasticSettingsViewModel
{
    public string Analyzer { get; set; }

    public string SearchIndex { get; set; }

    public IEnumerable<string> SearchIndexes { get; set; }

    public string SearchFields { get; set; }

    public string DefaultQuery { get; set; }

    public string SearchType { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> SearchTypes { get; set; }
}
