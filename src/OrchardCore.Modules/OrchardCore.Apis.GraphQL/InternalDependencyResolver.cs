using System;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;

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
            return _serviceProvider.GetService<T>();
        }

        public object Resolve(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}
