using System.IO.Enumeration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;

namespace OrchardCore.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostingShellServices(this IServiceCollection services)
        {
            services.AddSingleton<IShellHost, ShellHost>();
            services.AddSingleton<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<IShellHost>());

            {
                // Use a single default site by default, i.e. if WithTenants hasn't been called before
                services.TryAddSingleton<IShellSettingsManager, SingleShellSettingsManager>();
                services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }

            services.AddSingleton<IRunningShellTable, RunningShellTable>();

            services.AddHostedService<DistributedShellHostedService>();

            services.AddSingleton<IShellRemovalManager, ShellRemovalManager>();
            services.AddSingleton<IShellRemovingHandler, ShellWebRootRemovingHandler>();
            services.AddSingleton<IShellRemovingHandler, ShellSiteFolderRemovingHandler>();
            services.AddSingleton<IShellRemovingHandler, ShellSettingsRemovingHandler>();

            return services;
        }

        public static IServiceCollection AddAllFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, AllFeaturesShellDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddSetFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddSingleton<IShellDescriptorManager, SetFeaturesShellDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddNullFeatureProfilesService(this IServiceCollection services)
            => services.AddScoped<IFeatureProfilesService, NullFeatureProfilesService>();

        public static IServiceCollection AddFeatureValidation(this IServiceCollection services)
            => services
                .AddScoped<IFeatureValidationProvider, FeatureProfilesValidationProvider>()
                .AddScoped<IFeatureValidationProvider, DefaultTenantOnlyFeatureValidationProvider>();

        public static IServiceCollection ConfigureFeatureProfilesRuleOptions(this IServiceCollection services)
            => services
                .Configure<FeatureProfilesRuleOptions>(o =>
                {
                    o.Rules["Include"] = (expression, name) =>
                    {
                        if (FileSystemName.MatchesSimpleExpression(expression, name))
                        {
                            return (true, true);
                        }

                        return (false, false);
                    };

                    o.Rules["Exclude"] = (expression, name) =>
                    {
                        if (FileSystemName.MatchesSimpleExpression(expression, name))
                        {
                            return (true, false);
                        }

                        return (false, false);
                    };
                });
    }
}
