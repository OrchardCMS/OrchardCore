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
            services.AddSingleton<IManifestProvider, ManifestProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ManifestOptions>, ManifestOptionsSetup>());

            services.AddSingleton<IExtensionProvider, ExtensionProvider>();
            services.AddSingleton<IExtensionManager, ExtensionManager>();
            {
                services.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
                services.AddSingleton<IFeaturesProvider, FeaturesProvider>();

                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<ExtensionExpanderOptions>, ExtensionExpanderOptionsSetup>());


                services.AddSingleton<IExtensionLoader, AmbientExtensionLoader>();

                services.AddSingleton<IExtensionDependencyStrategy, ExtensionDependencyStrategy>();
                services.AddSingleton<IExtensionPriorityStrategy, ExtensionPriorityStrategy>();

                services.Configure<ExtensionProbingOptions>(options =>
                {
                    options.RootProbingName = rootProbingName;
                    options.DependencyProbingDirectoryName = dependencyProbingDirectoryName;
                });
            }

            return services;
        }

        public static IServiceCollection AddExtensionManager(this IServiceCollection services)
        {
            services.TryAddTransient<IFeatureHash, FeatureHash>();

            return services;
        }

        public static IServiceCollection AddExtensionLocation(
            this IServiceCollection services,
            string subPath)
        {
            return services.Configure<ExtensionExpanderOptions>(configureOptions: options =>
            {
                options.Options.Add(new ExtensionExpanderOption { SearchPath = subPath });
            });
        }
    }
}