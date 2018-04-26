using System;
using System.Linq;
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
        public static IServiceCollection AddModules(this IServiceCollection services, Action<ModularServiceCollection> configure = null)
        {
            services.AddWebHost();
            services.AddManifestDefinition("module");

            // ModularTenantRouterMiddleware which is configured with UseModules() calls UserRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            var modularServiceCollection = new ModularServiceCollection(services);

            // Use a single tenant and all features by default
            modularServiceCollection.Configure(internalServices =>
                internalServices.AddAllFeaturesDescriptor()
            );

            // Let the app change the default tenant behavior and set of features
            configure?.Invoke(modularServiceCollection);

            // Register the list of services to be resolved later on
            services.AddSingleton(_ => services);

            return services;
        }

        /// <summary>
        /// Registers a default tenant with a set of features that are used to setup and configure the actual tenants.
        /// For instance you can use this to add a custom Setup module.
        /// </summary>
        public static ModularServiceCollection WithDefaultFeatures(
            this ModularServiceCollection modules, params string[] featureIds)
        {
            modules.Configure(services =>
            {
                foreach (var featureId in featureIds)
                {
                    services.AddTransient(sp => new ShellFeature(featureId));
                };
            });

            return modules;
        }

        /// <summary>
        /// Registers tenants defined in configuration.
        /// </summary>
        public static ModularServiceCollection WithTenants(this ModularServiceCollection modules)
        {
            modules.Configure(services =>
            {
                services.AddSingleton<IShellSettingsConfigurationProvider, FileShellSettingsConfigurationProvider>();
                services.AddScoped<IShellDescriptorManager, FileShellDescriptorManager>();
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
                services.AddScoped<ShellSettingsWithTenants>();
            });

            return modules;
        }

        /// <summary>
        /// Registers a single tenant with the specified set of features.
        /// </summary>
        public static ModularServiceCollection WithFeatures(
            this ModularServiceCollection modules,
            params string[] featureIds)
        {
            var featuresList = featureIds.Select(featureId => new ShellFeature(featureId)).ToList();

            modules.Configure(services =>
            {
                foreach (var feature in featuresList)
                {
                    services.AddTransient(sp => feature);
                };

                services.AddSetFeaturesDescriptor(featuresList);
            });

            return modules;
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

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();

            return services;
        }
    }
}
