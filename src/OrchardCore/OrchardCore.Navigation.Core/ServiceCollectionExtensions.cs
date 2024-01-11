using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Navigation
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNavigation(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<INavigationManager, NavigationManager>());

            return services;
        }
    }
}
