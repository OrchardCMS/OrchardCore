using System;
using OrchardCore.Tenant.Builders.Models;
using OrchardCore.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Hosting.TenantBuilders
{
    /// <summary>
    /// The tenant context represents the tenant's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class TenantContext : IDisposable
    {
        private bool _disposed = false;

        public TenantSettings Settings { get; set; }
        public TenantBlueprint Blueprint { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Whether the tenant is activated.
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        public IServiceScope CreateServiceScope()
        {
            return ServiceProvider.CreateScope();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                // Disposes all the services registered for this tenant
                if (ServiceProvider != null)
                {
                    (ServiceProvider as IDisposable)?.Dispose();
                    ServiceProvider = null;
                }

                IsActivated = false;

                Settings = null;
                Blueprint = null;

                _disposed = true;
            }
        }

        ~TenantContext()
        {
            Dispose(false);
        }
    }
}