using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrchardCore.Redis.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private const int Timeout = 30;

    private readonly IRedisService _redis;

    public RedisHealthCheck(IRedisService redis) => _redis = redis;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis.Connection == null)
            {
                await _redis.ConnectAsync();

                if (_redis.Connection == null)
                {
                    return HealthCheckResult.Unhealthy(description: "Couldn't connect to the Redis server, check that the 'Configuration' string is set and valid.");
                }
            }

            if (_redis.Connection.IsConnected)
            {
                var time = await _redis.Database.PingAsync();
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
