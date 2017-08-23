using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.ContentManagement.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiContentManagementDisplay(this IServiceCollection services)
        {
            services.TryAddScoped<IApiContentManager, ApiContentManager>();
            return services;
        }
    }
}
