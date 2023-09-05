using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Registers at the host level a set of features which are always enabled for any tenant.
        /// </summary>
        public static OrchardCoreBuilder AddGlobalFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.ApplicationServices.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
            }

            return builder;
        }

        /// <summary>
        /// Registers at the tenant level a set of features which are always enabled.
        /// </summary>
        public static OrchardCoreBuilder AddTenantFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            builder.ConfigureServices(services =>
            {
                foreach (var featureId in featureIds)
                {
                    services.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
                }
            });

            return builder;
        }

        /// <summary>
        /// Registers a default tenant with a set of features that are used to setup and configure the actual tenants.
        /// For instance you can use this to add a custom Setup module.
        /// </summary>
        public static OrchardCoreBuilder AddSetupFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.ApplicationServices.AddTransient(sp => new ShellFeature(featureId));
            }

            return builder;
        }

        /// <summary>
        /// Registers tenants defined in configuration.
        /// </summary>
        public static OrchardCoreBuilder WithTenants(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.AddSingleton<IShellsSettingsSources, ShellsSettingsSources>();
            services.AddSingleton<IShellsConfigurationSources, ShellsConfigurationSources>();
            services.AddSingleton<IShellConfigurationSources, ShellConfigurationSources>();
            services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

            return builder.ConfigureServices(s =>
            {
                s.AddScoped<IShellDescriptorManager, ConfiguredFeaturesShellDescriptorManager>();
            });
        }

        /// <summary>
        /// Registers a single tenant with the specified set of features.
        /// </summary>
        public static OrchardCoreBuilder WithFeatures(this OrchardCoreBuilder builder, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                builder.ApplicationServices.AddTransient(sp => new ShellFeature(featureId));
            }

            builder.ApplicationServices.AddSetFeaturesDescriptor();

            return builder;
        }

        /// <summary>
        /// Registers and configures a background hosted service to manage tenant background tasks.
        /// </summary>
        public static OrchardCoreBuilder AddBackgroundService(this OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddHostedService<ModularBackgroundService>();

            builder.ApplicationServices
                .AddOptions<BackgroundServiceOptions>()
                .Configure<IConfiguration>((options, config) => config
                    .Bind("OrchardCore:OrchardCore_BackgroundService", options));

            builder.Configure(app =>
            {
                app.Use((context, next) =>
                {
                    // In the background only the endpoints middlewares need to be executed.
                    if (context.Items.ContainsKey("IsBackground"))
                    {
                        // Shortcut the tenant pipeline.
                        return Task.CompletedTask;
                    }

                    return next(context);
                });
            },
            order: Int32.MinValue);

            return builder;
        }
    }
}
