using Azure.Core;
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

        var credential = redisOptions.ToTokenCredential();

        if (credential is null)
        {
            throw new InvalidOperationException($"Unable to create a valid TokenCredential for RedisOptions '{_redisOptions.CredentialName}'.");
        }

        var requestContext = new TokenRequestContext([scope]);

        var result = await credential.GetTokenAsync(requestContext, CancellationToken.None);

        return new TokenResult
        {
            Token = result.Token,
        };
    }
}
