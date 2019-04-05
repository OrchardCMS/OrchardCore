using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAutorouteEntries _entries;
        private readonly LinkGenerator _linkGenerator;

        public AutorouteMiddleware(RequestDelegate next, IAutorouteEntries entries, LinkGenerator linkGenerator)
        {
            _next = next;
            _entries = entries;
            _linkGenerator = linkGenerator;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_entries.TryGetContentItemId(httpContext.Request.Path, out var contentItemId))
            {
                var autorouteRoute = httpContext.RequestServices.GetRequiredService<AutorouteRoute>();
                var routeValues = await autorouteRoute.GetValuesAsync(contentItemId);

                if (routeValues != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, routeValues);
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}
