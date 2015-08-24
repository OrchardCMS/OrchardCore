using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Extensions;
using Microsoft.Framework.OptionsModel;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.Extensions;
using OrchardVNext.Hosting.Extensions.Folders;
using OrchardVNext.Hosting.Extensions.Loaders;
using OrchardVNext.Hosting.ShellBuilders;

namespace OrchardVNext.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddHostCore(this IServiceCollection services) {
            services.AddOptions();

            services.AddTransient<IOrchardHost, DefaultOrchardHost>();
            {
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                    {
                        services.AddSingleton<IOrchardLibraryManager, OrchardLibraryManager>();
                        services.AddSingleton<IExtensionManager, ExtensionManager>();
                        {
                            services.AddSingleton<IExtensionAssemblyLoader, ExtensionAssemblyLoader>();
                            
                            services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();

                            services.TryAddEnumerable(
                                ServiceDescriptor.Transient<IConfigureOptions<ExtensionHarvestingOptions>, ExtensionHarvestingOptionsSetup>());
                            services.AddSingleton<IExtensionLocator, ExtensionLocator>();

                            services.AddSingleton<IExtensionLoader, CoreExtensionLoader>();
                            services.AddSingleton<IExtensionLoader, DynamicExtensionLoader>();
                        }
                    }

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }

            return services;
        }
    }
}