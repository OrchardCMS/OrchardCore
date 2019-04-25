using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ClonedSingletonDescriptor : ServiceDescriptor
    {
        public ClonedSingletonDescriptor(ServiceDescriptor hostDescriptor, object implementationInstance)
            : base(hostDescriptor.ServiceType, implementationInstance)
        {
            HostDescriptor = hostDescriptor;
        }

        public ClonedSingletonDescriptor(ServiceDescriptor hostDescriptor, Func<IServiceProvider, object> implementationFactory)
            : base(hostDescriptor.ServiceType, implementationFactory, ServiceLifetime.Singleton)
        {
            HostDescriptor = hostDescriptor;
        }

        public ServiceDescriptor HostDescriptor { get; }
    }
}