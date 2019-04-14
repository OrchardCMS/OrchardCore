using System;
using System.Collections.Generic;
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
            var singletons = new Dictionary<Type, (int Index, IEnumerable<object> Instances)>();

            foreach (var service in serviceCollection)
            {
                // Prevent hosting 'IStartupFilter' to re-add middlewares to the tenant pipeline.
                if (service.ServiceType == typeof(IStartupFilter))
                {
                    continue;
                }

                if (service.Lifetime != ServiceLifetime.Singleton || service.ServiceType.IsGenericTypeDefinition)
                {
                    // This is not a singleton or a generic type definition which is rather used to create other
                    // constructed generic types. So, we just need to pass the descriptor.
                    clonedCollection.Add(service);
                }
                else
                {
                    // This is a singleton, a non generic or a constructed generic type.
                    if (!singletons.TryGetValue(service.ServiceType, out var singleton))
                    {
                        // Resolve once all singletons of this type.
                        singleton = (0, serviceProvider.GetServices(service.ServiceType));

                        if (singleton.Instances.Count() > 1)
                        {
                            // Cache if multiple instances.
                            singletons[service.ServiceType] = singleton;
                        }
                    }

                    // Retrieve the indexed singleton instance to clone.
                    var instance = singleton.Instances.ElementAt(singleton.Index++);

                    // When a service from the main container is resolved, just add its instance to the container.
                    // It will be shared by all tenant service providers.
                    clonedCollection.AddSingleton(service.ServiceType, instance);

                    // Ideally the service should be resolved when first requested, but ASP.NET DI will call Dispose()
                    // and this would fail reusability of the instance across tenants' containers.
                }
            }

            return clonedCollection;
        }
    }
}