using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ClonedSingletonDescriptor : ServiceDescriptor
    {
        public ClonedSingletonDescriptor(ServiceDescriptor parent, object implementationInstance)
            : base(parent.ServiceType, implementationInstance)
        {
            Parent = parent;
        }

        public ClonedSingletonDescriptor(ServiceDescriptor parent, Func<IServiceProvider, object> implementationFactory)
            : base(parent.ServiceType, implementationFactory, ServiceLifetime.Singleton)
        {
            Parent = parent;
        }

        public ServiceDescriptor Parent { get; }
    }
}
