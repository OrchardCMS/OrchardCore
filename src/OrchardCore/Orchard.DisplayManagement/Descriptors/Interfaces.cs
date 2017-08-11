using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;

namespace Orchard.DisplayManagement.Descriptors
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

    public interface IShapeTableEventHandler : IEventHandler
    {
        void ShapeTableCreated(ShapeTable shapeTable);
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
    }
}