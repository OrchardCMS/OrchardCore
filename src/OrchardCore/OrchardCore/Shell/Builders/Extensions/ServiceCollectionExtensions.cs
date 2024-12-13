using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OrchardCore.Environment.Shell.Builders;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection CloneSingleton(
        this IServiceCollection services,
        ServiceDescriptor parent,
        object implementationInstance)
    {
        var cloned = parent.ServiceKey is not null
            ? new ClonedSingletonDescriptor(parent, parent.ServiceKey, implementationInstance)
            : new ClonedSingletonDescriptor(parent, implementationInstance);

        services.Add(cloned);

        return services;
    }

    public static IServiceCollection CloneSingleton(
        this IServiceCollection collection,
        ServiceDescriptor parent,
        Func<IServiceProvider, object> implementationFactory)
    {
        var cloned = new ClonedSingletonDescriptor(parent, implementationFactory);
        collection.Add(cloned);

        return collection;
    }

    public static IServiceCollection CloneSingleton(
        this IServiceCollection collection,
        ServiceDescriptor parent,
        Func<IServiceProvider, object?, object> implementationFactory)
    {
        var cloned = new ClonedSingletonDescriptor(parent, parent.ServiceKey, implementationFactory);
        collection.Add(cloned);

        return collection;
    }
}
