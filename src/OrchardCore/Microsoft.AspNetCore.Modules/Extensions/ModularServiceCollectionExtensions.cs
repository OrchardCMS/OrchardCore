using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Microsoft.AspNetCore.Modules
{
    public static class ModularServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleServices(this IServiceCollection services, Action<ModularServiceCollection> configure)
        {
            services.AddWebHost();
            services.AddManifestDefinition("Module.txt", "module");
            services.AddExtensionLocation("Modules");

            // Register custom services specific to modules
            var modularServiceCollection = new ModularServiceCollection(services);
            configure(modularServiceCollection);

            // Register the list of services to be resolved later on
            services.AddSingleton(_ => services);

            return services;
        }

        public static ModularServiceCollection AddConfiguration(
            this ModularServiceCollection modules, IConfiguration configuration)
        {
            // Register the configuration object for modules to register options with it
            if (configuration != null)
            {
                modules.Configure(services => services.AddSingleton<IConfiguration>(configuration));
            }

            return modules;
        }

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
        /// Enables all available features.
        /// </summary>
        public static ModularServiceCollection WithAllFeatures(
            this ModularServiceCollection modules)
        {
            modules.Configure(services =>
            {
                services.AddAllFeaturesDescriptor();
            });

            return modules;
        }

        /// <summary>
        /// Enables all available features.
        /// </summary>
        public static ModularServiceCollection WithSetFeatures(
            this ModularServiceCollection modules,
            params string[] featureIds)
        {
            modules.Configure(services =>
            {
                foreach (var featureId in featureIds)
                {
                    services.AddTransient(sp => new ShellFeature(featureId));
                };

                services.AddSetFeaturesDescriptor();
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
            services.AddExtensionManagerHost("App_Data", "dependencies");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();

            services.AddScoped<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();

            return services;
        }
    }
}