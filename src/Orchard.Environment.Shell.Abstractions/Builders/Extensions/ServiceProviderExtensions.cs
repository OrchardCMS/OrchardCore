using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Shell.Builders
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
            var serviceTypes = new HashSet<Type>();
            
            foreach (var service in serviceCollection)
            {
                // Register the singleton instances to all containers
                if (service.Lifetime == ServiceLifetime.Singleton)
                {
                    var serviceTypeInfo = service.ServiceType.GetTypeInfo();

                    // Treat open-generic registrations differently
                    if (serviceTypeInfo.IsGenericType && serviceTypeInfo.GenericTypeArguments.Length == 0)
                    {
                        // There is no Func based way to register an open-generic type, instead of
                        // tenantServiceCollection.AddSingleton(typeof(IEnumerable<>), typeof(List<>));
                        // Right now, we regsiter them as singleton per cloned scope even though it's wrong
                        // but in the actual examples it won't matter.
                        clonedCollection.AddSingleton(service.ServiceType, service.ImplementationType);
                    }
                    else
                    {
                        // When a service from the main container is resolved, just add its instance to the container.
                        // It will be shared by all tenant service providers.
                        clonedCollection.AddSingleton(service.ServiceType, serviceProvider.GetService(service.ServiceType));

                        if (!serviceTypes.Add(service.ServiceType))
                        {
                            var serviceType = typeof(IEnumerable<>).MakeGenericType(service.ServiceType);

                            if (serviceTypes.Add(serviceType))
                            {
                                clonedCollection.AddSingleton(serviceType, serviceProvider.GetServices(service.ServiceType));
                            }
                        }

                        // Ideally the service should be resolved when first requested, but ASp.NET DI will call Dispose()
                        // and this would fail reusability of the instance across tenants' containers.
                        //clonedCollection.AddSingleton(service.ServiceType, sp => serviceProvider.GetService(service.ServiceType));
                    }
                }
                else
                {
                    clonedCollection.Add(service);
                }
            }

            return clonedCollection;
        }
    }
}