using OrchardCore.Documents;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.Lucene;

public class MockLuceneIndexSettingsDocumentManager : IDocumentManager<LuceneIndexSettingsDocument>
{
    public Task<LuceneIndexSettingsDocument> GetOrCreateImmutableAsync(Func<Task<LuceneIndexSettingsDocument>> factoryAsync = null)
    {
        return Task.FromResult(new LuceneIndexSettingsDocument
        {
            LuceneIndexSettings = new Dictionary<string, LuceneIndexSettings>()
                {
                    { "idx1", new LuceneIndexSettings { IndexName = "idx1" } },
                    { "idx2", new LuceneIndexSettings { IndexName = "idx2" } },
                    { "testIndex", new LuceneIndexSettings { IndexName = "testIndex" } },
                }
        });
    }

    public Task<LuceneIndexSettingsDocument> GetOrCreateMutableAsync(Func<Task<LuceneIndexSettingsDocument>> factoryAsync = null)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(LuceneIndexSettingsDocument document, Func<LuceneIndexSettingsDocument, Task> afterUpdateAsync = null)
    {
        throw new NotImplementedException();
    }
}
