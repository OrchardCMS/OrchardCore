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
    private readonly IStringLocalizer S;

    public RedisHealthCheck(IServiceProvider serviceProvider, IStringLocalizer<RedisHealthCheck> stringLocalizer)
    {
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisService = _serviceProvider.GetService<IRedisService>();
            if (redisService == null)
            {
                return HealthCheckResult.Unhealthy(description: S["The service '{0}' isn't registered.", nameof(IRedisService)]);
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
                    return HealthCheckResult.Unhealthy(description: S["The Redis server isn't a live."]);
                }
                else
                {
                    return HealthCheckResult.Healthy();
                }
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: S["There's an issue in the Redis connection."]);
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(description: ex.Message);
        }
    }
}
