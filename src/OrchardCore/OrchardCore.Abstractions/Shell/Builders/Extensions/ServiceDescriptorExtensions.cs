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

            if (descriptor.ServiceKey == null)
            {
                var implementationType = GetImplementationTypeInternal(descriptor);
                var implementationInstance = GetImplementationInstance(descriptor);
                var implementationFactory = GetImplementationFactory(descriptor);

                if (implementationType != null)
                {
                    return implementationType;
                }
                else if (implementationInstance != null)
                {
                    return implementationInstance.GetType();
                }
                else if (implementationFactory != null)
                {
                    return implementationFactory.GetType().GenericTypeArguments[1];
                }
            }
            else
            {
                var keyedImplementationType = GetKeyedImplementationType(descriptor);
                var keyedImplementationInstance = GetKeyedImplementationInstance(descriptor);
                var keyedImplementationFactory = GetKeyedImplementationFactory(descriptor);

                if (keyedImplementationType != null)
                {
                    return keyedImplementationType;
                }
                else if (keyedImplementationInstance != null)
                {
                    return keyedImplementationInstance.GetType();
                }
                else if (keyedImplementationFactory != null)
                {
                    return keyedImplementationFactory.GetType().GenericTypeArguments[2];
                }
            }

            return null;
        }

        private static Type GetImplementationTypeInternal(ServiceDescriptor descriptor)
        {
            if (IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return descriptor.ImplementationType;
        }


        private static object GetImplementationInstance(ServiceDescriptor descriptor)
        {
            if (IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return descriptor.ImplementationInstance;
        }

        private static Func<IServiceProvider, object> GetImplementationFactory(ServiceDescriptor descriptor)
        {
            if (IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return (Func<IServiceProvider, object>)descriptor.ImplementationFactory;
        }

        private static Type GetKeyedImplementationType(ServiceDescriptor descriptor)
        {
            if (!IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return descriptor.KeyedImplementationType;
        }

        private static object GetKeyedImplementationInstance(ServiceDescriptor descriptor)
        {
            if (!IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return descriptor.KeyedImplementationInstance;
        }

        private static Func<IServiceProvider, object, object> GetKeyedImplementationFactory(ServiceDescriptor descriptor)
        {
            if (!IsKeyedService(descriptor))
            {
                throw new InvalidOperationException("The keyed descriptor misuse.");
            }

            return (Func<IServiceProvider, object, object>)descriptor.KeyedImplementationFactory;
        }

        private static bool IsKeyedService(ServiceDescriptor descriptor) => descriptor.ServiceKey != null;
    }
}
