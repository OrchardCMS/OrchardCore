using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host level shell services.
        /// </summary>
        public static OrchardCoreBuilder AddShellHost(this OrchardCoreBuilder builder)
        {
            AddShellHostServices(builder.Services);
            return builder;
        }

        public static void AddShellHostServices(IServiceCollection services)
        {
            // Register the type as it's implementing two interfaces which can be resolved independently
            services.AddSingleton<ShellHost>();
            services.AddSingleton<IShellHost>(sp => sp.GetRequiredService<ShellHost>());
            services.AddSingleton<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<ShellHost>());

            {
                // Use a single default site by default, i.e. if WithTenants hasn't been called before.
                services.TryAddSingleton<IShellSettingsManager, SingleShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }

            services.AddSingleton<IRunningShellTable, RunningShellTable>();
        }
    }
}
