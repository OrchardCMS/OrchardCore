using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis;

/// <summary>
/// The <see cref="IRedisService"/> of a tenant whose Redis configuration is missing or invalid: it never connects, so
/// <see cref="Connection"/> and <see cref="Database"/> stay <see langword="null"/>.
/// </summary>
public sealed class NullRedisService : ModularTenantEvents, IRedisService
{
    /// <inheritdoc/>
    public IConnectionMultiplexer Connection => null;

    /// <inheritdoc/>
    public string InstancePrefix => null;

    /// <inheritdoc/>
    public IDatabase Database => null;

    /// <inheritdoc/>
    public Task ConnectAsync() => Task.CompletedTask;
}
