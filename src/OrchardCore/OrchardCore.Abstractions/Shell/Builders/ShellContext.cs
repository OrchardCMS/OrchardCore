using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed = false;
        internal volatile int _refCount = 0;
        internal bool _released = false;
        private List<WeakReference<ShellContext>> _dependents;
        private object _synLock = new object();

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

        private bool _placeHolder;

        public class PlaceHolder : ShellContext
        {
            /// <summary>
            /// Used as a place holder for a shell that will be lazily created.
            /// </summary>
            public PlaceHolder()
            {
                _placeHolder = true;
                _released = true;
                _disposed = true;
            }
        }

        public ShellScope CreateScope()
        {
            if (_placeHolder)
            {
                return null;
            }

            var scope = new ShellScope(this);

            // A new scope can be only used on a non released shell.
            if (!_released)
            {
                return scope;
            }

            scope.Dispose();

            return null;
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
        /// Mark the <see cref="ShellContext"/> has a candidate to be released.
        /// </summary>
        public void Release()
        {
            if (_released == true)
            {
                // Prevent infinite loops with circular dependencies
                return;
            }

            // When a tenant is changed and should be restarted, its shell context is replaced with a new one,
            // so that new request can't use it anymore. However some existing request might still be running and try to
            // resolve or use its services. We then call this method to count the remaining references and dispose it
            // when the number reached zero.

            lock (_synLock)
            {
                if (_released == true)
                {
                    return;
                }

                _released = true;

                if (_dependents != null)
                {
                    foreach (var dependent in _dependents)
                    {
                        if (dependent.TryGetTarget(out var shellContext))
                        {
                            shellContext.Release();
                        }
                    }
                }

                // A ShellContext is usually disposed when the last scope is disposed, but if there are no scopes
                // then we need to dispose it right away.
                if (_refCount == 0)
                {
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Registers the specified shellContext as a dependency such that they are also reloaded when the current shell context is reloaded.
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

            // Disposes all the services registered for this shell
            if (ServiceProvider != null)
            {
                (ServiceProvider as IDisposable)?.Dispose();
                ServiceProvider = null;
            }

            IsActivated = false;
            Blueprint = null;
            Pipeline = null;

            _disposed = true;
        }

        ~ShellContext()
        {
            Close();
        }
    }
}
