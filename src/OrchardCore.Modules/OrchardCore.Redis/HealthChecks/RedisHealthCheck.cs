using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrchardCore.Redis.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private const int Timeout = 30;

    private readonly IServiceProvider _serviceProvider;

    public RedisHealthCheck(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

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
                if (time > TimeSpan.FromSeconds(Timeout))
                {
                    return HealthCheckResult.Unhealthy(description: $"The Redis server couldn't be reached within {Timeout} seconds and might be offline or have degraded performance.");
                }
                else
                {
                    return HealthCheckResult.Healthy();
                }
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: "Couldn't connect to the Redis server.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Retrieving the status of the Redis service failed.", ex);
        }
    }
}
