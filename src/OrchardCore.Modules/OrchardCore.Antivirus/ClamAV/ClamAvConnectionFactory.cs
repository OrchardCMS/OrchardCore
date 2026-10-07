using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Antivirus.ClamAV;

internal sealed class ClamAvConnectionFactory : IDisposable
{
    private static readonly ConcurrentDictionary<string, Lazy<ClamAvConnection>> s_connections = new();
    private static volatile int s_registered;
    private static volatile int s_refCount;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILoggerFactory _loggerFactory;

    public ClamAvConnectionFactory(
        IHostApplicationLifetime lifetime,
        ILoggerFactory loggerFactory)
    {
        Interlocked.Increment(ref s_refCount);

        _lifetime = lifetime;
        _loggerFactory = loggerFactory;

        if (Interlocked.CompareExchange(ref s_registered, 1, 0) == 0)
        {
            _lifetime.ApplicationStopped.Register(Release);
        }
    }

    public ClamAvConnection Create(ClamAvOptions options)
    {
        var key = $"{options.Host}:{options.Port}:{options.ConnectTimeoutSeconds}:{options.TransferTimeoutSeconds}";

        return s_connections.GetOrAdd(key, _ => new Lazy<ClamAvConnection>(() =>
            new ClamAvConnection(options, _loggerFactory.CreateLogger<ClamAvConnection>()))).Value;
    }

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
            var connections = s_connections.Values.ToArray();

            s_connections.Clear();

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
