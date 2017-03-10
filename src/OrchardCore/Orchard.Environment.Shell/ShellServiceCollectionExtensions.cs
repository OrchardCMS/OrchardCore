using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Shell.Builders;

namespace Orchard.Environment.Shell
{
    public static class ShellServiceCollectionExtensions
    {
        public static IServiceCollection AddHostingShellServices(this IServiceCollection services)
        {
            services.AddSingleton<ShellHost>();
            services.AddSingleton<IShellHost>(sp => sp.GetRequiredService<ShellHost>());
            services.AddSingleton<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<ShellHost>());
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
