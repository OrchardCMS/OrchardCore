using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis;

/// <summary>
/// Provides the Redis connection of a tenant. It's always registered while the <c>OrchardCore.Redis</c> feature is
/// enabled, as a <see cref="NullRedisService"/> when the Redis configuration is missing or invalid.
/// </summary>
public interface IRedisService : IModularTenantEvents
{
    /// <summary>
    /// Creates the connection to the Redis server, if it hasn't been created yet.
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// The connection to the Redis server, or <see langword="null"/> if it hasn't been created. A connection doesn't
    /// mean that the server is reachable, this is given by <see cref="IConnectionMultiplexer.IsConnected"/>.
    /// </summary>
    IConnectionMultiplexer Connection { get; }

    /// <summary>
    /// The prefix allowing a Redis instance to be shared.
    /// </summary>
    string InstancePrefix { get; }

    /// <summary>
    /// The Redis database, or <see langword="null"/> if the connection hasn't been created.
    /// </summary>
    IDatabase Database { get; }
}
