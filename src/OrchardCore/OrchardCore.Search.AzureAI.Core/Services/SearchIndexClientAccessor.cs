using System;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class SearchIndexClientAccessor(IOptions<AzureAISearchDefaultOptions> defaultOptions)
{
    private SearchIndexClient _instance;

    private readonly AzureAISearchDefaultOptions _defaultOptions = defaultOptions.Value;

    public SearchIndexClient Get()
    {
        if (_instance == null && _defaultOptions.IsConfigurationExists())
        {
            if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ApiKey && _defaultOptions.Credential != null)
            {
                _instance = new SearchIndexClient(new Uri(_defaultOptions.Endpoint), _defaultOptions.Credential);
            }
            else if (_defaultOptions.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
            {
                var identity = new ManagedIdentityCredential(_defaultOptions.IdentityClientId);

                _instance = new SearchIndexClient(new Uri(_defaultOptions.Endpoint), identity);
            }
            else
            {
                _instance = new SearchIndexClient(new Uri(_defaultOptions.Endpoint), new DefaultAzureCredential());
            }
        }

        return _instance ?? throw new Exception("Azure Search AI was not configured.");
    }
}
