using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI.Models;

namespace OrchardCore.AzureAI.Services;

public sealed class AzureAIClientFactory : IDisposable
{
    private readonly IOptionsMonitor<AzureAISearchDefaultOptions> _defaultOptions;
    private readonly IDisposable _optionsChangeRegistration;
    private readonly object _syncLock = new();

    private SearchIndexClient _searchIndexClient;

    private ConcurrentDictionary<string, SearchClient> _clients;

    public AzureAIClientFactory(IOptionsMonitor<AzureAISearchDefaultOptions> defaultOptions)
    {
        _defaultOptions = defaultOptions;
        _optionsChangeRegistration = _defaultOptions.OnChange((_, _) => ResetClients());
    }

    public SearchClient CreateSearchClient(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexFullName, nameof(indexFullName));

        _clients ??= [];
        var defaultOptions = _defaultOptions.CurrentValue;

        if (!_clients.TryGetValue(indexFullName, out var client))
        {
            if (!defaultOptions.ConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && defaultOptions.Credential != null)
            {
                client = new SearchClient(endpoint, indexFullName, defaultOptions.Credential);
            }
            else if (defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                client = new SearchClient(endpoint, indexFullName, GetManagedIdentityCredential(defaultOptions));
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
        var defaultOptions = _defaultOptions.CurrentValue;

        if (_searchIndexClient == null)
        {
            if (!defaultOptions.ConfigurationExists())
            {
                throw new Exception("Azure AI was not configured.");
            }

            if (!Uri.TryCreate(defaultOptions.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw new Exception("The Endpoint provided to Azure AI Options contains invalid value.");
            }

            if (defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && defaultOptions.Credential != null)
            {
                _searchIndexClient = new SearchIndexClient(endpoint, defaultOptions.Credential);
            }
            else if (defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                _searchIndexClient = new SearchIndexClient(endpoint, GetManagedIdentityCredential(defaultOptions));
            }
            else
            {
                _searchIndexClient = new SearchIndexClient(endpoint, new DefaultAzureCredential());
            }
        }

        return _searchIndexClient;
    }

    public void Dispose()
        => _optionsChangeRegistration.Dispose();

    private static ManagedIdentityCredential GetManagedIdentityCredential(AzureAISearchDefaultOptions defaultOptions)
        => !string.IsNullOrEmpty(defaultOptions.IdentityClientId)
        ? new(ManagedIdentityId.FromUserAssignedClientId(defaultOptions.IdentityClientId))
        : new(ManagedIdentityId.SystemAssigned);

    private void ResetClients()
    {
        lock (_syncLock)
        {
            _clients = null;
            _searchIndexClient = null;
        }
    }
}
