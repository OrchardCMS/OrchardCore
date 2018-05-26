using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing extensions.
        /// </summary>
        public static OrchardCoreBuilder AddExtensionManager(this OrchardCoreBuilder builder)
        {
            AddExtensionManagerHostServices(builder.Services);

            return builder.ConfigureTenantServices((collection, sp) =>
            {
                AddExtensionManagerTenantServices(collection);
            });
        }

        public static void AddExtensionManagerHostServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ManifestOptions>, ManifestOptionsSetup>());

            services.AddSingleton<IExtensionManager, ExtensionManager>();
            {
                services.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
                services.AddSingleton<IFeaturesProvider, FeaturesProvider>();
                services.AddSingleton<IExtensionDependencyStrategy, ExtensionDependencyStrategy>();
                services.AddSingleton<IExtensionPriorityStrategy, ExtensionPriorityStrategy>();
            }
        }

        public static void AddExtensionManagerTenantServices(IServiceCollection services)
        {
            services.TryAddTransient<IFeatureHash, FeatureHash>();
        }
    }
}