using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OrchardCore.Environment.Shell.Builders;

public class ClonedSingletonDescriptor : ServiceDescriptor
{
    public ClonedSingletonDescriptor(ServiceDescriptor parent, object implementationInstance)
        : base(parent.ServiceType, implementationInstance)
    {
        Parent = parent;
    }

    public ClonedSingletonDescriptor(ServiceDescriptor parent, object? serviceKey, object implementationInstance)
        : base(parent.ServiceType, serviceKey, implementationInstance)
    {
        Parent = parent;
    }

    public ClonedSingletonDescriptor(ServiceDescriptor parent, Func<IServiceProvider, object> implementationFactory)
        : base(parent.ServiceType, implementationFactory, ServiceLifetime.Singleton)
    {
        Parent = parent;
    }

    public ClonedSingletonDescriptor(ServiceDescriptor parent, object? serviceKey, Func<IServiceProvider, object?, object> implementationFactory)
        : base(parent.ServiceType, serviceKey, implementationFactory, ServiceLifetime.Singleton)
    {
        Parent = parent;
    }

    public ServiceDescriptor Parent { get; }
}
