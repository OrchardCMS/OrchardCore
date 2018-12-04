using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

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
                if (services.First().ServiceType == typeof(IStartupFilter))
                {
                    continue;
                }

                // if only one service of a given type
                if (services.Count() == 1)
                {
                    var service = services.First();

                    // Register the singleton instances to all containers
                    if (service.Lifetime == ServiceLifetime.Singleton)
                    {
                        // Treat open-generic registrations differently
                        if (service.ServiceType.IsGenericType && service.ServiceType.GenericTypeArguments.Length == 0)
                        {
                            // There is no Func based way to register an open-generic type, instead of
                            // tenantServiceCollection.AddSingleton(typeof(IEnumerable<>), typeof(List<>));
                            // Right now, we register them as singleton per cloned scope even though it's wrong
                            // but in the actual examples it won't matter.
                            clonedCollection.AddSingleton(service.ServiceType, service.ImplementationType);
                        }
                        else
                        {
                            // When a service from the main container is resolved, just add its instance to the container.
                            // It will be shared by all tenant service providers.
                            clonedCollection.AddSingleton(service.ServiceType, serviceProvider.GetService(service.ServiceType));

                            // Ideally the service should be resolved when first requested, but ASP.NET DI will call Dispose()
                            // and this would fail reusability of the instance across tenants' containers.
                            // clonedCollection.AddSingleton(service.ServiceType, sp => serviceProvider.GetService(service.ServiceType));
                        }
                    }
                    else
                    {
                        clonedCollection.Add(service);
                    }
                }

                // If multiple services of the same type
                else
                {
                    // If all services of the same type are not singletons.
                    if (services.All(s => s.Lifetime != ServiceLifetime.Singleton))
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
            }

            return clonedCollection;
        }
    }
}