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
                var firstService = services.First();

                // Prevent hosting 'IStartupFilter' to re-add middlewares to the tenant pipeline.
                if (firstService.ServiceType == typeof(IStartupFilter))
                {
                }

                // A generic type definition is rather used to create other constructed generic types.
                else if (firstService.ServiceType.IsGenericTypeDefinition)
                {
                    // So, we just need to pass the descriptor.
                    foreach (var service in services)
                    {
                        clonedCollection.Add(service);
                    }
                }

                // Fast path if only one service.
                else if (services.Count() == 1)
                {
                    if (firstService.Lifetime == ServiceLifetime.Singleton)
                    {
                        var instance = serviceProvider.GetService(firstService.ServiceType);

                        // When a service from the main container is resolved, just add its instance to the container.
                        // It will be shared by all tenant service providers.
                        clonedCollection.AddSingleton(firstService.ServiceType, instance);

                        // Ideally the service should be resolved when first requested, but ASP.NET DI will call Dispose()
                        // and this would fail reusability of the instance across tenants' containers.
                    }
                    else
                    {
                        clonedCollection.Add(firstService);
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

                    foreach (var instance in instances)
                    {
                        clonedCollection.AddSingleton(services.Key, instance);
                    }
                }

                // If singletons and scoped services are mixed.
                else
                {
                    // We need a service scope to resolve them.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var instances = scope.ServiceProvider.GetServices(services.Key);

                        // Then we only keep singleton instances.
                        for (var i = 0; i < services.Count(); i++)
                        {
                            if (services.ElementAt(i).Lifetime == ServiceLifetime.Singleton)
                            {
                                clonedCollection.AddSingleton(services.Key, instances.ElementAt(i));
                            }
                            else
                            {
                                clonedCollection.Add(services.ElementAt(i));
                            }
                        }
                    }
                }
            }

            return clonedCollection;
        }
    }
}