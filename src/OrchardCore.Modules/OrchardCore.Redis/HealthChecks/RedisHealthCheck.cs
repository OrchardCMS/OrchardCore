using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;

namespace OrchardCore.Redis.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IRedisService _redisService;

    public RedisHealthCheck(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _redisService.ConnectAsync();

            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
