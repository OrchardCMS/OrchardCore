using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

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

    public interface ITenantShapeTableProvider : IShapeTableProvider
    {
        public IChangeToken ChangeToken { get; }
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
