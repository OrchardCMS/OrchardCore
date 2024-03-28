using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Localization;

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
                return HealthCheckResult.Unhealthy(description: $"The service '{nameof(IRedisService)}' isn't registered.");
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
                    return HealthCheckResult.Unhealthy(description: "The Redis server couldn't be reached within {seconds} seconds and might be offline or have degraded performance.");
                }
                else
                {
                    return HealthCheckResult.Healthy();
                }
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: S["Couldn't connect to the Redis server."]);
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the Redis service failed.", ex);
        }
    }
}
