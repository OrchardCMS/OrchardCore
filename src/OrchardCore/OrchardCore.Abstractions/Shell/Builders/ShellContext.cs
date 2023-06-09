using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable, IAsyncDisposable
    {
        private bool _disposed;
        private List<WeakReference<ShellContext>> _dependents;
        private readonly SemaphoreSlim _semaphore = new(1);

        internal volatile int _refCount;
        internal volatile int _terminated;
        internal bool _released;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Whether the shell is activated.
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// The Pipeline built for this shell.
        /// </summary>
        public IShellPipeline Pipeline { get; set; }

        public class PlaceHolder : ShellContext
        {
            /// <summary>
            /// Used as a place holder for a shell that will be lazily created.
            /// </summary>
            public PlaceHolder()
            {
                _released = true;
                _disposed = true;
            }

            public bool PreCreated { get; init; }
        }

        /// <summary>
        /// Creates a <see cref="ShellScope"/> on this shell context.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use CreateScopeAsync instead.", false)]
        public ShellScope CreateScope() => CreateScopeAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Creates a <see cref="ShellScope"/> on this shell context.
        /// </summary>
        public async Task<ShellScope> CreateScopeAsync()
        {
            // Don't create a shell scope on a released shell.
            if (_released)
            {
                return null;
            }

            var scope = new ShellScope(this);

            // Don't start using a new scope on a released shell.
            if (_released)
            {
                // But let this scope manage the shell state as usual.
                await scope.TerminateShellAsync();
                return null;
            }

            return scope;
        }

        /// <summary>
        /// Whether the <see cref="ShellContext"/> instance is not yet built or has been released, for instance when a tenant is changed.
        /// </summary>
        public bool Released => _released;

        /// <summary>
        /// Returns the number of active scopes on this tenant.
        /// </summary>
        public int ActiveScopes => _refCount;

        /// <summary>
        /// Mark the <see cref="ShellContext"/> as released and then a candidate to be disposed.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use ReleaseAsync instead.", false)]
        public void Release() => ReleaseInternalAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Mark the <see cref="ShellContext"/> as released and then a candidate to be disposed.
        /// </summary>
        public Task ReleaseAsync() => ReleaseInternalAsync();

        internal Task ReleaseFromLastScopeAsync() => ReleaseInternalAsync(ReleaseMode.FromLastScope);

        internal Task ReleaseFromDependencyAsync() => ReleaseInternalAsync(ReleaseMode.FromDependency);

        internal async Task ReleaseInternalAsync(ReleaseMode mode = ReleaseMode.Normal)
        {
            if (_released)
            {
                // Prevent infinite loops with circular dependencies
                return;
            }

            // A disabled shell still in use will be released by its last scope, as checked at the host level.
            if (mode == ReleaseMode.FromDependency && Settings.State == TenantState.Disabled && _refCount != 0)
            {
                return;
            }

            // When a tenant is changed and should be restarted, its shell context is replaced with a new one,
            // so that new request can't use it anymore. However some existing request might still be running and try to
            // resolve or use its services. We then call this method to count the remaining references and dispose it
            // when the number reached zero.

            ShellScope scope = null;
            await _semaphore.WaitAsync();
            try
            {
                if (_released)
                {
                    return;
                }

                if (_dependents != null)
                {
                    foreach (var dependent in _dependents)
                    {
                        if (dependent.TryGetTarget(out var shellContext))
                        {
                            await shellContext.ReleaseFromDependencyAsync();
                        }
                    }
                }

                if (mode != ReleaseMode.FromLastScope && ServiceProvider != null)
                {
                    // Before marking the shell as released, we create a new scope that will manage the shell state,
                    // so that we always use the same shell scope logic to check if the reference counter reached 0.
                    scope = new ShellScope(this);
                }

                _released = true;
            }
            finally
            {
                _semaphore.Release();
            }

            if (mode == ReleaseMode.FromLastScope)
            {
                return;
            }

            if (scope != null)
            {
                // Use this scope to manage the shell state as usual.
                await scope.TerminateShellAsync();
                return;
            }

            await DisposeAsync();
        }

        internal enum ReleaseMode
        {
            Normal,
            FromLastScope,
            FromDependency
        }

        /// <summary>
        /// Registers the specified shellContext as dependent such that it is also released when the current shell context is released.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use AddDependentShellAsync instead.", false)]
        public void AddDependentShell(ShellContext shellContext) => AddDependentShellAsync(shellContext).GetAwaiter().GetResult();

        /// <summary>
        /// Registers the specified shellContext as dependent such that it is also released when the current shell context is released.
        /// </summary>
        public async Task AddDependentShellAsync(ShellContext shellContext)
        {
            // If the dependent is released, nothing to do.
            if (shellContext.Released)
            {
                return;
            }

            // If the dependency is already released.
            if (_released)
            {
                // The dependent is released immediately.
                await shellContext.ReleaseInternalAsync();
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                _dependents ??= new List<WeakReference<ShellContext>>();

                // Remove any previous instance that represent the same tenant in case it has been released (restarted).
                _dependents.RemoveAll(wref => !wref.TryGetTarget(out var shell) || shell.Settings.Name == shellContext.Settings.Name);

                _dependents.Add(new WeakReference<ShellContext>(shellContext));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            DisposeInternal();

            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            await DisposeInternalAsync();

            GC.SuppressFinalize(this);
        }

        public void DisposeInternal()
        {
            if (_disposed)
            {
                return;
            }

            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Terminate();
        }

        public async ValueTask DisposeInternalAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (ServiceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Terminate();
        }

        public void Terminate()
        {
            ServiceProvider = null;
            IsActivated = false;
            Blueprint = null;
            Pipeline = null;
        }

        ~ShellContext()
        {
            DisposeInternal();
        }
    }
}
