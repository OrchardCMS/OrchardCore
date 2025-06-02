using Lucene.Net.Index;
using Lucene.Net.Search;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Lucene.Core;

public interface ILuceneIndexStore
{
    Task<bool> ExistsAsync(string indexFullName);

    Task<bool> RemoveAsync(string indexFullName);

    Task SearchAsync(IndexEntity index, Func<IndexSearcher, Task> searcher);

    Task WriteAndClose(IndexEntity index);

    Task WriteAsync(IndexEntity index, Action<IndexWriter> action);
}
