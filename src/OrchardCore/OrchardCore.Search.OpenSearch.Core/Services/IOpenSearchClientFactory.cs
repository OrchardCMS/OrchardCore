using OpenSearch.Client;
using OrchardCore.Search.OpenSearch.Core.Models;

namespace OrchardCore.Search.OpenSearch.Core.Services;

public interface IOpenSearchClientFactory
{
    OpenSearchClient Create(OpenSearchConnectionOptions configuration);
}
