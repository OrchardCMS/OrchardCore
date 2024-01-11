using System;
using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class SearchClientFactory(
    IOptions<AzureAISearchDefaultOptions> defaultOptions,
    ILogger<SearchClientFactory> logger)
{
    private readonly ConcurrentDictionary<string, SearchClient> _clients = [];
    private readonly AzureAISearchDefaultOptions _defaultOptions = defaultOptions.Value;
    private readonly ILogger _logger = logger;

    public SearchClient Create(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        if (!_defaultOptions.IsConfigurationExists())
        {
            return null;
        }

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!Uri.TryCreate(_defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                _logger.LogError("Endpoint is missing from Azure AI Options.");

                return null;
            }

            if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && _defaultOptions.Credential != null)
            {
                client = new SearchClient(endpoint, indexFullName, _defaultOptions.Credential);
            }
            else if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                var identity = new ManagedIdentityCredential(_defaultOptions.IdentityClientId);

                client = new SearchClient(endpoint, indexFullName, identity);
            }
            else
            {
                client = new SearchClient(endpoint, indexFullName, new DefaultAzureCredential());
            }

            _clients.TryAdd(indexFullName, client);
        }

        return client;
    }
}
