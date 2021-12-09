using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Entities.Scripting;
using OrchardCore.Scripting;

namespace OrchardCore.Entities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdGeneration(this IServiceCollection services)
        {
            services.TryAddSingleton<IIdGenerator, DefaultIdGenerator>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IGlobalMethodProvider, IdGeneratorMethod>());
            return services;
        }
    }
}
