using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed;
        private List<WeakReference<ShellContext>> _dependents;
        private readonly object _synLock = new object();

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
            if (mode == ReleaseMode.FromDependency && Settings.State == TenantState.Disabled && _refCount != 0)
            {
                return;
            }

            // When a tenant is changed and should be restarted, its shell context is replaced with a new one,
            // so that new request can't use it anymore. However some existing request might still be running and try to
            // resolve or use its services. We then call this method to count the remaining references and dispose it
            // when the number reached zero.

            ShellScope scope = null;
            lock (_synLock)
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
                            shellContext.ReleaseFromDependency();
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

            if (mode == ReleaseMode.FromLastScope)
            {
                return;
            }

            if (scope != null)
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
                if (_dependents == null)
                {
                    _dependents = new List<WeakReference<ShellContext>>();
                }

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

            // Disposes all the services registered for this shell
            if (ServiceProvider != null)
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
