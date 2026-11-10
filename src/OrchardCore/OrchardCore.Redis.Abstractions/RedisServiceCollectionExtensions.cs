using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Returns whether the tenant has a Redis configuration that can be used, that is whether its <see cref="IRedisService"/>
    /// isn't the <see cref="NullRedisService"/>. To be called from the startup of a feature depending on <c>OrchardCore.Redis</c>,
    /// to keep a default implementation rather than replacing it with one relying on Redis.
    /// </summary>
    public static bool IsRedisConfigured(this IServiceCollection services)
        => services.Any(descriptor =>
            descriptor.ServiceType == typeof(IRedisService) &&
            descriptor.GetImplementationType() != typeof(NullRedisService));
}
