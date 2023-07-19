using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
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
            var servicesByType = serviceCollection.GroupBy(s => s.ServiceType);

            foreach (var services in servicesByType)
            {
                // Prevent hosting 'IStartupFilter' to re-add middlewares to the tenant pipeline.
                if (services.Key == typeof(IStartupFilter))
                {
                }

                // A generic type definition is rather used to create other constructed generic types.
                else if (services.Key.IsGenericTypeDefinition)
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

                        if (typeof(IDisposable).IsAssignableFrom(service.GetImplementationType()) || service.ImplementationFactory != null)
                        {
                            // If disposable, register an instance that we resolve immediately from the main container.
                            clonedCollection.CloneSingleton(service, serviceProvider.GetService(service.ServiceType));
                        }
                        else
                        {
                            // If not disposable, the singleton can be resolved through a factory when first requested.
                            clonedCollection.CloneSingleton(service, sp => serviceProvider.GetService(service.ServiceType));

                            // Note: Most of the time a singleton of a given type is unique and not disposable. So,
                            // most of the time it will be resolved when first requested through a tenant container.
                        }
                    }
                    else
                    {
                        clonedCollection.Add(service);
                    }
                }

                // If all services of the same type are not singletons.
                else if (services.All(s => s.Lifetime != ServiceLifetime.Singleton))
                {
                    // We don't need to resolve them.
                    foreach (var service in services)
                    {
                        clonedCollection.Add(service);
                    }
                }

                // If all services of the same type are singletons.
                else if (services.All(s => s.Lifetime == ServiceLifetime.Singleton))
                {
                    // We can resolve them from the main container.
                    var instances = serviceProvider.GetServices(services.Key);

                    for (var i = 0; i < services.Count(); i++)
                    {
                        clonedCollection.CloneSingleton(services.ElementAt(i), instances.ElementAt(i));
                    }
                }

                // If singletons and scoped services are mixed.
                else
                {
                    // We need a service scope to resolve them.
                    using var scope = serviceProvider.CreateScope();
                    var instances = scope.ServiceProvider.GetServices(services.Key);

                    // Then we only keep singleton instances.
                    for (var i = 0; i < services.Count(); i++)
                    {
                        if (services.ElementAt(i).Lifetime == ServiceLifetime.Singleton)
                        {
                            clonedCollection.CloneSingleton(services.ElementAt(i), instances.ElementAt(i));
                        }
                        else
                        {
                            clonedCollection.Add(services.ElementAt(i));
                        }
                    }
                }
            }

            return clonedCollection;
        }
    }
}
