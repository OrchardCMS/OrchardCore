
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexDocumentManager
{
    Task<bool> MergeOrUploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndexBase> documents);

    Task<bool> DeleteDocumentsAsync(IndexEntity index, IEnumerable<string> documentIds);

    Task<bool> DeleteAllDocumentsAsync(IndexEntity index);

    Task<long> GetLastTaskIdAsync(IndexEntity index);

    Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId);

    // TODO, remove this from here.
    IContentIndexSettings GetContentIndexSettings();
}
