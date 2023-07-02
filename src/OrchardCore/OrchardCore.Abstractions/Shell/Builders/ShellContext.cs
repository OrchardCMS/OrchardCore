using System;
using System.Collections.Generic;
using System.Threading;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// Represents the state of a tenant.
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed;
        private List<WeakReference<ShellContext>> _dependents;
        private readonly object _synLock = new();

        internal volatile int _refCount;
        internal volatile int _terminated;
        internal long _requestTicks;
        internal bool _released;

        /// <summary>
        /// The <see cref="ShellSettings"/> holding the tenant settings and configuration.
        /// </summary>
        public ShellSettings Settings { get; set; }

        /// <summary>
        /// The <see cref="ShellBlueprint"/> describing the tenant container.
        /// </summary>
        public ShellBlueprint Blueprint { get; set; }

        /// <summary>
        /// The <see cref="IServiceProvider"/> of the tenant container.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Whether the shell is activated or not.
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// The UTC date and time of the last request.
        /// Read and write operations are both atomic.
        /// </summary>
        public DateTime LastRequestTimeUtc
        {
            get => new(Interlocked.Read(ref _requestTicks));
            internal set => Interlocked.Exchange(ref _requestTicks, value.Ticks);
        }

        /// <summary>
        /// The Pipeline built for this shell.
        /// </summary>
        public IShellPipeline Pipeline { get; set; }

        /// <summary>
        /// PlaceHolder class used for shell lazy initialization.
        /// </summary>
        public class PlaceHolder : ShellContext
        {
            /// <summary>
            /// Initializes a placeHolder used for shell lazy initialization.
            /// </summary>
            public PlaceHolder()
            {
                _released = true;
                _disposed = true;
            }

            /// <summary>
            /// Wether or not the tenant has been pre-created on first loading.
            /// </summary>
            public bool PreCreated { get; init; }
        }

        /// <summary>
        /// Creates a <see cref="ShellScope"/> on this shell context.
        /// </summary>
        public ShellScope CreateScope()
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
                scope.TerminateShellAsync().GetAwaiter().GetResult();
                return null;
            }

            return scope;
        }

        /// <summary>
        /// Whether the <see cref="ShellContext"/> instance is not yet built or has been released,
        /// for instance when a tenant has changed.
        /// </summary>
        public bool Released => _released;

        /// <summary>
        /// Returns the number of active scopes on this tenant.
        /// </summary>
        public int ActiveScopes => _refCount;

        /// <summary>
        /// Mark the <see cref="ShellContext"/> as released and then a candidate to be disposed.
        /// </summary>
        public void Release() => ReleaseInternal();

        internal void ReleaseFromLastScope() => ReleaseInternal(ReleaseMode.FromLastScope);

        internal void ReleaseFromDependency() => ReleaseInternal(ReleaseMode.FromDependency);

        internal void ReleaseInternal(ReleaseMode mode = ReleaseMode.Normal)
        {
            if (_released)
            {
                // Prevent infinite loops with circular dependencies
                return;
            }

            // A disabled shell still in use will be released by its last scope, as checked at the host level.
            if (mode == ReleaseMode.FromDependency && Settings.IsDisabled() && _refCount != 0)
            {
                return;
            }

            // When a tenant has changed its shell context is replaced with a new one, so that new requests can't use it anymore.
            // However, some uncompleted requests may still try to use or resolve services from child shell scopes. In that case,
            // this is the last shell scope (when the shell reference count reaches zero) that disposes its parent shell context.

            ShellScope scope = null;
            lock (_synLock)
            {
                if (_released)
                {
                    return;
                }

                if (_dependents is not null)
                {
                    foreach (var dependent in _dependents)
                    {
                        if (dependent.TryGetTarget(out var shellContext))
                        {
                            shellContext.ReleaseFromDependency();
                        }
                    }
                }

                if (mode != ReleaseMode.FromLastScope && ServiceProvider is not null)
                {
                    // Before marking the shell as released, we create a new scope that will manage the shell state,
                    // so that we always use the same shell scope logic to check if the reference counter reached 0.
                    scope = new ShellScope(this);
                }

                _released = true;
            }

            if (mode == ReleaseMode.FromLastScope)
            {
                return;
            }

            if (scope is not null)
            {
                // Use this scope to manage the shell state as usual.
                scope.TerminateShellAsync().GetAwaiter().GetResult();
                return;
            }

            Dispose();
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
        public void AddDependentShell(ShellContext shellContext)
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
                shellContext.Release();
                return;
            }

            lock (_synLock)
            {
                _dependents ??= new List<WeakReference<ShellContext>>();

                // Remove any previous instance that represent the same tenant in case it has been released (restarted).
                _dependents.RemoveAll(x => !x.TryGetTarget(out var shell) || shell.Settings.Name == shellContext.Settings.Name);

                _dependents.Add(new WeakReference<ShellContext>(shellContext));
            }
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Disposes all the services registered for this shell.
            if (ServiceProvider is not null)
            {
                (ServiceProvider as IDisposable)?.Dispose();
                ServiceProvider = null;
            }

            IsActivated = false;
            Blueprint = null;
            Pipeline = null;
        }

        ~ShellContext()
        {
            Close();
        }
    }
}
