using System;
using GraphQL;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    /// <summary>
    /// Provides an implementation of <see cref="IDependencyResolver"/> by
    /// resolving the HttpContext request services when a type is resolved.
    /// This should be registered as Singleton.
    /// </summary>
    internal class RequestServicesDependencyResolver : IDependencyResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestServicesDependencyResolver(IHttpContextAccessor httpContextAccessor)
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
