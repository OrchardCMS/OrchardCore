using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrchardCore.Redis.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    public RedisHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisService = _serviceProvider.GetService<IRedisService>();
            if (redisService == null)
            {
                return HealthCheckResult.Unhealthy();
            }

            if (redisService.Connection == null)
            {
                await redisService.ConnectAsync();
            }

            if (redisService.Connection.IsConnected)
            {
                var time = await redisService.Database.PingAsync();
                if (time > TimeSpan.FromSeconds(30))
                {
                    return HealthCheckResult.Unhealthy();
                }
                else
                {
                    return HealthCheckResult.Healthy();
                }
            }
            else
            {
                return HealthCheckResult.Unhealthy();
            }
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
