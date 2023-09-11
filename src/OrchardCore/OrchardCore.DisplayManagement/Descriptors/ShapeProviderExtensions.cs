using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.DisplayManagement.Descriptors;

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
