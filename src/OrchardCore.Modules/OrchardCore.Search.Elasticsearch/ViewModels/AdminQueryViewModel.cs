using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class AdminQueryViewModel
{
    public string DecodedQuery { get; set; }
    public string IndexName { get; set; }
    public string Parameters { get; set; }

    [BindNever]
    public long Count { get; set; }

    [BindNever]
    public string[] Indices { get; set; }

    [BindNever]
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Documents { get; set; } = [];

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Fields { get; set; } = [];
}
