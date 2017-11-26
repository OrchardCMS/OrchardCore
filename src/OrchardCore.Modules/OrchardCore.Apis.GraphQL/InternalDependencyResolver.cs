using System;
using GraphQL;

namespace OrchardCore.Apis.GraphQL
{
    internal class InternalDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public InternalDependencyResolver(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var serviceType =  _serviceProvider.GetService(type);

            if (serviceType == null)
            {
                return Activator.CreateInstance(type);
            }

            return serviceType;
        }
    }
}
