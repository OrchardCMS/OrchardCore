using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates a child container.
    /// </summary>
    /// <param name="serviceProvider">The service provider to create a child container for.</param>
    /// <param name="serviceCollection">The services to clone.</param>
    public static IServiceCollection CreateChildContainer(this IServiceProvider serviceProvider, IServiceCollection serviceCollection)
    {
        IServiceCollection clonedCollection = new ServiceCollection();

        var servicesByType = serviceCollection.GroupBy(s => (s.ServiceType, s.ServiceKey));
        foreach (var services in servicesByType)
        {
            // Prevent hosting 'IStartupFilter' to re-add middleware to the tenant pipeline.
            if (services.Key.ServiceType == typeof(IStartupFilter))
            {
                continue;
            }

            // A generic type definition is rather used to create other constructed generic types.
            if (services.Key.ServiceType.IsGenericTypeDefinition)
            {
                // So, we just need to pass the descriptor.
                foreach (var service in services)
                {
                    clonedCollection.Add(service);
                }
            }

            // If only one service of a given type.
            else if (services.Count() == 1)
            {
                var service = services.First();
                if (service.Lifetime == ServiceLifetime.Singleton)
                {
                    // An host singleton is shared across tenant containers but only registered instances are not disposed
                    // by the DI, so we check if it is disposable or if it uses a factory which may return a different type.
                    if (typeof(IDisposable).IsAssignableFrom(service.GetImplementationType()) ||
                        service.GetImplementationFactory() is not null)
                    {
                        // If disposable, register an instance that we resolve immediately from the main container.
                        var instance = service.IsKeyedService
                            ? serviceProvider.GetRequiredKeyedService(services.Key.ServiceType, services.Key.ServiceKey)
                            : serviceProvider.GetRequiredService(services.Key.ServiceType);

                        clonedCollection.CloneSingleton(service, instance);
                    }
                    else if (!service.IsKeyedService)
                    {
                        // If not disposable, the singleton can be resolved through a factory when first requested.
                        clonedCollection.CloneSingleton(service, sp =>
                            serviceProvider.GetRequiredService(service.ServiceType));

                        // Note: Most of the time a singleton of a given type is unique and not disposable. So,
                        // most of the time it will be resolved when first requested through a tenant container.
                    }
                    else
                    {
                        clonedCollection.CloneSingleton(service, (sp, key) =>
                            serviceProvider.GetRequiredKeyedService(service.ServiceType, key));
                    }
                }
                else
                {
                    clonedCollection.Add(service);
                }
            }

            // If services of the same type have at least one singleton.
            else if (services.Any(s => s.Lifetime == ServiceLifetime.Singleton))
            {
                // We can resolve them from the main container.
                var instances = services.Key.ServiceKey is not null
                    ? serviceProvider.GetKeyedServices(services.Key.ServiceType, services.Key.ServiceKey)
                    : serviceProvider.GetServices(services.Key.ServiceType);

                // Then we only keep singleton instances.
                for (var i = 0; i < services.Count(); i++)
                {
                    var service = services.ElementAt(i);
                    if (service.Lifetime == ServiceLifetime.Singleton)
                    {
                        var instance = instances.ElementAt(i);
                        if (instance is null)
                        {
                            continue;
                        }

                        clonedCollection.CloneSingleton(service, instance);
                    }
                    else
                    {
                        clonedCollection.Add(service);
                    }
                }
            }
            else    // If all services of the same type are not singletons.
            {
                // We don't need to resolve them.
                foreach (var service in services)
                {
                    clonedCollection.Add(service);
                }
            }
        }

        return clonedCollection;
    }
}
