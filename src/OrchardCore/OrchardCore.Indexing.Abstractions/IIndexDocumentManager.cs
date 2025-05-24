
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexDocumentManager
{
    Task<bool> MergeOrUploadDocumentsAsync(string indexFullName, IEnumerable<DocumentIndexBase> documents, IndexEntity settings);

    Task<bool> DeleteDocumentsAsync(string indexFullName, IEnumerable<string> documentIds);

    Task<bool> DeleteAllDocumentsAsync(string indexFullName);

    Task<long> GetLastTaskIdAsync(IndexEntity index);

    Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId);

    // TO Do, remove this from here.
    IContentIndexSettings GetContentIndexSettings();
}
