using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Queries.Core.Services;

public class QueryManagerSession
{
    private readonly string _sessionKey;

    private readonly IDistributedCache _cache;

    public QueryManagerSession(ShellSettings shellSettings, IDistributedCache cache)
    {
        _cache = cache;
        _sessionKey = $"{shellSettings.Name}__{nameof(QueryManagerSession)}";
    }

    public async Task<string> GetKeyAsync()
    {
        var value = await _cache.GetStringAsync(_sessionKey);

        if (value == null)
        {
            value = IdGenerator.GenerateId();

            _cache.SetString(_sessionKey, value);
        }

        return value;
    }

    public async Task<string> GenerateKeyAsync()
    {
        await _cache.RemoveAsync(_sessionKey);

        return await GetKeyAsync();
    }
}
