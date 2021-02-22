using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Forms.Entities.Scripting;
using OrchardCore.Scripting;

namespace OrchardCore.Forms.Entities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidateRuleMethod(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IGlobalMethodProvider, ValidateRuleMethod>());
            return services;
        }
    }
}
