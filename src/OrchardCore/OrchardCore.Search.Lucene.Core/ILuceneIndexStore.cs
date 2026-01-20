using Lucene.Net.Index;
using Lucene.Net.Search;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Lucene.Core;

public interface ILuceneIndexStore
{
    Task<bool> ExistsAsync(string indexFullName);

    Task<bool> RemoveAsync(IndexProfile index);

    Task SearchAsync(IndexProfile index, Func<IndexSearcher, Task> searcher);

    Task WriteAndClose(IndexProfile index);

    Task WriteAsync(IndexProfile index, Action<IndexWriter> action);
}
