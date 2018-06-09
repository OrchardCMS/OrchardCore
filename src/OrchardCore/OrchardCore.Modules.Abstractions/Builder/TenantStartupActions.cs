using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Represents an action to configure the services of a tenant.
    /// </summary>
    /// <param name="services">The collection of service descriptors.</param>
    /// <param name="sp">A service provider containing the application services and a <see cref="ShellSettings"/> instance for the current tenant.</param>
    public delegate void ConfigureServicesDelegate(IServiceCollection services, IServiceProvider sp);

    /// <summary>
    /// Represents an action to configure the request's pipeline of a tenant.
    /// </summary>
    /// <param name="builder">The tenant request's pipeline.</param>
    /// <param name="routes">The route builder for the tenant.</param>
    /// <param name="sp">A service provider containing the tenant services.</param>
    public delegate void ConfigureDelegate(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider sp);

    public class TenantStartupActions
    {
        public TenantStartupActions(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public ICollection<ConfigureServicesDelegate> ConfigureServicesActions { get; } = new List<ConfigureServicesDelegate>();

        public ICollection<ConfigureDelegate> ConfigureActions { get; } = new List<ConfigureDelegate>();
    }
}
