using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class AdminQueryViewModel
{
    public string DecodedQuery { get; set; }

    public string Id { get; set; }

    public string Parameters { get; set; }

    [BindNever]
    public long Count { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Indexes { get; set; }

    [BindNever]
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Documents { get; set; } = [];

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Fields { get; set; } = [];
}
