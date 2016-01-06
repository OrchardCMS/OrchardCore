using Orchard.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using Microsoft.AspNet.Mvc.Razor;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingModule : IModule
    {
        /// <summary>
        /// This module looks for any method of IShapeTableProvider implementations
        /// that has the <see cref="ShapeAttribute"/>.
        /// </summary>
        public void Configure(IServiceCollection serviceCollection)
        {
            // Copy the collection as we are about to change it
            ServiceDescriptor[] serviceDescriptors = new ServiceDescriptor[serviceCollection.Count];
            serviceCollection.CopyTo(serviceDescriptors, 0);

            foreach (var serviceDescriptor in serviceDescriptors)
            {
                var serviceType = serviceDescriptor.ImplementationType;

                if (serviceType == null || !typeof(IShapeTableProvider).IsAssignableFrom(serviceType))
                {
                    continue;
                }

                bool hasShapes = false;
                foreach (var method in serviceType.GetMethods())
                {
                    var customAttributes = method.GetCustomAttributes(typeof(ShapeAttribute), false).OfType<ShapeAttribute>();
                    foreach (var customAttribute in customAttributes)
                    {
                        hasShapes = true;
                        // PERF: Analyze the impact of an important number of instances
                        // in the service collection
                        serviceCollection.AddInstance(
                            new ShapeAttributeOccurrence(customAttribute,
                            method,
                            serviceType));
                    }
                }

                // If the type was registered using IDependency, we need to
                // register it itself or the invocation won't be able
                // to be done on the correct instance as the default ASP.NET DI
                // won't resolve a type explicitely
                if (hasShapes && serviceDescriptor.ImplementationInstance == null)
                {
                    serviceCollection.AddScoped(serviceType);
                }
            }
        }
    }
}