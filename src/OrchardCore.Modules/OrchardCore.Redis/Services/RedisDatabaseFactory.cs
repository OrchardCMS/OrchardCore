using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
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

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;

    public RedisDatabaseFactory(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        ILogger<RedisDatabaseFactory> logger)
    {
        Interlocked.Increment(ref _refCount);
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;

        if (Interlocked.CompareExchange(ref _registered, 1, 0) == 0)
        {
            _lifetime.ApplicationStopped.Register(Release);
        }

        _logger = logger;
    }

    public Task<IDatabase> CreateAsync(RedisOptions options)
    {
        return _factories.GetOrAdd(options.Configuration, new Lazy<Task<IDatabase>>(async () =>
        {
            var provider = _serviceProvider.GetService<IRedisTokenProvider>();

            var config = options.ConfigurationOptions;

            if (provider is null)
            {
                var connection = await ConnectionMultiplexer.ConnectAsync(config);

                return connection.GetDatabase();
            }

            var authInfo = await provider.GetAuthenticationAsync();

            if (!string.IsNullOrEmpty(authInfo.Password))
            {
                config.Password = authInfo.Password;
            }

            if (!string.IsNullOrEmpty(authInfo.User))
            {
                config.User = authInfo.User;
            }

            var attempt = 0;

            while (attempt < 3)
            {
                try
                {
                    var connection = await ConnectionMultiplexer.ConnectAsync(config);
                    return connection.GetDatabase();
                }
                catch (RedisConnectionException ex) when (ex.Message.Contains("WRONGPASS") || ex.Message.Contains("NOAUTH"))
                {
                    attempt++;
                    _logger.LogWarning(ex, "Redis authentication failed, retry attempt {Attempt}", attempt);

                    if (provider == null)
                    {
                        break;
                    }

                    var info = await provider.GetAuthenticationAsync();

                    if (!string.IsNullOrEmpty(info.Password))
                    {
                        config.Password = info.Password;
                    }

                    if (!string.IsNullOrEmpty(info.User))
                    {
                        config.User = info.User;
                    }
                }
            }

            throw new InvalidOperationException("Unable to authenticate to Redis after multiple attempts.");
        })).Value;
    }

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
