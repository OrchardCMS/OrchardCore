using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Settings;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteRoute : IRouter
    {
        private readonly IRouter _target;
        private readonly IAutorouteEntries _entries;
        private readonly AutorouteOptions _options;

        public AutorouteRoute(IRouter target, IAutorouteEntries entries, AutorouteOptions options)
        {
            _target = target;
            _entries = entries;
            _options = options;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (_options.GlobalRouteValues.Count == 0)
            {
                return null;
            }

            string contentItemId = context.Values[_options.ContentItemIdKey]?.ToString();

            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            var homeRoute = context.HttpContext.Features.Get<HomeRouteFeature>()?.HomeRoute;

            if (homeRoute?[_options.ContentItemIdKey]?.ToString() == contentItemId)
            {
                return null;
            }

            foreach (var entry in _options.GlobalRouteValues)
            {
                if (!String.Equals(context.Values[entry.Key]?.ToString(), entry.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            if (_entries.TryGetPath(contentItemId, out string path))
            {
                if (context.Values.Count > _options.GlobalRouteValues.Count + 1)
                {
                    foreach (var entry in context.Values)
                    {
                        if (String.Equals(entry.Key, _options.ContentItemIdKey, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!_options.GlobalRouteValues.ContainsKey(entry.Key))
                        {
                            path = QueryHelpers.AddQueryString(path, entry.Key, entry.Value.ToString());
                        }
                    }
                }

                return new VirtualPathData(_target, path);
            }

            return null;
        }

        public async Task RouteAsync(RouteContext context)
        {
            if (_options.GlobalRouteValues.Count == 0)
            {
                return;
            }

            var requestPath = context.HttpContext.Request.Path.Value;

            if (_entries.TryGetContentItemId(requestPath, out var contentItemId))
            {
                EnsureRouteData(context, contentItemId);
                await _target.RouteAsync(context);
            }
        }

        private void EnsureRouteData(RouteContext context, string contentItemId)
        {
            foreach (var entry in _options.GlobalRouteValues)
            {
                context.RouteData.Values[entry.Key] = entry.Value;
            }

            context.RouteData.Values[_options.ContentItemIdKey] = contentItemId;

            context.RouteData.Routers.Add(_target);
        }
    }
}
