using System;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Extensions.DependencyInjection;
using Orchard.FileSystem;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Services;
using Orchard.Hosting.Services;

namespace Orchard.Hosting
{
    public static class HostServiceExtensions
    {
        public static IServiceCollection AddHost(
            this IServiceCollection services, Action<IServiceCollection> additionalDependencies)
        {
            services.AddFileSystems();

            // Caching - Move out
            //services.AddInstance<ICacheContextAccessor>(new CacheContextAccessor());
            //services.AddSingleton<ICache, Cache>();

            additionalDependencies(services);

            return services.AddFallback();
        }

        public static IServiceCollection AddHostCore(this IServiceCollection services)
        {
            services.AddSingleton<IClock, Clock>();

            services.AddSingleton<IOrchardHost, DefaultOrchardHost>();
            {
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                    {
                        services.AddSingleton<IOrchardLibraryManager, OrchardLibraryManager>();
                    }

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }
            services.AddSingleton<IRunningShellTable, RunningShellTable>();

            return services;
        }
    }
}