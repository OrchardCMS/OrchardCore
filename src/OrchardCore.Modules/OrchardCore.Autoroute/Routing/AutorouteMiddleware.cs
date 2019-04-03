using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentManagement;

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
                var displayRoutes = await GetContentItemDisplayRoutes(httpContext, contentItemId);

                if (displayRoutes != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, displayRoutes);
                }
            }

            await _next.Invoke(httpContext);
        }

        private async Task<RouteValueDictionary> GetContentItemDisplayRoutes(HttpContext context, string contentItemId)
        {
            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            var contentManager = context.RequestServices.GetService<IContentManager>();
            var contentItem = await contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return null;
            }

            return (await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem))?.DisplayRouteValues;
        }
    }
}
