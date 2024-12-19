using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceDescriptorExtensions
{
    public static Type? GetImplementationType(this ServiceDescriptor descriptor)
    {
        if (descriptor is ClonedSingletonDescriptor cloned)
        {
            // Use the parent descriptor as it was before being cloned.
            return cloned.Parent.GetImplementationType();
        }

        if (descriptor.TryGetImplementationTypeInternal(out var implementationType))
        {
            return implementationType;
        }

        if (descriptor.TryGetImplementationInstance(out var implementationInstance))
        {
            return implementationInstance?.GetType();
        }

        if (descriptor.TryGetImplementationFactory(out var implementationFactory))
        {
            return implementationFactory?.GetType().GenericTypeArguments[1];
        }

        return null;
    }

    public static object? GetImplementationInstance(this ServiceDescriptor serviceDescriptor) => serviceDescriptor.IsKeyedService
        ? serviceDescriptor.KeyedImplementationInstance
        : serviceDescriptor.ImplementationInstance;

    public static object? GetImplementationFactory(this ServiceDescriptor serviceDescriptor) => serviceDescriptor.IsKeyedService
        ? serviceDescriptor.KeyedImplementationFactory
        : serviceDescriptor.ImplementationFactory;

    public static bool TryGetImplementationType(this ServiceDescriptor serviceDescriptor, out Type? type)
    {
        type = serviceDescriptor.GetImplementationType();

        return type is not null;
    }

    public static bool TryGetImplementationInstance(this ServiceDescriptor serviceDescriptor, out object? instance)
    {
        instance = serviceDescriptor.GetImplementationInstance();

        return instance is not null;
    }

    public static bool TryGetImplementationFactory(this ServiceDescriptor serviceDescriptor, out object? factory)
    {
        factory = serviceDescriptor.GetImplementationFactory();

        return factory is not null;
    }

    internal static Type? GetImplementationTypeInternal(this ServiceDescriptor serviceDescriptor) => serviceDescriptor.IsKeyedService
        ? serviceDescriptor.KeyedImplementationType
        : serviceDescriptor.ImplementationType;

    internal static bool TryGetImplementationTypeInternal(this ServiceDescriptor serviceDescriptor, out Type? type)
    {
        type = serviceDescriptor.GetImplementationTypeInternal();

        return type is not null;
    }
}
