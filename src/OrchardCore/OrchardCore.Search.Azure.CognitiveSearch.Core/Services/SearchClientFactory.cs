using System;
using System.Collections.Concurrent;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Azure.CognitiveSearch.Models;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class SearchClientFactory
{
    private readonly ConcurrentDictionary<string, SearchClient> _clients = [];
    private readonly AzureCognitiveSearchOptions _azureCognitiveSearchOptions;
    private readonly ILogger _logger;

    public SearchClientFactory(
        IOptions<AzureCognitiveSearchOptions> azureCognitiveSearchOptions,
        ILogger<SearchClientFactory> logger)
    {
        _azureCognitiveSearchOptions = azureCognitiveSearchOptions.Value;
        _logger = logger;
    }
    public SearchClient Create(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!Uri.TryCreate(_azureCognitiveSearchOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                _logger.LogError("Endpoint is missing from Azure Cognitive Search Settings.");

                return null;
            }

            client = new SearchClient(endpoint, indexFullName, _azureCognitiveSearchOptions.Credential);

            _clients.TryAdd(indexFullName, client);
        }

        return client;
    }
}
