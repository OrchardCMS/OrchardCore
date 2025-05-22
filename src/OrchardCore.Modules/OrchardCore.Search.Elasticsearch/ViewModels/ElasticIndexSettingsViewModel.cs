using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class ElasticIndexSettingsViewModel
{
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public bool IsNew { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }
}
