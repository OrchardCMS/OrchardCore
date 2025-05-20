using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch;

public interface IElasticsearchIndexEvents
{
    /// <summary>
    /// This event is invoked before removing an existing that already exists. 
    /// </summary>
    Task RemovingAsync(ElasticsearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked after the index is removed.
    /// </summary>
    Task RemovedAsync(ElasticsearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked before the index is rebuilt.
    /// If the rebuild deletes the index and create a new one, other events like Removing, Removed, Creating, or Created should not be invoked.
    /// </summary>
    Task RebuildingAsync(ElasticsearchIndexRebuildContext context);

    /// <summary>
    /// This event is invoked after the index is rebuilt.
    /// </summary>
    Task RebuiltAsync(ElasticsearchIndexRebuildContext context);

    /// <summary>
    /// This event is invoked before the index is created.
    /// </summary>
    /// <param name="context"></param>
    Task CreatingAsync(ElasticsearchIndexCreateContext context);

    /// <summary>
    /// This event is invoked after the index is created.
    /// </summary>
    Task CreatedAsync(ElasticsearchIndexCreateContext context);
}
