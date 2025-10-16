using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;

namespace OrchardCore.Redis.Azure;

public sealed class AzureRedisTokenProvider : ITokenProvider
{
    private readonly IOptionsMonitor<AzureOptions> _options;
    private readonly RedisOptions _redisOptions;

    public AzureRedisTokenProvider(
        IOptionsMonitor<AzureOptions> options,
        IOptions<RedisOptions> redisOptions)
    {
        _options = options;
        _redisOptions = redisOptions.Value;
    }

    public async Task<TokenResult> GetTokenAsync()
    {
        var redisOptions = _options.Get(_redisOptions.CredentialName ?? AzureOptions.DefaultName);

        var scope = redisOptions.GetProperty<string>("Scope");

        if (string.IsNullOrEmpty(scope))
        {
            scope = "https://redis.azure.com/.default";
        }

        TokenCredential credential = redisOptions.AuthenticationType switch
        {
            AzureAuthenticationType.Default => new DefaultAzureCredential(),
            AzureAuthenticationType.ManagedIdentity => new ManagedIdentityCredential(),
            AzureAuthenticationType.AzureCli => new AzureCliCredential(),
            AzureAuthenticationType.AzurePower => new AzurePowerShellCredential(),
            AzureAuthenticationType.VisualStudio => new VisualStudioCredential(),
            _ => throw new NotSupportedException($"Authentication type {redisOptions.AuthenticationType} is not supported")
        };

        var requestContext = new TokenRequestContext([scope]);

        var result = await credential.GetTokenAsync(requestContext, CancellationToken.None);

        return new TokenResult
        {
            Token = result.Token,
        };
    }
}
