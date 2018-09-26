using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Hosting.ShellBuilders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed = false;
        private volatile int _refCount = 0;
        private bool _released = false;
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
        /// The HTTP Request delegate built for this shell.
        /// </summary>
        public RequestDelegate Pipeline { get; set; }

        public IServiceScope CreateScope()
        {
            var scope = new ServiceScopeWrapper(this);

            // A new scope can be only used on a non released shell.
            if (!_released)
            {
                return scope;
            }

            (scope as ServiceScopeWrapper).Dispose();

            return null;
        }

        /// <summary>
        /// Whether the <see cref="ShellContext"/> instance has been released, for instance when a tenant is changed.
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

        internal class ServiceScopeWrapper : IServiceScope
        {
            private readonly ShellContext _shellContext;
            private readonly IServiceScope _serviceScope;
            private readonly IServiceProvider _existingServices;
            private readonly HttpContext _httpContext;

            public ServiceScopeWrapper(ShellContext shellContext)
            {
                // Prevent the context from being disposed until the end of the scope
                Interlocked.Increment(ref shellContext._refCount);

                _shellContext = shellContext;

                // The service provider is null if we try to create
                // a scope on a disabled shell or already disposed.
                if (_shellContext.ServiceProvider == null)
                {
                    throw new ArgumentNullException(nameof(shellContext.ServiceProvider), $"Can't resolve a scope on tenant: {shellContext.Settings.Name}");
                }

                _serviceScope = shellContext.ServiceProvider.CreateScope();
                ServiceProvider = _serviceScope.ServiceProvider;

                var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                _httpContext = httpContextAccessor.HttpContext;
                _existingServices = _httpContext.RequestServices;
                _httpContext.RequestServices = ServiceProvider;
            }

            public IServiceProvider ServiceProvider { get; }

            /// <summary>
            /// Returns true if the shell context should be disposed consequently to this scope being released.
            /// </summary>
            private bool ScopeReleased()
            {
                // A disabled shell still in use is released by its last scope.
                if (_shellContext.Settings.State == TenantState.Disabled)
                {
                    if (Interlocked.CompareExchange(ref _shellContext._refCount, 1, 1) == 1)
                    {
                        _shellContext.Release();
                    }
                }

                // If the context is still being released, it will be disposed if the ref counter is equal to 0.
                // To prevent this while executing the terminating events, the ref counter is not decremented here.
                if (_shellContext._released && Interlocked.CompareExchange(ref _shellContext._refCount, 1, 1) == 1)
                {
                    var tenantEvents = _serviceScope.ServiceProvider.GetServices<IModularTenantEvents>();

                    foreach (var tenantEvent in tenantEvents)
                    {
                        tenantEvent.TerminatingAsync().GetAwaiter().GetResult();
                    }

                    foreach (var tenantEvent in tenantEvents.Reverse())
                    {
                        tenantEvent.TerminatedAsync().GetAwaiter().GetResult();
                    }

                    return true;
                }

                return false;
            }

            public void Dispose()
            {
                var disposeShellContext = ScopeReleased();

                _httpContext.RequestServices = _existingServices;
                _serviceScope.Dispose();

                if (disposeShellContext)
                {
                    _shellContext.Dispose();
                }

                // Decrement the counter at the very end of the scope
                Interlocked.Decrement(ref _shellContext._refCount);
            }
        }
    }
}
