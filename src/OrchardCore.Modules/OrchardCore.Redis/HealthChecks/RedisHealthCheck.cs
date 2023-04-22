using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Options;

namespace OrchardCore.Redis.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly RedisOptions _redisOptions;

    public RedisHealthCheck(IOptions<RedisOptions> redisOptions)
    {
        _redisOptions = redisOptions.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectionMultiplexer.ConnectAsync(_redisOptions.Configuration);

            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
