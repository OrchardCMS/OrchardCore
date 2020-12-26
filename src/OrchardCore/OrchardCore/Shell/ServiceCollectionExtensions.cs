using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Environment.Shell.Distributed;

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
                services.AddTransient<IConfigureOptions<ShellContextOptions>, ShellContextOptionsSetup>();
                services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }

            services.AddSingleton<IRunningShellTable, RunningShellTable>();

            services.AddSingleton<IHostedService, DistributedShellHostedService>();

            return services;
        }

        public static IServiceCollection AddAllFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, AllFeaturesShellDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddSetFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddSingleton<IShellDescriptorManager>(sp =>
            {
                var shellFeatures = sp.GetServices<ShellFeature>();
                return new SetFeaturesShellDescriptorManager(shellFeatures);
            });

            return services;
        }
    }
}
