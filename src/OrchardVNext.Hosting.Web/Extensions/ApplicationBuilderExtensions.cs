using Microsoft.AspNet.Builder;
using Microsoft.Framework.Logging;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Routing;
using OrchardVNext.Logging;

namespace OrchardVNext.Hosting {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder ConfigureWeb([NotNull] this IApplicationBuilder builder,
            ILoggerFactory loggerFactory) {

            loggerFactory.AddProvider(new TestLoggerProvider());

            builder.UseMiddleware<OrchardContainerMiddleware>();
            builder.UseMiddleware<OrchardShellHostMiddleware>();

            // Think this needs to be inserted in a different part of the pipeline, possibly
            // when DI is created for the shell
            builder.UseMiddleware<OrchardRouterMiddleware>();

            return builder;
        }

        public static void InitializeHost([NotNull] this IApplicationBuilder builder) {
            var host = builder.ApplicationServices.GetService<IOrchardHost>();
            host.Initialize();
        }
    }
}