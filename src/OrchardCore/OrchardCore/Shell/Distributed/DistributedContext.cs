using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell.Distributed;

/// <summary>
/// Isolated context based on the default tenant settings used to resolve the <see cref="IDistributedCache"/>.
/// </summary>
internal sealed class DistributedContext : IDisposable, IAsyncDisposable
{
    private readonly ShellContext _context;
    private volatile int _count;
    private bool _released;

    /// <summary>
    /// Initializes a new <see cref="DistributedContext"/>.
    /// </summary>
    public DistributedContext(ShellContext context)
    {
        Interlocked.Increment(ref _count);

        // By default marked as using shared settings that should not be disposed.
        _context = context.WithSharedSettings();

        // If the distributed feature is not enabled, the distributed cache is not set.
        if (context.ServiceProvider.GetService<DistributedShellMarkerService>() is null)
        {
            return;
        }

        // If the current cache is an in memory cache, the distributed cache is not set.
        var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
        if (distributedCache is null || distributedCache is MemoryDistributedCache)
        {
            return;
        }

        DistributedCache = distributedCache;
    }

    /// <summary>
    /// Gets the inner <see cref="ShellContext"/>.
    /// </summary>
    public ShellContext Context => _context;

    /// <summary>
    /// Gets the resolved <see cref="IDistributedCache"/>.
    /// </summary>
    public IDistributedCache DistributedCache { get; }

    /// <summary>
    /// Marks this instance as using unshared settings that can be disposed.
    /// </summary>
    public DistributedContext WithoutSharedSettings()
    {
        _context.WithoutSharedSettings();
        return this;
    }

    /// <summary>
    /// Tries to acquire this instance.
    /// </summary>
    public DistributedContext Acquire()
    {
        // Don't acquire a released context.
        if (_released)
        {
            return null;
        }

        Interlocked.Increment(ref _count);

        // Don't start using a released context.
        if (_released)
        {
            Dispose();
            return null;
        }

        return this;
    }

    /// <summary>
    /// Releases once this instance.
    /// </summary>
    public void Release()
    {
        _released = true;
        Dispose();
    }

    /// <summary>
    /// Releases once this instance.
    /// </summary>
    public async Task ReleaseAsync()
    {
        _released = true;
        await DisposeAsync();
    }

    /// <summary>
    /// Disposes this instance, the last owner dispose the inner <see cref="ShellContext"/>.
    /// </summary>
    public void Dispose()
    {
        // The last use disposes the shell context.
        if (Interlocked.Decrement(ref _count) == 0)
        {
            _context.Dispose();
        }
    }

    /// <summary>
    /// Disposes this instance, the last owner dispose the inner <see cref="ShellContext"/>.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        // The last use disposes the shell context.
        if (Interlocked.Decrement(ref _count) == 0)
        {
            return _context.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }
}
