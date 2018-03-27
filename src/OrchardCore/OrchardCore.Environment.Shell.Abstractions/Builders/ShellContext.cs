using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders.Models;

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
        private HashSet<ShellContext> _dependents;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Whether the shell is activated. 
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="track">Whether the returned scope is preventing the context from being recycled.</param>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        public IServiceScope EnterServiceScope(bool track = true)
        {
            if (_disposed)
            {
                throw new InvalidOperationException("Can't use EnterServiceScope on a disposed context");
            }

            if (_released)
            {
                throw new InvalidOperationException("Can't use EnterServiceScope on a released context");
            }

            return new ServiceScopeWrapper(track ? this : null, ServiceProvider.CreateScope());
        }

        /// <summary>
        /// Whether the <see cref="ShellContext"/> instance has been released, for instance when a tenant is changed.
        /// </summary>
        public bool Released => _released;

        /// <summary>
        /// Returns the number of active requests on this tenant.
        /// </summary>
        public int ActiveRequests => _refCount;

        /// <summary>
        /// Returns whether the shell can be used for new requests.
        /// </summary>
        public bool RequestStarted()
        {
            if (_released)
            {
                return false;
            }

            Interlocked.Increment(ref _refCount);

            return true;
        }

        /// <summary>
        /// Returns whether the shell can be released as a result of the request being ended.
        /// </summary>
        public bool RequestEnded()
        {
            var refCount = Interlocked.Decrement(ref _refCount);
            return _released && refCount == 0;
        }

        public bool CanTerminate => _released && _refCount == 0 && !_disposed;

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

            _released = true;

            lock (this)
            {
                if (_dependents == null)
                {
                    return;
                }

                foreach (var dependent in _dependents)
                {
                    dependent.Release();
                }
            }
        }

        /// <summary>
        /// Registers the specified shellContext as a dependency such that they are also reloaded when the current shell context is reloaded.
        /// </summary>
        public void AddDependentShell(ShellContext shellContext)
        {
            lock (this)
            {
                if (_dependents == null)
                {
                    _dependents = new HashSet<ShellContext>();
                }

                // The same item can safely be added multiple times in a Hashset
                _dependents.Add(shellContext);
            }
        }

        public void Dispose()
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

            Settings = null;
            Blueprint = null;

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~ShellContext()
        {
            Dispose();
        }

        internal class ServiceScopeWrapper : IServiceScope
        {
            private readonly ShellContext _shellContext;
            private readonly IServiceScope _serviceScope;
            private readonly IServiceProvider _existingServices;
            private readonly HttpContext _httpContext;

            public ServiceScopeWrapper(ShellContext shellContext, IServiceScope serviceScope)
            {
                ServiceProvider = serviceScope.ServiceProvider;
                _shellContext = shellContext;
                _serviceScope = serviceScope;

                var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                if (httpContextAccessor.HttpContext == null)
                {
                    httpContextAccessor.HttpContext = new DefaultHttpContext();
                }

                _httpContext = httpContextAccessor.HttpContext;
                _existingServices = _httpContext.RequestServices;
                _httpContext.RequestServices = ServiceProvider;

                // Prevents the context from being released until the end of the scope
                _shellContext?.RequestStarted();
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
                _httpContext.RequestServices = _existingServices;
                _serviceScope.Dispose();
                _shellContext?.RequestEnded();

                GC.SuppressFinalize(this);
            }

            ~ServiceScopeWrapper()
            {
                Dispose();
            }
        }
    }
}