using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Modules
{
    /// <summary>
    /// TODO: After RTM, resolve IStartup using the Application container, but inject tenant services in the Configure method.
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// Setup application services.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        void ConfigureServices(IServiceCollection services);

        void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider);
    }
}
