using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public static class ServiceDescriptorExtensions
    {
        public static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor is HostSingleton hostSingleton)
            {
                // Use the host descriptor as it was before cloning.
                return hostSingleton.HostDescriptor.GetImplementationType();
            }

            if (descriptor.ImplementationType is object)
            {
                return descriptor.ImplementationType;
            }

            if (descriptor.ImplementationInstance is object)
            {
                return descriptor.ImplementationInstance.GetType();
            }

            if (descriptor.ImplementationFactory is object)
            {
                return descriptor.ImplementationFactory.GetType().GenericTypeArguments[1];
            }

            return null;
        }
    }

    public class HostSingleton : ServiceDescriptor
    {
        public HostSingleton(ServiceDescriptor hostDescriptor, object implementationInstance)
            : base(hostDescriptor.ServiceType, implementationInstance)
        {
            HostDescriptor = hostDescriptor;
        }

        public HostSingleton(ServiceDescriptor hostDescriptor, Func<IServiceProvider, object> implementationFactory)
            : base(hostDescriptor.ServiceType, implementationFactory, ServiceLifetime.Singleton)
        {
            HostDescriptor = hostDescriptor;
        }

        public ServiceDescriptor HostDescriptor { get; }
    }
}