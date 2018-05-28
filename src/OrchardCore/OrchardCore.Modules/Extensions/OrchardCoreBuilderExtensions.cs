using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Modules;

namespace OrchardCore.Modules
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Registers at the host level a set of features which are always enabled for any tenant.
        /// </summary>
        public static OrchardCoreBuilder AddEnabledFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            builder.Services.AddEnabledFeatures(featureIds);
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
            builder.Services.AddSingleton<IShellSettingsConfigurationProvider, FileShellSettingsConfigurationProvider>();
            builder.Services.AddScoped<IShellDescriptorManager, FileShellDescriptorManager>();
            builder.Services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            builder.Services.AddScoped<ShellSettingsWithTenants>();

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

            builder.Startup.ConfigureServices((collection, sp) =>
            {
                var settings = sp.GetRequiredService<ShellSettings>();

                var tenantName = settings.Name;
                var tenantPrefix = "/" + settings.RequestUrlPrefix;

                collection.AddAntiforgery(options =>
                {
                    options.Cookie.Name = "orchantiforgery_" + tenantName;
                    options.Cookie.Path = tenantPrefix;
                });
            });

            return builder;
        }

        /// <summary>
        /// Adds host and tenant level authentication services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddAuthentication(this OrchardCoreBuilder builder)
        {
            builder.Services.AddAuthentication();

            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddAuthentication();

                // Note: IAuthenticationSchemeProvider is already registered at the host level.
                // We need to register it again so it is taken into account at the tenant level.
                collection.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            })

            .Configure((app, routes, sp) =>
            {
                app.UseAuthentication();
            });

            return builder;
        }

        /// <summary>
        /// Adds tenant level data protection services.
        /// </summary>
        public static OrchardCoreBuilder AddDataProtection(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                var settings = sp.GetRequiredService<ShellSettings>();
                var options = sp.GetRequiredService<IOptions<ShellOptions>>();

                var directory = Directory.CreateDirectory(Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName,
                settings.Name, "DataProtection-Keys"));

                // Re-register the data protection services to be tenant-aware so that modules that internally
                // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
                // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
                // By default, the key ring is stored in the tenant directory of the configured App_Data path.
                collection.Add(new ServiceCollection()
                    .AddDataProtection()
                    .PersistKeysToFileSystem(directory)
                    .SetApplicationName(settings.Name)
                    .Services);
            });

            return builder;
        }
    }
}
