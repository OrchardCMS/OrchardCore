using System;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Hosting.Services;
using Orchard.Services;

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
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

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