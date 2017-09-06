using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Display.ContentDisplay;

namespace OrchardCore.ContentManagement.Display
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentManagementDisplay(this IServiceCollection services)
        {
            services.TryAddTransient<IContentItemDisplayManager, ContentItemDisplayManager>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IContentDisplayHandler), typeof(ContentItemDisplayCoordinator), ServiceLifetime.Scoped));
            return services;
        }
    }
}
