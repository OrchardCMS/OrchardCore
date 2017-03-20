using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Tenant.Builders;

namespace OrchardCore.Tenant
{
    public static class TenantServiceCollectionExtensions
    {
        public static IServiceCollection AddHostingTenantServices(this IServiceCollection services)
        {
            services.AddSingleton<TenantHost>();
            services.AddSingleton<ITenantHost>(sp => sp.GetRequiredService<TenantHost>());
            services.AddSingleton<ITenantDescriptorManagerEventHandler>(sp => sp.GetRequiredService<TenantHost>());
            {
                // Use a single default site by default, i.e. if AddMultiTenancy hasn't been called before
                services.TryAddSingleton<ITenantSettingsManager, SingleTenantSettingsManager>();

                services.AddSingleton<ITenantContextFactory, TenantContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<ITenantContainerFactory, TenantContainerFactory>();
                }
            }
            services.AddSingleton<IRunningTenantTable, RunningTenantTable>();

            return services;
        }
    }
}
