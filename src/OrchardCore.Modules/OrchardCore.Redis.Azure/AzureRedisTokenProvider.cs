using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;

namespace OrchardCore.Redis.Azure;

public sealed class AzureRedisTokenProvider : IRedisTokenProvider
{
    private readonly IOptionsMonitor<AzureOptions> _options;
    private readonly ILogger _logger;

    public AzureRedisTokenProvider(
        IOptionsMonitor<AzureOptions> options,
        ILogger<AzureRedisTokenProvider> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync()
    {
        var redisOptions = _options.Get("Redis");

        var scopes = redisOptions.GetProperty<string[]>("Scopes");

        if (scopes is null || scopes.Length == 0)
        {
            _logger.LogWarning("No scope configured for Azure Redis authentication, returning empty token.");

            return null;
        }

        TokenCredential credential = redisOptions.AuthenticationType switch
        {
            AzureAuthenticationType.Default => new DefaultAzureCredential(),
            AzureAuthenticationType.ManagedIdentity => new ManagedIdentityCredential(),
            AzureAuthenticationType.AzureCli => new AzureCliCredential(),
            _ => throw new NotSupportedException($"Authentication type {redisOptions.AuthenticationType} is not supported")
        };

        var requestContext = new TokenRequestContext(scopes);

        var result = await credential.GetTokenAsync(requestContext, CancellationToken.None);

        return result.Token;
    }
}
