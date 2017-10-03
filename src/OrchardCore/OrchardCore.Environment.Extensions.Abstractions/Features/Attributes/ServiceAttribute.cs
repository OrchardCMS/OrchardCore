using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Extensions.Features.Attributes
{
    /// <summary>
    /// Apply this attribute to the type that should be used to register a service implementation.
    /// Used in conjunction with <see cref="ServiceImplAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute() : this(ServiceLifetime.Scoped)
        {
        }

        public ServiceAttribute(Type serviceType) : this(ServiceLifetime.Scoped, serviceType)
        {
        }

        public ServiceAttribute(ServiceLifetime serviceLifetime)
        {
            ServiceLifetime = serviceLifetime;
        }

        public ServiceAttribute(ServiceLifetime serviceLifetime, Type serviceType)
        {
            ServiceType = serviceType;
            ServiceLifetime = serviceLifetime;
        }

        public ServiceLifetime ServiceLifetime { get; }
        public Type ServiceType { get; }
    }
}