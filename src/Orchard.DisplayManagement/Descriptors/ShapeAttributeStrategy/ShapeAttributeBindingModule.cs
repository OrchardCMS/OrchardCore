using Orchard.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingModule : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            ServiceDescriptor[] serviceDescriptors = new ServiceDescriptor[serviceCollection.Count];
            serviceCollection.CopyTo(serviceDescriptors, 0);

            foreach (var serviceDescriptor in serviceDescriptors)
            {
                var methods = serviceDescriptor.ServiceType.GetMethods();
                foreach (var method in methods)
                {
                    var customAttributes = method.GetCustomAttributes(typeof(ShapeAttribute), false).OfType<ShapeAttribute>();
                    foreach (var customAttribute in customAttributes)
                    {
                        serviceCollection.AddInstance(
                            new ShapeAttributeOccurrence(customAttribute,
                            method,
                            serviceDescriptor.ServiceType));
                    }
                }
            }
        }
    }
}