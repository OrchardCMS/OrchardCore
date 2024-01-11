using System;
using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIClientFactory(IOptions<AzureAISearchDefaultOptions> defaultOptions)
{
    private readonly ConcurrentDictionary<string, SearchClient> _clients = [];
    private readonly AzureAISearchDefaultOptions _defaultOptions = defaultOptions.Value;

    private SearchIndexClient _searchIndexClient;

    public SearchClient CreateSearchClient(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!_defaultOptions.IsConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(_defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && _defaultOptions.Credential != null)
            {
                client = new SearchClient(endpoint, indexFullName, _defaultOptions.Credential);
            }
            else if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                client = new SearchClient(endpoint, indexFullName, GetManagedIdentityCredential());
            }
            else
            {
                client = new SearchClient(endpoint, indexFullName, new DefaultAzureCredential());
            }

            _clients.TryAdd(indexFullName, client);
        }

        return client;
    }

    public SearchIndexClient CreateSearchIndexClient()
    {
        if (_searchIndexClient == null)
        {
            if (!_defaultOptions.IsConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(_defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && _defaultOptions.Credential != null)
            {
                _searchIndexClient = new SearchIndexClient(endpoint, _defaultOptions.Credential);
            }
            else if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                _searchIndexClient = new SearchIndexClient(endpoint, GetManagedIdentityCredential());
            }
            else
            {
                _searchIndexClient = new SearchIndexClient(endpoint, new DefaultAzureCredential());
            }
        }

        return _searchIndexClient;
    }

    private ManagedIdentityCredential GetManagedIdentityCredential()
        => new(_defaultOptions.IdentityClientId);
}
