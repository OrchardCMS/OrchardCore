using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.Extensions;
using OrchardVNext.Hosting.Extensions.Folders;
using OrchardVNext.Hosting.Extensions.Loaders;
using OrchardVNext.Hosting.ShellBuilders;

namespace OrchardVNext.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddHostCore(this IServiceCollection services) {
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
                            services.AddSingleton<IPackageAssemblyLookup, PackageAssemblyLookup>();
                            
                            services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();
                            services.AddSingleton<IExtensionFolders, CoreModuleFolders>();
                            services.AddSingleton<IExtensionFolders, ModuleFolders>();

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