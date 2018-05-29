using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

                services.AddWebHost(builder);

                // ModularTenantRouterMiddleware which is configured with UseModules() calls UserRouter() which requires the routing services to be
                // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
                services.AddRouting();

                // Use a single tenant and all features by default
                builder.Services.AddAllFeaturesDescriptor();

                // Registers the application main feature
                services.AddTransient(sp =>
                {
                    return new ShellFeature(sp.GetRequiredService<IHostingEnvironment>()
                        .ApplicationName, alwaysEnabled: true);
                });

                // Register the list of services to be resolved later on
                services.AddSingleton(_ => services);
            }

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(builder);
            builder.AddStartups();

            return services;
        }

        /// <summary>
        /// Registers a set of features which are always enabled.
        /// </summary>
        public static IServiceCollection AddEnabledFeatures(this IServiceCollection services, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                services.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
            }

            return services;
        }

        public static IServiceCollection AddWebHost(this IServiceCollection services, OrchardCoreBuilder builder)
        {
            services.AddLogging();
            services.AddOptions();
            services.AddLocalization();
            services.AddHostingShellServices();
            services.AddExtensionManagerHost();
            services.AddWebEncoders();

            builder.AddManifestDefinition("module")
                .Startup.ConfigureServices((tenant, sp) =>
                {
                    tenant.Services.AddExtensionManager();
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddScoped<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();

            return services;
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
