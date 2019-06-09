using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public static class ServiceDescriptorExtensions
    {
        public static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor is ClonedSingletonDescriptor cloned)
            {
                // Use the parent descriptor as it was before being cloned.
                return cloned.Parent.GetImplementationType();
            }

<<<<<<< HEAD
            if (descriptor.ImplementationType is object)
=======
            if (descriptor.ImplementationType != null)
>>>>>>> origin/dev
            {
                return descriptor.ImplementationType;
            }

<<<<<<< HEAD
            if (descriptor.ImplementationInstance is object)
=======
            if (descriptor.ImplementationInstance != null)
>>>>>>> origin/dev
            {
                return descriptor.ImplementationInstance.GetType();
            }

<<<<<<< HEAD
            if (descriptor.ImplementationFactory is object)
=======
            if (descriptor.ImplementationFactory != null)
>>>>>>> origin/dev
            {
                return descriptor.ImplementationFactory.GetType().GenericTypeArguments[1];
            }

            return null;
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/dev
