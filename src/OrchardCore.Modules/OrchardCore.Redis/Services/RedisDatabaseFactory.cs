using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services;

/// <summary>
/// Factory allowing to share <see cref="IDatabase"/> instances across tenants.
/// </summary>
public sealed class RedisDatabaseFactory : IRedisDatabaseFactory, IDisposable
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<IDatabase>>> s_factories = new();
    private static volatile int s_registered;
    private static volatile int s_refCount;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;

    public RedisDatabaseFactory(IHostApplicationLifetime lifetime, ILogger<RedisDatabaseFactory> logger)
    {
        Interlocked.Increment(ref s_refCount);

        _lifetime = lifetime;
        if (Interlocked.CompareExchange(ref s_registered, 1, 0) == 0)
        {
            _lifetime.ApplicationStopped.Register(Release);
        }

        _logger = logger;
    }

    public Task<IDatabase> CreateAsync(RedisOptions options) =>
        s_factories.GetOrAdd(options.Configuration, new Lazy<Task<IDatabase>>(async () =>
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating a new instance of '{Name}'. A single instance per configuration should be created across tenants. Total instances prior creating is '{Count}'.", nameof(ConnectionMultiplexer), s_factories.Count);
                }

                return (await ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions)).GetDatabase();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to connect to Redis.");

                return null;
            }
        })).Value;

    public void Dispose()
    {
        if (Interlocked.Decrement(ref s_refCount) == 0 && _lifetime.ApplicationStopped.IsCancellationRequested)
        {
            Release();
        }
    }

    internal static void Release()
    {
        if (Interlocked.CompareExchange(ref s_refCount, 0, 0) == 0)
        {
            var factories = s_factories.Values.ToArray();

            s_factories.Clear();

            foreach (var factory in factories)
            {
                var database = factory.Value.GetAwaiter().GetResult();
                database.Multiplexer.Dispose();
            }
        }
    }
}
