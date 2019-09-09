using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Shapes
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds core shapes services to the given collection.
        /// </summary>
        public static IServiceCollection AddCoreShapeAttributes(this IServiceCollection services)
        {
            services.AddShapeAttributes<CoreShapes>();
            services.AddShapeAttributes<ZoneShapes>();
            services.AddShapeAttributes<DateTimeShapes>();
            services.AddShapeAttributes<PageTitleShapes>();

            return services;
        }

        /// <summary>
        /// Replaces core shapes services to the given collection.
        /// </summary>
        public static IServiceCollection ReplaceCoreShapeAttributes(this IServiceCollection services)
        {
            services.ReplaceShapeAttributes<CoreShapes>();
            services.ReplaceShapeAttributes<ZoneShapes>();
            services.ReplaceShapeAttributes<DateTimeShapes>();
            services.ReplaceShapeAttributes<PageTitleShapes>();

            return services;
        }
    }
}
