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

        if (descriptor.ServiceKey == null)
        {
            if (descriptor.IsKeyedService())
            {
                throw new InvalidOperationException("This service descriptor is keyed. Your service provider may not support keyed services.");
            }

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
                return descriptor.ImplementationFactory.GetType().GenericTypeArguments[2];
            }
        }
        else
        {
            if (!descriptor.IsKeyedService())
            {
                throw new InvalidOperationException("This service descriptor is not keyed.");
            }

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
                return descriptor.KeyedImplementationFactory.GetType().GenericTypeArguments[2];
            }
        }

        return null;
    }

    private static bool IsKeyedService(this ServiceDescriptor descriptor) => descriptor.ServiceKey != null;
}
