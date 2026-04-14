using Azure.Search.Documents.Models;

namespace OrchardCore.AzureAI;

public interface IAzureAISearchDocumentEvents
{
    Task UploadingAsync(IEnumerable<SearchDocument> docs);

    Task MergingOrUploadingAsync(IEnumerable<SearchDocument> docs);
}
