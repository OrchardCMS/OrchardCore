using OrchardCore.Redis.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class RedisHealthCheckExtensions
{
    public static IHealthChecksBuilder AddRedisCheck(this IHealthChecksBuilder healthChecksBuilder)
        => healthChecksBuilder.AddCheck<RedisHealthCheck>("Redis Health Check");
}
