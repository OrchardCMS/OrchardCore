using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantServicesBuilder
    {
        public TenantServicesBuilder(IServiceCollection services, IServiceProvider serviceProvider)
        {
            Services = services;
            ServiceProvider = serviceProvider;
        }

        public IServiceCollection Services { get; }
        public IServiceProvider ServiceProvider { get; }
    }
}
