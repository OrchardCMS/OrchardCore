using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tenant.Descriptor;
using OrchardCore.Tenant.Descriptor.Settings;

namespace OrchardCore.Tenant
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<ITenantDescriptorManager, AllFeaturesTenantDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddSetFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<ITenantDescriptorManager, SetFeaturesTenantDescriptorManager>();

            return services;
        }
    }
}