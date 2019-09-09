using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public interface IShapeTableManager
    {
        ShapeTable GetShapeTable(string themeId);
    }

    public interface IShapeTableProvider
    {
        void Discover(ShapeTableBuilder builder);
    }

    public interface IShapeTableHarvester : IShapeTableProvider
    {
    }

    /// <summary>
    /// Represents a marker interface for classes that have Shape methods tagged with <see cref="ShapeAttribute"/>. 
    /// </summary>
    public interface IShapeAttributeProvider
    {
    }

    public static class ShapeProviderExtensions
    {
        public static IServiceCollection AddShapeAttributes<T>(this IServiceCollection services) where T : class, IShapeAttributeProvider
        {
            services.AddScoped<T>();
            services.AddScoped<IShapeAttributeProvider, T>();

            return services;
        }

        public static IServiceCollection ReplaceShapeAttributes<T>(this IServiceCollection services) where T : class, IShapeAttributeProvider
        {
            var descriptors = services.Where(d => d.GetImplementationType() == typeof(T)).ToArray();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddShapeAttributes<T>();

            return services;
        }
    }
}
