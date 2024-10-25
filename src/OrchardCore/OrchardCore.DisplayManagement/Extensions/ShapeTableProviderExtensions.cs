using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Provides an extension methods for <see cref="IShapeTableProvider"/>.
/// </summary>
public static class ShapeTableProviderExtensions
{
    /// <summary>
    /// Registers required services for the specified <see cref="IShapeTableProvider"/>.
    /// </summary>
    /// <typeparam name="TShapeTableProvider">The <see cref="IShapeTableProvider"/> to be registered.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static IServiceCollection AddShapeTableProvider<TShapeTableProvider>(this IServiceCollection services)
        where TShapeTableProvider : class, IShapeTableProvider
        => services.AddScoped<IShapeTableProvider, TShapeTableProvider>();
}
