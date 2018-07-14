using System;
using GraphQL;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    internal class InternalDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public InternalDependencyResolver(
            IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
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
