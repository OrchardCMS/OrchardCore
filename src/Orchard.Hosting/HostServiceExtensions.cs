using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor;
using Orchard.FileSystem;
using Orchard.Hosting.Services;
using Orchard.Services;
using System;

namespace Orchard.Hosting
{
    public static class HostServiceExtensions
    {
        public static IServiceCollection AddHost(
            this IServiceCollection services, Action<IServiceCollection> additionalDependencies)
        {
            services.AddFileSystems();
            additionalDependencies(services);

            return services;
        }

        public static IServiceCollection AddHostCore(this IServiceCollection services)
        {
            services.AddSingleton<IClock, Clock>();

            services.AddSingleton<IOrchardHost, DefaultOrchardHost>();
            {
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<IShellDescriptorCache, ShellDescriptorCache>();

                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }
            services.AddSingleton<IRunningShellTable, RunningShellTable>();

            return services;
        }
    }
}