using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Orchard.Autoroute.Services;

namespace Orchard.Autoroute.Routing
{
    public class AutorouteRoute : IRouter
    {
        private readonly IAutorouteEntries _entries;
        private readonly IRouter _target;

        public AutorouteRoute(IAutorouteEntries entries, IRouter target)
        {
            _target = target;
            _entries = entries;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context.Values["area"]?.ToString() == "Orchard.Contents" &&
                context.Values["controller"]?.ToString() == "Item" &&
                context.Values["action"]?.ToString() == "Display" &&
                context.Values["contentItemId"] != null)
            {
                // Matching route value, lookup for the id

                string contentItemId = context.Values["contentItemId"]?.ToString();
                string path;

                if (_entries.TryGetPath(contentItemId, out path))
                {
                    return new VirtualPathData(_target, path);
                }
            }

            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            var requestPath = context.HttpContext.Request.Path.ToString();

            string contentItemId;

            if(_entries.TryGetContentItemId(requestPath, out contentItemId))
            {
                context.RouteData.Values["area"] = "Orchard.Contents";
                context.RouteData.Values["controller"] = "Item";
                context.RouteData.Values["action"] = "Display";
                context.RouteData.Values["contentItemId"] = contentItemId;

                context.RouteData.Routers.Add(_target);
                return _target.RouteAsync(context);
            }

            return Task.CompletedTask;
        }
    }
}
