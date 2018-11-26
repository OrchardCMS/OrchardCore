using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagment.Routable;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteRoute : IRouter
    {
        private readonly IAutorouteEntries _entries;
        private readonly IRouter _target;
        private static HashSet<string> _keys = new HashSet<string>(new[] { "area", "controller", "action", "contentItemId", "jsonPath" }, StringComparer.OrdinalIgnoreCase); 

        public AutorouteRoute(IAutorouteEntries entries, IRouter target)
        {
            _target = target;
            _entries = entries;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            string contentItemId = context.Values["contentItemId"]?.ToString();

            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            var displayRouteData = GetContentItemDisplayRoutes(context.HttpContext, contentItemId).Result;
            
            if (string.Equals(context.Values["area"]?.ToString(), displayRouteData?["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) 
                && string.Equals(context.Values["controller"]?.ToString(), displayRouteData?["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(context.Values["action"]?.ToString(), displayRouteData?["action"]?.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(context.Values["jsonPath"]?.ToString(), displayRouteData?["jsonPath"]?.ToString(), StringComparison.OrdinalIgnoreCase)
                )
            {
                if (_entries.TryGetAutorouteEntryByContentItemId(contentItemId, out var entry))
                {
                    var path = entry.Path;

                    if (context.Values.Count > 4)
                    {
                        foreach (var data in context.Values)
                        {
                            if (!_keys.Contains(data.Key))
                            {
                                path = QueryHelpers.AddQueryString(path, data.Key, data.Value.ToString());
                            }
                        }
                    }

                    return new VirtualPathData(_target, path);
                }
            }

            return null;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var requestPath = context.HttpContext.Request.Path.Value;

            if (_entries.TryGetAutorouteEntryByPath(requestPath, out var entry))
            {
                await EnsureRouteData(context, entry);
                await _target.RouteAsync(context);
            }
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

        private async Task EnsureRouteData(RouteContext context, AutorouteEntry entry)
        {
            var displayRoutes = await GetContentItemDisplayRoutes(context.HttpContext, entry.ActualContentItemId);

            if (displayRoutes == null)
            {
                return;
            }

            foreach (var key in _keys)
            {
                if (displayRoutes.ContainsKey(key))
                {
                    context.RouteData.Values[key] = displayRoutes[key];
                }
            }

            if (!String.IsNullOrEmpty(entry.JsonPath))
            {
                context.RouteData.Values["jsonPath"] = entry.JsonPath;
            }

            context.RouteData.Routers.Add(_target);
        }
    }
}
