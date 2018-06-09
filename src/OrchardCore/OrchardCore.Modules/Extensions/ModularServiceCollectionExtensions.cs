using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModularServiceCollectionExtensions
    {
        /// <summary>
        /// Adds modules services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddModules(this IServiceCollection services, Action<IServiceCollection> configure = null)
        {
            services.AddWebHost();
            services.AddManifestDefinition("module");

            // ModularTenantRouterMiddleware which is configured with UseModules() calls UserRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            // Use a single tenant and all features by default
            services.AddAllFeaturesDescriptor();

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(services);

            // Registers the application feature
            services.AddTransient(sp =>
            {
                return new ShellFeature(sp.GetRequiredService<IHostingEnvironment>().ApplicationName);
            });

            // Register the list of services to be resolved later on
            services.AddSingleton(_ => services);

            return services;
        }

        /// <summary>
        /// Registers a default tenant with a set of features that are used to setup and configure the actual tenants.
        /// For instance you can use this to add a custom Setup module.
        /// </summary>
        public static IServiceCollection WithDefaultFeatures(
            this IServiceCollection services, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                services.AddTransient(sp => new ShellFeature(featureId));
            }

            return services;
        }

        /// <summary>
        /// Registers tenants defined in configuration.
        /// </summary>
        public static IServiceCollection WithTenants(this IServiceCollection services)
        {
            services.AddSingleton<IShellSettingsConfigurationProvider, FileShellSettingsConfigurationProvider>();
            services.AddScoped<IShellDescriptorManager, FileShellDescriptorManager>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddScoped<ShellSettingsWithTenants>();

            return services;
        }

        /// <summary>
        /// Registers a single tenant with the specified set of features.
        /// </summary>
        public static IServiceCollection WithFeatures(
            this IServiceCollection services, params string[] featureIds)
        {
            services.WithDefaultFeatures(featureIds);
            services.AddSetFeaturesDescriptor();

            return services;
        }

        public static IServiceCollection AddWebHost(
            this IServiceCollection services)
        {
            services.AddLogging();
            services.AddOptions();
            services.AddLocalization();
            services.AddHostingShellServices();
            services.AddExtensionManagerHost();
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