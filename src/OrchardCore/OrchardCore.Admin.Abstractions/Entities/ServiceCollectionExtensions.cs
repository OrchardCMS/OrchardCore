using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Entities.Scripting;
using OrchardCore.Scripting;

namespace OrchardCore.Entities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdminOptions(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IGlobalMethodProvider, AdminOptionsMethod>());
            return services;
        }
    }
}
