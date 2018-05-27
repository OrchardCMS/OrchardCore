using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the minimum essential OrchardCore services.
        /// </summary>
        public static IServiceCollection AddOrchardCore(this IServiceCollection services, Action<OrchardCoreBuilder> configure = null)
        {
            var builder = GetServiceFromCollection<OrchardCoreBuilder>(services);

            if (builder == null)
            {
                builder = new OrchardCoreBuilder(services);
                services.AddSingleton(builder);

                builder
                    .AddShellHost()
                    .AddExtensionManager()
                    .AddManifestDefinition("module");

                AddDefaultHostServices(builder.Services);

                // Use a single tenant and all features by default
                services.AddAllFeaturesDescriptor();

                // Registers the application main feature
                services.AddTransient(sp =>
                {
                    return new ShellFeature(sp.GetRequiredService<IHostingEnvironment>().ApplicationName);
                });

                // Register the list of services to be resolved later on
                services.AddSingleton(_ => services);
            }

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(builder);

            return builder.AddStartups().Services;
        }

        public static void AddDefaultHostServices(IServiceCollection services)
        {
            services
                .AddLogging()
                .AddOptions()
                .AddLocalization()
                .AddWebEncoders()

                // ModularTenantRouterMiddleware which is configured with UseModules() calls UserRouter() which requires the routing services to be
                // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
                .AddRouting();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
