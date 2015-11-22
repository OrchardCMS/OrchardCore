using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.OptionsModel;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;

namespace Orchard.Environment
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExtensionManager(this IServiceCollection services)
        {
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

            return services;
        }
    }
}