using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Logging;
using Orchard.Hosting.Extensions;
using Orchard.Hosting.Web.Routing;

namespace Orchard.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureWebHost(
            this IApplicationBuilder builder,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddOrchardLogging(builder.ApplicationServices);

            // Add static files to the request pipeline.
            builder.UseStaticFiles();

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            builder.UseMiddleware<OrchardContainerMiddleware>();

            // Route the request to the correct Orchard pipeline
            builder.UseMiddleware<OrchardRouterMiddleware>();

            return builder;
        }
    }
}