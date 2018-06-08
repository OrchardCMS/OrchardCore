using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantStartupBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static TenantStartupBuilder ConfigureServices(this TenantStartupBuilder startup,
            Action<TenantServicesBuilder> configureServices, int order = 0)
        {
            return startup.AddConfigureServices(configureServices, order);
        }

        /// <summary>
        /// Configure the tenant pipeline before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static TenantStartupBuilder Configure(this TenantStartupBuilder startup,
            Action<TenantApplicationBuilder, IRouteBuilder> configure, int order = 0)
        {
            return startup.AddConfigure(configure, order);
        }
    }
}
