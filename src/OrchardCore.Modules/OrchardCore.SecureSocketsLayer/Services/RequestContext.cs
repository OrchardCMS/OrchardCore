using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.SecureSocketsLayer.Services
{
    public class RequestContext
    {
        public RequestContext(HttpContext httpContext, RouteData routeData)
        {
            HttpContext = httpContext;
            RouteData = routeData;
        }

        public HttpContext HttpContext { get; }
        public RouteData RouteData { get; }
    }
}