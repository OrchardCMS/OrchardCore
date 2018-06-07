using Microsoft.AspNetCore.Builder;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Registers at the host level a set of features which are always enabled for any tenant.
        /// </summary>
        public static OrchardCoreBuilder AddEnabledFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.Services.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
            }

            return builder;
        }

        /// <summary>
        /// Registers a default tenant with a set of features that are used to setup and configure the actual tenants.
        /// For instance you can use this to add a custom Setup module.
        /// </summary>
        public static OrchardCoreBuilder WithDefaultFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.Services.AddTransient(sp => new ShellFeature(featureId));
            }

            return builder;
        }

        /// <summary>
        /// Registers tenants defined in configuration.
        /// </summary>
        public static OrchardCoreBuilder WithTenants(this OrchardCoreBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IShellSettingsConfigurationProvider, FileShellSettingsConfigurationProvider>();
            services.AddScoped<IShellDescriptorManager, FileShellDescriptorManager>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddScoped<ShellSettingsWithTenants>();

            return builder;
        }

        /// <summary>
        /// Registers a single tenant with the specified set of features.
        /// </summary>
        public static OrchardCoreBuilder WithFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.Services.AddTransient(sp => new ShellFeature(featureId));
            }

            builder.Services.AddSetFeaturesDescriptor();

            return builder;
        }

        /// <summary>
        /// Adds host and tenant level antiforgery services.
        /// </summary>
        public static OrchardCoreBuilder AddAntiForgery(this OrchardCoreBuilder builder)
        {
            builder.Services.AddAntiforgery();

            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddAntiForgery();
            });

            return builder;
        }

        /// <summary>
        /// Adds host and tenant level authentication services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddAuthentication(this OrchardCoreBuilder builder)
        {
            builder.Services.AddAuthentication();

            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddAuthentication();
            })

            .Configure((tenant, routes) =>
            {
                tenant.ApplicationBuilder.UseAuthentication();
            });

            return builder;
        }

        /// <summary>
        /// Adds tenant level data protection services.
        /// </summary>
        public static OrchardCoreBuilder AddDataProtection(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddDataProtection();
            });

            return builder;
        }
    }
}
