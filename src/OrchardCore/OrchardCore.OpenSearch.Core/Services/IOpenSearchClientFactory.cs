using OpenSearch.Client;
using OrchardCore.OpenSearch.Core.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public interface IOpenSearchClientFactory
{
    OpenSearchClient Create(OpenSearchConnectionOptions configuration);
}
