using System;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;

namespace Orchard.Hosting
{
    public static class HostServiceExtensions
    {
        public static IServiceCollection AddHost(
            this IServiceCollection services, Action<IServiceCollection> additionalDependencies)
        {
            additionalDependencies(services);

            return services;
        }

        public static IServiceCollection AddHostCore(this IServiceCollection services)
        {
            services.AddSingleton<IClock, Clock>();

            services.AddSingleton<DefaultOrchardHost>();
            services.AddSingleton<IOrchardHost>(sp => sp.GetRequiredService<DefaultOrchardHost>());
            services.AddSingleton<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<DefaultOrchardHost>());
            {
                // Use a single default site by default, i.e. if AddMultiTenancy hasn't been called before
                services.TryAddSingleton<IShellSettingsManager, SingleShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }
            services.AddSingleton<IRunningShellTable, RunningShellTable>();

            return services;
        }
    }
}