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

            builder.UseMiddleware<OrchardContainerMiddleware>();

            // Think this needs to be inserted in a different part of the pipeline, possibly
            // when DI is created for the shell
            builder.UseMiddleware<OrchardRouterMiddleware>();

            return builder;
        }
    }
}