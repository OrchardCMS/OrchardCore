using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;

namespace Orchard.Environment.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add host level services for managing extensions.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddExtensionManagerHost(
            this IServiceCollection services,
            string rootProbingName,
            string dependencyProbingDirectoryName)
        {
            services.AddSingleton<IManifestBuilder, ManifestBuilder>();
            services.AddSingleton<IManifestProvider, ManifestProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ManifestOptions>, ManifestOptionsSetup>());

            services.AddSingleton<IExtensionProvider, ExtensionProvider>();
            services.AddSingleton<IExtensionManager, ExtensionManager>();
            {
                services.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();

                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<ExtensionOptions>, ExtensionOptionsSetup>());


                services.AddSingleton<IExtensionLoader, AmbientExtensionLoader>();
                services.AddSingleton<IExtensionLoader, DynamicExtensionLoader>();
                services.AddSingleton<IExtensionLoader, PrecompiledExtensionLoader>();

                services.Configure<ExtensionProbingOptions>(options =>
                {
                    options.RootProbingName = rootProbingName;
                    options.DependencyProbingDirectoryName = dependencyProbingDirectoryName;
                });

                services.AddSingleton<IExtensionLibraryService, ExtensionLibraryService>();
            }

            services.AddSingleton<IFeatureManager, FeatureManager>();

            return services;
        }

        public static IServiceCollection AddExtensionManager(this IServiceCollection services)
        {
            services.TryAddScoped<IFeatureManager, FeatureManager>();
            services.TryAddTransient<IFeatureHash, FeatureHash>();

            return services;
        }
    }
}