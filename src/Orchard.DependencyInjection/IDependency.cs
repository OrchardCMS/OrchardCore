using Microsoft.Extensions.DependencyInjection;
using System;

namespace Orchard.DependencyInjection
{
    /// <summary>
    /// Base interface for services that are instantiated per unit of work (i.e. web request).
    /// </summary>
    public interface IDependency
    {
    }

    /// <summary>
    /// Base interface for services that are instantiated per shell/tenant.
    /// </summary>
    public interface ISingletonDependency : IDependency
    {
    }

    /// <summary>
    /// Base interface for services that are instantiated per usage.
    /// </summary>
    public interface ITransientDependency : IDependency
    {
    }

    public abstract class ServiceScopeAttribute : Attribute
    {
        public ServiceScopeAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; private set; }

        public abstract void Register(IServiceCollection services, Type implementationType);
    }

    /// <summary>
    /// Attribute for components that are instantiated per unit of work (i.e. web request).
    /// </summary>
    public class ScopedComponentAttribute: ServiceScopeAttribute
    {
        public ScopedComponentAttribute(Type serviceType) : base(serviceType)
        {
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            services.AddScoped(ServiceType, implementationType);
        }
    }

    /// <summary>
    /// Attribute for components that are instantiated per shell/tenant.
    /// </summary>
    public class SingletonComponentAttribute : ServiceScopeAttribute
    {
        public SingletonComponentAttribute(Type serviceType) : base(serviceType)
        {
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            services.AddSingleton(ServiceType, implementationType);
        }
    }

    /// <summary>
    /// Attribute for components that are instantiated per usage.
    /// </summary>
    public class TransientComponentAttribute : ServiceScopeAttribute
    {
        public TransientComponentAttribute(Type serviceType) : base(serviceType)
        {
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            services.AddTransient(ServiceType, implementationType);
        }
    }
}