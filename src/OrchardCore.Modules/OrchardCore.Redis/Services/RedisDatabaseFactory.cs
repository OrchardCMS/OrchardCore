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
    private static readonly ConcurrentDictionary<string, IDatabase> _databases = new();
    private static readonly SemaphoreSlim _semaphore = new(1);
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

    public async Task<IDatabase> CreateAsync(RedisOptions options)
    {
        if (_databases.TryGetValue(options.Configuration, out var database))
        {
            return database;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_databases.TryGetValue(options.Configuration, out database))
            {
                return database;
            }

            database = (await ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions)).GetDatabase();

            return _databases[options.Configuration] = database;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to connect to Redis.");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _refCount) == 0 && _lifetime.ApplicationStopped.IsCancellationRequested)
        {
            Release();
        }
    }

    public static void Release()
    {
        if (Interlocked.CompareExchange(ref _refCount, 0, 0) == 0)
        {
            var databases = _databases.Values.ToArray();

            _databases.Clear();

            foreach (var database in databases)
            {
                database.Multiplexer.Dispose();
            }
        }
    }
}
