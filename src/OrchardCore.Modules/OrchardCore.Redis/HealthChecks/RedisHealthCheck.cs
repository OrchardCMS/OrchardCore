using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
            if (_redisService.Connection == null)
            {
                await _redisService.ConnectAsync();
            }

            if (_redisService.Connection.IsConnected)
            {
                var time = await _redisService.Database.PingAsync();
                if (time > TimeSpan.FromSeconds(30))
                {
                    return HealthCheckResult.Healthy();
                }
            }

            return HealthCheckResult.Unhealthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
