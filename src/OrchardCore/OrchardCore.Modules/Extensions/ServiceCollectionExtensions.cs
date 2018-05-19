using System;
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
        /// Create a new <see cref="OrchardCore.Modules.ModularServicesBuilder"/>.
        /// </summary>
        public static ModularServicesBuilder ToModules(this IServiceCollection services)
        {
            return new ModularServicesBuilder(services);
        }

        /// <summary>
        /// Adds modules services.
        /// </summary>
        public static ModularServicesBuilder AddModules(this IServiceCollection services)
        {
            return services.AddModules(null).ToModules();
        }

        /// <summary>
        /// Adds modules services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
        /// Kept for backward compatibility to still allow to pass a configure action and return a regular service collection.
        /// </summary>
        public static IServiceCollection AddModules(this IServiceCollection services, Action<ModularServicesBuilder> configure)
        {
            services.AddWebHost();
            services.AddManifestDefinition("module");

            // ModularTenantRouterMiddleware which is configured with UseModules() calls UserRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            // Use a single tenant and all features by default
            services.AddAllFeaturesDescriptor();

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(services.ToModules());

            // Registers the application main feature
            services.AddTransient(sp =>
            {
                return new ShellFeature(sp.GetRequiredService<IHostingEnvironment>().ApplicationName);
            });

            // Register the list of services to be resolved later on
            services.AddSingleton(_ => services);

            return services;
        }

        public static IServiceCollection AddWebHost(
            this IServiceCollection services)
        {
            services.AddLogging();
            services.AddOptions();
            services.AddLocalization();
            services.AddHostingShellServices();

            services.AddExtensionManagerHost().ConfigureTenantServices(collection =>
            {
                collection.AddExtensionManager();
            });

            services.AddWebEncoders();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();

            return services;
        }
    }
}
