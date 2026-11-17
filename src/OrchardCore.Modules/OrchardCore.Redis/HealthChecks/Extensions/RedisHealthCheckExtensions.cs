using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Redis;
using OrchardCore.Redis.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class RedisHealthCheckExtensions
{
    public static IHealthChecksBuilder AddRedisCheck(this IHealthChecksBuilder healthChecksBuilder)
    {
        healthChecksBuilder.Services.TryAddSingleton<IRedisService, NullRedisService>();

        return healthChecksBuilder.AddCheck<RedisHealthCheck>("Redis Health Check");
    }
}
