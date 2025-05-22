using OrchardCore.DisplayManagement;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class IndexViewModel
{
    public ElasticIndexSettings Index { get; set; }

    public IShape Shape { get; set; }
}
