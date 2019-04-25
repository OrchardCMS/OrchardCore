using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public static class ServiceDescriptorExtensions
    {
        public static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor is ClonedSingletonDescriptor clonedSingleton)
            {
                // Use the parent descriptor as it was before being cloned.
                return clonedSingleton.Parent.GetImplementationType();
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
}