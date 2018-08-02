using System;
using GraphQL;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    internal class InternalDependencyResolver : IDependencyResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InternalDependencyResolver(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var serviceType = _httpContextAccessor.HttpContext.RequestServices.GetService(type);

            if (serviceType == null)
            {
                return Activator.CreateInstance(type);
            }

            return serviceType;
        }
    }
}
