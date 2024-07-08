using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Queries.Core.Services;

public sealed class DefaultQueryManagerSession
{
    private readonly string _sessionKey;

    private readonly IDistributedCache _cache;

    public DefaultQueryManagerSession(ShellSettings shellSettings, IDistributedCache cache)
    {
        _cache = cache;
        _sessionKey = $"{shellSettings.Name}__{nameof(DefaultQueryManagerSession)}";
    }

    public async Task<string> GetKeyAsync()
    {
        var value = await _cache.GetStringAsync(_sessionKey)
            ?? await GenerateKeyAsync();

        return value;
    }

    public async Task<string> GenerateKeyAsync()
    {
        var value = IdGenerator.GenerateId();

        await _cache.SetStringAsync(_sessionKey, value);

        return value;
    }
}
