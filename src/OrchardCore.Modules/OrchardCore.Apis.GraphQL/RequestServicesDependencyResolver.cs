using System;
using GraphQL;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    /// <summary>
    /// Provides an implementation of <see cref="IServiceProvider"/> by
    /// resolving the HttpContext request services when a type is resolved.
    /// This should be registered as Singleton.
    /// </summary>
    internal class RequestServicesDependencyResolver : IServiceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestServicesDependencyResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                return Activator.CreateInstance(serviceType);
            }

            return serviceType;
        }
    }
}
