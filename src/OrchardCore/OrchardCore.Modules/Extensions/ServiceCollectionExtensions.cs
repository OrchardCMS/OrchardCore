using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the essential OrchardCore services.
        /// </summary>
        public static OrchardCoreBuilder AddOrchardCore(this IServiceCollection services)
        {
            return GetBuilder(services);
        }

        /// <summary>
        /// Adds the essential OrchardCore services.
        /// </summary>
        public static IServiceCollection AddOrchardCore(this IServiceCollection services, Action<OrchardCoreBuilder> configure)
        {
            var builder = GetBuilder(services);

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(builder);

            return services;
        }

        private static OrchardCoreBuilder GetBuilder(IServiceCollection services)
        {
            var builder = GetServiceFromCollection<OrchardCoreBuilder>(services);

            if (builder == null)
            {
                builder = new OrchardCoreBuilder(services);
                services.AddSingleton(builder);

                ConfigureServices(builder);

                // Register the list of services to be resolved later on
                services.AddSingleton(services);
            }

            return builder;
        }

        private static void ConfigureServices(OrchardCoreBuilder builder)
        {
            var services = builder.Services;

            services.AddLogging();
            services.AddOptions();
            services.AddLocalization();
            services.AddWebEncoders();

            // ModularTenantRouterMiddleware which is configured with UseModules() calls UseRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            services.AddHostingShellServices();

            // Use a single tenant and all features by default
            services.AddAllFeaturesDescriptor();

            // Registers the application main feature
            services.AddTransient(sp => new ShellFeature
            (
                sp.GetRequiredService<IHostingEnvironment>().ApplicationName, alwaysEnabled: true)
            );

            services.AddExtensionManagerHost();
            builder.AddManifestDefinition("module");
            builder.Startup.ConfigureServices((tenant, sp) =>
            {
                tenant.Services.AddExtensionManager();
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services.LastOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance;
        }
    }
}
