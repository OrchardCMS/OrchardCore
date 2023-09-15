using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell.Distributed
{
    internal class DistributedContext : IDisposable, IAsyncDisposable
    {
        private readonly ShellContext _context;
        private volatile int _count;
        private bool _released;

        public DistributedContext(ShellContext context)
        {
            Interlocked.Increment(ref _count);
            _context = context;

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

        public ShellContext Context => _context;

        public IDistributedCache DistributedCache { get; }

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

        public void Release()
        {
            _released = true;
            Dispose();
        }

        public async Task ReleaseAsync()
        {
            _released = true;
            await DisposeAsync();
        }

        public void Dispose()
        {
            // The last use disposes the shell context.
            if (Interlocked.Decrement(ref _count) == 0)
            {
                _context.Dispose();
            }
        }

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
}
