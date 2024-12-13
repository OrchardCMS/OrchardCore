using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Instantiates a new object of the specified type, but with support for constructor injection.
    /// </summary>
    public static TResult CreateInstance<TResult>(this IServiceProvider provider) where TResult : class
    {
        return CreateInstance<TResult>(provider, typeof(TResult));
    }

    /// <summary>
    /// Instantiates a new object of the specified type, but with support for constructor injection.
    /// </summary>
    public static TResult CreateInstance<TResult>(this IServiceProvider provider, Type type) where TResult : class
    {
        return (TResult)ActivatorUtilities.CreateInstance(provider, type);
    }

    /// <summary>
    /// Gets the service object of the specified type with the specified key.
    /// </summary>
    public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (provider is IKeyedServiceProvider keyedServiceProvider)
        {
            return keyedServiceProvider.GetKeyedService(serviceType, serviceKey);
        }

        throw new InvalidOperationException("This service provider doesn't support keyed services.");
    }
}
