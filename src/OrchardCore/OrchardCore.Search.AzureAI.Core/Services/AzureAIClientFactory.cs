using System.Collections.Concurrent;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIClientFactory
{
    private readonly AzureAISearchDefaultOptions _defaultOptions;
    private readonly IOptionsMonitor<AzureOptions> _optionsMonitor;

    private SearchIndexClient _searchIndexClient;

    private ConcurrentDictionary<string, SearchClient> _clients;

    public AzureAIClientFactory(
        IOptions<AzureAISearchDefaultOptions> defaultOptions,
        IOptionsMonitor<AzureOptions> optionsMonitor)
    {
        _defaultOptions = defaultOptions.Value;
        _optionsMonitor = optionsMonitor;
    }

    public SearchClient CreateSearchClient(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        _clients ??= [];

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!_defaultOptions.ConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(_defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (_defaultOptions.AuthenticationType == AzureAuthenticationType.ApiKey)
            {
                client = new SearchClient(endpoint, indexFullName, new AzureKeyCredential(_defaultOptions.ApiKey));
            }
            else if (_defaultOptions.AuthenticationType == AzureAuthenticationType.ManagedIdentity)
            {
                client = new SearchClient(endpoint, indexFullName, GetManagedIdentityCredential());
            }
            else
            {
                var credentials = _optionsMonitor.Get(_defaultOptions.CredentialName ?? AzureOptions.DefaultName).ToTokenCredential();

                client = new SearchClient(endpoint, indexFullName, credentials);
            }

            if (client is null)
            {
                throw new NotSupportedException($"The Authentication Type '{_defaultOptions.AuthenticationType}' is not supported.");
            }

            _clients.TryAdd(indexFullName, client);
        }

        return client;
    }

    public SearchIndexClient CreateSearchIndexClient()
    {
        if (_searchIndexClient == null)
        {
            if (!_defaultOptions.ConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(_defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (_defaultOptions.AuthenticationType == AzureAuthenticationType.ApiKey)
            {
                _searchIndexClient = new SearchIndexClient(endpoint, new AzureKeyCredential(_defaultOptions.ApiKey));
            }
            else if (_defaultOptions.AuthenticationType == AzureAuthenticationType.ManagedIdentity)
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
