using System;
using System.Collections.Concurrent;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class SearchClientFactory(
    IOptions<AzureAISearchDefaultOptions> azureAIOptions,
    ILogger<SearchClientFactory> logger)
{
    private readonly ConcurrentDictionary<string, SearchClient> _clients = [];
    private readonly AzureAISearchDefaultOptions _azureAIOptions = azureAIOptions.Value;
    private readonly ILogger _logger = logger;

    public SearchClient Create(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!Uri.TryCreate(_azureAIOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                _logger.LogError("Endpoint is missing from Azure AI Options.");

                return null;
            }

            client = new SearchClient(endpoint, indexFullName, _azureAIOptions.Credential);

            _clients.TryAdd(indexFullName, client);
        }

        return client;
    }
}
