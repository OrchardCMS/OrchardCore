using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Redis.Azure;

public sealed class AzureRedisTokenProvider : IRedisTokenProvider
{
    private readonly AzureRedisOptions _options;
    private readonly ILogger _logger;

    public AzureRedisTokenProvider(
        IOptions<AzureRedisOptions> options,
        ILogger<AzureRedisTokenProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RedisAuthenticationInfo> GetAuthenticationAsync()
    {
        TokenCredential credential = _options.AuthenticationType switch
        {
            AzureRedisAuthType.DefaultAzureCredential => new DefaultAzureCredential(),
            AzureRedisAuthType.ManagedIdentity => new ManagedIdentityCredential(),
            AzureRedisAuthType.ClientSecret => new ClientSecretCredential(
                _options.TenantId ?? throw new ArgumentException("TenantId is required for ClientSecretCredential"),
                _options.ClientId ?? throw new ArgumentException("ClientId is required for ClientSecretCredential"),
                _options.ClientSecret ?? throw new ArgumentException("ClientSecret is required for ClientSecretCredential")
            ),
            _ => throw new NotSupportedException($"Authentication type {_options.AuthenticationType} is not supported")
        };

        if (_options.Scopes is null || _options.Scopes.Length == 0)
        {
            _logger.LogWarning("No scope configured for Azure Redis authentication, returning empty token.");

            return new RedisAuthenticationInfo();
        }

        var requestContext = new TokenRequestContext(_options.Scopes);

        var token = await credential.GetTokenAsync(requestContext, CancellationToken.None);

        return new RedisAuthenticationInfo
        {
            Password = token.Token,
        };
    }
}
