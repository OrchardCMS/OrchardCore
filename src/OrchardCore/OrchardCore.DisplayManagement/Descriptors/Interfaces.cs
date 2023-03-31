using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DisplayManagement.Handlers;

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
            services.TryAddScoped<T>();
            services.TryAddEnumerable(
                ServiceDescriptor.Scoped<IShapeAttributeProvider, T>(sp => sp.GetRequiredService<T>()));
            return services;
        }
    }

    /// <summary>
    /// Represents a marker interface for classes that provide Shape placement informations
    /// </summary>
    public interface IShapePlacementProvider
    {
        /// <summary>
        /// Builds a contextualized <see cref="IPlacementInfoResolver"/>
        /// </summary>
        /// <param name="context">The <see cref="IBuildShapeContext"/> for which we need a placement resolver</param>
        /// <returns>An instance of <see cref="IPlacementInfoResolver"/> for the current context or <see langword="null"/> if this provider is not concerned.</returns>
        Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context);
    }

    /// <summary>
    /// Represents a class capable of resolving <see cref="PlacementInfo"/> of Shapes
    /// </summary>
    public interface IPlacementInfoResolver
    {
        /// <summary>
        /// Resolves <see cref="PlacementInfo"/> for the provided <see cref="ShapePlacementContext"/>
        /// </summary>
        /// <param name="placementContext">The <see cref="ShapePlacementContext"/></param>
        /// <returns>An <see cref="PlacementInfo"/> or <see langword="null"/> if not concerned.</returns>
        PlacementInfo ResolvePlacement(ShapePlacementContext placementContext);
    }
}
