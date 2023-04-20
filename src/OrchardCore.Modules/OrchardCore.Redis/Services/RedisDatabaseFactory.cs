using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services;

/// <summary>
/// Factory allowing to share <see cref="IDatabase"/> instances across tenants.
/// </summary>
public sealed class RedisDatabaseFactory : IRedisDatabaseFactory, IDisposable
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<IDatabase>>> _factories = new();
    private static volatile int _registered;
    private static volatile int _refCount;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;

    public RedisDatabaseFactory(IHostApplicationLifetime lifetime, ILogger<RedisDatabaseFactory> logger)
    {
        Interlocked.Increment(ref _refCount);

        _lifetime = lifetime;
        if (Interlocked.CompareExchange(ref _registered, 1, 0) == 0)
        {
            _lifetime.ApplicationStopped.Register(Release);
        }

        _logger = logger;
    }

    public Task<IDatabase> CreateAsync(RedisOptions options) =>
        _factories.GetOrAdd(options.Configuration, new Lazy<Task<IDatabase>>(async () =>
        {
            try
            {
                return (await ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions)).GetDatabase();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to connect to Redis.");
                throw;
            }
        })).Value;

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _refCount) == 0 && _lifetime.ApplicationStopped.IsCancellationRequested)
        {
            Release();
        }
    }

    internal static void Release()
    {
        if (Interlocked.CompareExchange(ref _refCount, 0, 0) == 0)
        {
            var factories = _factories.Values.ToArray();

            _factories.Clear();

            foreach (var factory in factories)
            {
                var database = factory.Value.GetAwaiter().GetResult();
                database.Multiplexer.Dispose();
            }
        }
    }
}
