using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    public class ShellRouteConstraint<T> : IRouteConstraint where T : class, IRouteConstraint
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShellRouteConstraint(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var constraint = _httpContextAccessor.HttpContext?.RequestServices
                .GetServices<IRouteConstraint>()
                .OfType<T>()
                .FirstOrDefault();

            if (constraint == null)
            {
                return true;
            }

            return constraint.Match(httpContext, route, routeKey, values, routeDirection);
        }
    }
}