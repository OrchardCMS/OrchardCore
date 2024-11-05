using OrchardCore.Documents;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.Elasticsearch;

public class MockElasticIndexSettingsDocumentManager : IDocumentManager<ElasticIndexSettingsDocument>
{
    public Task<ElasticIndexSettingsDocument> GetOrCreateImmutableAsync(Func<Task<ElasticIndexSettingsDocument>> factoryAsync = null)
    {
        return Task.FromResult(new ElasticIndexSettingsDocument
        {
            ElasticIndexSettings = new Dictionary<string, ElasticIndexSettings>()
                {
                    { "idx1", new ElasticIndexSettings { IndexName = "idx1" } },
                    { "idx2", new ElasticIndexSettings { IndexName = "idx2" } },
                    { "testIndex", new ElasticIndexSettings { IndexName = "testIndex" } },
                }
        });
    }

    public Task<ElasticIndexSettingsDocument> GetOrCreateMutableAsync(Func<Task<ElasticIndexSettingsDocument>> factoryAsync = null)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ElasticIndexSettingsDocument document, Func<ElasticIndexSettingsDocument, Task> afterUpdateAsync = null)
    {
        throw new NotImplementedException();
    }
}
