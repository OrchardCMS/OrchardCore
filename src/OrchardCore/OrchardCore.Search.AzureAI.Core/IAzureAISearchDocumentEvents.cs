using Azure.Search.Documents.Models;

namespace OrchardCore.Search.AzureAI;

public interface IAzureAISearchDocumentEvents
{
    Task UploadingAsync(IEnumerable<SearchDocument> docs);

    Task MergingOrUploadingAsync(IEnumerable<SearchDocument> docs);
}
