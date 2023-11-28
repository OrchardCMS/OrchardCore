using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceDescriptorExtensions
{
    public static Type GetImplementationType(this ServiceDescriptor descriptor)
    {
        if (descriptor is ClonedSingletonDescriptor cloned)
        {
            // Use the parent descriptor as it was before being cloned.
            return cloned.Parent.GetImplementationType();
        }

        if (descriptor.IsKeyedService())
        {
            if (descriptor.KeyedImplementationType != null)
            {
                return descriptor.KeyedImplementationType;
            }

            if (descriptor.KeyedImplementationInstance != null)
            {
                return descriptor.KeyedImplementationInstance.GetType();
            }

            if (descriptor.KeyedImplementationFactory != null)
            {
                return descriptor.KeyedImplementationFactory.GetType().GenericTypeArguments[1];
            }
        }
        else
        {
            if (descriptor.ImplementationType != null)
            {
                return descriptor.ImplementationType;
            }

            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance.GetType();
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory.GetType().GenericTypeArguments[1];
            }
        }

        return null;
    }

    internal static bool IsKeyedService(this ServiceDescriptor descriptor) => descriptor.ServiceKey != null;
}
