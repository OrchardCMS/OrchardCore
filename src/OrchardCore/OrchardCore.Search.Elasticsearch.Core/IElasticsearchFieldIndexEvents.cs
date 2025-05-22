using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core;

public interface IElasticsearchFieldIndexEvents
{
    Task MappingAsync(SearchIndexDefinition context);

    Task MappedAsync(SearchIndexDefinition context);
}
