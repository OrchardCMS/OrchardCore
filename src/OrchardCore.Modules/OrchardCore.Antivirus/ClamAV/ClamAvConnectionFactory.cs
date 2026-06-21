using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Antivirus.ClamAV;

internal sealed class ClamAvConnectionFactory : IDisposable
{
    private static readonly ConcurrentDictionary<string, Lazy<ClamAvConnection>> _connections = new();
    private static volatile int _registered;
    private static volatile int _refCount;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILoggerFactory _loggerFactory;

    public ClamAvConnectionFactory(
        IHostApplicationLifetime lifetime,
        ILoggerFactory loggerFactory)
    {
        Interlocked.Increment(ref _refCount);

        _lifetime = lifetime;
        _loggerFactory = loggerFactory;

        if (Interlocked.CompareExchange(ref _registered, 1, 0) == 0)
        {
            _lifetime.ApplicationStopped.Register(Release);
        }
    }

    public ClamAvConnection Create(ClamAvOptions options)
    {
        var key = $"{options.Host}:{options.Port}:{options.ConnectTimeoutSeconds}:{options.TransferTimeoutSeconds}";

        return _connections.GetOrAdd(key, _ => new Lazy<ClamAvConnection>(() =>
            new ClamAvConnection(options, _loggerFactory.CreateLogger<ClamAvConnection>()))).Value;
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
            var connections = _connections.Values.ToArray();

            _connections.Clear();

            foreach (var connection in connections)
            {
                if (connection.IsValueCreated)
                {
                    connection.Value.Dispose();
                }
            }
        }
    }
}
